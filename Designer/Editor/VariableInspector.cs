using BehaviorDesigner.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class VariableInspector : ScriptableObject
	{
		private string[] sharedValueTypes;

		private string variableName = "";

		private int variableTypeIndex;

		private Vector2 mScrollPosition = Vector2.zero;

		private bool mFocusNameField;

		public void OnEnable()
		{
            base.hideFlags = HideFlags.HideAndDontSave;
			Array values = Enum.GetValues(typeof(SharedVariableTypes));
			this.sharedValueTypes = new string[values.Length];
			int num = 0;
			foreach (object current in values)
			{
				this.sharedValueTypes[num] = current.ToString();
				num++;
			}
		}

		public void clearFocus()
		{
			GUIUtility.keyboardControl=0;
		}

		public bool hasFocus()
		{
			return GUIUtility.keyboardControl != 0;
		}

		public void focusNameField()
		{
			this.mFocusNameField = true;
		}

		public bool drawVariables(BehaviorSource behaviorSource)
		{
			if (behaviorSource == null)
			{
				return false;
			}
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Name", new GUILayoutOption[0]);
			GUI.SetNextControlName("Name");
			this.variableName = GUILayout.TextField(this.variableName, new GUILayoutOption[]
			{
				GUILayout.Width(220f)
			});
			if (this.mFocusNameField)
			{
				GUI.FocusControl("Name");
				this.mFocusNameField = false;
			}
			GUILayout.EndHorizontal();
			GUILayout.Space(2f);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Label("Type", new GUILayoutOption[0]);
			this.variableTypeIndex = EditorGUILayout.Popup(this.variableTypeIndex, this.sharedValueTypes, EditorStyles.toolbarPopup, new GUILayoutOption[]
			{
				GUILayout.Width(169f)
			});
			GUILayout.Space(8f);
			bool flag = false;
			List<SharedVariable> list = behaviorSource.Variables;
			bool flag2 = !this.variableName.Equals("") && behaviorSource.GetVariable(this.variableName) == null;
			GUI.enabled=flag2;
			if (GUILayout.Button("Add", EditorStyles.toolbarButton, new GUILayoutOption[]
			{
				GUILayout.Width(40f)
			}) && flag2)
			{
				SharedVariable sharedVariable = ScriptableObject.CreateInstance(string.Format("Shared{0}", this.sharedValueTypes[this.variableTypeIndex])) as SharedVariable;
				sharedVariable.name=this.variableName;
				sharedVariable.IsShared = true;
				list.Add(sharedVariable);
				this.variableName = "";
				flag = true;
			}
			GUI.enabled=true;
			GUILayout.Space(6f);
			GUILayout.EndHorizontal();
			BehaviorDesignerUtility.DrawContentSeperator(2);
			GUILayout.Space(4f);
			this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					EditorGUI.BeginChangeCheck();
					SharedVariable sharedVariable2 = list[i];
					if (sharedVariable2 == null)
					{
						list = null;
						flag = true;
						break;
					}
					GUILayout.BeginHorizontal(new GUILayoutOption[0]);
					if (GUILayout.Button(BehaviorDesignerUtility.DeleteButtonTexture, BehaviorDesignerUtility.PlainButtonGUIStyle, new GUILayoutOption[]
					{
						GUILayout.Width(14f)
					}))
					{
						BehaviorUndo.RegisterUndo("Variable", sharedVariable2, true, true);
						list.RemoveAt(i);
						flag = true;
						break;
					}
					GUILayout.Label(sharedVariable2.name, new GUILayoutOption[]
					{
						GUILayout.Width(128f)
					});
					bool flag3 = false;
					switch (sharedVariable2.ValueType)
					{
					case SharedVariableTypes.Vector2:
					case SharedVariableTypes.Vector3:
					case SharedVariableTypes.Vector4:
					case SharedVariableTypes.Quaternion:
					case SharedVariableTypes.Rect:
						GUILayout.EndHorizontal();
						flag3 = true;
						break;
					}
					VariableInspector.DrawSharedVariableValue(sharedVariable2, 160);
					if (!flag3)
					{
						GUILayout.EndHorizontal();
					}
					GUILayout.Space(4f);
					if (EditorGUI.EndChangeCheck())
					{
						BehaviorUndo.RegisterUndo("Variable", sharedVariable2, true, false);
						flag = true;
					}
				}
			}
			GUILayout.EndScrollView();
			if (flag)
			{
				behaviorSource.Variables = list;
			}
			return flag;
		}

		public static void DrawSharedVariableValue(SharedVariable sharedVariable, int width)
		{
			if (sharedVariable == null)
			{
				return;
			}
			try
			{
				switch (sharedVariable.ValueType)
				{
				case SharedVariableTypes.Int:
					sharedVariable.SetValue(EditorGUILayout.IntField((int)sharedVariable.GetValue(), new GUILayoutOption[]
					{
						GUILayout.Width((float)width)
					}));
					break;
				case SharedVariableTypes.Float:
					sharedVariable.SetValue(EditorGUILayout.FloatField((float)sharedVariable.GetValue(), new GUILayoutOption[]
					{
						GUILayout.Width((float)width)
					}));
					break;
				case SharedVariableTypes.Bool:
					sharedVariable.SetValue(EditorGUILayout.Toggle((bool)sharedVariable.GetValue(), EditorStyles.toggle, new GUILayoutOption[]
					{
						GUILayout.Width((float)width)
					}));
					break;
				case SharedVariableTypes.String:
					sharedVariable.SetValue(EditorGUILayout.TextField((string)sharedVariable.GetValue(), new GUILayoutOption[]
					{
						GUILayout.Width((float)width)
					}));
					break;
				case SharedVariableTypes.Vector2:
					sharedVariable.SetValue(VariableInspector.Vector2Field((Vector2)sharedVariable.GetValue()));
					break;
				case SharedVariableTypes.Vector3:
					sharedVariable.SetValue(VariableInspector.Vector3Field((Vector3)sharedVariable.GetValue()));
					break;
				case SharedVariableTypes.Vector4:
					sharedVariable.SetValue(VariableInspector.Vector4Field((Vector4)sharedVariable.GetValue()));
					break;
				case SharedVariableTypes.Quaternion:
				{
					Vector4 vector = VariableInspector.QuaternionToVector4((Quaternion)sharedVariable.GetValue());
					Vector4 vector2 = VariableInspector.Vector4Field(vector);
					if (vector != vector2)
					{
						sharedVariable.SetValue(new Quaternion(vector2.x, vector2.y, vector2.z, vector2.w));
					}
					break;
				}
				case SharedVariableTypes.Color:
					sharedVariable.SetValue(EditorGUILayout.ColorField((Color)sharedVariable.GetValue(), new GUILayoutOption[]
					{
						GUILayout.Width((float)width)
					}));
					break;
				case SharedVariableTypes.Rect:
					sharedVariable.SetValue(EditorGUILayout.RectField((Rect)sharedVariable.GetValue(), new GUILayoutOption[0]));
					break;
				case SharedVariableTypes.GameObject:
					sharedVariable.SetValue(EditorGUILayout.ObjectField((GameObject)sharedVariable.GetValue(), typeof(GameObject), true, new GUILayoutOption[]
					{
						GUILayout.Width((float)width)
					}));
					break;
				case SharedVariableTypes.Transform:
					sharedVariable.SetValue(EditorGUILayout.ObjectField((Transform)sharedVariable.GetValue(), typeof(Transform), true, new GUILayoutOption[]
					{
						GUILayout.Width((float)width)
					}));
					break;
				case SharedVariableTypes.Object:
					sharedVariable.SetValue(EditorGUILayout.ObjectField((UnityEngine.Object)sharedVariable.GetValue(), typeof(UnityEngine.Object), true, new GUILayoutOption[]
					{
						GUILayout.Width((float)width)
					}));
					break;
				}
			}
			catch (Exception)
			{
			}
		}

		private static Vector4 QuaternionToVector4(Quaternion quaternion)
		{
			return new Vector4(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
		}

		private static Vector2 Vector2Field(Vector2 value)
		{
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Space(10f);
			EditorGUILayout.LabelField("X", new GUILayoutOption[]
			{
				GUILayout.Width(14f)
			});
			value.x = EditorGUILayout.FloatField(value.x, new GUILayoutOption[]
			{
				GUILayout.Width(120f)
			});
			EditorGUILayout.LabelField("Y", new GUILayoutOption[]
			{
				GUILayout.Width(14f)
			});
			value.y = EditorGUILayout.FloatField(value.y, new GUILayoutOption[]
			{
				GUILayout.Width(120f)
			});
			EditorGUILayout.EndHorizontal();
			return value;
		}

		private static Vector3 Vector3Field(Vector3 value)
		{
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Space(10f);
			EditorGUILayout.LabelField("X", new GUILayoutOption[]
			{
				GUILayout.Width(14f)
			});
			value.x = EditorGUILayout.FloatField(value.x, new GUILayoutOption[]
			{
				GUILayout.Width(73f)
			});
			EditorGUILayout.LabelField("Y", new GUILayoutOption[]
			{
				GUILayout.Width(14f)
			});
			value.y = EditorGUILayout.FloatField(value.y, new GUILayoutOption[]
			{
				GUILayout.Width(73f)
			});
			EditorGUILayout.LabelField("Z", new GUILayoutOption[]
			{
				GUILayout.Width(14f)
			});
			value.z = EditorGUILayout.FloatField(value.z, new GUILayoutOption[]
			{
				GUILayout.Width(73f)
			});
			EditorGUILayout.EndHorizontal();
			return value;
		}

		private static Vector4 Vector4Field(Vector4 value)
		{
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Space(10f);
			EditorGUILayout.LabelField("X", new GUILayoutOption[]
			{
				GUILayout.Width(14f)
			});
			value.x = EditorGUILayout.FloatField(value.x, new GUILayoutOption[]
			{
				GUILayout.Width(50f)
			});
			EditorGUILayout.LabelField("Y", new GUILayoutOption[]
			{
				GUILayout.Width(14f)
			});
			value.y = EditorGUILayout.FloatField(value.y, new GUILayoutOption[]
			{
				GUILayout.Width(50f)
			});
			EditorGUILayout.LabelField("Z", new GUILayoutOption[]
			{
				GUILayout.Width(14f)
			});
			value.z = EditorGUILayout.FloatField(value.z, new GUILayoutOption[]
			{
				GUILayout.Width(49f)
			});
			EditorGUILayout.LabelField("W", new GUILayoutOption[]
			{
				GUILayout.Width(14f)
			});
			value.w = EditorGUILayout.FloatField(value.w, new GUILayoutOption[]
			{
				GUILayout.Width(49f)
			});
			EditorGUILayout.EndHorizontal();
			return value;
		}
	}
}
