using System;

namespace SkillEditor.Runtime.Tasks
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public class InheritedFieldAttribute : Attribute
	{
	}
}
