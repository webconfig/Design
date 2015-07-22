using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[Serializable]
	public class TaskSerializer
	{
		public Type taskType;

		public FieldInfo[] fieldInfo;

		public object[] fieldValue;

		public Vector2 position;

		public string friendlyName;

		public string comment;

		public List<int> childrenIndex;
	}
}
