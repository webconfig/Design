using BehaviorDesigner.Runtime;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
    public class VariableCopier : UnityEditor.Editor
	{
		public static bool CopySerialized(SharedVariable variable, out SharedVariable newVariable, HideFlags hideFlags)
		{
			if (variable == null)
			{
				newVariable = null;
				return false;
			}
			newVariable = (ScriptableObject.CreateInstance(variable.GetType()) as SharedVariable);
			FieldInfo[] fields = variable.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				fields[i].SetValue(newVariable, fields[i].GetValue(variable));
			}
			newVariable.name=variable.name;
			newVariable.hideFlags=hideFlags;
			return newVariable;
		}
	}
}
