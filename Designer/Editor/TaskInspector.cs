using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class TaskInspector : ScriptableObject
	{
		private BehaviorDesignerWindow behaviorDesignerWindow;

		private Task activeReferenceTask;

		private FieldInfo activeReferenceTaskFieldInfo;

		private Vector2 mScrollPosition = Vector2.zero;

		public Task ActiveReferenceTask
		{
			get
			{
				return this.activeReferenceTask;
			}
		}

		public FieldInfo ActiveReferenceTaskFieldInfo
		{
			get
			{
				return this.activeReferenceTaskFieldInfo;
			}
		}

		public void OnEnable()
		{
            base.hideFlags = HideFlags.HideAndDontSave;
		}

		public void clearFocus()
		{
			GUIUtility.keyboardControl=0;
		}

		public bool hasFocus()
		{
			return GUIUtility.keyboardControl != 0;
		}

		public bool drawTaskFields(BehaviorSource behaviorSource, Task task)
		{
			if (task == null || (task.NodeData.NodeDesigner as NodeDesigner).IsEntryDisplay)
			{
				return false;
			}
			this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);
			if (this.behaviorDesignerWindow == null)
			{
				this.behaviorDesignerWindow = BehaviorDesignerWindow.instance;
			}
			bool result = false;
			EditorGUIUtility.labelWidth=(float)BehaviorDesignerUtility.TaskPropertiesLabelWidth;
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Name", new GUILayoutOption[]
			{
				GUILayout.Width(90f)
			});
			task.NodeData.FriendlyName = EditorGUILayout.TextField(task.NodeData.FriendlyName, new GUILayoutOption[0]);
			if (GUILayout.Button(BehaviorDesignerUtility.DocTexture, BehaviorDesignerUtility.TransparentButtonGUIStyle, new GUILayoutOption[0]))
			{
				this.openHelpURL(task);
			}
			if (GUILayout.Button(BehaviorDesignerUtility.GearTexture, BehaviorDesignerUtility.TransparentButtonGUIStyle, new GUILayoutOption[0]))
			{
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(new GUIContent("Edit Script"), false, new GenericMenu.MenuFunction2(this.openInFileEditor), task);
				genericMenu.AddItem(new GUIContent("Reset"), false, new GenericMenu.MenuFunction2(this.resetTask), task);
				genericMenu.ShowAsContext();
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Instant", new GUILayoutOption[]
			{
				GUILayout.Width(90f)
			});
			task.IsInstant = EditorGUILayout.Toggle(task.IsInstant, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Comment", new GUILayoutOption[0]);
			task.NodeData.Comment = EditorGUILayout.TextArea(task.NodeData.Comment, BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, new GUILayoutOption[]
			{
				GUILayout.Height(48f)
			});
			if (EditorGUI.EndChangeCheck())
			{
				result = true;
			}
			BehaviorDesignerUtility.DrawContentSeperator(2);
			GUILayout.Space(6f);
			HideFlags hideFlags = task.hideFlags;
			task.hideFlags=0;
			SerializedObject serializedObject = new SerializedObject(task);
			serializedObject.Update();
			FieldInfo[] fields = task.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (fields[i].GetCustomAttributes(typeof(NonSerializedAttribute), false).Length <= 0 && ((!fields[i].IsPrivate && !fields[i].IsFamily) || fields[i].GetCustomAttributes(typeof(SerializeField), false).Length != 0) && (!task.GetType().IsSubclassOf(typeof(ParentTask)) || !fields[i].Name.Equals("children")))
				{
					SerializedProperty serializedProperty = serializedObject.FindProperty(fields[i].Name);
					if (serializedProperty != null)
					{
						BehaviorDesigner.Runtime.Tasks.TooltipAttribute[] array;
						GUIContent gUIContent;
                        if ((array = (fields[i].GetCustomAttributes(typeof(BehaviorDesigner.Runtime.Tasks.TooltipAttribute), false) as BehaviorDesigner.Runtime.Tasks.TooltipAttribute[])).Length > 0)
						{
							gUIContent = new GUIContent(BehaviorDesignerUtility.SplitCamelCase(fields[i].Name), array[0].Tooltip);
						}
						else
						{
							gUIContent = new GUIContent(BehaviorDesignerUtility.SplitCamelCase(fields[i].Name));
						}
						EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
						bool flag = (fields[i].FieldType.IsArray && (fields[i].FieldType.GetElementType().Equals(typeof(Task)) || fields[i].FieldType.GetElementType().IsSubclassOf(typeof(Task)))) || fields[i].FieldType.Equals(typeof(Task)) || fields[i].FieldType.IsSubclassOf(typeof(Task));
						if (!flag && !fields[i].FieldType.IsArray)
						{
							GUILayout.Space(3f);
							bool flag2 = task.NodeData.containsWatchedField(fields[i]);
							if (GUILayout.Button(flag2 ? BehaviorDesignerUtility.VariableWatchButtonSelectedTexture : BehaviorDesignerUtility.VariableWatchButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
							{
								GUILayout.Width(15f)
							}))
							{
								if (flag2)
								{
									task.NodeData.removeWatchedField(fields[i]);
								}
								else
								{
									task.NodeData.addWatchedField(fields[i]);
								}
								result = true;
							}
						}
						if (flag)
						{
							GUILayout.Label(gUIContent, BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[]
							{
								GUILayout.Width(203f)
							});
							bool flag3 = this.behaviorDesignerWindow.isReferencingField(fields[i]);
							Color backgroundColor = GUI.backgroundColor;
							if (flag3)
							{
								GUI.backgroundColor=new Color(0.5f, 1f, 0.5f);
							}
							if (GUILayout.Button(flag3 ? "Done" : "Select", EditorStyles.miniButtonMid, new GUILayoutOption[]
							{
								GUILayout.Width(100f)
							}))
							{
								if (this.behaviorDesignerWindow.isReferencingTasks() && !flag3)
								{
									this.behaviorDesignerWindow.toggleReferenceTasks();
								}
								this.behaviorDesignerWindow.toggleReferenceTasks(task, fields[i]);
							}
							GUI.backgroundColor=backgroundColor;
							EditorGUILayout.EndHorizontal();
							if (fields[i].FieldType.IsArray)
							{
								Task[] array2 = fields[i].GetValue(task) as Task[];
								if (array2 == null || array2.Length == 0)
								{
									GUILayout.Label("No Tasks Referenced", BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[0]);
								}
								else
								{
									for (int j = 0; j < array2.Length; j++)
									{
										EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
										GUILayout.Label(array2[j].NodeData.NodeDesigner.ToString(), BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[]
										{
											GUILayout.Width(272f)
										});
										if (GUILayout.Button(BehaviorDesignerUtility.DeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
										{
											GUILayout.Width(14f)
										}))
										{
											this.referenceTasks(task, (array2[j].NodeData.NodeDesigner as NodeDesigner).Task, fields[i]);
											result = true;
										}
										GUILayout.Space(3f);
										if (GUILayout.Button(BehaviorDesignerUtility.IdentifyButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
										{
											GUILayout.Width(14f)
										}))
										{
											this.behaviorDesignerWindow.identifyNode(array2[j].NodeData.NodeDesigner as NodeDesigner);
										}
										EditorGUILayout.EndHorizontal();
									}
								}
							}
							else
							{
								EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
								Task task2 = fields[i].GetValue(task) as Task;
								GUILayout.Label((task2 != null) ? task2.NodeData.NodeDesigner.ToString() : "No Tasks Referenced", BehaviorDesignerUtility.TaskInspectorGUIStyle, new GUILayoutOption[]
								{
									GUILayout.Width(272f)
								});
								if (task2 != null)
								{
									if (GUILayout.Button(BehaviorDesignerUtility.DeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
									{
										GUILayout.Width(14f)
									}))
									{
										this.referenceTasks(task, (task2.NodeData.NodeDesigner as NodeDesigner).Task, fields[i]);
										result = true;
									}
									GUILayout.Space(3f);
									if (GUILayout.Button(BehaviorDesignerUtility.IdentifyButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
									{
										GUILayout.Width(14f)
									}))
									{
										this.behaviorDesignerWindow.identifyNode(task2.NodeData.NodeDesigner as NodeDesigner);
									}
								}
								EditorGUILayout.EndHorizontal();
							}
						}
						else if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable)))
						{
							EditorGUI.BeginChangeCheck();
							SharedVariable sharedVariable = fields[i].GetValue(task) as SharedVariable;
							GUILayout.BeginVertical(new GUILayoutOption[0]);
							GUILayout.Space(2f);
							GUILayout.Label(gUIContent, new GUILayoutOption[]
							{
								GUILayout.Width(146f)
							});
							GUILayout.EndVertical();
							bool flag4 = false;
							switch (sharedVariable.ValueType)
							{
							case SharedVariableTypes.Vector2:
							case SharedVariableTypes.Vector3:
							case SharedVariableTypes.Vector4:
							case SharedVariableTypes.Quaternion:
							case SharedVariableTypes.Rect:
								flag4 = true;
								break;
							}
							if (sharedVariable.IsShared)
							{
								string[] array3 = null;
								int num = this.getVariablesOfType(sharedVariable, behaviorSource, out array3);
								Color backgroundColor2 = GUI.backgroundColor;
								if (num == 0)
								{
									GUI.backgroundColor=Color.red;
								}
								int num2 = num;
								num = EditorGUILayout.Popup(num, array3, EditorStyles.toolbarPopup, new GUILayoutOption[0]);
								GUI.backgroundColor=backgroundColor2;
								if (num != num2)
								{
									if (num == 0)
									{
										SharedVariable sharedVariable2 = ScriptableObject.CreateInstance(fields[i].FieldType) as SharedVariable;
										sharedVariable2.IsShared = true;
										fields[i].SetValue(task, sharedVariable2);
									}
									else
									{
										fields[i].SetValue(task, behaviorSource.GetVariable(array3[num]));
									}
								}
								GUILayout.Space(8f);
							}
							if (!sharedVariable.IsShared && !flag4)
							{
								VariableInspector.DrawSharedVariableValue(sharedVariable, 121);
								fields[i].SetValue(task, sharedVariable);
							}
							if (GUILayout.Button(sharedVariable.IsShared ? BehaviorDesignerUtility.VariableButtonSelectedTexture : BehaviorDesignerUtility.VariableButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
							{
								GUILayout.Width(15f)
							}))
							{
								bool isShared = !sharedVariable.IsShared;
								sharedVariable = (ScriptableObject.CreateInstance(fields[i].FieldType) as SharedVariable);
								sharedVariable.IsShared = isShared;
								fields[i].SetValue(task, sharedVariable);
							}
							GUILayout.Space(4f);
							GUILayout.EndHorizontal();
							if (!sharedVariable.IsShared && flag4)
							{
								VariableInspector.DrawSharedVariableValue(sharedVariable, 100);
								fields[i].SetValue(task, sharedVariable);
							}
							if (EditorGUI.EndChangeCheck())
							{
								BehaviorUndo.RegisterUndo("Inspector", task, true, true);
								result = true;
							}
							GUILayout.Space(4f);
						}
						else
						{
							EditorGUI.BeginChangeCheck();
							EditorGUILayout.PropertyField(serializedProperty, gUIContent, true, new GUILayoutOption[0]);
							if (EditorGUI.EndChangeCheck())
							{
								serializedObject.ApplyModifiedProperties();
								result = true;
							}
							EditorGUILayout.EndHorizontal();
						}
					}
				}
			}
			task.hideFlags=hideFlags;
			GUILayout.EndScrollView();
			return result;
		}

		public void setActiveReferencedTasks(Task referenceTask, FieldInfo fieldInfo)
		{
			this.activeReferenceTask = referenceTask;
			this.activeReferenceTaskFieldInfo = fieldInfo;
		}

		public bool referenceTasks(Task referenceTask)
		{
			return this.referenceTasks(this.activeReferenceTask, referenceTask, this.activeReferenceTaskFieldInfo);
		}

		private bool referenceTasks(Task sourceTask, Task referenceTask, FieldInfo sourceFieldInfo)
		{
			bool flag = false;
			bool showReferenceIcon = false;
			if (TaskInspector.referenceTasks(sourceTask, referenceTask, sourceFieldInfo, ref flag, ref showReferenceIcon, true, true, false))
			{
				(referenceTask.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = showReferenceIcon;
				if (flag)
				{
					this.performFullSync(this.activeReferenceTask);
				}
				return true;
			}
			return false;
		}

		public static bool referenceTasks(Task sourceTask, Task referenceTask, FieldInfo sourceFieldInfo, ref bool fullSync, ref bool doReference, bool synchronize, bool registerUndo, bool unreferenceAll)
		{
			if (referenceTask == null || referenceTask.Equals(sourceTask) || (!sourceFieldInfo.FieldType.IsArray && !sourceFieldInfo.FieldType.Equals(referenceTask.GetType()) && !referenceTask.GetType().IsSubclassOf(sourceFieldInfo.FieldType)) || (sourceFieldInfo.FieldType.IsArray && !sourceFieldInfo.FieldType.GetElementType().Equals(referenceTask.GetType()) && !referenceTask.GetType().IsSubclassOf(sourceFieldInfo.FieldType.GetElementType())))
			{
				return false;
			}
			if (synchronize && !TaskInspector.isFieldLinked(sourceFieldInfo))
			{
				synchronize = false;
			}
			if (unreferenceAll)
			{
				sourceFieldInfo.SetValue(sourceTask, null);
				(sourceTask.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = false;
			}
			else
			{
				doReference = true;
				bool flag = false;
				if (sourceFieldInfo.FieldType.IsArray)
				{
					Task[] array = sourceFieldInfo.GetValue(sourceTask) as Task[];
					IList list = Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
					{
						sourceFieldInfo.FieldType.GetElementType()
					})) as IList;
					if (array != null)
					{
						for (int i = 0; i < array.Length; i++)
						{
							if (referenceTask.Equals(array[i]))
							{
								doReference = false;
							}
							else
							{
								list.Add(array[i]);
							}
						}
					}
					if (synchronize)
					{
						if (array != null && array.Length > 0)
						{
							for (int j = 0; j < array.Length; j++)
							{
								TaskInspector.referenceTasks(array[j], referenceTask, array[j].GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, registerUndo, false);
								if (doReference)
								{
									TaskInspector.referenceTasks(referenceTask, array[j], referenceTask.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, registerUndo, false);
								}
							}
						}
						else if (doReference)
						{
							array = (referenceTask.GetType().GetField(sourceFieldInfo.Name).GetValue(referenceTask) as Task[]);
							if (array != null)
							{
								for (int k = 0; k < array.Length; k++)
								{
									list.Add(array[k]);
									(array[k].NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = true;
									TaskInspector.referenceTasks(array[k], sourceTask, array[k].GetType().GetField(sourceFieldInfo.Name), ref doReference, ref flag, false, registerUndo, false);
								}
								doReference = true;
							}
						}
						TaskInspector.referenceTasks(referenceTask, sourceTask, referenceTask.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, registerUndo, !doReference);
					}
					if (doReference)
					{
						list.Add(referenceTask);
					}
					Array array2 = Array.CreateInstance(sourceFieldInfo.FieldType.GetElementType(), list.Count);
					list.CopyTo(array2, 0);
					if (registerUndo)
					{
						BehaviorUndo.RegisterUndo(doReference ? "Select" : "Deselect", sourceTask, true, true);
					}
					sourceFieldInfo.SetValue(sourceTask, array2);
				}
				else
				{
					Task task = sourceFieldInfo.GetValue(sourceTask) as Task;
					doReference = !referenceTask.Equals(task);
					if (registerUndo)
					{
						BehaviorUndo.RegisterUndo(doReference ? "Select" : "Deselect", sourceTask, true, true);
					}
					if (TaskInspector.isFieldLinked(sourceFieldInfo) && task != null)
					{
						TaskInspector.referenceTasks(task, sourceTask, task.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, registerUndo, true);
					}
					if (synchronize)
					{
						TaskInspector.referenceTasks(referenceTask, sourceTask, referenceTask.GetType().GetField(sourceFieldInfo.Name), ref flag, ref doReference, false, registerUndo, !doReference);
					}
					sourceFieldInfo.SetValue(sourceTask, doReference ? referenceTask : null);
				}
				if (synchronize)
				{
					(referenceTask.NodeData.NodeDesigner as NodeDesigner).ShowReferenceIcon = doReference;
				}
				fullSync = (doReference && synchronize);
			}
			return true;
		}

		public bool isActiveTaskArray()
		{
			return this.activeReferenceTaskFieldInfo.FieldType.IsArray;
		}

		public bool isActiveTaskNull()
		{
			return this.activeReferenceTaskFieldInfo.GetValue(this.activeReferenceTask) == null;
		}

		public static bool isFieldLinked(FieldInfo field)
		{
			return field.GetCustomAttributes(typeof(LinkedTaskAttribute), false).Length > 0;
		}

		public static List<Task> GetReferencedTasks(Task task)
		{
			List<Task> list = new List<Task>();
			FieldInfo[] fields = task.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (!fields[i].IsPrivate || fields[i].GetCustomAttributes(typeof(SerializeField), false).Length != 0)
				{
					if (fields[i].FieldType.IsArray && fields[i].FieldType.GetElementType().IsSubclassOf(typeof(Task)))
					{
						Task[] array = fields[i].GetValue(task) as Task[];
						if (array != null)
						{
							for (int j = 0; j < array.Length; j++)
							{
								list.Add(array[j]);
							}
						}
					}
					else if (fields[i].FieldType.IsSubclassOf(typeof(Task)) && fields[i].GetValue(task) != null)
					{
						list.Add(fields[i].GetValue(task) as Task);
					}
				}
			}
			if (list.Count <= 0)
			{
				return null;
			}
			return list;
		}

		private void performFullSync(Task task)
		{
			List<Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
			if (referencedTasks != null)
			{
				List<bool> list = new List<bool>();
				for (int i = 0; i < referencedTasks.Count; i++)
				{
					list.Add(true);
				}
				bool registerUndo = true;
				FieldInfo[] fields = task.GetType().GetFields();
				for (int j = 0; j < fields.Length; j++)
				{
					if (!TaskInspector.isFieldLinked(fields[j]))
					{
						for (int k = 0; k < referencedTasks.Count; k++)
						{
							FieldInfo field;
							if ((field = referencedTasks[k].GetType().GetField(fields[j].Name)) != null)
							{
								if (list[k])
								{
									BehaviorUndo.RegisterUndo("Inspector", referencedTasks[k], registerUndo, true);
									list[k] = false;
									registerUndo = false;
								}
								field.SetValue(referencedTasks[k], fields[j].GetValue(task));
							}
						}
					}
				}
			}
		}

		private void openInFileEditor(object task)
		{
			MonoScript monoScript = MonoScript.FromScriptableObject(task as Task);
			AssetDatabase.OpenAsset(monoScript);
		}

		private void resetTask(object task)
		{
			(task as Task).OnReset();
		}

		private void openHelpURL(Task task)
		{
			HelpURLAttribute[] array;
			if ((array = (task.GetType().GetCustomAttributes(typeof(HelpURLAttribute), false) as HelpURLAttribute[])).Length > 0)
			{
				Application.OpenURL(array[0].URL);
			}
		}

		private int getVariablesOfType(SharedVariable thisVariable, BehaviorSource behaviorSource, out string[] names)
		{
			List<SharedVariable> variables = behaviorSource.Variables;
			int result = 0;
			int num = 1;
			List<string> list = new List<string>();
			list.Add("None");
			if (variables != null)
			{
				for (int i = 0; i < variables.Count; i++)
				{
					if (variables[i].GetType().Equals(thisVariable.GetType()))
					{
						list.Add(variables[i].name);
						if (variables[i].name.Equals(thisVariable.name))
						{
							result = num;
						}
						num++;
					}
				}
			}
			names = list.ToArray();
			return result;
		}
	}
}
