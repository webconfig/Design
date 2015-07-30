using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public abstract class ExternalBehavior : ScriptableObject, IBehavior
	{
		[SerializeField]
		private BehaviorSource mBehaviorSource;

		[SerializeField]
		private List<UnityEngine.Object> mUnityObjects;

		public BehaviorSource BehaviorSource
		{
			get
			{
				return this.mBehaviorSource;
			}
			set
			{
				this.mBehaviorSource = value;
			}
		}

		public BehaviorSource GetBehaviorSource()
		{
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
			return "External Behavior";
		}

		public SharedVariable GetVariable(string name)
		{
			return this.mBehaviorSource.GetVariable(name);
		}

		public void SetVariable(string name, SharedVariable item)
		{
			this.mBehaviorSource.SetVariable(name, item);
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
			UnityEngine.Object _object = this.mUnityObjects[id];
			if (_object == null)
			{
				return null;
			}
            return _object;
		}

		int IBehavior.GetInstanceID()
		{
			return base.GetInstanceID();
		}
	}
}
