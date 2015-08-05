using BehaviorDesigner.Runtime.Tasks;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	public class DeserializeJSON : UnityEngine.Object
	{
		private struct TaskField
		{
			public Task task;

			public FieldInfo fieldInfo;

			public TaskField(Task t, FieldInfo f)
			{
				this.task = t;
				this.fieldInfo = f;
			}
		}

		public static bool Deserialize(BehaviorSource behaviorSource)
		{
			bool result = false;
			Dictionary<DeserializeJSON.TaskField, List<int>> dictionary = new Dictionary<DeserializeJSON.TaskField, List<int>>();
			Dictionary<int, Task> dictionary2 = new Dictionary<int, Task>();
            //Debug.Log("Serialization:" + behaviorSource.Serialization);
			Dictionary<string, object> dictionary3 = Json.Deserialize(behaviorSource.Serialization) as Dictionary<string, object>;
			if (dictionary3.ContainsKey("Variables"))
			{
				List<SharedVariable> list = new List<SharedVariable>();
				IList list2 = dictionary3["Variables"] as IList;
				for (int i = 0; i < list2.Count; i++)
				{
					SharedVariable item = DeserializeJSON.DeserializeSharedVariable(list2[i] as Dictionary<string, object>, behaviorSource);
					list.Add(item);
				}
				behaviorSource.Variables = list;
				result = true;
			}
            //if (dictionary3.ContainsKey("EntryTask"))
            //{
            //    behaviorSource.EntryTask = DeserializeJSON.DeserializeTask(behaviorSource, dictionary3["EntryTask"] as Dictionary<string, object>, ref dictionary2, ref dictionary);
            //    result = true;
            //}
			if (dictionary3.ContainsKey("RootTask"))
			{
				behaviorSource.RootTask = DeserializeJSON.DeserializeTask(behaviorSource, dictionary3["RootTask"] as Dictionary<string, object>, ref dictionary2, ref dictionary);
				result = true;
			}
			if (dictionary3.ContainsKey("DetachedTasks"))
			{
				List<Task> list3 = new List<Task>();
				foreach (Dictionary<string, object> dict in (dictionary3["DetachedTasks"] as IEnumerable))
				{
					list3.Add(DeserializeJSON.DeserializeTask(behaviorSource, dict, ref dictionary2, ref dictionary));
				}
				behaviorSource.DetachedTasks = list3;
				result = true;
			}
			if (dictionary.Count > 0)
			{
				foreach (DeserializeJSON.TaskField current in dictionary.Keys)
				{
					List<int> list4 = dictionary[current];
					if (current.fieldInfo.FieldType.IsArray)
					{
						IList list5 = Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
						{
							current.fieldInfo.FieldType.GetElementType()
						})) as IList;
						for (int j = 0; j < list4.Count; j++)
						{
							list5.Add(dictionary2[list4[j]]);
						}
						Array array = Array.CreateInstance(current.fieldInfo.FieldType.GetElementType(), list5.Count);
						list5.CopyTo(array, 0);
						current.fieldInfo.SetValue(current.task, array);
					}
					else
					{
						current.fieldInfo.SetValue(current.task, dictionary2[list4[0]]);
					}
				}
			}
			return result;
		}

		private static Task DeserializeTask(BehaviorSource behaviorSource, Dictionary<string, object> dict, ref Dictionary<int, Task> IDtoTask, ref Dictionary<DeserializeJSON.TaskField, List<int>> taskIDs)
		{
			Task task = null;
			try
			{
				Type type = Type.GetType(dict["ObjectType"] as string);
				if (type == null)
				{
					type = Type.GetType(string.Format("{0}, Assembly-CSharp", dict["ObjectType"] as string));
					if (type == null)
					{
						type = Type.GetType(string.Format("{0}, Assembly-CSharp-firstpass", dict["ObjectType"] as string));
					}
					if (type == null)
					{
						type = Type.GetType(string.Format("{0}, Assembly-UnityScript", dict["ObjectType"] as string));
					}
					if (type == null)
					{
						type = Type.GetType(string.Format("{0}, Assembly-UnityScript-firstpass", dict["ObjectType"] as string));
					}
				}
				if (type == null)
				{
					if (dict.ContainsKey("Children"))
					{
						type = typeof(UnknownParentTask);
					}
					else
					{
						type = typeof(UnknownTask);
					}
				}
				task = (ScriptableObject.CreateInstance(type) as Task);
			}
			catch (Exception)
			{
			}
			if (task == null)
			{
				Debug.Log("Error: task is null of type " + dict["ObjectType"]);
				return null;
			}
            task.hideFlags = HideFlags.HideAndDontSave;// 13;
			task.ID = Convert.ToInt32(dict["ID"]);
			if (dict.ContainsKey("Instant"))
			{
				task.IsInstant = (bool)Convert.ChangeType(dict["Instant"], typeof(bool));
			}
			IDtoTask.Add(task.ID, task);
			task.Owner = (behaviorSource.Owner as Behavior);
			task.NodeData = new NodeData();
			task.NodeData.deserialize(dict["NodeData"] as Dictionary<string, object>, task);
			if (task.GetType().Equals(typeof(UnknownTask)) || task.GetType().Equals(typeof(UnknownParentTask)))
			{
				if (!task.NodeData.FriendlyName.Contains("Unknown "))
				{
					task.NodeData.FriendlyName = string.Format("Unknown {0}", task.NodeData.FriendlyName);
				}
				if (!task.NodeData.Comment.Contains("Loaded from an unknown type. Was a task renamed or deleted?"))
				{
					task.NodeData.Comment = string.Format("Loaded from an unknown type. Was a task renamed or deleted?{0}", task.NodeData.Comment.Equals("") ? "" : string.Format("\0{0}", task.NodeData.Comment));
				}
			}
			DeserializeJSON.DeserializeObject(task, task, dict, behaviorSource, ref taskIDs);
			if (task.GetType().IsSubclassOf(typeof(ParentTask)) && dict.ContainsKey("Children"))
			{
				ParentTask parentTask = task as ParentTask;
				if (parentTask != null)
				{
					foreach (Dictionary<string, object> dict2 in (dict["Children"] as IEnumerable))
					{
						Task child = DeserializeJSON.DeserializeTask(behaviorSource, dict2, ref IDtoTask, ref taskIDs);
						int index = (parentTask.Children == null) ? 0 : parentTask.Children.Count;
						parentTask.AddChild(child, index);
					}
				}
			}
			return task;
		}

		private static SharedVariable DeserializeSharedVariable(Dictionary<string, object> dict, BehaviorSource behaviorSource)
		{
			SharedVariable sharedVariable = null;
			if (behaviorSource != null && behaviorSource.Variables != null && dict["Name"] != null)
			{
				sharedVariable = behaviorSource.GetVariable(dict["Name"] as string);
			}
			if (sharedVariable == null)
			{
				Type type = Type.GetType(dict["Type"] as string);
				if (type == null)
				{
					type = Type.GetType(string.Format("{0}, Assembly-CSharp", dict["Type"] as string));
					if (type == null)
					{
						type = Type.GetType(string.Format("{0}, Assembly-CSharp-firstpass", dict["ObjectType"] as string));
					}
					if (type == null)
					{
						type = Type.GetType(string.Format("{0}, Assembly-UnityScript", dict["ObjectType"] as string));
					}
					if (type == null)
					{
						type = Type.GetType(string.Format("{0}, Assembly-UnityScript-firstpass", dict["ObjectType"] as string));
					}
				}
				sharedVariable = (ScriptableObject.CreateInstance(type) as SharedVariable);
				sharedVariable.name=dict["Name"] as string;
				sharedVariable.IsShared = (!sharedVariable.name.Equals("") || (dict.ContainsKey("IsShared") && (bool)Convert.ChangeType(dict["IsShared"], typeof(bool))));
                sharedVariable.hideFlags = HideFlags.HideAndDontSave;//13;
				if (dict.ContainsKey("Value"))
				{
					switch (sharedVariable.ValueType)
					{
					case SharedVariableTypes.Int:
						sharedVariable.SetValue(Convert.ChangeType(dict["Value"], typeof(int)));
						break;
					case SharedVariableTypes.Float:
						sharedVariable.SetValue(Convert.ChangeType(dict["Value"], typeof(float)));
						break;
					case SharedVariableTypes.Bool:
						sharedVariable.SetValue(Convert.ChangeType(dict["Value"], typeof(bool)));
						break;
					case SharedVariableTypes.String:
						sharedVariable.SetValue(Convert.ChangeType(dict["Value"], typeof(string)));
						break;
					case SharedVariableTypes.Vector2:
						sharedVariable.SetValue(DeserializeJSON.StringToVector2((string)dict["Value"]));
						break;
					case SharedVariableTypes.Vector3:
						sharedVariable.SetValue(DeserializeJSON.StringToVector3((string)dict["Value"]));
						break;
					case SharedVariableTypes.Vector4:
						sharedVariable.SetValue(DeserializeJSON.StringToVector4((string)dict["Value"]));
						break;
					case SharedVariableTypes.Quaternion:
						sharedVariable.SetValue(DeserializeJSON.StringToQuaternion((string)dict["Value"]));
						break;
					case SharedVariableTypes.Color:
						sharedVariable.SetValue(DeserializeJSON.StringToColor((string)dict["Value"]));
						break;
					case SharedVariableTypes.Rect:
						sharedVariable.SetValue(DeserializeJSON.StringToRect((string)dict["Value"]));
						break;
					case SharedVariableTypes.GameObject:
					case SharedVariableTypes.Transform:
					case SharedVariableTypes.Object:
					{
						object obj = dict["Value"];
						if (!obj.GetType().Equals(typeof(string)))
						{
							sharedVariable.SetValue(behaviorSource.Owner.DeserializeUnityObject(Convert.ToInt32(obj)));
						}
						break;
					}
					}
				}
			}
			return sharedVariable;
		}

		private static void DeserializeObject(Task task, object obj, Dictionary<string, object> dict, BehaviorSource behaviorSource, ref Dictionary<DeserializeJSON.TaskField, List<int>> taskIDs)
		{
			if (dict == null)
			{
				return;
			}
			FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				if (dict.ContainsKey(fields[i].Name))
				{
					if (fields[i].FieldType.IsArray)
					{
						IList list = dict[fields[i].Name] as IList;
						if (list != null)
						{
							Type elementType = fields[i].FieldType.GetElementType();
							bool flag = elementType.Equals(typeof(Task)) || elementType.IsSubclassOf(typeof(Task));
							IList list2 = Activator.CreateInstance(typeof(List<>).MakeGenericType(new Type[]
							{
								flag ? typeof(int) : elementType
							})) as IList;
							for (int j = 0; j < list.Count; j++)
							{
								if (elementType.Equals(typeof(Task)) || elementType.IsSubclassOf(typeof(Task)))
								{
									list2.Add(Convert.ToInt32(list[j]));
								}
								else
								{
									list2.Add(DeserializeJSON.ValueToObject(task, elementType, list[j], behaviorSource, ref taskIDs));
								}
							}
							if (flag)
							{
								taskIDs.Add(new DeserializeJSON.TaskField(task, fields[i]), list2 as List<int>);
							}
							else
							{
								Array array = Array.CreateInstance(elementType, list2.Count);
								list2.CopyTo(array, 0);
								fields[i].SetValue(obj, array);
							}
						}
					}
					else
					{
						Type fieldType = fields[i].FieldType;
						if (fieldType.Equals(typeof(Task)) || fieldType.IsSubclassOf(typeof(Task)))
						{
							List<int> list3 = new List<int>();
							list3.Add(Convert.ToInt32(obj));
							taskIDs.Add(new DeserializeJSON.TaskField(task, fields[i]), list3);
						}
						else
						{
							fields[i].SetValue(obj, DeserializeJSON.ValueToObject(task, fieldType, dict[fields[i].Name], behaviorSource, ref taskIDs));
						}
					}
				}
			}
		}

		private static object ValueToObject(Task task, Type type, object obj, BehaviorSource behaviorSource, ref Dictionary<DeserializeJSON.TaskField, List<int>> taskIDs)
		{
			if (type.IsSubclassOf(typeof(SharedVariable)))
			{
				return DeserializeJSON.DeserializeSharedVariable(obj as Dictionary<string, object>, behaviorSource);
			}
			if (type.Equals(typeof(UnityEngine.Object)) || type.IsSubclassOf(typeof(UnityEngine.Object)))
			{
				if (!obj.GetType().Equals(typeof(string)))
				{
					return behaviorSource.Owner.DeserializeUnityObject(Convert.ToInt32(obj));
				}
				return null;
			}
			else
			{
				if (type.IsPrimitive || type.Equals(typeof(string)))
				{
					return Convert.ChangeType(obj, type);
				}
				if (type.IsSubclassOf(typeof(Enum)))
				{
					return Enum.Parse(type, (string)obj);
				}
				if (type.Equals(typeof(Vector2)))
				{
					return DeserializeJSON.StringToVector2((string)obj);
				}
				if (type.Equals(typeof(Vector3)))
				{
					return DeserializeJSON.StringToVector3((string)obj);
				}
				if (type.Equals(typeof(Vector4)))
				{
					return DeserializeJSON.StringToVector4((string)obj);
				}
				if (type.Equals(typeof(Quaternion)))
				{
					return DeserializeJSON.StringToQuaternion((string)obj);
				}
				if (type.Equals(typeof(Matrix4x4)))
				{
					return DeserializeJSON.StringToMatrix4x4((string)obj);
				}
				if (type.Equals(typeof(Color)))
				{
					return DeserializeJSON.StringToColor((string)obj);
				}
				if (type.Equals(typeof(Rect)))
				{
					return DeserializeJSON.StringToRect((string)obj);
				}
				if (type.Equals(typeof(LayerMask)))
				{
					return DeserializeJSON.ValueToLayerMask(Convert.ToInt32(obj));
				}
				object obj2 = Activator.CreateInstance(type);
				DeserializeJSON.DeserializeObject(task, obj2, obj as Dictionary<string, object>, behaviorSource, ref taskIDs);
				return obj2;
			}
		}

		private static Vector2 StringToVector2(string vector2String)
		{
			string[] array = vector2String.Substring(1, vector2String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector2(float.Parse(array[0]), float.Parse(array[1]));
		}

		private static Vector3 StringToVector3(string vector3String)
		{
			string[] array = vector3String.Substring(1, vector3String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector3(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]));
		}

		private static Vector4 StringToVector4(string vector4String)
		{
			string[] array = vector4String.Substring(1, vector4String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector4(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}

		private static Quaternion StringToQuaternion(string quaternionString)
		{
			string[] array = quaternionString.Substring(1, quaternionString.Length - 2).Split(new char[]
			{
				','
			});
			return new Quaternion(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}

		private static Matrix4x4 StringToMatrix4x4(string matrixString)
		{
			string[] array = matrixString.Split(null);
			return new Matrix4x4
			{
				m00 = float.Parse(array[0]),
				m01 = float.Parse(array[1]),
				m02 = float.Parse(array[2]),
				m03 = float.Parse(array[3]),
				m10 = float.Parse(array[4]),
				m11 = float.Parse(array[5]),
				m12 = float.Parse(array[6]),
				m13 = float.Parse(array[7]),
				m20 = float.Parse(array[8]),
				m21 = float.Parse(array[9]),
				m22 = float.Parse(array[10]),
				m23 = float.Parse(array[11]),
				m30 = float.Parse(array[12]),
				m31 = float.Parse(array[13]),
				m32 = float.Parse(array[14]),
				m33 = float.Parse(array[15])
			};
		}

		private static Color StringToColor(string colorString)
		{
			string[] array = colorString.Substring(6, colorString.Length - 7).Split(new char[]
			{
				','
			});
			return new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
		}

		private static Rect StringToRect(string rectString)
		{
			string[] array = rectString.Substring(1, rectString.Length - 2).Split(new char[]
			{
				','
			});
			return new Rect(float.Parse(array[0].Substring(2, array[0].Length - 2)), float.Parse(array[1].Substring(3, array[1].Length - 3)), float.Parse(array[2].Substring(7, array[2].Length - 7)), float.Parse(array[3].Substring(8, array[3].Length - 8)));
		}

		private static LayerMask ValueToLayerMask(int value)
		{
			LayerMask result = default(LayerMask);
			result.value=value;
			return result;
		}
	}
}
