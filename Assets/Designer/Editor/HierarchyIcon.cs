using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	[InitializeOnLoad]
	public class HierarchyIcon : ScriptableObject
	{
		private static Texture2D icon;

		static HierarchyIcon()
		{
			HierarchyIcon.icon = (Resources.LoadAssetAtPath("Assets/Gizmos/Behavior Designer Hier Icon.png", typeof(Texture2D)) as Texture2D);
			if (HierarchyIcon.icon != null)
			{
				EditorApplication.hierarchyWindowItemOnGUI = (EditorApplication.HierarchyWindowItemCallback)Delegate.Combine(EditorApplication.hierarchyWindowItemOnGUI, new EditorApplication.HierarchyWindowItemCallback(HierarchyIcon.hierarchyWindowItemOnGUI));
			}
		}

		private static void hierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
		{
			if (BehaviorDesignerPreferences.GetBool(BDPreferneces.ShowHierarchyIcon))
			{
				GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
				if (gameObject != null && gameObject.GetComponent<Behavior>() != null)
				{
					Rect rect = new Rect(selectionRect);
					rect.x=rect.width - 1f;
					rect.width=16f;
					rect.height=16f;
					GUI.DrawTexture(rect, HierarchyIcon.icon);
				}
			}
		}
	}
}
