using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[CustomEditor(typeof(ExternalBehavior))]
    public class ExternalBehaviorInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			ExternalBehavior externalBehavior = base.target as ExternalBehavior;
			if (externalBehavior == null)
			{
				return;
			}
			if (ExternalBehaviorInspector.DrawInspectorGUI(externalBehavior.BehaviorSource) && BehaviorDesignerWindow.instance != null && externalBehavior.GetBehaviorSource().BehaviorID == BehaviorDesignerWindow.instance.ActiveBehaviorID)
			{
				BehaviorDesignerWindow.instance.UpdateGraphStatus();
			}
			if (GUILayout.Button("Open Behavior Designer", new GUILayoutOption[0]))
			{
				BehaviorDesignerWindow.ShowWindow();
				BehaviorDesignerWindow.instance.LoadBehavior(externalBehavior.GetBehaviorSource(), false, false, true, true);
			}
		}

		public void Reset()
		{
			ExternalBehavior externalBehavior = base.target as ExternalBehavior;
			if (externalBehavior.BehaviorSource.Owner == null)
			{
				externalBehavior.BehaviorSource.Owner = externalBehavior;
			}
			TaskCopier.CheckTasks(base.target as ExternalBehavior);
		}

		public static bool DrawInspectorGUI(BehaviorSource behaviorSource)
		{
			EditorGUI.BeginChangeCheck();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.LabelField("Behavior Name", new GUILayoutOption[]
			{
				GUILayout.Width(120f)
			});
			behaviorSource.behaviorName = EditorGUILayout.TextField(behaviorSource.behaviorName, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
			EditorGUILayout.LabelField("Behavior Description", new GUILayoutOption[0]);
			behaviorSource.behaviorDescription = EditorGUILayout.TextArea(behaviorSource.behaviorDescription, new GUILayoutOption[]
			{
				GUILayout.Height(48f)
			});
			return EditorGUI.EndChangeCheck();
		}
	}
}
