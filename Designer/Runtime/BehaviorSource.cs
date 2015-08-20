using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    /// <summary>
    /// 数据源
    /// </summary>
	[Serializable]
	public class BehaviorSource
	{
		public string behaviorName = "Behavior";

		public string behaviorDescription = "";
		[SerializeField]
		private string mSerialization;

		[SerializeField]
		private List<SharedVariable> mVariables;

		private Dictionary<string, int> mSharedVariableIndex;

        //[SerializeField]
        //private IBehavior mOwner;

        public int BehaviorID;

        public bool IsDirty;

        /// <summary>
        /// 离散节点
        /// </summary>
        public List<Task> DetachedTasks;

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

        //public IBehavior Owner
        //{
        //    get
        //    {
        //        return this.mOwner;
        //    }
        //    set
        //    {
        //        this.mOwner = value;
        //    }
        //}

		public BehaviorSource(IBehavior owner)
		{
            //this.mOwner = owner;
		}
        public BehaviorSource()
        {
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="rootTask"></param>
        /// <param name="detachedTasks"></param>
		public void save(List<Task> detachedTasks)
		{
            DetachedTasks = detachedTasks;
		}

        public List<Task> GetTasks()
		{
            return  DetachedTasks;
		}

        public void CheckForJSONSerialization(bool force)
        {
            //Debug.Log((this.mSerialization != null) + "--" + (!this.mSerialization.Equals("")) + "--" + ((this.mRootTask == null && (this.mVariables == null || this.mVariables.Count == 0 || this.mVariables[0] == null)) || force));
            if (this.mSerialization != null &&
                !this.mSerialization.Equals("") &&
                (((this.mVariables == null || this.mVariables.Count == 0 || this.mVariables[0] == null)) || force))
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
            return "ToString";
            //if (this.mOwner == null)
            //{
            //    return this.behaviorName;
            //}
            //return string.Format("{0} - {1}", this.Owner.GetOwnerName(), this.behaviorName);
		}

        /// <summary>
        /// xml 序列化
        /// </summary>
        /// <param name="data"></param>
        public string SerializeXml()
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder();
            for(int i=0;i<DetachedTasks.Count;i++)
            {
              str.Append(DetachedTasks[i].SerializeUI());
            }
            return str.ToString();

        }

	}
}
