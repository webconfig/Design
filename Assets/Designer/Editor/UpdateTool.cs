using BehaviorDesigner.Runtime;
using System;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class UpdateTool : EditorWindow
	{
		private Vector2 scrollPosition = Vector2.zero;

		public static void ShowWindow()
		{
			UpdateTool updateTool = EditorWindow.GetWindow(typeof(UpdateTool)) as UpdateTool;
			UnityEngine.Object.DontDestroyOnLoad(updateTool);
		}

		public void OnGUI()
		{
			bool flag = false;
			Behavior[] array = Resources.FindObjectsOfTypeAll(typeof(Behavior)) as Behavior[];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].HasDeprecatedTasks())
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				GUILayout.Label("The data format has changed in version 1.1.\nThe following behaviors will be updated:", new GUILayoutOption[0]);
				GUILayout.Space(10f);
				this.scrollPosition = GUILayout.BeginScrollView(this.scrollPosition, new GUILayoutOption[0]);
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j].HasDeprecatedTasks())
					{
						GUILayout.Label(array[j].ToString(), new GUILayoutOption[0]);
					}
				}
				GUILayout.EndScrollView();
				GUILayout.Space(10f);
				if (GUILayout.Button("Update", new GUILayoutOption[0]))
				{
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k].HasDeprecatedTasks())
						{
							array[k].UpdateDeprecatedTasks();
							EditorUtility.SetDirty(array[k]);
						}
					}
					return;
				}
			}
			else
			{
				GUILayout.Label("No behaviors need updating.", new GUILayoutOption[0]);
			}
		}
	}
}
