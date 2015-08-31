using System;

namespace SkillEditor.Runtime.Tasks
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class TooltipAttribute : Attribute
	{
		public readonly string mTooltip;

		public string Tooltip
		{
			get
			{
				return this.mTooltip;
			}
		}

		public TooltipAttribute(string tooltip)
		{
			this.mTooltip = tooltip;
		}
	}
}
