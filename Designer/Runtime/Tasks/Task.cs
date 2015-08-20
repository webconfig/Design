using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	public abstract class Task : ScriptableObject
	{
        [System.NonSerialized]
        public DesignerNodeData NodeData;
        //[System.NonSerialized]
        //public Behavior Owner;
        [System.NonSerialized]
        public int ID;
        [System.NonSerialized]
        public bool IsInstant;
        [System.NonSerialized]
        public int ReferenceID;
         [System.NonSerialized]
        public List<Task> Children;

		public virtual void OnAwake()
		{
		}

		public virtual void OnStart()
		{
		}

		public virtual TaskStatus OnUpdate()
		{
			return TaskStatus.Success;
		}

		public virtual void OnEnd()
		{
		}

		public virtual void OnPause(bool paused)
		{
		}

		public virtual float GetPriority()
		{
			return 0f;
		}

		public virtual void OnBehaviorRestart()
		{
		}

		public virtual void OnReset()
		{
		}


        public void AddChild(Task child, int index)
        {
            if (Children == null)
            {
                Children = new List<Task>();
            }
            Children.Insert(index, child);
        }

        public void ReplaceAddChild(Task child, int index)
        {
            if (Children != null && index < Children.Count)
            {
                Children[index] = child;
                return;
            }
            this.AddChild(child, index);
        }


        /// <summary>
        /// 序列化节点内容
        /// </summary>
        /// <returns></returns>
        public virtual string Serialize()
        {
            return string.Empty;
        }


        /// <summary>
        /// 序列化节点UI
        /// </summary>
        /// <returns></returns>
        public virtual string SerializeUI()
        {
            return string.Empty;
        }
	}
}
