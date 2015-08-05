using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	public class BehaviorManager : MonoBehaviour
	{
		public enum UpdateIntervalType
		{
			EveryFrame,
			SpecifySeconds,
			Manual
		}

		public delegate void TaskBreakpointHandler();

		public class BehaviorTree
		{
			public List<Task> taskList;

			public List<int> parentIndex;

			public List<List<int>> childrenIndex;

			public List<int> relativeChildIndex;

			public List<Stack<int>> activeStack;

			public List<TaskStatus> nonInstantTaskStatus;

			public List<int> interruptionIndex;

			public Behavior behavior;

			public List<Task> originalTaskList;

			public List<int> originalIndex;
		}

		public class TaskAddData
		{
			public bool fromExternalTask;

			public ParentTask parentTask;

			public int parentIndex = -1;

			public Dictionary<string, object> sharedVariables;

			public Dictionary<string, object> inheritedFields;

			public Vector2 nodeOffset = Vector2.zero;
		}

		public enum ThirdPartyObjectType
		{
			PlayMaker,
			uScript,
			DialogueSystem
		}

		public static BehaviorManager instance;

		private BehaviorManager.UpdateIntervalType updateInterval;

		private float updateIntervalSeconds;

		private WaitForSeconds updateWait;

		private List<BehaviorManager.BehaviorTree> behaviorTrees = new List<BehaviorManager.BehaviorTree>();

		private Dictionary<Behavior, BehaviorManager.BehaviorTree> pausedBehaviorTrees = new Dictionary<Behavior, BehaviorManager.BehaviorTree>();

		private Dictionary<Behavior, BehaviorManager.BehaviorTree> behaviorTreeMap = new Dictionary<Behavior, BehaviorManager.BehaviorTree>();

		private Dictionary<object, BehaviorManager.BehaviorTree> objectTreeMap = new Dictionary<object, BehaviorManager.BehaviorTree>();

		private Dictionary<BehaviorManager.BehaviorTree, object> treeObjectMap = new Dictionary<BehaviorManager.BehaviorTree, object>();

		private bool atBreakpoint;

		private bool showExternalTrees;

		private bool dirty;

		public event BehaviorManager.TaskBreakpointHandler onTaskBreakpoint;

		public BehaviorManager.UpdateIntervalType UpdateInterval
		{
			get
			{
				return this.updateInterval;
			}
			set
			{
				this.updateInterval = value;
			}
		}

		public float UpdateIntervalSeconds
		{
			get
			{
				return this.updateIntervalSeconds;
			}
			set
			{
				this.updateIntervalSeconds = value;
			}
		}

		public bool AtBreakpoint
		{
			get
			{
				return this.atBreakpoint;
			}
			set
			{
				this.atBreakpoint = value;
			}
		}

		public bool Dirty
		{
			get
			{
				return this.dirty;
			}
			set
			{
				this.dirty = value;
			}
		}

		public void Awake()
		{
			BehaviorManager.instance = this;
			if (this.updateInterval != BehaviorManager.UpdateIntervalType.EveryFrame)
			{
				this.startCoroutineUpdate();
				base.enabled=false;
			}
		}

		public void startCoroutineUpdate()
		{
			if (this.updateInterval == BehaviorManager.UpdateIntervalType.SpecifySeconds)
			{
				this.updateWait = new WaitForSeconds(this.updateIntervalSeconds);
				base.StartCoroutine("coroutineUpdate");
			}
		}

		public void stopCoroutineUpdate()
		{
			base.StopCoroutine("coroutineUpdate");
		}

		public void enableBehavior(Behavior behavior)
		{
			BehaviorManager.BehaviorTree behaviorTree;
			if (this.isBehaviorEnabled(behavior))
			{
				if (this.pausedBehaviorTrees.ContainsKey(behavior))
				{
					behaviorTree = this.pausedBehaviorTrees[behavior];
					this.behaviorTrees.Add(behaviorTree);
					this.pausedBehaviorTrees.Remove(behavior);
					for (int i = 0; i < behaviorTree.taskList.Count; i++)
					{
						behaviorTree.taskList[i].OnPause(false);
					}
				}
				return;
			}
			if (behavior.GetBehaviorSource().RootTask == null && behavior.externalBehavior != null)
			{
				Task task = ScriptableObject.CreateInstance("EntryTask") as Task;
				task.ID = 0;
				task.Owner = behavior;
				task.NodeData = new NodeData();
				//behavior.GetBehaviorSource().EntryTask = task;
                behavior.GetBehaviorSource().RootTask=task;
				BehaviorReference behaviorReference = ScriptableObject.CreateInstance("BehaviorTreeReference") as BehaviorReference;
				behaviorReference.ID = 1;
				behaviorReference.Owner = behavior;
				behaviorReference.NodeData = new NodeData();
				behaviorReference.NodeData.Position = new Vector2(0f, 100f);
				behaviorReference.externalBehaviors = new ExternalBehavior[]
				{
					behavior.externalBehavior
				};
				behavior.GetBehaviorSource().RootTask = behaviorReference;
			}
			if (behavior.GetBehaviorSource().RootTask == null)
			{
				Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains no root task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
				return;
			}
			behaviorTree = new BehaviorManager.BehaviorTree();
			behaviorTree.taskList = new List<Task>();
			behaviorTree.behavior = behavior;
			behaviorTree.parentIndex = new List<int>();
			behaviorTree.childrenIndex = new List<List<int>>();
			behaviorTree.relativeChildIndex = new List<int>();
			behaviorTree.originalTaskList = new List<Task>();
			behaviorTree.originalIndex = new List<int>();
			behaviorTree.parentIndex.Add(-1);
			behaviorTree.relativeChildIndex.Add(-1);
			if (behavior.GetInstanceID() < 0)
			{
				BehaviorSource behaviorSource = behavior.GetBehaviorSource();
				if (behaviorSource.Variables != null)
				{
					for (int j = 0; j < behaviorSource.Variables.Count; j++)
					{
						behaviorSource.Variables[j] = this.copySharedVariable(behaviorSource.Variables[j]);
					}
				}
			}
			bool flag = false;
			int num = this.addToTaskList(behaviorTree, behavior.GetBehaviorSource().RootTask, ref flag, new BehaviorManager.TaskAddData());
			if (num >= 0)
			{
				this.dirty = (behaviorTree.taskList[0].GetInstanceID() < 0 || flag);
				if (this.dirty)
				{
					behavior.GetBehaviorSource().RootTask = behaviorTree.taskList[0];
					TaskReferences.CheckReferences(behaviorTree.behavior, behaviorTree.taskList);
				}
				behaviorTree.activeStack = new List<Stack<int>>();
				behaviorTree.interruptionIndex = new List<int>();
				behaviorTree.nonInstantTaskStatus = new List<TaskStatus>();
				behaviorTree.activeStack.Add(new Stack<int>());
				behaviorTree.interruptionIndex.Add(-1);
				behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);
				if (behaviorTree.behavior.logTaskChanges)
				{
					for (int k = 0; k < behaviorTree.taskList.Count; k++)
					{
						Debug.Log(string.Format("{0}: Task {1} (index {2})", this.roundedTime(), behaviorTree.taskList[k].GetType(), k));
					}
				}
				Animation animation = behavior.animation;
				AudioSource audio = behavior.audio;
				Camera camera = behavior.camera;
				Collider collider = behavior.collider;
				Collider2D collider2D = behavior.collider2D;
				ConstantForce constantForce = behavior.constantForce;
				GameObject gameObject = behavior.gameObject;
				GUIText guiText = behavior.guiText;
				GUITexture guiTexture = behavior.guiTexture;
				HingeJoint hingeJoint = behavior.hingeJoint;
				Light light = behavior.light;
				ParticleEmitter particleEmitter = behavior.particleEmitter;
				ParticleSystem particleSystem = behavior.particleSystem;
				Renderer renderer = behavior.renderer;
				Rigidbody rigidbody = behavior.rigidbody;
				Rigidbody2D rigidbody2D = behavior.rigidbody2D;
				Transform transform = behavior.transform;
				for (int l = 0; l < behaviorTree.taskList.Count; l++)
				{
					behaviorTree.taskList[l].Animation = animation;
					behaviorTree.taskList[l].Audio = audio;
					behaviorTree.taskList[l].Camera = camera;
					behaviorTree.taskList[l].Collider = collider;
					behaviorTree.taskList[l].Collider2D = collider2D;
					behaviorTree.taskList[l].ConstantForce = constantForce;
					behaviorTree.taskList[l].GameObject = gameObject;
					behaviorTree.taskList[l].GUIText = guiText;
					behaviorTree.taskList[l].GUITexture = guiTexture;
					behaviorTree.taskList[l].HingeJoint = hingeJoint;
					behaviorTree.taskList[l].Light = light;
					behaviorTree.taskList[l].ParticleEmitter = particleEmitter;
					behaviorTree.taskList[l].ParticleSystem = particleSystem;
					behaviorTree.taskList[l].Renderer = renderer;
					behaviorTree.taskList[l].Rigidbody = rigidbody;
					behaviorTree.taskList[l].Rigidbody2D = rigidbody2D;
					behaviorTree.taskList[l].Transform = transform;
					behaviorTree.taskList[l].Owner = behaviorTree.behavior;
					behaviorTree.taskList[l].OnAwake();
				}
				this.behaviorTrees.Add(behaviorTree);
				this.behaviorTreeMap.Add(behavior, behaviorTree);
				if (!behaviorTree.taskList[0].NodeData.Disabled)
				{
					this.pushTask(behaviorTree, 0, 0);
				}
				return;
			}
			switch (num)
			{
			case -4:
				Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains multiple external behavior trees at the root task or as a child of a parent task which cannot contain so many children (such as a decorator task). This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
				return;
			case -3:
				Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" contains an invalid task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
				return;
			case -2:
				Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" cannot find the referenced external task. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
				return;
			case -1:
				Debug.LogError(string.Format("The behavior \"{0}\" on GameObject \"{1}\" is invalid. This behavior will be disabled.", behavior.GetBehaviorSource().behaviorName, behavior.gameObject.name));
				return;
			default:
				return;
			}
		}

		public int addToTaskList(BehaviorManager.BehaviorTree behaviorTree, Task task, ref bool hasExternalBehavior, BehaviorManager.TaskAddData data)
		{
			if (task == null)
			{
				return -3;
			}
			if (task.GetType() == typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior) || task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || task.GetType() == typeof(BehaviorReference) || task.GetType().IsSubclassOf(typeof(BehaviorReference)))
			{
				BehaviorSource[] array;
				if (task.GetType() == typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior) || task.GetType().IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)))
				{
					Debug.LogWarning(string.Format("{0}: The External Behavior Tree task has been deprecated. Use the Behavior Tree Reference task instead.", behaviorTree.behavior.ToString()));
					BehaviorDesigner.Runtime.Tasks.ExternalBehavior externalBehavior = task as BehaviorDesigner.Runtime.Tasks.ExternalBehavior;
					if (!(externalBehavior != null) || !(externalBehavior.externalTask != null))
					{
						return -2;
					}
					Behavior behavior = externalBehavior.externalTask.GetComponent<Behavior>();
					if (!(behavior != null))
					{
						return -2;
					}
					behavior = (UnityEngine.Object.Instantiate(behavior) as Behavior);
					array = new BehaviorSource[]
					{
						behavior.GetBehaviorSource()
					};
				}
				else
				{
					BehaviorReference behaviorReference = task as BehaviorReference;
					if (!(behaviorReference != null))
					{
						return -2;
					}
					ExternalBehavior[] externalBehaviors;
					if ((externalBehaviors = behaviorReference.getExternalBehaviors()) == null)
					{
						return -2;
					}
					array = new BehaviorSource[externalBehaviors.Length];
					for (int i = 0; i < externalBehaviors.Length; i++)
					{
						array[i] = externalBehaviors[i].BehaviorSource;
						array[i].Owner = externalBehaviors[i];
					}
				}
				if (array == null)
				{
					return -2;
				}
				ParentTask parentTask = data.parentTask;
				int parentIndex = data.parentIndex;
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].Owner as Behavior != null && (array[j].Owner as Behavior).UpdateDeprecatedTasks())
					{
						Debug.LogWarning(string.Format("{0}: the data format for this behavior tree has been deprecated. Run the Behavior Designer Update tool or select this GameObject within the inspector to update this behavior tree.", this.ToString()));
					}
					array[j].CheckForJSONSerialization(true);
					Task rootTask = array[j].RootTask;
					if (!(rootTask != null))
					{
						return -2;
					}
					if (!data.fromExternalTask && j == 0)
					{
						if (behaviorTree.behavior.GetInstanceID() < 0)
						{
							behaviorTree.originalTaskList.Add(this.copyTask(behaviorTree, task, data));
						}
						else
						{
							behaviorTree.originalTaskList.Add(task);
						}
					}
					if (array[j].Variables != null)
					{
						for (int k = 0; k < array[j].Variables.Count; k++)
						{
							if (behaviorTree.behavior.GetVariable(array[j].Variables[k].name) == null)
							{
								SharedVariable sharedVariable = this.copySharedVariable(array[j].Variables[k]);
								behaviorTree.behavior.SetVariable(sharedVariable.name, sharedVariable);
								if (data.sharedVariables == null)
								{
									data.sharedVariables = new Dictionary<string, object>();
								}
								if (!data.sharedVariables.ContainsKey(sharedVariable.name))
								{
									data.sharedVariables.Add(sharedVariable.name, sharedVariable);
								}
							}
						}
					}
					FieldInfo[] fields = task.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
					for (int l = 0; l < fields.Length; l++)
					{
						if (fields[l].GetCustomAttributes(typeof(InheritedFieldAttribute), false).Length > 0)
						{
							if (data.inheritedFields == null)
							{
								data.inheritedFields = new Dictionary<string, object>();
							}
							if (!data.inheritedFields.ContainsKey(fields[l].Name))
							{
								if (fields[j].FieldType.IsSubclassOf(typeof(SharedVariable)))
								{
									SharedVariable sharedVariable2 = fields[l].GetValue(task) as SharedVariable;
									if (sharedVariable2.IsShared)
									{
										SharedVariable sharedVariable3 = behaviorTree.behavior.GetVariable(sharedVariable2.name);
										if (sharedVariable3 == null && data.sharedVariables != null && data.sharedVariables.ContainsKey(sharedVariable2.name))
										{
											sharedVariable3 = (data.sharedVariables[sharedVariable2.name] as SharedVariable);
										}
										data.inheritedFields.Add(fields[l].Name, sharedVariable3);
									}
									else
									{
										data.inheritedFields.Add(fields[l].Name, sharedVariable2);
									}
								}
								else
								{
									data.inheritedFields.Add(fields[l].Name, fields[l].GetValue(task));
								}
							}
						}
					}
					if (j > 0)
					{
						data.parentTask = parentTask;
						data.parentIndex = parentIndex;
						if (data.parentTask == null || j >= data.parentTask.MaxChildren())
						{
							return -4;
						}
						behaviorTree.parentIndex.Add(data.parentIndex);
						behaviorTree.relativeChildIndex.Add(data.parentTask.Children.Count);
						behaviorTree.childrenIndex[data.parentIndex].Add(behaviorTree.taskList.Count);
						data.parentTask.AddChild(rootTask, data.parentTask.Children.Count);
					}
					hasExternalBehavior = true;
					bool fromExternalTask = data.fromExternalTask;
					data.fromExternalTask = true;
					int result;
					if ((result = this.addToTaskList(behaviorTree, rootTask, ref hasExternalBehavior, data)) < 0)
					{
						return result;
					}
					data.fromExternalTask = fromExternalTask;
				}
			}
			else
			{
				Task task2;
				if (data.fromExternalTask || behaviorTree.behavior.GetInstanceID() < 0)
				{
					task2 = this.copyTask(behaviorTree, task, data);
					behaviorTree.taskList.Add(task2);
					if (data.fromExternalTask)
					{
						if (data.parentTask == null)
						{
							task2.NodeData.Position = behaviorTree.behavior.GetBehaviorSource().RootTask.NodeData.Position;
						}
						else
						{
							task2.NodeData.Position = data.parentTask.NodeData.Position + data.nodeOffset;
							int index = behaviorTree.relativeChildIndex[behaviorTree.relativeChildIndex.Count - 1];
							int index2 = behaviorTree.parentIndex[behaviorTree.parentIndex.Count - 1];
							Type type = behaviorTree.originalTaskList[behaviorTree.originalIndex[index2]].GetType();
							if (type == typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior) || type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type == typeof(BehaviorReference) || type.IsSubclassOf(typeof(BehaviorReference)))
							{
								data.parentTask.ReplaceAddChild(task2, index);
							}
							else
							{
								data.parentTask.ReplaceAddChild(behaviorTree.originalTaskList[behaviorTree.originalTaskList.Count - 1], index);
							}
						}
					}
					else if (data.parentTask != null)
					{
						int index3 = behaviorTree.relativeChildIndex[behaviorTree.relativeChildIndex.Count - 1];
						data.parentTask.ReplaceAddChild(task2, index3);
					}
				}
				else
				{
					task.ReferenceID = behaviorTree.taskList.Count;
					behaviorTree.taskList.Add(task);
					task2 = task;
				}
				if (behaviorTree.originalTaskList.Count == 0)
				{
					behaviorTree.originalTaskList.Add(task2);
					behaviorTree.originalIndex.Add(0);
				}
				else
				{
					if (!data.fromExternalTask)
					{
						behaviorTree.originalTaskList.Add(task2);
					}
					behaviorTree.originalIndex.Add(behaviorTree.originalTaskList.Count - 1);
				}
				if (task.GetType().IsSubclassOf(typeof(ParentTask)))
				{
					ParentTask parentTask2 = task as ParentTask;
					if (parentTask2.Children.Count == 0)
					{
						return -1;
					}
					int num = behaviorTree.taskList.Count - 1;
					behaviorTree.childrenIndex.Add(new List<int>());
					int count = parentTask2.Children.Count;
					for (int m = 0; m < count; m++)
					{
						behaviorTree.parentIndex.Add(num);
						behaviorTree.relativeChildIndex.Add(m);
						behaviorTree.childrenIndex[num].Add(behaviorTree.taskList.Count);
						data.parentTask = (task2 as ParentTask);
						data.parentIndex = num;
						data.nodeOffset = parentTask2.Children[m].NodeData.Position - parentTask2.NodeData.Position;
						int result2;
						if ((result2 = this.addToTaskList(behaviorTree, parentTask2.Children[m], ref hasExternalBehavior, data)) < 0)
						{
							return result2;
						}
					}
				}
				else
				{
					behaviorTree.childrenIndex.Add(null);
				}
			}
			return 0;
		}

		private Task copyTask(BehaviorManager.BehaviorTree behaviorTree, Task task, BehaviorManager.TaskAddData data)
		{
			Task task2 = ScriptableObject.CreateInstance(task.GetType()) as Task;
			FieldInfo[] fields = task.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (task.GetType().IsSubclassOf(typeof(ParentTask)) && fields[i].Name.Equals("children"))
				{
					fields[i].SetValue(task2, null);
				}
				else
				{
					object value = fields[i].GetValue(task);
					if (data.inheritedFields != null && data.inheritedFields.ContainsKey(fields[i].Name) && fields[i].GetCustomAttributes(typeof(InheritedFieldAttribute), false).Length > 0)
					{
						fields[i].SetValue(task2, data.inheritedFields[fields[i].Name]);
					}
					else if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable)) && value != null)
					{
						SharedVariable sharedVariable = value as SharedVariable;
						string name = sharedVariable.name;
						if (!sharedVariable.IsShared)
						{
							fields[i].SetValue(task2, this.copySharedVariable(sharedVariable));
						}
						else if (behaviorTree.behavior.GetVariable(name) != null)
						{
							fields[i].SetValue(task2, behaviorTree.behavior.GetVariable(name));
						}
						else if (data.sharedVariables != null && data.sharedVariables.ContainsKey(name))
						{
							fields[i].SetValue(task2, data.sharedVariables[name]);
						}
					}
					else
					{
						fields[i].SetValue(task2, value);
					}
				}
			}
			task2.ReferenceID = (task.ReferenceID = behaviorTree.taskList.Count);
			task2.ID = task.ID;
			task2.IsInstant = task.IsInstant;
			task2.Owner = task.Owner;
			task2.NodeData = new NodeData();
			task2.NodeData.copyFrom(task.NodeData, task2);
			return task2;
		}

		private SharedVariable copySharedVariable(SharedVariable variable)
		{
			SharedVariable sharedVariable = ScriptableObject.CreateInstance(variable.GetType()) as SharedVariable;
			sharedVariable.name=variable.name;
			sharedVariable.SetValue(variable.GetValue());
			sharedVariable.IsShared = variable.IsShared;
			return sharedVariable;
		}

		public void disableBehavior(Behavior behavior)
		{
			this.disableBehavior(behavior, false);
		}

		public void disableBehavior(Behavior behavior, bool paused)
		{
			if (!this.isBehaviorEnabled(behavior))
			{
				return;
			}
			if (behavior.logTaskChanges)
			{
				Debug.Log(string.Format("{0}: {1} {2}", this.roundedTime(), paused ? "Pausing" : "Disabling", behavior.ToString()));
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			if (paused)
			{
				if (!this.pausedBehaviorTrees.ContainsKey(behavior))
				{
					this.pausedBehaviorTrees.Add(behavior, behaviorTree);
					for (int i = 0; i < behaviorTree.taskList.Count; i++)
					{
						behaviorTree.taskList[i].OnPause(true);
					}
				}
			}
			else
			{
				TaskStatus taskStatus = TaskStatus.Success;
				for (int j = behaviorTree.activeStack.Count - 1; j > -1; j--)
				{
					while (behaviorTree.activeStack[j].Count > 0)
					{
						int count = behaviorTree.activeStack[j].Count;
						this.popTask(behaviorTree, behaviorTree.activeStack[j].Peek(), j, ref taskStatus, true, false);
						if (count == 1)
						{
							break;
						}
					}
				}
				this.behaviorTreeMap.Remove(behavior);
			}
			this.behaviorTrees.Remove(behaviorTree);
		}

		public void restartBehavior(Behavior behavior)
		{
			if (!this.isBehaviorEnabled(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			TaskStatus taskStatus = TaskStatus.Success;
			for (int i = behaviorTree.activeStack.Count - 1; i > -1; i--)
			{
				while (behaviorTree.activeStack[i].Count > 0)
				{
					int count = behaviorTree.activeStack[i].Count;
					this.popTask(behaviorTree, behaviorTree.activeStack[i].Peek(), i, ref taskStatus, true, false);
					if (count == 1)
					{
						break;
					}
				}
			}
			this.restart(behaviorTree);
		}

		public bool isBehaviorEnabled(Behavior behavior)
		{
			return this.behaviorTreeMap != null && behavior != null && this.behaviorTreeMap.ContainsKey(behavior);
		}

		public void Update()
		{
			this.Tick();
		}

		private IEnumerator coroutineUpdate()
		{
			while (true)
			{
				this.Tick();
				yield return this.updateWait;
			}
			yield break;
		}

		[Obsolete("BehaviorManager.tick is deprecated. Use BehaviorManager.Tick")]
		public void tick()
		{
			this.Tick();
		}

		public void Tick()
		{
			for (int i = 0; i < this.behaviorTrees.Count; i++)
			{
				BehaviorManager.BehaviorTree behaviorTree = this.behaviorTrees[i];
				for (int j = behaviorTree.activeStack.Count - 1; j > -1; j--)
				{
					TaskStatus taskStatus = TaskStatus.Inactive;
					int num;
					if (j < behaviorTree.interruptionIndex.Count && (num = behaviorTree.interruptionIndex[j]) != -1)
					{
						behaviorTree.interruptionIndex[j] = -1;
						while (behaviorTree.activeStack[j].Peek() != num)
						{
							int count = behaviorTree.activeStack[j].Count;
							this.popTask(behaviorTree, behaviorTree.activeStack[j].Peek(), j, ref taskStatus, true);
							if (count == 1)
							{
								break;
							}
						}
						if (j < behaviorTree.activeStack.Count && behaviorTree.taskList[num] == behaviorTree.taskList[behaviorTree.activeStack[j].Peek()])
						{
							taskStatus = (behaviorTree.taskList[num] as ParentTask).OverrideStatus();
							this.popTask(behaviorTree, num, j, ref taskStatus, true);
						}
					}
					int num2 = 0;
					int num3 = -1;
					while (taskStatus != TaskStatus.Running && j < behaviorTree.activeStack.Count && behaviorTree.activeStack[j].Count > 0)
					{
						int num4 = behaviorTree.activeStack[j].Peek();
						if ((j < behaviorTree.activeStack.Count && behaviorTree.activeStack[j].Count > 0 && num3 == behaviorTree.activeStack[j].Peek()) || !this.isBehaviorEnabled(behaviorTree.behavior))
						{
							break;
						}
						num3 = num4;
						taskStatus = this.runTask(behaviorTree, num4, j, taskStatus);
						if (++num2 > behaviorTree.taskList.Count)
						{
							Debug.LogError(string.Format("Error: Every task within Behavior \"{0}\" has been called and no taks is running. Disabling Behavior to prevent infinite loop.", behaviorTree.behavior));
							this.disableBehavior(behaviorTree.behavior);
							break;
						}
					}
				}
			}
		}

        /// <summary>
        /// 添加任务
        /// </summary>
        /// <param name="behaviorTree"></param>
        /// <param name="taskIndex"></param>
        /// <param name="stackIndex"></param>
		private void pushTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, int stackIndex)
		{
			if (!this.isBehaviorEnabled(behaviorTree.behavior))
			{
				return;
			}
			if (behaviorTree.activeStack[stackIndex].Count == 0 || behaviorTree.activeStack[stackIndex].Peek() != taskIndex)
			{
				behaviorTree.activeStack[stackIndex].Push(taskIndex);
				behaviorTree.nonInstantTaskStatus[stackIndex] = TaskStatus.Running;
				Type type = behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].GetType();
				if (type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.Equals(typeof(BehaviorReference)) || type.IsSubclassOf(typeof(BehaviorReference)))
				{
					int num = behaviorTree.parentIndex[taskIndex];
					if (num != -1)
					{
						type = behaviorTree.originalTaskList[behaviorTree.originalIndex[num]].GetType();
					}
					if (num == -1 || (!type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && !type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && !type.Equals(typeof(BehaviorReference)) && !type.IsSubclassOf(typeof(BehaviorReference))))
					{
						behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.PushTime = Time.realtimeSinceStartup;
					}
				}
				behaviorTree.taskList[taskIndex].NodeData.PushTime = Time.realtimeSinceStartup;//添加任务时间
				this.setInactiveExecutionStatus(behaviorTree, taskIndex);
				if (behaviorTree.taskList[taskIndex].NodeData.IsBreakpoint)
				{
					this.atBreakpoint = true;
					if (this.onTaskBreakpoint != null)
					{
						this.onTaskBreakpoint();
					}
				}
				if (behaviorTree.behavior.logTaskChanges)
				{
					MonoBehaviour.print(string.Format("{0}: {1}: Push task {2} (index {3}) at stack index {4}", new object[]
					{
						this.roundedTime(),
						behaviorTree.behavior.ToString(),
						behaviorTree.taskList[taskIndex].GetType(),
						taskIndex,
						stackIndex
					}));
				}
				behaviorTree.taskList[taskIndex].OnStart();//任务开始运行
			}
		}


        /// <summary>
        /// 任务运行
        /// </summary>
        /// <param name="behaviorTree"></param>
        /// <param name="taskIndex"></param>
        /// <param name="stackIndex"></param>
        /// <param name="previousStatus"></param>
        /// <returns></returns>
        private TaskStatus runTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, int stackIndex, TaskStatus previousStatus)
        {
            Task task = behaviorTree.taskList[taskIndex];
            if (task == null)
            {
                return previousStatus;
            }
            if (behaviorTree.taskList[taskIndex].NodeData.Disabled)
            {
                if (behaviorTree.behavior.logTaskChanges)
                {
                    MonoBehaviour.print(string.Format("{0}: {1}: Skip task {2} (index {3}) at stack index {4} (task disabled)", new object[]
					{
						this.roundedTime(),
						behaviorTree.behavior.ToString(),
						behaviorTree.taskList[taskIndex].GetType(),
						taskIndex,
						stackIndex
					}));
                }
                if (behaviorTree.parentIndex[taskIndex] != -1)
                {
                    ParentTask parentTask = behaviorTree.taskList[behaviorTree.parentIndex[taskIndex]] as ParentTask;
                    if (!parentTask.CanRunParallelChildren())
                    {
                        parentTask.OnChildExecuted(TaskStatus.Success);
                    }
                    else
                    {
                        parentTask.OnChildExecuted(behaviorTree.relativeChildIndex[taskIndex], TaskStatus.Success);
                    }
                }
                return TaskStatus.Success;
            }
            TaskStatus taskStatus = previousStatus;
            if (!task.IsInstant && (behaviorTree.nonInstantTaskStatus[stackIndex] == TaskStatus.Failure || behaviorTree.nonInstantTaskStatus[stackIndex] == TaskStatus.Success))
            {
                taskStatus = behaviorTree.nonInstantTaskStatus[stackIndex];
                this.popTask(behaviorTree, taskIndex, stackIndex, ref taskStatus, true);
                return taskStatus;
            }
            this.pushTask(behaviorTree, taskIndex, stackIndex);
            if (this.atBreakpoint)
            {
                return TaskStatus.Running;
            }
            if (task is ParentTask)
            {
                ParentTask parentTask2 = task as ParentTask;
                if (!parentTask2.CanRunParallelChildren() || parentTask2.OverrideStatus(TaskStatus.Running) != TaskStatus.Running)
                {
                    int num = 0;
                    TaskStatus taskStatus2 = TaskStatus.Inactive;
                    int num2 = stackIndex;
                    int num3 = -1;
                    while (parentTask2.CanExecute() && (taskStatus2 != TaskStatus.Running || parentTask2.CanRunParallelChildren()))
                    {
                        List<int> list = behaviorTree.childrenIndex[taskIndex];
                        int num4 = parentTask2.CurrentChildIndex();
                        if ((num4 == num3 && taskStatus != TaskStatus.Running) || !this.isBehaviorEnabled(behaviorTree.behavior))
                        {
                            taskStatus = TaskStatus.Running;
                            break;
                        }
                        num3 = num4;
                        if (parentTask2.CanRunParallelChildren())
                        {
                            behaviorTree.activeStack.Add(new Stack<int>());
                            behaviorTree.interruptionIndex.Add(-1);
                            behaviorTree.nonInstantTaskStatus.Add(TaskStatus.Inactive);
                            stackIndex = behaviorTree.activeStack.Count - 1;
                            parentTask2.OnChildRunning(num4);
                        }
                        else
                        {
                            parentTask2.OnChildRunning();
                        }
                        taskStatus2 = (taskStatus = this.runTask(behaviorTree, list[num4], stackIndex, taskStatus));
                        if (++num > behaviorTree.taskList.Count)
                        {
                            Debug.LogError(string.Format("Error: Every task within Behavior \"{0}\" has been called and no taks is running. Disabling Behavior to prevent infinite loop.", behaviorTree.behavior));
                            this.disableBehavior(behaviorTree.behavior);
                            break;
                        }
                    }
                    stackIndex = num2;
                }
                taskStatus = parentTask2.OverrideStatus(taskStatus);
            }
            else
            {
                taskStatus = task.OnUpdate();//任务更新
            }
            if (taskStatus != TaskStatus.Running)
            {
                if (task.IsInstant)
                {
                    this.popTask(behaviorTree, taskIndex, stackIndex, ref taskStatus, true);
                }
                else
                {
                    behaviorTree.nonInstantTaskStatus[stackIndex] = taskStatus;
                }
            }
            return taskStatus;
        }




		private void setInactiveExecutionStatus(BehaviorManager.BehaviorTree behaviorTree, int taskIndex)
		{
			if (behaviorTree.taskList[taskIndex].NodeData.ExecutionStatus != TaskStatus.Inactive)
			{
				behaviorTree.taskList[taskIndex].NodeData.ExecutionStatus = TaskStatus.Inactive;
				Type type = behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].GetType();
				if (type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.Equals(typeof(BehaviorReference)) || type.IsSubclassOf(typeof(BehaviorReference)))
				{
					behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.ExecutionStatus = TaskStatus.Inactive;
				}
				if (behaviorTree.taskList[taskIndex] is ParentTask)
				{
					for (int i = 0; i < behaviorTree.childrenIndex[taskIndex].Count; i++)
					{
						this.setInactiveExecutionStatus(behaviorTree, behaviorTree.childrenIndex[taskIndex][i]);
					}
				}
			}
		}

		private void popTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, int stackIndex, ref TaskStatus status, bool popChildren)
		{
			this.popTask(behaviorTree, taskIndex, stackIndex, ref status, popChildren, true);
		}

		private void popTask(BehaviorManager.BehaviorTree behaviorTree, int taskIndex, int stackIndex, ref TaskStatus status, bool popChildren, bool notifyOnEmptyStack)
		{
			if (!this.isBehaviorEnabled(behaviorTree.behavior))
			{
				return;
			}
			if (taskIndex != behaviorTree.activeStack[stackIndex].Peek())
			{
				MonoBehaviour.print(string.Concat(new object[]
				{
					"error: popping ",
					taskIndex,
					" but ",
					behaviorTree.activeStack[stackIndex].Peek(),
					" is on top"
				}));
			}
			behaviorTree.activeStack[stackIndex].Pop();
			behaviorTree.nonInstantTaskStatus[stackIndex] = TaskStatus.Inactive;
			behaviorTree.taskList[taskIndex].OnEnd();
			Type type = behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].GetType();
			if (type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.Equals(typeof(BehaviorReference)) || type.IsSubclassOf(typeof(BehaviorReference)))
			{
				int num = behaviorTree.parentIndex[taskIndex];
				if (num != -1)
				{
					type = behaviorTree.originalTaskList[behaviorTree.originalIndex[num]].GetType();
				}
				if (num == -1 || (!type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && !type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && !type.Equals(typeof(BehaviorReference)) && !type.IsSubclassOf(typeof(BehaviorReference))))
				{
					behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.PushTime = -1f;
					behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.PopTime = Time.realtimeSinceStartup;
					behaviorTree.originalTaskList[behaviorTree.originalIndex[taskIndex]].NodeData.ExecutionStatus = status;
				}
			}
			behaviorTree.taskList[taskIndex].NodeData.PushTime = -1f;
			behaviorTree.taskList[taskIndex].NodeData.PopTime = Time.realtimeSinceStartup;
			behaviorTree.taskList[taskIndex].NodeData.ExecutionStatus = status;
			if (behaviorTree.behavior.logTaskChanges)
			{
				MonoBehaviour.print(string.Format("{0}: {1}: Pop task {2} (index {3}) at stack index {4} with status {5}", new object[]
				{
					this.roundedTime(),
					behaviorTree.behavior.ToString(),
					behaviorTree.taskList[taskIndex].GetType(),
					taskIndex,
					stackIndex,
					status
				}));
			}
			if (behaviorTree.parentIndex[taskIndex] != -1)
			{
				ParentTask parentTask = behaviorTree.taskList[behaviorTree.parentIndex[taskIndex]] as ParentTask;
				if (!parentTask.CanRunParallelChildren())
				{
					parentTask.OnChildExecuted(status);
					status = parentTask.Decorate(status);
				}
				else
				{
					parentTask.OnChildExecuted(behaviorTree.relativeChildIndex[taskIndex], status);
				}
			}
			if (popChildren)
			{
				for (int i = behaviorTree.activeStack.Count - 1; i > stackIndex; i--)
				{
					if (behaviorTree.activeStack[i].Count > 0 && this.isParentTask(behaviorTree, taskIndex, behaviorTree.activeStack[i].Peek()))
					{
						TaskStatus taskStatus = TaskStatus.Failure;
						while (i < behaviorTree.activeStack.Count && behaviorTree.activeStack[i].Count > 0)
						{
							this.popTask(behaviorTree, behaviorTree.activeStack[i].Peek(), i, ref taskStatus, false, notifyOnEmptyStack);
						}
					}
				}
			}
			if (behaviorTree.activeStack[stackIndex].Count == 0)
			{
				if (stackIndex == 0)
				{
					if (notifyOnEmptyStack)
					{
						if (behaviorTree.behavior.restartWhenComplete)
						{
							this.restart(behaviorTree);
						}
						else
						{
							this.disableBehavior(behaviorTree.behavior);
						}
					}
					status = TaskStatus.Inactive;
					return;
				}
				this.removeStack(behaviorTree, stackIndex);
				status = TaskStatus.Running;
			}
		}

		private void restart(BehaviorManager.BehaviorTree behaviorTree)
		{
			if (behaviorTree.behavior.logTaskChanges)
			{
				Debug.Log(string.Format("{0}: Restarting {1}", this.roundedTime(), behaviorTree.behavior.ToString()));
			}
			for (int i = 0; i < behaviorTree.taskList.Count; i++)
			{
				behaviorTree.taskList[i].OnBehaviorRestart();
			}
			this.pushTask(behaviorTree, 0, 0);
		}

		private bool isParentTask(BehaviorManager.BehaviorTree behaviorTree, int possibleParent, int possibleChild)
		{
			int num2;
			for (int num = possibleChild; num != -1; num = num2)
			{
				num2 = behaviorTree.parentIndex[num];
				if (num2 == possibleParent)
				{
					return true;
				}
			}
			return false;
		}

		public void interrupt(Behavior behavior, Task task)
		{
			if (this.behaviorTreeMap == null || this.behaviorTreeMap.Count == 0 || behavior == null || !this.behaviorTreeMap.ContainsKey(behavior))
			{
				return;
			}
			int num = -1;
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			for (int i = 0; i < behaviorTree.taskList.Count; i++)
			{
				if (behaviorTree.taskList[i].ReferenceID == task.ReferenceID)
				{
					num = i;
					break;
				}
			}
			int num2 = -1;
			if (num > -1)
			{
				for (int j = 0; j < behaviorTree.activeStack.Count; j++)
				{
					if (behaviorTree.activeStack[j].Count > 0)
					{
						int num3 = behaviorTree.activeStack[j].Peek();
						while (num3 != -1)
						{
							if (num3 == num)
							{
								behaviorTree.interruptionIndex[j] = num;
								num2 = j;
								if (behavior.logTaskChanges)
								{
									Debug.Log(string.Format("{0}: {1}: Interrupt task {2} with interrupt index {3} at stack index {4}", new object[]
									{
										this.roundedTime(),
										behaviorTree.behavior.ToString(),
										task.GetType().ToString(),
										num,
										num2
									}));
									break;
								}
								break;
							}
							else
							{
								num3 = behaviorTree.parentIndex[num3];
							}
						}
					}
				}
			}
			if (this.treeObjectMap.ContainsKey(behaviorTree))
			{
				object obj = this.treeObjectMap[behaviorTree];
				this.objectTreeMap.Remove(this.treeObjectMap[behaviorTree]);
				this.treeObjectMap.Remove(behaviorTree);
				bool flag = false;
				Type type = Type.GetType("BehaviorDesigner.Runtime.BehaviorManager_PlayMaker");
				if (type != null)
				{
					MethodInfo method = type.GetMethod("StopPlayMaker");
					if (method != null)
					{
						object obj2 = null;
						if (num2 != -1)
						{
							int num4 = behaviorTree.activeStack[num2].Peek();
							Type type2 = Type.GetType("BehaviorDesigner.Runtime.Tasks.Start_PlayMakerFSM");
							while (num4 != -1)
							{
								if (behaviorTree.taskList[num4].GetType().Equals(type2))
								{
									obj2 = behaviorTree.taskList[num4];
									break;
								}
								num4 = behaviorTree.parentIndex[num4];
							}
						}
						flag = (bool)method.Invoke(null, new object[]
						{
							obj,
							obj2
						});
					}
				}
				if (!flag)
				{
					type = Type.GetType("BehaviorDesigner.Runtime.BehaviorManager_uScript");
					if (type != null)
					{
						MethodInfo method2 = type.GetMethod("StopuScript");
						if (method2 != null)
						{
							flag = (bool)method2.Invoke(null, new object[]
							{
								obj
							});
						}
					}
				}
				if (!flag)
				{
					type = Type.GetType("BehaviorDesigner.Runtime.BehaviorManager_DialogueSystem");
					if (type != null)
					{
						object obj3 = null;
						if (num2 != -1)
						{
							int num5 = behaviorTree.activeStack[num2].Peek();
							Type type3 = Type.GetType("BehaviorDesigner.Runtime.Tasks.StartConversation");
							Type type4 = Type.GetType("BehaviorDesigner.Runtime.Tasks.StartSequence");
							while (num5 != -1)
							{
								Type type5 = behaviorTree.taskList[num5].GetType();
								if (type5.Equals(type3) || type5.Equals(type4))
								{
									obj3 = behaviorTree.taskList[num5];
									break;
								}
								num5 = behaviorTree.parentIndex[num5];
							}
						}
						MethodInfo method3 = type.GetMethod("StopDialogueSystem");
						if (method3 != null)
						{
							method3.Invoke(null, new object[]
							{
								obj,
								obj3
							});
						}
					}
				}
			}
		}

		private void removeStack(BehaviorManager.BehaviorTree behaviorTree, int stackIndex)
		{
			behaviorTree.activeStack.RemoveAt(stackIndex);
			behaviorTree.interruptionIndex.RemoveAt(stackIndex);
			behaviorTree.nonInstantTaskStatus.RemoveAt(stackIndex);
		}

		public void BehaviorOnCollisionEnter(Collision collision, Behavior behavior)
		{
			if (this.behaviorTreeMap == null || this.behaviorTreeMap.Count == 0 || behavior == null || !this.behaviorTreeMap.ContainsKey(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			for (int i = 0; i < behaviorTree.activeStack.Count; i++)
			{
				int index = behaviorTree.activeStack[i].Peek();
				behaviorTree.taskList[index].OnCollisionEnter(collision);
			}
		}

		public void BehaviorOnCollisionExit(Collision collision, Behavior behavior)
		{
			if (this.behaviorTreeMap == null || this.behaviorTreeMap.Count == 0 || behavior == null || !this.behaviorTreeMap.ContainsKey(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			for (int i = 0; i < behaviorTree.activeStack.Count; i++)
			{
				int index = behaviorTree.activeStack[i].Peek();
				behaviorTree.taskList[index].OnCollisionExit(collision);
			}
		}

		public void BehaviorOnCollisionStay(Collision collision, Behavior behavior)
		{
			if (this.behaviorTreeMap == null || this.behaviorTreeMap.Count == 0 || behavior == null || !this.behaviorTreeMap.ContainsKey(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			for (int i = 0; i < behaviorTree.activeStack.Count; i++)
			{
				int index = behaviorTree.activeStack[i].Peek();
				behaviorTree.taskList[index].OnCollisionStay(collision);
			}
		}

		public void BehaviorOnTriggerEnter(Collider other, Behavior behavior)
		{
			if (this.behaviorTreeMap == null || this.behaviorTreeMap.Count == 0 || behavior == null || !this.behaviorTreeMap.ContainsKey(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			for (int i = 0; i < behaviorTree.activeStack.Count; i++)
			{
				int index = behaviorTree.activeStack[i].Peek();
				behaviorTree.taskList[index].OnTriggerEnter(other);
			}
		}

		public void BehaviorOnTriggerExit(Collider other, Behavior behavior)
		{
			if (this.behaviorTreeMap == null || this.behaviorTreeMap.Count == 0 || behavior == null || !this.behaviorTreeMap.ContainsKey(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			for (int i = 0; i < behaviorTree.activeStack.Count; i++)
			{
				int index = behaviorTree.activeStack[i].Peek();
				behaviorTree.taskList[index].OnTriggerExit(other);
			}
		}

		public void BehaviorOnTriggerStay(Collider other, Behavior behavior)
		{
			if (this.behaviorTreeMap == null || this.behaviorTreeMap.Count == 0 || behavior == null || !this.behaviorTreeMap.ContainsKey(behavior))
			{
				return;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			for (int i = 0; i < behaviorTree.activeStack.Count; i++)
			{
				int index = behaviorTree.activeStack[i].Peek();
				behaviorTree.taskList[index].OnTriggerStay(other);
			}
		}

		public bool mapObjectToTree(object objectKey, Behavior behavior, BehaviorManager.ThirdPartyObjectType objectType)
		{
			if (this.objectTreeMap.ContainsKey(objectKey))
			{
				string arg = "";
				switch (objectType)
				{
				case BehaviorManager.ThirdPartyObjectType.PlayMaker:
					arg = "PlayMaker FSM";
					break;
				case BehaviorManager.ThirdPartyObjectType.uScript:
					arg = "uScript Graph";
					break;
				case BehaviorManager.ThirdPartyObjectType.DialogueSystem:
					arg = "Dialogue System";
					break;
				}
				Debug.LogError(string.Format("Only one behavior can be mapped to the same instance of the {0}.", arg));
				return false;
			}
			this.objectTreeMap.Add(objectKey, this.behaviorTreeMap[behavior]);
			this.treeObjectMap.Add(this.behaviorTreeMap[behavior], objectKey);
			return true;
		}

		public BehaviorManager.BehaviorTree treeForObject(object objectKey)
		{
			if (!this.objectTreeMap.ContainsKey(objectKey))
			{
				return null;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.objectTreeMap[objectKey];
			this.objectTreeMap.Remove(objectKey);
			this.treeObjectMap.Remove(behaviorTree);
			return behaviorTree;
		}

		public int stackCount(BehaviorManager.BehaviorTree behaviorTree)
		{
			return behaviorTree.activeStack.Count;
		}

		public Task taskWithTreeAndStackIndex(BehaviorManager.BehaviorTree behaviorTree, int stackIndex)
		{
			if (behaviorTree.activeStack[stackIndex].Count == 0)
			{
				return null;
			}
			return behaviorTree.taskList[behaviorTree.activeStack[stackIndex].Peek()];
		}

		private decimal roundedTime()
		{
			return Math.Round((decimal)Time.time, 5, MidpointRounding.AwayFromZero);
		}

		public List<Task> getTaskList(Behavior behavior)
		{
			if (this.behaviorTreeMap == null || this.behaviorTreeMap.Count == 0 || behavior == null || !this.behaviorTreeMap.ContainsKey(behavior))
			{
				return null;
			}
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			return behaviorTree.taskList;
		}

		public bool setShouldShowExternalTree(Behavior behavior, bool show)
		{
			if (this.behaviorTreeMap == null || this.behaviorTreeMap.Count == 0 || behavior == null || !this.behaviorTreeMap.ContainsKey(behavior))
			{
				return false;
			}
			this.showExternalTrees = show;
			bool result = false;
			BehaviorManager.BehaviorTree behaviorTree = this.behaviorTreeMap[behavior];
			for (int i = 0; i < behaviorTree.taskList.Count; i++)
			{
				Type type = behaviorTree.originalTaskList[behaviorTree.originalIndex[i]].GetType();
				if (type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) || type.Equals(typeof(BehaviorReference)) || type.IsSubclassOf(typeof(BehaviorReference)))
				{
					int num = behaviorTree.parentIndex[i];
					if (num != -1)
					{
						type = behaviorTree.originalTaskList[behaviorTree.originalIndex[num]].GetType();
					}
					if (num == -1)
					{
						Task rootTask = this.showExternalTrees ? behaviorTree.taskList[i] : behaviorTree.originalTaskList[behaviorTree.originalIndex[i]];
						behavior.GetBehaviorSource().RootTask = rootTask;
						result = true;
					}
					else if (!type.Equals(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && !type.IsSubclassOf(typeof(BehaviorDesigner.Runtime.Tasks.ExternalBehavior)) && !type.Equals(typeof(BehaviorReference)) && !type.IsSubclassOf(typeof(BehaviorReference)))
					{
						(behaviorTree.taskList[num] as ParentTask).Children[behaviorTree.relativeChildIndex[i]] = (this.showExternalTrees ? behaviorTree.taskList[i] : behaviorTree.originalTaskList[behaviorTree.originalIndex[i]]);
						result = true;
					}
				}
			}
			return result;
		}
	}
}
