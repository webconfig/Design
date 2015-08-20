using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
    /// <summary>
    /// 自定义Inspector视图
    /// </summary>
	[CustomEditor(typeof(Behavior))]
    public class BehaviorInspector : UnityEditor.Editor
	{
		private List<Task> onSceneGUITasks;

		public void OnEnable()
		{
			this.FindOnSceneGUITasks();
		}

        //public void OnSceneGUI()
        //{
        //    if (this.onSceneGUITasks != null && this.onSceneGUITasks.Count > 0)
        //    {
        //        for (int i = 0; i < this.onSceneGUITasks.Count; i++)
        //        {
        //            if (this.onSceneGUITasks[i] == null)
        //            {
        //                this.FindOnSceneGUITasks();
        //                return;
        //            }
        //            this.onSceneGUITasks[i].OnSceneGUI();
        //        }
        //    }
        //}

        private void FindOnSceneGUITasks()
        {
            BehaviorSource behaviorSource = (base.target as Behavior).GetBehaviorSource();
            //if (behaviorSource.RootTask != null)
            //{
                this.onSceneGUITasks = new List<Task>();
                Type[] types = Assembly.GetAssembly(typeof(Task)).GetTypes();
                for (int i = 0; i < types.Length; i++)
                {
                    MethodInfo method;
                    if (types[i].IsSubclassOf(typeof(Task)) && (method = types[i].GetMethod("OnSceneGUI")) != null && method.DeclaringType == types[i])
                    {

                        //this.AddTasksOfType(behaviorSource.RootTask, types[i], ref this.onSceneGUITasks);

                        if (behaviorSource.DetachedTasks != null)
                        {
                            for (int j = 0; j < behaviorSource.DetachedTasks.Count; j++)
                            {
                                this.AddTasksOfType(behaviorSource.DetachedTasks[j], types[i], ref this.onSceneGUITasks);
                            }
                        }
                    }
                }
            //}
        }

        private void AddTasksOfType(Task task, Type type, ref List<Task> taskList)
        {
            if (task == null)
            {
                return;
            }
            if (task.GetType().Equals(type))
            {
                taskList.Add(task);
            }

            if (task.Children != null)
            {
                for (int i = 0; i < task.Children.Count; i++)
                {
                    this.AddTasksOfType(task.Children[i], type, ref taskList);
                }
            }

        }
        /// <summary>
        /// 绘制自定义Inspector视图
        /// </summary>
		public override void OnInspectorGUI()
		{
            //Behavior behavior = base.target as Behavior;
            //if (behavior == null)
            //{
            //    return;
            //}
            //behavior.showBehaviorDesignerGizmo = BehaviorDesignerPreferences.GetBool(BDPreferneces.ShowSceneIcon);
            //if (BehaviorInspector.DrawInspectorGUI(behavior, base.serializedObject))
            //{
            //    EditorUtility.SetDirty(behavior);//Unity将会知道对象的内容被修改了，需要重新序列化并保存到磁盘上
            //    if (BehaviorDesignerWindow.instance != null && behavior.GetBehaviorSource().BehaviorID == BehaviorDesignerWindow.instance.ActiveBehaviorID)
            //    {
            //        BehaviorDesignerWindow.instance.UpdateGraphStatus();
            //    }
            //}
            //if (GUILayout.Button("Open Behavior Designer", new GUILayoutOption[0]))
            //{//弹出行为树设计窗口
            //    BehaviorDesignerWindow.ShowWindow();

            //    //Debug.Log(" BehaviorDesignerWindow.instance:" + BehaviorDesignerWindow.instance);

            //    BehaviorDesignerWindow.instance.LoadBehavior(behavior.GetBehaviorSource(), false, false, true, true);
            //}
		}

        //public static bool DrawInspectorGUI(Behavior behavior, SerializedObject serializedObject)
        //{
        //    serializedObject.Update();
        //    EditorGUI.BeginChangeCheck();
        //    GUILayout.BeginHorizontal(new GUILayoutOption[0]);
        //    EditorGUILayout.LabelField("Behavior Name", new GUILayoutOption[]
        //    {
        //        GUILayout.Width(120f)
        //    });
        //    behavior.GetBehaviorSource().behaviorName = EditorGUILayout.TextField(behavior.GetBehaviorSource().behaviorName, new GUILayoutOption[0]);
        //    GUILayout.EndHorizontal();
        //    EditorGUILayout.LabelField("Behavior Description", new GUILayoutOption[0]);
        //    behavior.GetBehaviorSource().behaviorDescription = EditorGUILayout.TextArea(behavior.GetBehaviorSource().behaviorDescription, BehaviorDesignerUtility.TaskInspectorCommentGUIStyle, new GUILayoutOption[]
        //    {
        //        GUILayout.Height(48f)
        //    });
        //    SerializedProperty serializedProperty = serializedObject.FindProperty("group");
        //    EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
        //    serializedProperty = serializedObject.FindProperty("startWhenEnabled");
        //    EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
        //    serializedProperty = serializedObject.FindProperty("pauseWhenDisabled");
        //    EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
        //    serializedProperty = serializedObject.FindProperty("restartWhenComplete");
        //    EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
        //    serializedProperty = serializedObject.FindProperty("logTaskChanges");
        //    EditorGUILayout.PropertyField(serializedProperty, true, new GUILayoutOption[0]);
        //    if (EditorGUI.EndChangeCheck())
        //    {
        //        serializedObject.ApplyModifiedProperties();
        //        return true;
        //    }
        //    return false;
        //}

        //public void Reset()
        //{
        //    Behavior behavior = base.target as Behavior;
        //    if (behavior.UpdateDeprecatedTasks())
        //    {
        //        EditorUtility.SetDirty(behavior);
        //        Debug.Log(string.Format("Successfully updated {0} to most current data format.", behavior.ToString()));
        //    }
        //    TaskCopier.CheckTasks(behavior);
        //}
	}
}
