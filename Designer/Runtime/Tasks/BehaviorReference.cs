using System;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
	[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=53"), TaskDescription("Behavior Reference allows you to run another behavior tree within the current behavior tree."), TaskIcon("BehaviorTreeReferenceIcon.png")]
	public abstract class BehaviorReference : Action
	{
		[Obsolete("The field BehaviorReference.externalBehavior is deprecated. Use the array externalBehaviors instead.")]
		public BehaviorDesigner.Runtime.ExternalBehavior externalBehavior;

		public BehaviorDesigner.Runtime.ExternalBehavior[] externalBehaviors;

		public virtual BehaviorDesigner.Runtime.ExternalBehavior[] getExternalBehaviors()
		{
			if (this.externalBehavior != null)
			{
				Debug.LogWarning("The field BehaviorReference.externalBehavior is deprecated. Use the array externalBehaviors instead.");
				return new BehaviorDesigner.Runtime.ExternalBehavior[]
				{
					this.externalBehavior
				};
			}
			return this.externalBehaviors;
		}

		public override void OnReset()
		{
			this.externalBehaviors = null;
		}
	}
}
