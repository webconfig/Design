using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[CustomEditor(typeof(BehaviorManager))]
    public class BehaviorManagerInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			BehaviorManager behaviorManager = base.target as BehaviorManager;
			behaviorManager.UpdateInterval = (BehaviorManager.UpdateIntervalType)EditorGUILayout.EnumPopup("Update Interval", behaviorManager.UpdateInterval, new GUILayoutOption[0]);
			if (behaviorManager.UpdateInterval == BehaviorManager.UpdateIntervalType.SpecifySeconds)
			{
				behaviorManager.UpdateIntervalSeconds = EditorGUILayout.FloatField("Seconds", behaviorManager.UpdateIntervalSeconds, new GUILayoutOption[0]);
			}
		}
	}
}
