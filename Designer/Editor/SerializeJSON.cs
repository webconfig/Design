using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class SerializeJSON : UnityEngine.Object
	{
        /// <summary>
        /// 序列化行为树
        /// </summary>
        /// <param name="origBehaviorSource"></param>
        /// <param name="newBehaviorSource"></param>
        /// <returns></returns>
		public static string Serialize(BehaviorSource origBehaviorSource, BehaviorSource newBehaviorSource = null)
		{
			origBehaviorSource.CheckForJSONSerialization(false);
			if (newBehaviorSource == null)
			{
				newBehaviorSource = origBehaviorSource;
			}
			newBehaviorSource.Owner.ClearUnityObjects();
			int num = 0;
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			if (origBehaviorSource.EntryTask != null)
            {//序列化根节点
				dictionary.Add("EntryTask", SerializeJSON.SerializeTask(origBehaviorSource.EntryTask, newBehaviorSource.Owner, ref num));
			}
			if (origBehaviorSource.RootTask != null)
            {//序列化根节点
				dictionary.Add("RootTask", SerializeJSON.SerializeTask(origBehaviorSource.RootTask, newBehaviorSource.Owner, ref num));
			}
			if (origBehaviorSource.DetachedTasks != null && origBehaviorSource.DetachedTasks.Count > 0)
            {//序列化未连接的节点
				Dictionary<string, object>[] array = new Dictionary<string, object>[origBehaviorSource.DetachedTasks.Count];
				for (int i = 0; i < origBehaviorSource.DetachedTasks.Count; i++)
				{
					array[i] = SerializeJSON.SerializeTask(origBehaviorSource.DetachedTasks[i], newBehaviorSource.Owner, ref num);
				}
				dictionary.Add("DetachedTasks", array);
			}
			if (origBehaviorSource.Variables != null && origBehaviorSource.Variables.Count > 0)
            {//序列化节点里面的变量
				Dictionary<string, object>[] array2 = new Dictionary<string, object>[origBehaviorSource.Variables.Count];
				for (int j = 0; j < origBehaviorSource.Variables.Count; j++)
				{
					array2[j] = SerializeJSON.SerializeVariable(origBehaviorSource.Variables[j], newBehaviorSource.Owner);
				}
				dictionary.Add("Variables", array2);
			}
			dictionary.Add("TaskCount", num);
			return Json.Serialize(dictionary);
		}

        /// <summary>
        /// 序列化任务
        /// </summary>
        /// <param name="task"></param>
        /// <param name="behavior"></param>
        /// <param name="taskCount"></param>
        /// <returns></returns>
		private static Dictionary<string, object> SerializeTask(Task task, IBehavior behavior, ref int taskCount)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("ObjectType", task.GetType());
			dictionary.Add("NodeData", task.NodeData.serialize());
			dictionary.Add("ID", task.ID);
			dictionary.Add("Instant", task.IsInstant);
			taskCount++;
			SerializeJSON.SerializeFields(task, ref dictionary, behavior);
			if (task.GetType().IsSubclassOf(typeof(ParentTask)))
            {//序列化子节点
				ParentTask parentTask = task as ParentTask;
				if (parentTask.Children != null && parentTask.Children.Count > 0)
				{
					Dictionary<string, object>[] array = new Dictionary<string, object>[parentTask.Children.Count];
					for (int i = 0; i < parentTask.Children.Count; i++)
					{
						array[i] = SerializeJSON.SerializeTask(parentTask.Children[i], behavior, ref taskCount);
					}
					dictionary.Add("Children", array);
				}
			}
			return dictionary;
		}

        /// <summary>
        /// 序列化变量
        /// </summary>
        /// <param name="sharedVariable"></param>
        /// <param name="behavior"></param>
        /// <returns></returns>
		private static Dictionary<string, object> SerializeVariable(SharedVariable sharedVariable, IBehavior behavior)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Type", sharedVariable.GetType());
			dictionary.Add("Name", sharedVariable.name);
			dictionary.Add("IsShared", sharedVariable.IsShared);
			switch (sharedVariable.ValueType)
			{
			case SharedVariableTypes.GameObject:
			case SharedVariableTypes.Transform:
			case SharedVariableTypes.Object:
			{
				UnityEngine.Object _object = sharedVariable.GetValue() as UnityEngine.Object;
				if (!object.ReferenceEquals(_object, null) && _object != null)
				{
                    int num = behavior.SerializeUnityObject(_object);
					dictionary.Add("Value", num);
				}
				break;
			}
			default:
				dictionary.Add("Value", sharedVariable.GetValue());
				break;
			}
			return dictionary;
		}

		private static void SerializeFields(object obj, ref Dictionary<string, object> dict, IBehavior behavior)
		{
			FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (fields[i].GetCustomAttributes(typeof(NonSerializedAttribute), false).Length <= 0 && ((!fields[i].IsPrivate && !fields[i].IsFamily) || fields[i].GetCustomAttributes(typeof(SerializeField), false).Length != 0) && (!obj.GetType().IsSubclassOf(typeof(ParentTask)) || !fields[i].Name.Equals("children")) && fields[i].GetValue(obj) != null)
				{
					if (fields[i].FieldType.IsArray)
					{
						IList list = fields[i].GetValue(obj) as IList;
						if (list != null)
						{
							Type elementType = fields[i].FieldType.GetElementType();
							List<object> list2 = new List<object>();
							for (int j = 0; j < list.Count; j++)
							{
								if (list[j] == null)
								{
									list2.Add(-1);
								}
								if (elementType.Equals(typeof(Task)) || elementType.IsSubclassOf(typeof(Task)))
								{
									Task task = list[j] as Task;
									list2.Add(task.ID);
								}
								else if (elementType.IsSubclassOf(typeof(SharedVariable)))
								{
									list2.Add(SerializeJSON.SerializeVariable(list[j] as SharedVariable, behavior));
								}
								else if (elementType.Equals(typeof(UnityEngine.Object)) || elementType.IsSubclassOf(typeof(UnityEngine.Object)))
								{
									UnityEngine.Object _object = list[j] as UnityEngine.Object;
									if (!object.ReferenceEquals(_object, null) && _object != null)
									{
                                        list2.Add(behavior.SerializeUnityObject(_object));
									}
								}
								else if (elementType.Equals(typeof(LayerMask)))
								{
									list2.Add(((LayerMask)list[j]).value);
								}
								else if (elementType.IsPrimitive || elementType.IsEnum || elementType.Equals(typeof(string)) || elementType.Equals(typeof(Vector2)) || elementType.Equals(typeof(Vector3)) || elementType.Equals(typeof(Vector4)) || elementType.Equals(typeof(Quaternion)) || elementType.Equals(typeof(Matrix4x4)) || elementType.Equals(typeof(Color)) || elementType.Equals(typeof(Rect)))
								{
									list2.Add(list[j]);
								}
								else
								{
									Dictionary<string, object> item = new Dictionary<string, object>();
									SerializeJSON.SerializeFields(list[j], ref item, behavior);
									list2.Add(item);
								}
							}
							if (list2 != null)
							{
								dict.Add(fields[i].Name, list2);
							}
						}
					}
					else if (fields[i].FieldType.Equals(typeof(Task)) || fields[i].FieldType.IsSubclassOf(typeof(Task)))
					{
						Task task2 = fields[i].GetValue(obj) as Task;
						dict.Add(fields[i].Name, task2.ID);
					}
					else if (fields[i].FieldType.IsSubclassOf(typeof(SharedVariable)))
					{
						dict.Add(fields[i].Name, SerializeJSON.SerializeVariable(fields[i].GetValue(obj) as SharedVariable, behavior));
					}
					else if (fields[i].FieldType.Equals(typeof(UnityEngine.Object)) || fields[i].FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
					{
						UnityEngine.Object object2 = fields[i].GetValue(obj) as UnityEngine.Object;
						if (!object.ReferenceEquals(object2, null) && object2 != null)
						{
							int num = behavior.SerializeUnityObject(object2);
							dict.Add(fields[i].Name, num);
						}
					}
					else if (fields[i].FieldType.Equals(typeof(LayerMask)))
					{
						dict.Add(fields[i].Name, ((LayerMask)fields[i].GetValue(obj)).value);
					}
					else if (fields[i].FieldType.IsPrimitive || fields[i].FieldType.IsEnum || fields[i].FieldType.Equals(typeof(string)) || fields[i].FieldType.Equals(typeof(Vector2)) || fields[i].FieldType.Equals(typeof(Vector3)) || fields[i].FieldType.Equals(typeof(Vector4)) || fields[i].FieldType.Equals(typeof(Quaternion)) || fields[i].FieldType.Equals(typeof(Matrix4x4)) || fields[i].FieldType.Equals(typeof(Color)) || fields[i].FieldType.Equals(typeof(Rect)))
					{
						dict.Add(fields[i].Name, fields[i].GetValue(obj));
					}
					else
					{
						Dictionary<string, object> value = new Dictionary<string, object>();
						SerializeJSON.SerializeFields(fields[i].GetValue(obj), ref value, behavior);
						dict.Add(fields[i].Name, value);
					}
				}
			}
		}
	}
}
