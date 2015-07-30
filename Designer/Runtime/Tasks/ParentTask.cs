using System;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	public abstract class ParentTask : Task
	{
		[SerializeField]
		protected List<Task> children;

		public List<Task> Children
		{
			get
			{
				return this.children;
			}
			private set
			{
				this.children = value;
			}
		}

		public virtual int MaxChildren()
		{
			return 2147483647;
		}

		public virtual bool CanRunParallelChildren()
		{
			return false;
		}

		public virtual int CurrentChildIndex()
		{
			return 0;
		}

		public virtual bool CanExecute()
		{
			return true;
		}

		public virtual TaskStatus Decorate(TaskStatus status)
		{
			return status;
		}

		public virtual void OnChildExecuted(TaskStatus childStatus)
		{
		}

		public virtual void OnChildExecuted(int childIndex, TaskStatus childStatus)
		{
		}

		public virtual void OnChildRunning()
		{
		}

		public virtual void OnChildRunning(int childIndex)
		{
		}

		public virtual TaskStatus OverrideStatus(TaskStatus status)
		{
			return status;
		}

		public virtual TaskStatus OverrideStatus()
		{
			return TaskStatus.Running;
		}

		public void AddChild(Task child, int index)
		{
			if (this.children == null)
			{
				this.children = new List<Task>();
			}
			this.children.Insert(index, child);
		}

		public void ReplaceAddChild(Task child, int index)
		{
			if (this.children != null && index < this.children.Count)
			{
				this.children[index] = child;
				return;
			}
			this.AddChild(child, index);
		}
	}
}
