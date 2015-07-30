using System;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	public abstract class SharedVariable : ScriptableObject
	{
		[SerializeField]
		protected bool mIsShared;

		[SerializeField]
		protected SharedVariableTypes mValueType;

		public bool IsShared
		{
			get
			{
				return this.mIsShared;
			}
			set
			{
				this.mIsShared = value;
			}
		}

		public SharedVariableTypes ValueType
		{
			get
			{
				return this.mValueType;
			}
		}

		public abstract object GetValue();

		public abstract void SetValue(object value);
	}
}
