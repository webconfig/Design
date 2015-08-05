using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	[Serializable]
	public class BehaviorSource
	{
		public string behaviorName = "Behavior";

		public string behaviorDescription = "";

		private int behaviorID = -1;

		private bool isDirty = true;

        //[SerializeField]
        //private Task mEntryTask;

		[SerializeField]
		private Task mRootTask;

		[SerializeField]
		private List<Task> mDetachedTasks;

		[SerializeField]
		private string mSerialization;

		[SerializeField]
		private List<SharedVariable> mVariables;

		private Dictionary<string, int> mSharedVariableIndex;

		[SerializeField]
		private IBehavior mOwner;

		public int BehaviorID
		{
			get
			{
				return this.behaviorID;
			}
			set
			{
				this.behaviorID = value;
			}
		}

		public bool IsDirty
		{
			get
			{
				return this.isDirty;
			}
			set
			{
				this.isDirty = value;
			}
		}

        //public Task EntryTask
        //{
        //    get
        //    {
        //        return this.mEntryTask;
        //    }
        //    set
        //    {
        //        this.mEntryTask = value;
        //    }
        //}

		public Task RootTask
		{
			get
			{
				return this.mRootTask;
			}
			set
			{
				this.mRootTask = value;
			}
		}

		public List<Task> DetachedTasks
		{
			get
			{
				return this.mDetachedTasks;
			}
			set
			{
				this.mDetachedTasks = value;
			}
		}

		public string Serialization
		{
			get
			{
				return this.mSerialization;
			}
			set
			{
				this.mSerialization = value;
			}
		}

		public List<SharedVariable> Variables
		{
			get
			{
				return this.mVariables;
			}
			set
			{
				this.mVariables = value;
				this.updateVariablesIndex();
			}
		}

		public IBehavior Owner
		{
			get
			{
				return this.mOwner;
			}
			set
			{
				this.mOwner = value;
			}
		}

		public BehaviorSource(IBehavior owner)
		{
			this.mOwner = owner;
		}

		public void save(Task rootTask, List<Task> detachedTasks)
		{
            //this.mEntryTask = entryTask;
			this.mRootTask = rootTask;
			this.mDetachedTasks = detachedTasks;
		}

		public void load(out Task rootTask, out List<Task> detachedTasks)
		{
			rootTask = this.mRootTask;
			detachedTasks = this.mDetachedTasks;
		}

		public void CheckForJSONSerialization(bool force)
		{
            //Debug.Log((this.mSerialization != null) + "--" + (!this.mSerialization.Equals("")) + "--" + ((this.mRootTask == null && (this.mVariables == null || this.mVariables.Count == 0 || this.mVariables[0] == null)) || force));
			if (this.mSerialization != null && 
                !this.mSerialization.Equals("") && 
                ((this.mRootTask == null && (this.mVariables == null || this.mVariables.Count == 0 || this.mVariables[0] == null)) || force))
			{
				DeserializeJSON.Deserialize(this);
			}
		}

		public SharedVariable GetVariable(string name)
		{
			if (this.mVariables != null)
			{
				if (this.mSharedVariableIndex == null || this.mSharedVariableIndex.Count != this.mVariables.Count)
				{
					this.mSharedVariableIndex = new Dictionary<string, int>(this.Variables.Count);
					for (int i = 0; i < this.mVariables.Count; i++)
					{
						if (this.mVariables[i] == null)
						{
							return null;
						}
						this.mSharedVariableIndex.Add(this.mVariables[i].name, i);
					}
				}
				if (this.mSharedVariableIndex.ContainsKey(name))
				{
					return this.mVariables[this.mSharedVariableIndex[name]];
				}
			}
			return null;
		}

		public void SetVariable(string name, SharedVariable item)
		{
			if (this.mVariables == null)
			{
				this.mVariables = new List<SharedVariable>();
			}
			if (this.mSharedVariableIndex != null && this.mSharedVariableIndex.ContainsKey(name))
			{
				this.mVariables[this.mSharedVariableIndex[name]] = item;
				return;
			}
			this.mVariables.Add(item);
			this.updateVariablesIndex();
		}

		private void updateVariablesIndex()
		{
			if (this.mVariables == null)
			{
				if (this.mSharedVariableIndex != null)
				{
					this.mSharedVariableIndex = null;
				}
				return;
			}
			if (this.mSharedVariableIndex == null)
			{
				this.mSharedVariableIndex = new Dictionary<string, int>(this.mVariables.Count);
			}
			else
			{
				this.mSharedVariableIndex.Clear();
			}
			for (int i = 0; i < this.mVariables.Count; i++)
			{
				if (!(this.mVariables[i] == null))
				{
					this.mSharedVariableIndex.Add(this.mVariables[i].name, i);
				}
			}
		}

		public override string ToString()
		{
			if (this.mOwner == null)
			{
				return this.behaviorName;
			}
			return string.Format("{0} - {1}", this.Owner.GetOwnerName(), this.behaviorName);
		}
	}
}
