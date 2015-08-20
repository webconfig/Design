//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace BehaviorDesigner.Runtime.Tasks
//{
//    /// <summary>
//    /// 拥有子节点的模块
//    /// </summary>
//    public abstract class ParentTask : Task
//    {

        

//        public virtual bool CanRunParallelChildren()
//        {
//            return false;
//        }

//        public virtual int CurrentChildIndex()
//        {
//            return 0;
//        }

//        public virtual bool CanExecute()
//        {
//            return true;
//        }

//        public virtual TaskStatus Decorate(TaskStatus status)
//        {
//            return status;
//        }

//        public virtual void OnChildExecuted(TaskStatus childStatus)
//        {
//        }

//        public virtual void OnChildExecuted(int childIndex, TaskStatus childStatus)
//        {
//        }

//        public virtual void OnChildRunning()
//        {
//        }

//        public virtual void OnChildRunning(int childIndex)
//        {
//        }

//        public virtual TaskStatus OverrideStatus(TaskStatus status)
//        {
//            return status;
//        }

//        public virtual TaskStatus OverrideStatus()
//        {
//            return TaskStatus.Running;
//        }

		
//    }
//}
