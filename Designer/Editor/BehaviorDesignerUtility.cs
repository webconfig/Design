using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MiniJSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public static class BehaviorDesignerUtility
	{
		public static readonly int ToolBarHeight = 18;

		public static readonly int PropertyBoxWidth = 320;

		public static readonly int ScrollBarSize = 15;

		public static readonly int EditorWindowTabHeight = 21;

		public static readonly int PreferencesPaneWidth = 290;

		public static readonly int PreferencesPaneHeight = 208;

		public static readonly float GraphZoomMax = 1f;

		public static readonly float GraphZoomMin = 0.4f;

		public static readonly float GraphZoomSensitivity = 150f;

		public static readonly int LineSelectionThreshold = 7;

		public static readonly int TaskBackgroundShadowSize = 3;

		public static readonly int TitleHeight = 20;

		public static readonly int IconAreaHeight = 52;

		public static readonly int IconSize = 44;

		public static readonly int IconBorderSize = 46;

		public static readonly int ConnectionWidth = 42;

		public static readonly int TopConnectionHeight = 14;

		public static readonly int BottomConnectionHeight = 16;

		public static readonly int TaskConnectionCollapsedWidth = 26;

		public static readonly int TaskConnectionCollapsedHeight = 6;

		public static readonly int MinWidth = 100;

		public static readonly int MaxWidth = 220;

		public static readonly int MaxCommentHeight = 100;

		public static readonly int TextPadding = 20;

		public static readonly float NodeFadeDuration = 0.5f;

		public static readonly int IdentifyUpdateFadeTime = 500;

		public static readonly int MaxIdentifyUpdateCount = 2000;

		public static readonly int TaskPropertiesLabelWidth = 150;

		public static readonly int MaxTaskDescriptionBoxWidth = 400;

		public static readonly int MaxTaskDescriptionBoxHeight = 300;

		private static GUIStyle graphStatusGUIStyle = null;

		private static GUIStyle taskFoldoutGUIStyle = null;

		private static GUIStyle taskTitleGUIStyle = null;

		private static GUIStyle taskGUIStyle = null;

		private static GUIStyle taskSelectedGUIStyle = null;

		private static GUIStyle taskRunningGUIStyle = null;

		private static GUIStyle taskRunningSelectedGUIStyle = null;

		private static GUIStyle taskIdentifyGUIStyle = null;

		private static GUIStyle taskIdentifySelectedGUIStyle = null;

		private static GUIStyle taskCommentGUIStyle = null;

		private static GUIStyle taskCommentLeftAlignGUIStyle = null;

		private static GUIStyle taskCommentRightAlignGUIStyle = null;

		private static GUIStyle graphBackgroundGUIStyle = null;

		private static GUIStyle selectionGUIStyle = null;

		private static GUIStyle labelWrapGUIStyle = null;

		private static GUIStyle taskInspectorCommentGUIStyle = null;

		private static GUIStyle taskInspectorGUIStyle = null;

		private static GUIStyle toolbarButtonSelectionGUIStyle = null;

		private static GUIStyle propertyBoxGUIStyle = null;

		private static GUIStyle preferencesPaneGUIStyle = null;

		private static GUIStyle plainButtonGUIStyle = null;

		private static GUIStyle transparentButtonGUIStyle = null;

		private static GUIStyle welcomeScreenIntroGUIStyle = null;

		private static GUIStyle welcomeScreenTextHeaderGUIStyle = null;

		private static GUIStyle welcomeScreenTextDescriptionGUIStyle = null;

		private static Texture2D taskBorderTexture = null;

		private static Texture2D taskBorderRunningTexture = null;

		private static Texture2D taskBorderIdentifyTexture = null;

		private static Texture2D taskConnectionTexture = null;

		private static Texture2D taskConnectionTopTexture = null;

		private static Texture2D taskConnectionBottomTexture = null;

		private static Texture2D taskConnectionRunningTopTexture = null;

		private static Texture2D taskConnectionRunningBottomTexture = null;

		private static Texture2D taskConnectionIdentifyTopTexture = null;

		private static Texture2D taskConnectionIdentifyBottomTexture = null;

		private static Texture2D taskConnectionCollapsedTexture = null;

		private static Texture2D contentSeparatorTexture = null;

		private static Texture2D docTexture = null;

		private static Texture2D gearTexture = null;

		private static Texture2D syncedTexture = null;

		private static Texture2D sharedTexture = null;

		private static Texture2D variableButtonTexture = null;

		private static Texture2D variableButtonSelectedTexture = null;

		private static Texture2D variableWatchButtonTexture = null;

		private static Texture2D variableWatchButtonSelectedTexture = null;

		private static Texture2D referencedTexture = null;

		private static Texture2D deleteButtonTexture = null;

		private static Texture2D identifyButtonTexture = null;

		private static Texture2D breakpointTexture = null;

		private static Texture2D enableTaskTexture = null;

		private static Texture2D disableTaskTexture = null;

		private static Texture2D expandTaskTexture = null;

		private static Texture2D collapseTaskTexture = null;

		private static Texture2D executionSuccessTexture = null;

		private static Texture2D executionFailureTexture = null;

		public static GUIStyle GraphStatusGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.graphStatusGUIStyle == null)
				{
					BehaviorDesignerUtility.initGraphStatusGUIStyle();
				}
				return BehaviorDesignerUtility.graphStatusGUIStyle;
			}
		}

		public static GUIStyle TaskFoldoutGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskFoldoutGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskFoldoutGUIStyle();
				}
				return BehaviorDesignerUtility.taskFoldoutGUIStyle;
			}
		}

		public static GUIStyle TaskTitleGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskTitleGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskTitleGUIStyle();
				}
				return BehaviorDesignerUtility.taskTitleGUIStyle;
			}
		}

		public static GUIStyle TaskGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskGUIStyle();
				}
				return BehaviorDesignerUtility.taskGUIStyle;
			}
		}

		public static GUIStyle TaskSelectedGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskSelectedGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskSelectedGUIStyle();
				}
				return BehaviorDesignerUtility.taskSelectedGUIStyle;
			}
		}

		public static GUIStyle TaskRunningGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskRunningGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskRunningGUIStyle();
				}
				return BehaviorDesignerUtility.taskRunningGUIStyle;
			}
		}

		public static GUIStyle TaskRunningSelectedGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskRunningSelectedGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskRunningSelectedGUIStyle();
				}
				return BehaviorDesignerUtility.taskRunningSelectedGUIStyle;
			}
		}

		public static GUIStyle TaskIdentifyGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskIdentifyGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskIdentifyGUIStyle();
				}
				return BehaviorDesignerUtility.taskIdentifyGUIStyle;
			}
		}

		public static GUIStyle TaskIdentifySelectedGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskIdentifySelectedGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskIdentifySelectedGUIStyle();
				}
				return BehaviorDesignerUtility.taskIdentifySelectedGUIStyle;
			}
		}

		public static GUIStyle TaskCommentGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskCommentGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskCommentGUIStyle();
				}
				return BehaviorDesignerUtility.taskCommentGUIStyle;
			}
		}

		public static GUIStyle TaskCommentLeftAlignGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskCommentLeftAlignGUIStyle();
				}
				return BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle;
			}
		}

		public static GUIStyle TaskCommentRightAlignGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskCommentRightAlignGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskCommentRightAlignGUIStyle();
				}
				return BehaviorDesignerUtility.taskCommentRightAlignGUIStyle;
			}
		}

		public static GUIStyle GraphBackgroundGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.graphBackgroundGUIStyle == null)
				{
					BehaviorDesignerUtility.initGraphBackgroundGUIStyle();
				}
				return BehaviorDesignerUtility.graphBackgroundGUIStyle;
			}
		}

		public static GUIStyle SelectionGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.selectionGUIStyle == null)
				{
					BehaviorDesignerUtility.initSelectionGUIStyle();
				}
				return BehaviorDesignerUtility.selectionGUIStyle;
			}
		}

		public static GUIStyle LabelWrapGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.labelWrapGUIStyle == null)
				{
					BehaviorDesignerUtility.initLabelWrapGUIStyle();
				}
				return BehaviorDesignerUtility.labelWrapGUIStyle;
			}
		}

		public static GUIStyle TaskInspectorCommentGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskInspectorCommentGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskInspectorCommentGUIStyle();
				}
				return BehaviorDesignerUtility.taskInspectorCommentGUIStyle;
			}
		}

		public static GUIStyle TaskInspectorGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.taskInspectorGUIStyle == null)
				{
					BehaviorDesignerUtility.initTaskInspectorGUIStyle();
				}
				return BehaviorDesignerUtility.taskInspectorGUIStyle;
			}
		}

		public static GUIStyle ToolbarButtonSelectionGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle == null)
				{
					BehaviorDesignerUtility.initToolbarButtonSelectionGUIStyle();
				}
				return BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle;
			}
		}

		public static GUIStyle PropertyBoxGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.propertyBoxGUIStyle == null)
				{
					BehaviorDesignerUtility.initPropertyBoxGUIStyle();
				}
				return BehaviorDesignerUtility.propertyBoxGUIStyle;
			}
		}

		public static GUIStyle PreferencesPaneGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.preferencesPaneGUIStyle == null)
				{
					BehaviorDesignerUtility.initPreferencesPaneGUIStyle();
				}
				return BehaviorDesignerUtility.preferencesPaneGUIStyle;
			}
		}

		public static GUIStyle PlainButtonGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.plainButtonGUIStyle == null)
				{
					BehaviorDesignerUtility.initPlainButtonGUIStyle();
				}
				return BehaviorDesignerUtility.plainButtonGUIStyle;
			}
		}

		public static GUIStyle TransparentButtonGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.transparentButtonGUIStyle == null)
				{
					BehaviorDesignerUtility.initTransparentButtonGUIStyle();
				}
				return BehaviorDesignerUtility.transparentButtonGUIStyle;
			}
		}

		public static GUIStyle WelcomeScreenIntroGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.welcomeScreenIntroGUIStyle == null)
				{
					BehaviorDesignerUtility.initWelcomeScreenIntroGUIStyle();
				}
				return BehaviorDesignerUtility.welcomeScreenIntroGUIStyle;
			}
		}

		public static GUIStyle WelcomeScreenTextHeaderGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle == null)
				{
					BehaviorDesignerUtility.initWelcomeScreenTextHeaderGUIStyle();
				}
				return BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle;
			}
		}

		public static GUIStyle WelcomeScreenTextDescriptionGUIStyle
		{
			get
			{
				if (BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle == null)
				{
					BehaviorDesignerUtility.initWelcomeScreenTextDescriptionGUIStyle();
				}
				return BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle;
			}
		}

		public static Texture2D TaskBorderTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskBorderTexture == null)
				{
					BehaviorDesignerUtility.initTaskBorderTexture();
				}
				return BehaviorDesignerUtility.taskBorderTexture;
			}
		}

		public static Texture2D TaskBorderRunningTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskBorderRunningTexture == null)
				{
					BehaviorDesignerUtility.initTaskBorderRunningTexture();
				}
				return BehaviorDesignerUtility.taskBorderRunningTexture;
			}
		}

		public static Texture2D TaskBorderIdentifyTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskBorderIdentifyTexture == null)
				{
					BehaviorDesignerUtility.initTaskBorderIdentifyTexture();
				}
				return BehaviorDesignerUtility.taskBorderIdentifyTexture;
			}
		}

		public static Texture2D TaskConnectionTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionTexture == null)
				{
					BehaviorDesignerUtility.initTaskConnectionTexture();
				}
				return BehaviorDesignerUtility.taskConnectionTexture;
			}
		}

		public static Texture2D TaskConnectionTopTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionTopTexture == null)
				{
					BehaviorDesignerUtility.initTaskConnectionTopTexture();
				}
				return BehaviorDesignerUtility.taskConnectionTopTexture;
			}
		}

		public static Texture2D TaskConnectionBottomTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionBottomTexture == null)
				{
					BehaviorDesignerUtility.initTaskConnectionBottomTexture();
				}
				return BehaviorDesignerUtility.taskConnectionBottomTexture;
			}
		}

		public static Texture2D TaskConnectionRunningTopTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionRunningTopTexture == null)
				{
					BehaviorDesignerUtility.initTaskConnectionRunningTopTexture();
				}
				return BehaviorDesignerUtility.taskConnectionRunningTopTexture;
			}
		}

		public static Texture2D TaskConnectionRunningBottomTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionRunningBottomTexture == null)
				{
					BehaviorDesignerUtility.initTaskConnectionRunningBottomTexture();
				}
				return BehaviorDesignerUtility.taskConnectionRunningBottomTexture;
			}
		}

		public static Texture2D TaskConnectionIdentifyTopTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionIdentifyTopTexture == null)
				{
					BehaviorDesignerUtility.initTaskConnectionIdentifyTopTexture();
				}
				return BehaviorDesignerUtility.taskConnectionIdentifyTopTexture;
			}
		}

		public static Texture2D TaskConnectionIdentifyBottomTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionIdentifyBottomTexture == null)
				{
					BehaviorDesignerUtility.initTaskConnectionIdentifyBottomTexture();
				}
				return BehaviorDesignerUtility.taskConnectionIdentifyBottomTexture;
			}
		}

		public static Texture2D TaskConnectionCollapsedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.taskConnectionCollapsedTexture == null)
				{
					BehaviorDesignerUtility.initTaskConnectionCollapsedTexture();
				}
				return BehaviorDesignerUtility.taskConnectionCollapsedTexture;
			}
		}

		public static Texture2D ContentSeparatorTexture
		{
			get
			{
				if (BehaviorDesignerUtility.contentSeparatorTexture == null)
				{
					BehaviorDesignerUtility.initContentSeparatorTexture();
				}
				return BehaviorDesignerUtility.contentSeparatorTexture;
			}
		}

		public static Texture2D DocTexture
		{
			get
			{
				if (BehaviorDesignerUtility.docTexture == null)
				{
					BehaviorDesignerUtility.initDocTexture();
				}
				return BehaviorDesignerUtility.docTexture;
			}
		}

		public static Texture2D GearTexture
		{
			get
			{
				if (BehaviorDesignerUtility.gearTexture == null)
				{
					BehaviorDesignerUtility.initGearTexture();
				}
				return BehaviorDesignerUtility.gearTexture;
			}
		}

		public static Texture2D SyncedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.syncedTexture == null)
				{
					BehaviorDesignerUtility.initSyncedTexture();
				}
				return BehaviorDesignerUtility.syncedTexture;
			}
		}

		public static Texture2D SharedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.sharedTexture == null)
				{
					BehaviorDesignerUtility.initSharedTexture();
				}
				return BehaviorDesignerUtility.sharedTexture;
			}
		}

		public static Texture2D VariableButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableButtonTexture == null)
				{
					BehaviorDesignerUtility.initVariableButtonTexture();
				}
				return BehaviorDesignerUtility.variableButtonTexture;
			}
		}

		public static Texture2D VariableButtonSelectedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableButtonSelectedTexture == null)
				{
					BehaviorDesignerUtility.initVariableButtonSelectedTexture();
				}
				return BehaviorDesignerUtility.variableButtonSelectedTexture;
			}
		}

		public static Texture2D VariableWatchButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableWatchButtonTexture == null)
				{
					BehaviorDesignerUtility.initVariableWatchButtonTexture();
				}
				return BehaviorDesignerUtility.variableWatchButtonTexture;
			}
		}

		public static Texture2D VariableWatchButtonSelectedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.variableWatchButtonSelectedTexture == null)
				{
					BehaviorDesignerUtility.initVariableWatchButtonSelectedTexture();
				}
				return BehaviorDesignerUtility.variableWatchButtonSelectedTexture;
			}
		}

		public static Texture2D ReferencedTexture
		{
			get
			{
				if (BehaviorDesignerUtility.referencedTexture == null)
				{
					BehaviorDesignerUtility.initReferencedTexture();
				}
				return BehaviorDesignerUtility.referencedTexture;
			}
		}

		public static Texture2D DeleteButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.deleteButtonTexture == null)
				{
					BehaviorDesignerUtility.initDeleteButtonTexture();
				}
				return BehaviorDesignerUtility.deleteButtonTexture;
			}
		}

		public static Texture2D IdentifyButtonTexture
		{
			get
			{
				if (BehaviorDesignerUtility.identifyButtonTexture == null)
				{
					BehaviorDesignerUtility.initIdentifyButtonTexture();
				}
				return BehaviorDesignerUtility.identifyButtonTexture;
			}
		}

		public static Texture2D BreakpointTexture
		{
			get
			{
				if (BehaviorDesignerUtility.breakpointTexture == null)
				{
					BehaviorDesignerUtility.initBreakpointTexture();
				}
				return BehaviorDesignerUtility.breakpointTexture;
			}
		}

		public static Texture2D EnableTaskTexture
		{
			get
			{
				if (BehaviorDesignerUtility.enableTaskTexture == null)
				{
					BehaviorDesignerUtility.initEnableTaskTexture();
				}
				return BehaviorDesignerUtility.enableTaskTexture;
			}
		}

		public static Texture2D DisableTaskTexture
		{
			get
			{
				if (BehaviorDesignerUtility.disableTaskTexture == null)
				{
					BehaviorDesignerUtility.initDisableTaskTexture();
				}
				return BehaviorDesignerUtility.disableTaskTexture;
			}
		}

		public static Texture2D ExpandTaskTexture
		{
			get
			{
				if (BehaviorDesignerUtility.expandTaskTexture == null)
				{
					BehaviorDesignerUtility.initExpandTaskTexture();
				}
				return BehaviorDesignerUtility.expandTaskTexture;
			}
		}

		public static Texture2D CollapseTaskTexture
		{
			get
			{
				if (BehaviorDesignerUtility.collapseTaskTexture == null)
				{
					BehaviorDesignerUtility.initCollapseTaskTexture();
				}
				return BehaviorDesignerUtility.collapseTaskTexture;
			}
		}

		public static Texture2D ExecutionSuccessTexture
		{
			get
			{
				if (BehaviorDesignerUtility.executionSuccessTexture == null)
				{
					BehaviorDesignerUtility.initExecutionSuccessTexture();
				}
				return BehaviorDesignerUtility.executionSuccessTexture;
			}
		}

		public static Texture2D ExecutionFailureTexture
		{
			get
			{
				if (BehaviorDesignerUtility.executionFailureTexture == null)
				{
					BehaviorDesignerUtility.initExecutionFailureTexture();
				}
				return BehaviorDesignerUtility.executionFailureTexture;
			}
		}

		public static string SplitCamelCase(string s)
		{
			if (s.Equals(""))
			{
				return s;
			}
			s = s.Replace("_uScript", "uScript");
			s = s.Replace("_PlayMaker", "PlayMaker");
			if (s.Length > 2 && s.Substring(0, 2).CompareTo("m_") == 0)
			{
				s = s.Substring(2, s.Length - 2);
			}
			Regex regex = new Regex("(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
			s = regex.Replace(s, " ");
			s = s.Replace("_", " ");
			s = s.Replace("u Script", " uScript");
			s = s.Replace("Play Maker", "PlayMaker");
			return (char.ToUpper(s[0]) + s.Substring(1)).Trim();
		}

		public static List<Task> GetAllTasks(BehaviorSource behaviorSource)
		{
			List<Task> result = new List<Task>();
			if (behaviorSource.RootTask != null)
			{
				BehaviorDesignerUtility.GetAllTasks(behaviorSource.RootTask, ref result);
			}
			if (behaviorSource.DetachedTasks != null)
			{
				for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
				{
					BehaviorDesignerUtility.GetAllTasks(behaviorSource.DetachedTasks[i], ref result);
				}
			}
			return result;
		}

		private static void GetAllTasks(Task task, ref List<Task> taskList)
		{
			taskList.Add(task);
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					BehaviorDesignerUtility.GetAllTasks(parentTask.Children[i], ref taskList);
				}
			}
		}

		public static int TaskCount(BehaviorSource behaviorSource)
		{
			int num = 0;
            //if (behaviorSource.EntryTask != null)
            //{
            //    num++;
            //}
			if (behaviorSource.RootTask != null)
			{
				BehaviorDesignerUtility.TaskCount(behaviorSource.RootTask, ref num);
			}
			if (behaviorSource.DetachedTasks != null)
			{
				for (int i = 0; i < behaviorSource.DetachedTasks.Count; i++)
				{
					BehaviorDesignerUtility.TaskCount(behaviorSource.DetachedTasks[i], ref num);
				}
			}
			return num;
		}

		private static void TaskCount(Task task, ref int count)
		{
			count++;
			ParentTask parentTask;
			if ((parentTask = (task as ParentTask)) != null && parentTask.Children != null)
			{
				for (int i = 0; i < parentTask.Children.Count; i++)
				{
					BehaviorDesignerUtility.TaskCount(parentTask.Children[i], ref count);
				}
			}
		}

		public static int JSONTaskCount(string serialization)
		{
			if (string.IsNullOrEmpty(serialization))
			{
				return -1;
			}
			Dictionary<string, object> dictionary = Json.Deserialize(serialization) as Dictionary<string, object>;
			if (dictionary == null || !dictionary.ContainsKey("TaskCount"))
			{
				return -1;
			}
			return Convert.ToInt32(dictionary["TaskCount"]);
		}

		private static string GetEditorBaseDirectory(ScriptableObject obj = null)
		{
			string codeBase = Assembly.GetExecutingAssembly().CodeBase;
			string text = Uri.UnescapeDataString(new UriBuilder(codeBase).Path);
			return Path.GetDirectoryName(text.Substring(Application.dataPath.Length - 6));
		}

		public static Texture2D LoadTexture(string imageName, bool useSkinColor = true, ScriptableObject obj = null)
		{
			Texture2D texture2D = null;
            imageName = Application.dataPath + @"\" + string.Format(@"\trunk\Designer\Resource\{0}{1}", useSkinColor ? (EditorGUIUtility.isProSkin ? "Dark" : "Light") : "", imageName);
            Stream manifestResourceStream = null;
            try
            {
                manifestResourceStream = File.OpenRead(imageName);//Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName);
            }
            catch
            {
                Debug.Log("文件不存在！");
                return texture2D;
            }
            //Debug.Log(imageName + "----" + (manifestResourceStream != null));

			if (manifestResourceStream != null)
			{
				texture2D = new Texture2D(0, 0);
				texture2D.LoadImage(BehaviorDesignerUtility.ReadToEnd(manifestResourceStream));
				manifestResourceStream.Close();
			}
			if (texture2D != null)
			{
                texture2D.hideFlags = HideFlags.HideAndDontSave;
			}
			return texture2D;
		}

		public static Texture2D LoadIcon(string iconName, ScriptableObject obj = null)
		{
			Texture2D texture2D = null;
			Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("BehaviorDesignerEditor.Resources.{0}", iconName.Replace("{SkinColor}", EditorGUIUtility.isProSkin ? "Dark" : "Light")));
			if (manifestResourceStream != null)
			{
				texture2D = new Texture2D(0, 0);
				texture2D.LoadImage(BehaviorDesignerUtility.ReadToEnd(manifestResourceStream));
				manifestResourceStream.Close();
			}
			if (texture2D == null)
			{
				texture2D = (Resources.LoadAssetAtPath(iconName.Replace("{SkinColor}", EditorGUIUtility.isProSkin ? "Dark" : "Light"), typeof(Texture2D)) as Texture2D);
			}
			if (texture2D != null)
			{
                texture2D.hideFlags = HideFlags.HideAndDontSave;
			}
			return texture2D;
		}

		private static byte[] ReadToEnd(Stream stream)
		{
			byte[] array = new byte[16384];
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				int count;
				while ((count = stream.Read(array, 0, array.Length)) > 0)
				{
					memoryStream.Write(array, 0, count);
				}
				result = memoryStream.ToArray();
			}
			return result;
		}

		public static void DrawContentSeperator(int yOffset)
		{
			Rect lastRect = GUILayoutUtility.GetLastRect();
			lastRect.x=-5f;
			lastRect.y=lastRect.y + (lastRect.height + (float)yOffset);
			lastRect.height=2f;
			lastRect.width=lastRect.width + 10f;
			GUI.DrawTexture(lastRect, BehaviorDesignerUtility.ContentSeparatorTexture);
		}

		private static void initGraphStatusGUIStyle()
		{
			BehaviorDesignerUtility.graphStatusGUIStyle = new GUIStyle(GUI.skin.label);
            BehaviorDesignerUtility.graphStatusGUIStyle.alignment = TextAnchor.MiddleLeft;
			BehaviorDesignerUtility.graphStatusGUIStyle.fontSize=20;
            BehaviorDesignerUtility.graphStatusGUIStyle.fontStyle = FontStyle.Bold;
			if (EditorGUIUtility.isProSkin)
			{
				BehaviorDesignerUtility.graphStatusGUIStyle.normal.textColor=new Color(0.7058f, 0.7058f, 0.7058f);
				return;
			}
			BehaviorDesignerUtility.graphStatusGUIStyle.normal.textColor=new Color(0.8058f, 0.8058f, 0.8058f);
		}

		private static void initTaskFoldoutGUIStyle()
		{
			BehaviorDesignerUtility.taskFoldoutGUIStyle = new GUIStyle(EditorStyles.foldout);
            BehaviorDesignerUtility.taskFoldoutGUIStyle.alignment = TextAnchor.MiddleLeft;
			BehaviorDesignerUtility.taskFoldoutGUIStyle.fontSize=15;
            BehaviorDesignerUtility.taskFoldoutGUIStyle.fontStyle = FontStyle.Bold;
		}

		private static void initTaskTitleGUIStyle()
		{
			BehaviorDesignerUtility.taskTitleGUIStyle = new GUIStyle(GUI.skin.label);
            BehaviorDesignerUtility.taskTitleGUIStyle.alignment = TextAnchor.UpperCenter;
			BehaviorDesignerUtility.taskTitleGUIStyle.fontSize=12;
            BehaviorDesignerUtility.taskTitleGUIStyle.fontStyle = FontStyle.Normal;
		}

		private static void initTaskGUIStyle()
		{
			BehaviorDesignerUtility.taskGUIStyle = BehaviorDesignerUtility.initTaskGUIStyle(BehaviorDesignerUtility.LoadTexture("Task.png", true, null), new RectOffset(5, 3, 3, 5));
		}

		private static void initTaskSelectedGUIStyle()
		{
			BehaviorDesignerUtility.taskSelectedGUIStyle = BehaviorDesignerUtility.initTaskGUIStyle(BehaviorDesignerUtility.LoadTexture("TaskSelected.png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static void initTaskRunningGUIStyle()
		{
			BehaviorDesignerUtility.taskRunningGUIStyle = BehaviorDesignerUtility.initTaskGUIStyle(BehaviorDesignerUtility.LoadTexture("TaskRunning.png", true, null), new RectOffset(5, 3, 3, 5));
		}

		private static void initTaskRunningSelectedGUIStyle()
		{
			BehaviorDesignerUtility.taskRunningSelectedGUIStyle = BehaviorDesignerUtility.initTaskGUIStyle(BehaviorDesignerUtility.LoadTexture("TaskRunningSelected.png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static void initTaskIdentifyGUIStyle()
		{
			BehaviorDesignerUtility.taskIdentifyGUIStyle = BehaviorDesignerUtility.initTaskGUIStyle(BehaviorDesignerUtility.LoadTexture("TaskIdentify.png", true, null), new RectOffset(5, 3, 3, 5));
		}

		private static void initTaskIdentifySelectedGUIStyle()
		{
			BehaviorDesignerUtility.taskIdentifySelectedGUIStyle = BehaviorDesignerUtility.initTaskGUIStyle(BehaviorDesignerUtility.LoadTexture("TaskIdentifySelected.png", true, null), new RectOffset(5, 4, 4, 4));
		}

		private static GUIStyle initTaskGUIStyle(Texture2D texture, RectOffset overflow)
		{
			GUIStyle gUIStyle = new GUIStyle(GUI.skin.box);
			gUIStyle.border=new RectOffset(10, 10, 10, 10);
			gUIStyle.overflow=overflow;
			gUIStyle.normal.background=texture;
			gUIStyle.active.background=texture;
			gUIStyle.hover.background=texture;
			gUIStyle.focused.background=texture;
			gUIStyle.normal.textColor=Color.white;
			gUIStyle.active.textColor=Color.white;
			gUIStyle.hover.textColor=Color.white;
			gUIStyle.focused.textColor=Color.white;
			gUIStyle.stretchHeight=true;
			gUIStyle.stretchWidth=true;
			return gUIStyle;
		}

		private static void initTaskCommentGUIStyle()
		{
			BehaviorDesignerUtility.taskCommentGUIStyle = new GUIStyle(GUI.skin.label);
            BehaviorDesignerUtility.taskCommentGUIStyle.alignment = TextAnchor.UpperCenter;
			BehaviorDesignerUtility.taskCommentGUIStyle.fontSize=12;
			BehaviorDesignerUtility.taskCommentGUIStyle.fontStyle=0;
			BehaviorDesignerUtility.taskCommentGUIStyle.wordWrap=true;
		}

		private static void initTaskCommentLeftAlignGUIStyle()
		{
			BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle = new GUIStyle(GUI.skin.label);
            BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.alignment = TextAnchor.UpperLeft;
			BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.fontSize=12;
			BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.fontStyle=0;
			BehaviorDesignerUtility.taskCommentLeftAlignGUIStyle.wordWrap=false;
		}

		private static void initTaskCommentRightAlignGUIStyle()
		{
			BehaviorDesignerUtility.taskCommentRightAlignGUIStyle = new GUIStyle(GUI.skin.label);
            BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.alignment = TextAnchor.UpperRight;
			BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.fontSize=12;
			BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.fontStyle=0;
			BehaviorDesignerUtility.taskCommentRightAlignGUIStyle.wordWrap=false;
		}

		private static void initGraphBackgroundGUIStyle()
		{
			Texture2D texture2D = new Texture2D(1, 1);
			if (EditorGUIUtility.isProSkin)
			{
				texture2D.SetPixel(1, 1, new Color(0.1333f, 0.1333f, 0.1333f));
			}
			else
			{
				texture2D.SetPixel(1, 1, new Color(0.3647f, 0.3647f, 0.3647f));
			}
            texture2D.hideFlags = HideFlags.HideAndDontSave;
			texture2D.Apply();
			BehaviorDesignerUtility.graphBackgroundGUIStyle = new GUIStyle(GUI.skin.box);
			BehaviorDesignerUtility.graphBackgroundGUIStyle.normal.background=texture2D;
			BehaviorDesignerUtility.graphBackgroundGUIStyle.active.background=texture2D;
			BehaviorDesignerUtility.graphBackgroundGUIStyle.hover.background=texture2D;
			BehaviorDesignerUtility.graphBackgroundGUIStyle.focused.background=texture2D;
			BehaviorDesignerUtility.graphBackgroundGUIStyle.normal.textColor=Color.white;
			BehaviorDesignerUtility.graphBackgroundGUIStyle.active.textColor=Color.white;
			BehaviorDesignerUtility.graphBackgroundGUIStyle.hover.textColor=Color.white;
			BehaviorDesignerUtility.graphBackgroundGUIStyle.focused.textColor=Color.white;
		}

		private static void initSelectionGUIStyle()
		{
			Texture2D texture2D = new Texture2D(1, 1);
			Color color;
			if (EditorGUIUtility.isProSkin)
			{
				color = new Color(0.188f, 0.4588f, 0.6862f, 0.5f);
			}
			else
			{
				color = new Color(0.243f, 0.5686f, 0.839f, 0.5f);
			}
			texture2D.SetPixel(1, 1, color);
            texture2D.hideFlags = HideFlags.HideAndDontSave;
			texture2D.Apply();
			BehaviorDesignerUtility.selectionGUIStyle = new GUIStyle(GUI.skin.box);
			BehaviorDesignerUtility.selectionGUIStyle.normal.background=texture2D;
			BehaviorDesignerUtility.selectionGUIStyle.active.background=texture2D;
			BehaviorDesignerUtility.selectionGUIStyle.hover.background=texture2D;
			BehaviorDesignerUtility.selectionGUIStyle.focused.background=texture2D;
			BehaviorDesignerUtility.selectionGUIStyle.normal.textColor=Color.white;
			BehaviorDesignerUtility.selectionGUIStyle.active.textColor=Color.white;
			BehaviorDesignerUtility.selectionGUIStyle.hover.textColor=Color.white;
			BehaviorDesignerUtility.selectionGUIStyle.focused.textColor=Color.white;
		}

		private static void initLabelWrapGUIStyle()
		{
			BehaviorDesignerUtility.labelWrapGUIStyle = new GUIStyle(GUI.skin.label);
			BehaviorDesignerUtility.labelWrapGUIStyle.wordWrap=true;
            BehaviorDesignerUtility.labelWrapGUIStyle.alignment = TextAnchor.MiddleCenter;
		}

		private static void initTaskInspectorCommentGUIStyle()
		{
			BehaviorDesignerUtility.taskInspectorCommentGUIStyle = new GUIStyle(GUI.skin.textArea);
			BehaviorDesignerUtility.taskInspectorCommentGUIStyle.wordWrap=true;
		}

		private static void initTaskInspectorGUIStyle()
		{
			BehaviorDesignerUtility.taskInspectorGUIStyle = new GUIStyle(GUI.skin.label);
            BehaviorDesignerUtility.taskInspectorGUIStyle.alignment = TextAnchor.MiddleLeft;
			BehaviorDesignerUtility.taskInspectorGUIStyle.fontSize=11;
			BehaviorDesignerUtility.taskInspectorGUIStyle.fontStyle=0;
		}

		private static void initToolbarButtonSelectionGUIStyle()
		{
			BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle = new GUIStyle(EditorStyles.toolbarButton);
			BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle.normal.background=BehaviorDesignerUtility.toolbarButtonSelectionGUIStyle.active.background;
		}

		private static void initPreferencesPaneGUIStyle()
		{
			BehaviorDesignerUtility.preferencesPaneGUIStyle = new GUIStyle(GUI.skin.box);
			BehaviorDesignerUtility.preferencesPaneGUIStyle.normal.background=EditorStyles.toolbarButton.normal.background;
		}

		private static void initPropertyBoxGUIStyle()
		{
			BehaviorDesignerUtility.propertyBoxGUIStyle = new GUIStyle();
			BehaviorDesignerUtility.propertyBoxGUIStyle.padding=new RectOffset(2, 2, 0, 0);
		}

		private static void initPlainButtonGUIStyle()
		{
			BehaviorDesignerUtility.plainButtonGUIStyle = new GUIStyle(GUI.skin.button);
			BehaviorDesignerUtility.plainButtonGUIStyle.border=new RectOffset(0, 0, 0, 0);
			BehaviorDesignerUtility.plainButtonGUIStyle.margin=new RectOffset(0, 0, 2, 2);
			BehaviorDesignerUtility.plainButtonGUIStyle.padding=new RectOffset(0, 0, 1, 0);
			BehaviorDesignerUtility.plainButtonGUIStyle.normal.background=null;
			BehaviorDesignerUtility.plainButtonGUIStyle.active.background=null;
			BehaviorDesignerUtility.plainButtonGUIStyle.hover.background=null;
			BehaviorDesignerUtility.plainButtonGUIStyle.focused.background=null;
			BehaviorDesignerUtility.plainButtonGUIStyle.normal.textColor=Color.white;
			BehaviorDesignerUtility.plainButtonGUIStyle.active.textColor=Color.white;
			BehaviorDesignerUtility.plainButtonGUIStyle.hover.textColor=Color.white;
			BehaviorDesignerUtility.plainButtonGUIStyle.focused.textColor=Color.white;
		}

		private static void initTransparentButtonGUIStyle()
		{
			BehaviorDesignerUtility.transparentButtonGUIStyle = new GUIStyle(GUI.skin.button);
			BehaviorDesignerUtility.transparentButtonGUIStyle.border=new RectOffset(0, 0, 0, 0);
			BehaviorDesignerUtility.transparentButtonGUIStyle.margin=new RectOffset(4, 4, 2, 2);
			BehaviorDesignerUtility.transparentButtonGUIStyle.padding=new RectOffset(2, 2, 1, 0);
			BehaviorDesignerUtility.transparentButtonGUIStyle.normal.background=null;
			BehaviorDesignerUtility.transparentButtonGUIStyle.active.background=null;
			BehaviorDesignerUtility.transparentButtonGUIStyle.hover.background=null;
			BehaviorDesignerUtility.transparentButtonGUIStyle.focused.background=null;
			BehaviorDesignerUtility.transparentButtonGUIStyle.normal.textColor=Color.white;
			BehaviorDesignerUtility.transparentButtonGUIStyle.active.textColor=Color.white;
			BehaviorDesignerUtility.transparentButtonGUIStyle.hover.textColor=Color.white;
			BehaviorDesignerUtility.transparentButtonGUIStyle.focused.textColor=Color.white;
		}

		private static void initWelcomeScreenIntroGUIStyle()
		{
			BehaviorDesignerUtility.welcomeScreenIntroGUIStyle = new GUIStyle(GUI.skin.label);
			BehaviorDesignerUtility.welcomeScreenIntroGUIStyle.fontSize=16;
            BehaviorDesignerUtility.welcomeScreenIntroGUIStyle.fontStyle = FontStyle.Bold;
			BehaviorDesignerUtility.welcomeScreenIntroGUIStyle.normal.textColor=new Color(0.706f, 0.706f, 0.706f);
		}

		private static void initWelcomeScreenTextHeaderGUIStyle()
		{
			BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle = new GUIStyle(GUI.skin.label);
            BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle.alignment = TextAnchor.MiddleLeft;
			BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle.fontSize=14;
            BehaviorDesignerUtility.welcomeScreenTextHeaderGUIStyle.fontStyle = FontStyle.Bold;
		}

		private static void initWelcomeScreenTextDescriptionGUIStyle()
		{
			BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle = new GUIStyle(GUI.skin.label);
			BehaviorDesignerUtility.welcomeScreenTextDescriptionGUIStyle.wordWrap=true;
		}

		private static void initTaskBorderTexture()
		{
			BehaviorDesignerUtility.taskBorderTexture = BehaviorDesignerUtility.LoadTexture("TaskBorder.png", true, null);
		}

		private static void initTaskBorderRunningTexture()
		{
			BehaviorDesignerUtility.taskBorderRunningTexture = BehaviorDesignerUtility.LoadTexture("TaskBorderRunning.png", true, null);
		}

		private static void initTaskBorderIdentifyTexture()
		{
			BehaviorDesignerUtility.taskBorderIdentifyTexture = BehaviorDesignerUtility.LoadTexture("TaskBorderIdentify.png", true, null);
		}

		private static void initTaskConnectionTexture()
		{
			BehaviorDesignerUtility.taskConnectionTexture = BehaviorDesignerUtility.LoadTexture("TaskConnection.png", false, null);
		}

		private static void initTaskConnectionTopTexture()
		{
			BehaviorDesignerUtility.taskConnectionTopTexture = BehaviorDesignerUtility.LoadTexture("TaskConnectionTop.png", true, null);
		}

		private static void initTaskConnectionBottomTexture()
		{
			BehaviorDesignerUtility.taskConnectionBottomTexture = BehaviorDesignerUtility.LoadTexture("TaskConnectionBottom.png", true, null);
		}

		private static void initTaskConnectionRunningTopTexture()
		{
			BehaviorDesignerUtility.taskConnectionRunningTopTexture = BehaviorDesignerUtility.LoadTexture("TaskConnectionRunningTop.png", true, null);
		}

		private static void initTaskConnectionRunningBottomTexture()
		{
			BehaviorDesignerUtility.taskConnectionRunningBottomTexture = BehaviorDesignerUtility.LoadTexture("TaskConnectionRunningBottom.png", true, null);
		}

		private static void initTaskConnectionIdentifyTopTexture()
		{
			BehaviorDesignerUtility.taskConnectionIdentifyTopTexture = BehaviorDesignerUtility.LoadTexture("TaskConnectionIdentifyTop.png", true, null);
		}

		private static void initTaskConnectionIdentifyBottomTexture()
		{
			BehaviorDesignerUtility.taskConnectionIdentifyBottomTexture = BehaviorDesignerUtility.LoadTexture("TaskConnectionIdentifyBottom.png", true, null);
		}

		private static void initTaskConnectionCollapsedTexture()
		{
			BehaviorDesignerUtility.taskConnectionCollapsedTexture = BehaviorDesignerUtility.LoadTexture("TaskConnectionCollapsed.png", true, null);
		}

		private static void initContentSeparatorTexture()
		{
			BehaviorDesignerUtility.contentSeparatorTexture = BehaviorDesignerUtility.LoadTexture("ContentSeparator.png", true, null);
		}

		private static void initDocTexture()
		{
			BehaviorDesignerUtility.docTexture = BehaviorDesignerUtility.LoadTexture("DocIcon.png", true, null);
		}

		private static void initGearTexture()
		{
			BehaviorDesignerUtility.gearTexture = BehaviorDesignerUtility.LoadTexture("GearIcon.png", true, null);
		}

		private static void initSyncedTexture()
		{
			BehaviorDesignerUtility.syncedTexture = BehaviorDesignerUtility.LoadTexture("SyncedIcon.png", true, null);
		}

		private static void initSharedTexture()
		{
			BehaviorDesignerUtility.sharedTexture = BehaviorDesignerUtility.LoadTexture("SharedIcon.png", true, null);
		}

		private static void initVariableButtonTexture()
		{
			BehaviorDesignerUtility.variableButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableButton.png", true, null);
		}

		private static void initVariableButtonSelectedTexture()
		{
			BehaviorDesignerUtility.variableButtonSelectedTexture = BehaviorDesignerUtility.LoadTexture("VariableButtonSelected.png", true, null);
		}

		private static void initVariableWatchButtonTexture()
		{
			BehaviorDesignerUtility.variableWatchButtonTexture = BehaviorDesignerUtility.LoadTexture("VariableWatchButton.png", true, null);
		}

		private static void initVariableWatchButtonSelectedTexture()
		{
			BehaviorDesignerUtility.variableWatchButtonSelectedTexture = BehaviorDesignerUtility.LoadTexture("VariableWatchButtonSelected.png", true, null);
		}

		private static void initReferencedTexture()
		{
			BehaviorDesignerUtility.referencedTexture = BehaviorDesignerUtility.LoadTexture("LinkedIcon.png", true, null);
		}

		private static void initDeleteButtonTexture()
		{
			BehaviorDesignerUtility.deleteButtonTexture = BehaviorDesignerUtility.LoadTexture("DeleteButton.png", true, null);
		}

		private static void initIdentifyButtonTexture()
		{
			BehaviorDesignerUtility.identifyButtonTexture = BehaviorDesignerUtility.LoadTexture("IdentifyButton.png", true, null);
		}

		private static void initBreakpointTexture()
		{
			BehaviorDesignerUtility.breakpointTexture = BehaviorDesignerUtility.LoadTexture("BreakpointIcon.png", false, null);
		}

		private static void initEnableTaskTexture()
		{
			BehaviorDesignerUtility.enableTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskEnableIcon.png", false, null);
		}

		private static void initDisableTaskTexture()
		{
			BehaviorDesignerUtility.disableTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskDisableIcon.png", false, null);
		}

		private static void initExpandTaskTexture()
		{
			BehaviorDesignerUtility.expandTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskExpandIcon.png", false, null);
		}

		private static void initCollapseTaskTexture()
		{
			BehaviorDesignerUtility.collapseTaskTexture = BehaviorDesignerUtility.LoadTexture("TaskCollapseIcon.png", false, null);
		}

		private static void initExecutionSuccessTexture()
		{
			BehaviorDesignerUtility.executionSuccessTexture = BehaviorDesignerUtility.LoadTexture("ExecutionSuccess.png", false, null);
		}

		private static void initExecutionFailureTexture()
		{
			BehaviorDesignerUtility.executionFailureTexture = BehaviorDesignerUtility.LoadTexture("ExecutionFailure.png", false, null);
		}
	}
}
