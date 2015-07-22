using System;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=15"), TaskDescription("External Behavior allows you to run another behavior tree within the current behavior tree."), TaskIcon("ExternalBehaviorTreeIcon.png"), Obsolete("External Behavior Task is deprecated. Use Behavior Reference Task instead.")]
	public abstract class ExternalBehavior : Action
	{
		[Tooltip("External task that this task should reference")]
		public GameObject externalTask;

		public override void OnReset()
		{
			this.externalTask = null;
		}
	}
}
