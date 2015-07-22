using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class NodeData
	{
		[SerializeField]
		private object nodeDesigner;

		[SerializeField]
		private Vector2 position;

		[SerializeField]
		private string friendlyName = "";

		[SerializeField]
		private string comment = "";

		[SerializeField]
		private bool isBreakpoint;

		[SerializeField]
		private Texture icon;

		[SerializeField]
		private bool collapsed;

		[SerializeField]
		private bool disabled;

		[SerializeField]
		private List<string> watchedFieldNames;

		private List<FieldInfo> watchedFields;

		private float pushTime = -1f;

		private float popTime = -1f;

		private TaskStatus executionStatus;

		public object NodeDesigner
		{
			get
			{
				return this.nodeDesigner;
			}
			set
			{
				this.nodeDesigner = value;
			}
		}

		public Vector2 Position
		{
			get
			{
				return this.position;
			}
			set
			{
				this.position = value;
			}
		}

		public string FriendlyName
		{
			get
			{
				return this.friendlyName;
			}
			set
			{
				this.friendlyName = value;
			}
		}

		public string Comment
		{
			get
			{
				return this.comment;
			}
			set
			{
				this.comment = value;
			}
		}

		public bool IsBreakpoint
		{
			get
			{
				return this.isBreakpoint;
			}
			set
			{
				this.isBreakpoint = value;
			}
		}

		public Texture Icon
		{
			get
			{
				return this.icon;
			}
			set
			{
				this.icon = value;
			}
		}

		public bool Collapsed
		{
			get
			{
				return this.collapsed;
			}
			set
			{
				this.collapsed = value;
			}
		}

		public bool Disabled
		{
			get
			{
				return this.disabled;
			}
			set
			{
				this.disabled = value;
			}
		}

		public List<FieldInfo> WatchedFields
		{
			get
			{
				return this.watchedFields;
			}
		}

		public float PushTime
		{
			get
			{
				return this.pushTime;
			}
			set
			{
				this.pushTime = value;
			}
		}

		public float PopTime
		{
			get
			{
				return this.popTime;
			}
			set
			{
				this.popTime = value;
			}
		}

		public TaskStatus ExecutionStatus
		{
			get
			{
				return this.executionStatus;
			}
			set
			{
				this.executionStatus = value;
			}
		}

		public void initWatchedFields(Task task)
		{
			if (this.watchedFieldNames != null && this.watchedFieldNames.Count > 0)
			{
				this.watchedFields = new List<FieldInfo>();
				for (int i = 0; i < this.watchedFieldNames.Count; i++)
				{
					FieldInfo field = task.GetType().GetField(this.watchedFieldNames[i]);
					if (field != null)
					{
						this.watchedFields.Add(field);
					}
				}
			}
		}

		public void copyFrom(NodeData nodeData, Task task)
		{
			this.nodeDesigner = nodeData.NodeDesigner;
			this.position = nodeData.Position;
			this.friendlyName = nodeData.FriendlyName;
			this.comment = nodeData.Comment;
			this.isBreakpoint = nodeData.IsBreakpoint;
			this.collapsed = nodeData.Collapsed;
			this.disabled = nodeData.Disabled;
			if (nodeData.WatchedFields != null && nodeData.WatchedFields.Count > 0)
			{
				this.watchedFields = new List<FieldInfo>();
				this.watchedFieldNames = new List<string>();
				for (int i = 0; i < nodeData.watchedFields.Count; i++)
				{
					FieldInfo field = task.GetType().GetField(nodeData.WatchedFields[i].Name);
					if (field != null)
					{
						this.watchedFields.Add(field);
						this.watchedFieldNames.Add(field.Name);
					}
				}
			}
		}

		public bool containsWatchedField(FieldInfo field)
		{
			return this.watchedFields != null && this.watchedFields.Contains(field);
		}

		public void addWatchedField(FieldInfo field)
		{
			if (this.watchedFields == null)
			{
				this.watchedFields = new List<FieldInfo>();
				this.watchedFieldNames = new List<string>();
			}
			this.watchedFields.Add(field);
			this.watchedFieldNames.Add(field.Name);
		}

		public void removeWatchedField(FieldInfo field)
		{
			if (this.watchedFields != null)
			{
				this.watchedFields.Remove(field);
				this.watchedFieldNames.Remove(field.Name);
			}
		}

		public Dictionary<string, object> serialize()
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("Position", this.position);
			if (this.friendlyName.Length > 0)
			{
				dictionary.Add("FriendlyName", this.friendlyName);
			}
			if (this.comment.Length > 0)
			{
				dictionary.Add("Comment", this.comment);
			}
			if (this.collapsed)
			{
				dictionary.Add("Collapsed", this.collapsed);
			}
			if (this.disabled)
			{
				dictionary.Add("Disabled", this.disabled);
			}
			if (this.watchedFieldNames != null && this.watchedFieldNames.Count > 0)
			{
				dictionary.Add("WatchedFields", this.watchedFieldNames);
			}
			return dictionary;
		}

		public void deserialize(Dictionary<string, object> dict, Task task)
		{
			this.position = NodeData.StringToVector2((string)dict["Position"]);
			if (dict.ContainsKey("FriendlyName"))
			{
				this.friendlyName = (string)dict["FriendlyName"];
			}
			if (dict.ContainsKey("Comment"))
			{
				this.comment = (string)dict["Comment"];
			}
			if (dict.ContainsKey("Collapsed"))
			{
				this.collapsed = Convert.ToBoolean(dict["Collapsed"]);
			}
			if (dict.ContainsKey("Disabled"))
			{
				this.collapsed = Convert.ToBoolean(dict["Disabled"]);
			}
			if (dict.ContainsKey("WatchedFields"))
			{
				this.watchedFieldNames = new List<string>();
				this.watchedFields = new List<FieldInfo>();
				IList list = dict["WatchedFields"] as IList;
				for (int i = 0; i < list.Count; i++)
				{
					FieldInfo field = task.GetType().GetField((string)list[i]);
					if (field != null)
					{
						this.watchedFieldNames.Add(field.Name);
						this.watchedFields.Add(field);
					}
				}
			}
		}

		private static Vector2 StringToVector2(string vector2String)
		{
			string[] array = vector2String.Substring(1, vector2String.Length - 2).Split(new char[]
			{
				','
			});
			return new Vector3(float.Parse(array[0]), float.Parse(array[1]));
		}
	}
}
