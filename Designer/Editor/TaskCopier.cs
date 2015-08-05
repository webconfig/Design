using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
    public class TaskCopier : UnityEditor.Editor
	{
		public static TaskSerializer CopySerialized(Task task)
		{
			TaskSerializer taskSerializer = new TaskSerializer();
			taskSerializer.taskType = task.GetType();
			taskSerializer.fieldInfo = task.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			taskSerializer.fieldValue = new object[taskSerializer.fieldInfo.Length];
			for (int i = 0; i < taskSerializer.fieldInfo.Length; i++)
			{
				bool flag = !taskSerializer.fieldInfo[i].Name.Equals("children") && ((taskSerializer.fieldInfo[i].FieldType.IsArray && !taskSerializer.fieldInfo[i].FieldType.GetElementType().Equals(typeof(Task)) && !taskSerializer.fieldInfo[i].FieldType.GetElementType().IsSubclassOf(typeof(Task))) || (!taskSerializer.fieldInfo[i].FieldType.IsArray && !taskSerializer.fieldInfo[i].FieldType.Equals(typeof(Task)) && !taskSerializer.fieldInfo[i].FieldType.IsSubclassOf(typeof(Task))));
				if (flag)
				{
					taskSerializer.fieldValue[i] = taskSerializer.fieldInfo[i].GetValue(task);
				}
			}
			taskSerializer.position = task.NodeData.Position + new Vector2(10f, 10f);
			taskSerializer.friendlyName = task.NodeData.FriendlyName;
			taskSerializer.comment = task.NodeData.Comment;
			return taskSerializer;
		}

		public static bool CopySerialized(BehaviorSource behaviorSource, Task task, out Task newTask, HideFlags hideFlags)
		{
			if (task == null)
			{
				newTask = null;
				return false;
			}
			newTask = (ScriptableObject.CreateInstance(task.GetType()) as Task);
			bool flag = task.GetType().IsSubclassOf(typeof(ParentTask));
			FieldInfo[] fields = task.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (!flag || !fields[i].Name.Equals("children"))
				{
					if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable)))
					{
						SharedVariable sharedVariable = fields[i].GetValue(task) as SharedVariable;
						if (sharedVariable != null)
						{
                            SharedVariable sharedVariable2 = behaviorSource.GetVariable(sharedVariable.name);
							if (sharedVariable2 == null)
							{
								sharedVariable2 = (ScriptableObject.CreateInstance(sharedVariable.GetType()) as SharedVariable);
								sharedVariable2.SetValue(sharedVariable.GetValue());
								sharedVariable2.IsShared = sharedVariable.IsShared;
							}
							fields[i].SetValue(newTask, sharedVariable2);
						}
					}
					else
					{
						fields[i].SetValue(newTask, fields[i].GetValue(task));
					}
				}
			}
			newTask.NodeData = new NodeData();
			newTask.NodeData.copyFrom(task.NodeData, newTask);
			newTask.NodeData.NodeDesigner = null;
			newTask.ID = task.ID;
			newTask.IsInstant = task.IsInstant;
			newTask.Owner = (behaviorSource.Owner as Behavior);
			newTask.hideFlags=hideFlags;
			EditorUtility.SetDirty(newTask);
			if (flag)
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null)
				{
					ParentTask parentTask2 = newTask as ParentTask;
					for (int j = 0; j < parentTask.Children.Count; j++)
					{
						Task child;
						TaskCopier.CopySerialized(behaviorSource, parentTask.Children[j], out child, hideFlags);
						parentTask2.AddChild(child, j);
					}
				}
			}
			return true;
		}

		public static void CheckTasks(IBehavior behavior)
		{
			bool flag = false;
			BehaviorSource behaviorSource = behavior.GetBehaviorSource();
			if (behaviorSource.BehaviorID != behavior.GetInstanceID() || behaviorSource.IsDirty)
			{
				Behavior behavior2 = EditorUtility.InstanceIDToObject(behaviorSource.BehaviorID) as Behavior;
				if (behavior2 != null || behaviorSource.BehaviorID == -1)
				{
					BehaviorSource behaviorSource2 = (behaviorSource.BehaviorID == -1) ? behaviorSource : behavior2.GetBehaviorSource();
					behaviorSource = new BehaviorSource(behavior);
                    HideFlags hideFlags = (!AssetDatabase.GetAssetPath(behavior.GetObject()).Equals("")) ? HideFlags.DontSave:HideFlags.None;//  4 : 0;
                    behaviorSource2.CheckForJSONSerialization(PrefabUtility.GetPrefabType(behavior.GetObject()) == PrefabType.Prefab || PrefabUtility.GetPrefabType(behavior.GetObject()) == PrefabType.PrefabInstance);
					if (behaviorSource2.Variables != null)
					{
						List<SharedVariable> list = new List<SharedVariable>();
						for (int i = 0; i < behaviorSource2.Variables.Count; i++)
						{
							SharedVariable item;
							VariableCopier.CopySerialized(behaviorSource2.Variables[i], out item, hideFlags);
							list.Add(item);
						}
						behaviorSource.Variables = list;
					}
					Task task;
                    //if (TaskCopier.CopySerialized(behaviorSource, behaviorSource2.EntryTask, out task, hideFlags))
                    //{
                    //    behaviorSource.EntryTask = task;
                    //}
					if (TaskCopier.CopySerialized(behaviorSource, behaviorSource2.RootTask, out task, hideFlags))
					{
						behaviorSource.RootTask = task;
					}
					if (behaviorSource2.DetachedTasks != null)
					{
						List<Task> list2 = new List<Task>();
						for (int j = 0; j < behaviorSource2.DetachedTasks.Count; j++)
						{
							TaskCopier.CopySerialized(behaviorSource, behaviorSource2.DetachedTasks[j], out task, hideFlags);
							list2.Add(task);
						}
						behaviorSource.DetachedTasks = list2;
					}
					behaviorSource.behaviorName = behaviorSource2.behaviorName;
					behaviorSource.behaviorDescription = behaviorSource2.behaviorDescription;
					TaskReferences.CheckReferences(behaviorSource);
					behaviorSource.Serialization = SerializeJSON.Serialize(behaviorSource2, behaviorSource);
					if (!AssetDatabase.GetAssetPath(behavior.GetObject()).Equals(""))
					{
						behaviorSource.Variables = null;
                        //behaviorSource.EntryTask = null;
						behaviorSource.RootTask = null;
						behaviorSource.DetachedTasks = null;
					}
					behavior.SetBehaviorSource(behaviorSource);
				}
				flag = true;
				behaviorSource.BehaviorID = behavior.GetInstanceID();
				behaviorSource.IsDirty = false;
			}
			if (flag)
			{
				EditorUtility.SetDirty(behavior.GetObject());
				if (BehaviorDesignerWindow.instance != null && behaviorSource.BehaviorID == BehaviorDesignerWindow.instance.ActiveBehaviorID)
				{
					BehaviorDesignerWindow.instance.LoadBehavior(behaviorSource, true, false, true);
				}
			}
		}
	}
}
