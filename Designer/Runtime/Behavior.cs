using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public  class Behavior : MonoBehaviour, IBehavior
	{
		public bool startWhenEnabled = true;

		public bool pauseWhenDisabled;

		public bool restartWhenComplete;

		public bool logTaskChanges;

		public int group;

		public ExternalBehavior externalBehavior;

		[SerializeField]
		private BehaviorSource mBehaviorSource;

		[SerializeField]
		private Task entryTask;

		[SerializeField]
		private Task rootTask;

		[SerializeField]
		private List<Task> detachedTasks;

		[SerializeField]
		private List<SharedVariable> variables;

		[SerializeField]
		private string serialization;

		[SerializeField]
		private string behaviorName;

		[SerializeField]
		private string behaviorDescription;

		private bool isPaused;

		[SerializeField]
		private List<UnityEngine.Object> mUnityObjects;

		private Dictionary<string, List<TaskCoroutine>> activeTaskCoroutines;

		public bool showBehaviorDesignerGizmo = true;

		public List<UnityEngine.Object> UnityObjects
		{
			get
			{
				return this.mUnityObjects;
			}
			set
			{
				this.mUnityObjects = value;
			}
		}

		public BehaviorSource GetBehaviorSource()
		{
            //Debug.Log("mBehaviorSource:" + mBehaviorSource);
			return this.mBehaviorSource;
		}

		public void SetBehaviorSource(BehaviorSource behaviorSource)
		{
			this.mBehaviorSource = behaviorSource;
		}

		public UnityEngine.Object GetObject()
		{
			return this;
		}

		public string GetOwnerName()
		{
			return base.gameObject.name;
		}

		public void ClearUnityObjects()
		{
			if (this.mUnityObjects != null)
			{
				this.mUnityObjects.Clear();
			}
		}

		public int SerializeUnityObject(UnityEngine.Object unityObject)
		{
			if (this.mUnityObjects == null)
			{
				this.mUnityObjects = new List<UnityEngine.Object>();
			}
			this.mUnityObjects.Add(unityObject);
			return this.mUnityObjects.Count - 1;
		}

		public UnityEngine.Object DeserializeUnityObject(int id)
		{
			if (id < 0 || id >= this.mUnityObjects.Count)
			{
				return null;
			}
			return this.mUnityObjects[id];
		}

		public void OnDrawGizmosSelected()
		{
			if (this.showBehaviorDesignerGizmo)
			{
				Gizmos.DrawIcon(base.transform.position, "Behavior Designer Scene Icon.png");
			}
		}

		public Behavior()
		{
			this.mBehaviorSource = new BehaviorSource(this);
		}

		public void Awake()
		{
			if (this.UpdateDeprecatedTasks())
			{
				Debug.LogWarning(string.Format("{0}: the data format for this behavior tree has been deprecated. Run the Behavior Designer Update tool or select this game object within the inspector to update this behavior tree.", this.ToString()));
			}
		}

		public bool HasDeprecatedTasks()
		{
			return this.entryTask != null || (this.serialization != null && !this.serialization.Equals(""));
		}

		public bool UpdateDeprecatedTasks()
		{
			if (this.mBehaviorSource == null)
			{
				this.mBehaviorSource = new BehaviorSource(this);
			}
			bool result = false;
			if (this.entryTask != null)
			{
				this.mBehaviorSource.EntryTask = this.entryTask;
				this.entryTask = null;
				result = true;
			}
			if (this.rootTask != null)
			{
				this.mBehaviorSource.RootTask = this.rootTask;
				this.rootTask = null;
				result = true;
			}
			if (this.detachedTasks != null && this.detachedTasks.Count > 0)
			{
				this.mBehaviorSource.DetachedTasks = this.detachedTasks;
				this.detachedTasks = null;
				result = true;
			}
			if (this.variables != null && this.variables.Count > 0)
			{
				this.mBehaviorSource.Variables = this.variables;
				this.variables = null;
				result = true;
			}
			if (this.serialization != null && !this.serialization.Equals(""))
			{
				this.mBehaviorSource.Serialization = this.serialization;
				this.serialization = null;
				result = true;
			}
			if (this.behaviorName != null && !this.behaviorName.Equals(""))
			{
				this.mBehaviorSource.behaviorName = this.behaviorName;
				this.behaviorName = "";
			}
			if (this.behaviorDescription != null && !this.behaviorDescription.Equals(""))
			{
				this.mBehaviorSource.behaviorDescription = this.behaviorDescription;
				this.behaviorDescription = "";
			}
			this.mBehaviorSource.Owner = this;
			return result;
		}

		public void Start()
		{
			if (this.startWhenEnabled)
			{
				this.EnableBehavior();
			}
		}

		[Obsolete("Behavior.enableBehavior has been deprectead. Use Behavior.EnableBehavior.")]
		public void enableBehavior()
		{
			this.EnableBehavior();
		}

        /// <summary>
        ///  运行行为树
        /// </summary>
		public void EnableBehavior()
		{
			this.mBehaviorSource.CheckForJSONSerialization(false);
			if (this.mBehaviorSource.RootTask != null || this.externalBehavior != null)
			{
				Behavior.CreateBehaviorManager();
				BehaviorManager.instance.enableBehavior(this);
			}
		}

		[Obsolete("Behavior.disableBehavior has been deprectead. Use Behavior.DisableBehavior.")]
		public void disableBehavior()
		{
			this.DisableBehavior();
		}

		public void DisableBehavior()
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.disableBehavior(this, this.pauseWhenDisabled);
				this.isPaused = this.pauseWhenDisabled;
			}
		}

		public void DisableBehavior(bool pause)
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.disableBehavior(this, pause);
				this.isPaused = pause;
			}
		}

		public void OnEnable()
		{
			if (BehaviorManager.instance != null && this.isPaused)
			{
				BehaviorManager.instance.enableBehavior(this);
				this.isPaused = false;
			}
		}

		public void OnDisable()
		{
			this.DisableBehavior();
		}

		public SharedVariable GetVariable(string name)
		{
			return this.mBehaviorSource.GetVariable(name);
		}

		public void SetVariable(string name, SharedVariable item)
		{
			this.mBehaviorSource.SetVariable(name, item);
		}

		public void OnCollisionEnter(Collision collision)
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnCollisionEnter(collision, this);
			}
		}

		public void OnCollisionExit(Collision collision)
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnCollisionExit(collision, this);
			}
		}

		public void OnCollisionStay(Collision collision)
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnCollisionStay(collision, this);
			}
		}

		public void OnTriggerEnter(Collider other)
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnTriggerEnter(other, this);
			}
		}

		public void OnTriggerExit(Collider other)
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnTriggerExit(other, this);
			}
		}

		public void OnTriggerStay(Collider other)
		{
			if (BehaviorManager.instance != null)
			{
				BehaviorManager.instance.BehaviorOnTriggerStay(other, this);
			}
		}

		public void StartTaskCoroutine(Task task, string methodName)
		{
			if (this.activeTaskCoroutines == null)
			{
				this.activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
			}
			MethodInfo method = task.GetType().GetMethod(methodName);
			TaskCoroutine item = new TaskCoroutine(this, (IEnumerator)method.Invoke(task, new object[0]), methodName);
			if (this.activeTaskCoroutines.ContainsKey(methodName))
			{
				List<TaskCoroutine> list = this.activeTaskCoroutines[methodName];
				list.Add(item);
				this.activeTaskCoroutines[methodName] = list;
				return;
			}
			List<TaskCoroutine> list2 = new List<TaskCoroutine>();
			list2.Add(item);
			this.activeTaskCoroutines.Add(methodName, list2);
		}

		public void StartTaskCoroutine(Task task, string methodName, object value)
		{
			if (this.activeTaskCoroutines == null)
			{
				this.activeTaskCoroutines = new Dictionary<string, List<TaskCoroutine>>();
			}
			MethodInfo method = task.GetType().GetMethod(methodName);
			TaskCoroutine item = new TaskCoroutine(this, (IEnumerator)method.Invoke(task, new object[]
			{
				value
			}), methodName);
			if (this.activeTaskCoroutines.ContainsKey(methodName))
			{
				List<TaskCoroutine> list = this.activeTaskCoroutines[methodName];
				list.Add(item);
				this.activeTaskCoroutines[methodName] = list;
				return;
			}
			List<TaskCoroutine> list2 = new List<TaskCoroutine>();
			list2.Add(item);
			this.activeTaskCoroutines.Add(methodName, list2);
		}

		public void StopTaskCoroutine(string methodName)
		{
			if (!this.activeTaskCoroutines.ContainsKey(methodName))
			{
				return;
			}
			List<TaskCoroutine> list = this.activeTaskCoroutines[methodName];
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Stop();
			}
		}

		public void StopAllTaskCoroutines()
		{
			base.StopAllCoroutines();
			foreach (KeyValuePair<string, List<TaskCoroutine>> current in this.activeTaskCoroutines)
			{
				List<TaskCoroutine> value = current.Value;
				for (int i = 0; i < value.Count; i++)
				{
					value[i].Stop();
				}
			}
		}

		public void TaskCoroutineEnded(TaskCoroutine taskCoroutine, string coroutineName)
		{
			if (this.activeTaskCoroutines.ContainsKey(coroutineName))
			{
				List<TaskCoroutine> list = this.activeTaskCoroutines[coroutineName];
				if (list.Count == 1)
				{
					this.activeTaskCoroutines.Remove(coroutineName);
					return;
				}
				list.Remove(taskCoroutine);
				this.activeTaskCoroutines[coroutineName] = list;
			}
		}

		public override string ToString()
		{
			return this.mBehaviorSource.ToString();
		}

		public static BehaviorManager CreateBehaviorManager()
		{
			if (BehaviorManager.instance == null)
			{
				GameObject gameObject = new GameObject();
				gameObject.name="Behavior Manager";
				return gameObject.AddComponent<BehaviorManager>();
			}
			return null;
		}

		int IBehavior.GetInstanceID()
		{
			return base.GetInstanceID();
		}
	}
}
