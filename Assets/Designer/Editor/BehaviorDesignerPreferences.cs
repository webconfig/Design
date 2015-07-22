using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class BehaviorDesignerPreferences : UnityEditor.Editor
	{
		public static void InitPrefernces()
		{
			if (!EditorPrefs.HasKey(string.Format("BehaviorDesigner{0}", BDPreferneces.ShowWelcomeScreen)))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowWelcomeScreen, true);
			}
			if (!EditorPrefs.HasKey(string.Format("BehaviorDesigner{0}", BDPreferneces.ShowSceneIcon)))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowSceneIcon, true);
			}
			if (!EditorPrefs.HasKey(string.Format("BehaviorDesigner{0}", BDPreferneces.ShowHierarchyIcon)))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowHierarchyIcon, true);
			}
			if (!EditorPrefs.HasKey(string.Format("BehaviorDesigner{0}", BDPreferneces.OpenInspectorOnTaskSelection)))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferneces.OpenInspectorOnTaskSelection, false);
			}
			if (!EditorPrefs.HasKey(string.Format("BehaviorDesigner{0}", BDPreferneces.OpenInspectorOnTaskDoubleClick)))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferneces.OpenInspectorOnTaskDoubleClick, false);
			}
			if (!EditorPrefs.HasKey(string.Format("BehaviorDesigner{0}", BDPreferneces.FadeNodes)))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferneces.FadeNodes, true);
			}
			if (!EditorPrefs.HasKey(string.Format("BehaviorDesigner{0}", BDPreferneces.PropertiesPanelOnLeft)))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferneces.PropertiesPanelOnLeft, true);
			}
			if (!EditorPrefs.HasKey(string.Format("BehaviorDesigner{0}", BDPreferneces.ShowExternalTrees)))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowExternalTrees, true);
			}
			if (!EditorPrefs.HasKey(string.Format("BehaviorDesigner{0}", BDPreferneces.ShowTaskDescription)))
			{
				BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowTaskDescription, true);
			}
		}

		public static void DrawPreferencesPane(PreferenceChangeHandler callback)
		{
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferneces.ShowWelcomeScreen, "Show welcome screen", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferneces.ShowSceneIcon, "Show Behavior Designer icon in the scene", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferneces.ShowHierarchyIcon, "Show Behavior Designer icon in the hierarchy window", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferneces.OpenInspectorOnTaskSelection, "Open inspector on single task selection", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferneces.OpenInspectorOnTaskDoubleClick, "Open inspector on task double click", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferneces.FadeNodes, "Fade tasks after they are done running", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferneces.PropertiesPanelOnLeft, "Position properties panel on the left", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferneces.ShowExternalTrees, "Show external behavior trees while running", callback);
			BehaviorDesignerPreferences.DrawBoolPref(BDPreferneces.ShowTaskDescription, "Show selected task description", callback);
			if (GUILayout.Button("Restore to Defaults", EditorStyles.miniButtonMid, new GUILayoutOption[0]))
			{
				BehaviorDesignerPreferences.ResetPrefs();
			}
		}

		private static void DrawBoolPref(BDPreferneces pref, string text, PreferenceChangeHandler callback)
		{
			bool _bool = BehaviorDesignerPreferences.GetBool(pref);
            bool flag = GUILayout.Toggle(_bool, text, new GUILayoutOption[0]);
			if (flag != _bool)
			{
				BehaviorDesignerPreferences.SetBool(pref, flag);
				callback(pref, flag);
			}
		}

		private static void ResetPrefs()
		{
			BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowWelcomeScreen, true);
			BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowSceneIcon, true);
			BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowHierarchyIcon, true);
			BehaviorDesignerPreferences.SetBool(BDPreferneces.OpenInspectorOnTaskSelection, false);
			BehaviorDesignerPreferences.SetBool(BDPreferneces.OpenInspectorOnTaskDoubleClick, false);
			BehaviorDesignerPreferences.SetBool(BDPreferneces.FadeNodes, true);
			BehaviorDesignerPreferences.SetBool(BDPreferneces.PropertiesPanelOnLeft, true);
			BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowExternalTrees, true);
			BehaviorDesignerPreferences.SetBool(BDPreferneces.ShowTaskDescription, true);
		}

		public static void SetBool(BDPreferneces pref, bool value)
		{
			EditorPrefs.SetBool(string.Format("BehaviorDesigner{0}", pref), value);
		}

		public static bool GetBool(BDPreferneces pref)
		{
			return EditorPrefs.GetBool(string.Format("BehaviorDesigner{0}", pref));
		}
	}
}
