using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
	public class BehaviorDesignerWindow : EditorWindow
	{
		[SerializeField]
		public static BehaviorDesignerWindow instance;

		private Rect mGraphRect;

		private Rect mFileToolBarRect;

		private Rect mPropertyToolbarRect;

		private Rect mPropertyBoxRect;

		private Rect mPreferencesPaneRect;

		private Vector2 mGraphScrollSize = new Vector2(20000f, 20000f);

		private bool mSizesInitialized;

		private Vector2 mCurrentMousePosition = Vector2.zero;

		private Vector2 mGraphScrollPosition = new Vector2(-1f, -1f);

		private Vector2 mGraphOffset = Vector2.zero;

		private float mGraphZoom = 1f;

		private int mBehaviorToolbarSelection = 1;
        
		private string[] mBehaviorToolbarStrings = new string[]
		{
			"Behavior",
			"Tasks",
			"Variables",
			"Inspector"
		};

		private string mGraphStatus = "";

		private Vector2 mSelectStartPosition = Vector2.zero;

		private bool mIsSelecting;

		private bool mIsDragging;

		private bool mKeepTasksSelected;

		private bool mNodeClicked;

		private bool mUpdateNodeTaskMap;

		private bool mStepApplication;

		private Dictionary<NodeDesigner, Task> mNodeDesignerTaskMap;

		private bool mEditorAtBreakpoint;

		private GenericMenu mRightClickMenu = new GenericMenu();

		private GenericMenu mBreadcrumbGameObjectBehaviorMenu = new GenericMenu();

		private GenericMenu mBreadcrumbGameObjectMenu = new GenericMenu();

		private GenericMenu mBreadcrumbBehaviorMenu = new GenericMenu();

		private bool mShowRightClickMenu;

		private bool mShowPrefPane;

		[SerializeField]
		private GraphDesigner mGraphDesigner = ScriptableObject.CreateInstance<GraphDesigner>();

		private TaskInspector mTaskInspector;

		private TaskList mTaskList;

		private VariableInspector mVariableInspector;

		[SerializeField]
		private UnityEngine.Object mActiveObject;

		private UnityEngine.Object mPrevActiveObject;

		private BehaviorSource mActiveBehaviorSource;

		private int mActiveBehaviorID = -1;

		private BehaviorManager mBehaviorManager;

		private bool mLockActiveGameObject;

		private BehaviorSource mInspectorBehaviorSourceLoad;

		private List<TaskSerializer> mCopiedTasks;

		public int ActiveBehaviorID
		{
			get
			{
				return this.mActiveBehaviorID;
			}
		}

		[MenuItem("Window/Behavior Designer")]
		public static void ShowWindow()
		{
			BehaviorDesignerWindow behaviorDesignerWindow = EditorWindow.GetWindow(typeof(BehaviorDesignerWindow)) as BehaviorDesignerWindow;//��ʾ����
			behaviorDesignerWindow.wantsMouseMove=true;
			behaviorDesignerWindow.minSize=new Vector2(600f, 500f);
			UnityEngine.Object.DontDestroyOnLoad(behaviorDesignerWindow);
			BehaviorDesignerPreferences.InitPrefernces();
		}

		public void OnEnable()
		{
			this.mSizesInitialized = false;
			base.Repaint();
            if (this.mGraphDesigner == null)
            {
                this.mGraphDesigner = ScriptableObject.CreateInstance<GraphDesigner>();
            }
            if (this.mTaskInspector == null)
            {
                this.mTaskInspector = ScriptableObject.CreateInstance<TaskInspector>();
            }
            if (this.mVariableInspector == null)
            {
                this.mVariableInspector = ScriptableObject.CreateInstance<VariableInspector>();
            }
            EditorApplication.hierarchyWindowChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.hierarchyWindowChanged, new EditorApplication.CallbackFunction(this.OnSelectionChange));
            EditorApplication.projectWindowChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.projectWindowChanged, new EditorApplication.CallbackFunction(this.OnProjectWindowChange));
            EditorApplication.playmodeStateChanged = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.playmodeStateChanged, new EditorApplication.CallbackFunction(this.OnPlaymodeStateChange));
            this.init();
            this.setBehaviorManager();
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
            if (flag && EditorUtility.DisplayDialog("Update Behavior Trees", "The behavior tree data format has been updated. Run the update tool to update your behavior trees to the new format.", "OK"))
            {
                UpdateTool.ShowWindow();
                EditorWindow.FocusWindowIfItsOpen<UpdateTool>();
            }
		}

		public void OnFocus()
		{
            //Debug.Log("�����ڻ�ý���ʱ����һ��");
            BehaviorDesignerWindow.instance = this;
            base.wantsMouseMove = true;
            this.init();
            if (!this.mLockActiveGameObject)
            {
                this.mActiveObject = Selection.activeObject;
            }
            this.ReloadPreviousBehavior();
            this.UpdateGraphStatus();
		}

		public void OnSelectionChange()
		{
			if (!this.mLockActiveGameObject)
			{
				this.UpdateTree(false);
			}
			this.UpdateGraphStatus();
		}

		public void OnProjectWindowChange()
		{
			this.ReloadPreviousBehavior();
		}

		private void ReloadPreviousBehavior()
		{
			if (this.mActiveObject != null)
			{
				if (this.mActiveObject as GameObject)
				{
					GameObject gameObject = this.mActiveObject as GameObject;
					int num = -1;
					Behavior[] components = gameObject.GetComponents<Behavior>();
					for (int i = 0; i < components.Length; i++)
					{
						if (components[i].GetBehaviorSource().BehaviorID == this.mActiveBehaviorID)
						{
							num = i;
							break;
						}
					}
					if (num != -1)
					{
						this.LoadBehavior(components[num].GetBehaviorSource(), true, false, false, false);
						return;
					}
					if (components.Count<Behavior>() > 0)
					{
						this.LoadBehavior(components[0].GetBehaviorSource(), true, false, false, false);
						return;
					}
					if (this.mGraphDesigner != null)
					{
						this.ClearGraph();
						return;
					}
				}
				else
				{
					if (this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior)
					{
						BehaviorDesigner.Runtime.ExternalBehavior externalBehavior = this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior;
						BehaviorSource behaviorSource = externalBehavior.BehaviorSource;
						if (externalBehavior.BehaviorSource.Owner == null)
						{
							externalBehavior.BehaviorSource.Owner = externalBehavior;
						}
						this.LoadBehavior(behaviorSource, true, false, false, false);
						return;
					}
					if (this.mGraphDesigner != null)
					{
						this.mActiveObject = null;
						this.ClearGraph();
						return;
					}
				}
			}
			else if (this.mGraphDesigner != null)
			{
				this.ClearGraph();
				base.Repaint();
			}
		}

		private void UpdateTree(bool firstLoad)
		{
			bool flag = firstLoad;
			if (Selection.activeObject != null)
			{
				bool flag2 = false;
				if (!Selection.activeObject.Equals(this.mActiveObject))
				{
					this.mActiveObject = Selection.activeObject;
					flag = true;
				}
				BehaviorSource behaviorSource = null;
				GameObject gameObject = this.mActiveObject as GameObject;
				if (gameObject != null && gameObject.GetComponent<Behavior>() != null)
				{
					if (flag || this.mInspectorBehaviorSourceLoad != null)
					{
						if (this.mInspectorBehaviorSourceLoad != null && this.mInspectorBehaviorSourceLoad.EntryTask != null)
						{
							behaviorSource = this.mInspectorBehaviorSourceLoad;
							this.mInspectorBehaviorSourceLoad = null;
						}
						else if (this.mActiveObject.Equals(this.mPrevActiveObject) && this.mActiveBehaviorID != -1)
						{
							flag2 = true;
							int num = -1;
							Behavior[] components = (this.mActiveObject as GameObject).GetComponents<Behavior>();
							for (int i = 0; i < components.Length; i++)
							{
								if (components[i].GetBehaviorSource().BehaviorID == this.mActiveBehaviorID)
								{
									num = i;
									break;
								}
							}
							if (num != -1)
							{
								behaviorSource = gameObject.GetComponents<Behavior>()[num].GetBehaviorSource();
							}
							else if (components.Count<Behavior>() > 0)
							{
								behaviorSource = gameObject.GetComponents<Behavior>()[0].GetBehaviorSource();
							}
						}
						else
						{
							behaviorSource = gameObject.GetComponents<Behavior>()[0].GetBehaviorSource();
						}
					}
					else
					{
						Behavior[] components2 = gameObject.GetComponents<Behavior>();
						bool flag3 = false;
						if (this.mActiveBehaviorSource != null)
						{
							for (int j = 0; j < components2.Length; j++)
							{
								if (components2[j].Equals(this.mActiveBehaviorSource.Owner))
								{
									flag3 = true;
									break;
								}
							}
						}
						if (!flag3)
						{
							behaviorSource = gameObject.GetComponents<Behavior>()[0].GetBehaviorSource();
						}
						else
						{
							behaviorSource = this.mActiveBehaviorSource;
							flag2 = true;
						}
					}
				}
				else if (this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior != null)
				{
					BehaviorDesigner.Runtime.ExternalBehavior externalBehavior = this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior;
					if (externalBehavior.BehaviorSource.Owner == null)
					{
						externalBehavior.BehaviorSource.Owner = externalBehavior;
					}
					if (flag && this.mActiveObject.Equals(this.mPrevActiveObject))
					{
						flag2 = true;
					}
					behaviorSource = externalBehavior.BehaviorSource;
				}
				else
				{
					this.mPrevActiveObject = null;
				}
				if (behaviorSource != null)
				{
					this.LoadBehavior(behaviorSource, flag2, firstLoad, !flag2 || (!firstLoad && !this.mSizesInitialized && !EditorApplication.isPlaying), false);
					return;
				}
				if (behaviorSource == null)
				{
					this.ClearGraph();
					return;
				}
			}
			else
			{
				if (this.mActiveObject != null && this.mActiveBehaviorSource != null)
				{
					this.mPrevActiveObject = this.mActiveObject;
				}
				this.mActiveObject = null;
				this.ClearGraph();
			}
		}

		private void init()
		{
			if (this.mTaskList == null)
			{
				this.mTaskList = ScriptableObject.CreateInstance<TaskList>();
			}
			this.mTaskList.init();
			this.UpdateBreadcrumbMenus();
		}

		public void UpdateGraphStatus()
		{
			if (this.mActiveObject == null || (this.mActiveObject as GameObject == null && this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior == null))
			{
				this.mGraphStatus = "Select a Game UnityEngine.Object";
			}
			else if (this.mActiveObject as GameObject != null && (this.mActiveObject as GameObject).GetComponent<Behavior>() == null)
			{
				this.mGraphStatus = "Add a Behavior Tree Component";
			}
			else if (!this.mGraphDesigner.hasEntryNode())
			{
				this.mGraphStatus = "Add a Task";
			}
			else if (this.isReferencingTasks())
			{
				this.mGraphStatus = "Select tasks to reference (right click to complete)";
			}
			else
			{
				this.mGraphStatus = this.mActiveBehaviorSource.ToString();
			}
			base.Repaint();
		}

		private void UpdateBreadcrumbMenus()
		{
			this.mBreadcrumbGameObjectBehaviorMenu = new GenericMenu();
			this.mBreadcrumbGameObjectMenu = new GenericMenu();
			this.mBreadcrumbBehaviorMenu = new GenericMenu();
			List<Behavior> list = (Resources.FindObjectsOfTypeAll(typeof(Behavior)) as Behavior[]).ToList<Behavior>();
			HashSet<string> hashSet = new HashSet<string>();
			Dictionary<GameObject, string> dictionary = new Dictionary<GameObject, string>();
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].gameObject != null && !dictionary.ContainsKey(list[i].gameObject))
				{
					string text = list[i].gameObject.name;
					if (!AssetDatabase.GetAssetPath(list[i]).Equals(""))
					{
						text += " (prefab)";
					}
					int num = 1;
					while (hashSet.Contains(text))
					{
						text = string.Format("{0} ({1})", list[i].gameObject.name, num);
						num++;
					}
					hashSet.Add(text);
					dictionary.Add(list[i].gameObject, text);
				}
				if (list[i].ToString().Equals(""))
				{
					list[i].GetBehaviorSource().behaviorName = "(Behavior)";
				}
			}
			list.Sort(new AlphanumComparator<Behavior>());
			for (int j = 0; j < list.Count; j++)
			{
				this.mBreadcrumbGameObjectBehaviorMenu.AddItem(new GUIContent(list[j].ToString()), list[j].Equals((this.mActiveBehaviorSource != null) ? this.mActiveBehaviorSource.Owner : null), new GenericMenu.MenuFunction2(this.behaviorSelectionCallback), list[j]);
				this.mBreadcrumbGameObjectMenu.AddItem(new GUIContent(dictionary[list[j].gameObject]), list[j].gameObject.Equals(this.mActiveObject as GameObject), new GenericMenu.MenuFunction2(this.behaviorSelectionCallback), list[j]);
				if (list[j].gameObject.Equals(this.mActiveObject as GameObject))
				{
					this.mBreadcrumbBehaviorMenu.AddItem(new GUIContent(list[j].GetBehaviorSource().behaviorName), list[j].Equals((this.mActiveBehaviorSource != null) ? this.mActiveBehaviorSource.Owner : null), new GenericMenu.MenuFunction2(this.behaviorSelectionCallback), list[j]);
				}
			}
		}

		private void buildRightClickMenu(NodeDesigner clickedNode)
		{
			if (this.mActiveObject == null || EditorApplication.isCompiling)
			{
				return;
			}
			this.mRightClickMenu = new GenericMenu();
			if (clickedNode == null && !EditorApplication.isPlaying)
			{
				this.mTaskList.addTasksToMenu(this, ref this.mRightClickMenu);
				if (this.mCopiedTasks != null && this.mCopiedTasks.Count > 0)
				{
					this.mRightClickMenu.AddItem(new GUIContent("Paste Tasks"), false, new GenericMenu.MenuFunction(this.pasteNodes));
				}
				else
				{
					this.mRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
				}
			}
			if (clickedNode != null && !clickedNode.IsEntryDisplay)
			{
				if (this.mGraphDesigner.SelectedNodes.Count == 1)
				{
					this.mRightClickMenu.AddItem(new GUIContent(clickedNode.Task.NodeData.Disabled ? "Enable" : "Disable"), false, new GenericMenu.MenuFunction2(this.toggleEnableState), clickedNode);
					if (clickedNode.IsParent)
					{
						this.mRightClickMenu.AddItem(new GUIContent(clickedNode.Task.NodeData.Collapsed ? "Expand" : "Collapse"), false, new GenericMenu.MenuFunction2(this.toggleCollapseState), clickedNode);
					}
					this.mRightClickMenu.AddItem(new GUIContent(clickedNode.Task.NodeData.IsBreakpoint ? "Remove Breakpoint" : "Set Breakpoint"), false, new GenericMenu.MenuFunction2(this.toggleBreakpoint), clickedNode);
				}
				if (!EditorApplication.isPlaying)
				{
					this.mRightClickMenu.AddItem(new GUIContent(string.Format("Copy Task{0}", (this.mGraphDesigner.SelectedNodes.Count > 1) ? "s" : "")), false, new GenericMenu.MenuFunction(this.copyNodes));
					if (this.mCopiedTasks != null && this.mCopiedTasks.Count > 0)
					{
						this.mRightClickMenu.AddItem(new GUIContent(string.Format("Paste Task{0}", (this.mCopiedTasks.Count > 1) ? "s" : "")), false, new GenericMenu.MenuFunction(this.pasteNodes));
					}
					else
					{
						this.mRightClickMenu.AddDisabledItem(new GUIContent("Paste Tasks"));
					}
					this.mRightClickMenu.AddItem(new GUIContent(string.Format("Delete Task{0}", (this.mGraphDesigner.SelectedNodes.Count > 1) ? "s" : "")), false, new GenericMenu.MenuFunction(this.deleteNodes));
				}
			}
			if (!EditorApplication.isPlaying && this.mActiveObject as GameObject != null)
			{
				if (clickedNode != null && !clickedNode.IsEntryDisplay)
				{
					this.mRightClickMenu.AddSeparator("");
				}
				this.mRightClickMenu.AddItem(new GUIContent("Add Behavior Tree"), false, new GenericMenu.MenuFunction(this.addBehavior));
				if (this.mActiveBehaviorSource != null)
				{
					this.mRightClickMenu.AddItem(new GUIContent("Remove Behavior Tree"), false, new GenericMenu.MenuFunction(this.removeBehavior));
				}
			}
		}

        /// <summary>
        ///���ڻ��ƺ���
        /// </summary>
		public void OnGUI()
		{
            this.mCurrentMousePosition = Event.current.mousePosition;
            this.setupSizes();
            if (!this.mSizesInitialized)
            {
                if (!this.mLockActiveGameObject || this.mActiveObject == null)
                {
                    this.UpdateTree(true);
                }
                else if (this.mActiveObject as GameObject)
                {
                    BehaviorSource behaviorSource = null;
                    Behavior[] components = (this.mActiveObject as GameObject).GetComponents<Behavior>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        if (components[i].GetBehaviorSource().BehaviorID == this.mActiveBehaviorSource.BehaviorID)
                        {
                            behaviorSource = components[i].GetBehaviorSource();
                        }
                    }
                    this.LoadBehavior(behaviorSource, true, true, false, false);
                }
                else if (this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior)
                {
                    BehaviorDesigner.Runtime.ExternalBehavior externalBehavior = this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior;
                    BehaviorSource behaviorSource2 = externalBehavior.BehaviorSource;
                    if (externalBehavior.BehaviorSource.Owner == null)
                    {
                        externalBehavior.BehaviorSource.Owner = externalBehavior;
                    }
                    this.LoadBehavior(behaviorSource2, true, true, false, false);
                }
                this.mSizesInitialized = true;
            }
            if (Application.isPlaying && this.mBehaviorManager == null)
            {
                this.setBehaviorManager();
            }
            if (this.mBehaviorManager != null && this.mBehaviorManager.Dirty)
            {
                if (this.mActiveBehaviorSource != null)
                {
                    this.LoadBehavior(this.mActiveBehaviorSource, true, false, false);
                }
                this.mBehaviorManager.Dirty = false;
            }
            this.handleEvents();
            if (this.draw())
            {
                base.Repaint();
            }
		}

		public void OnPlaymodeStateChange()
		{
			if (EditorApplication.isPlaying && !EditorApplication.isPaused)
			{
				if (this.mBehaviorManager == null)
				{
					this.setBehaviorManager();
					if (this.mBehaviorManager == null)
					{
						return;
					}
				}
				if (this.mBehaviorManager.AtBreakpoint && this.mEditorAtBreakpoint)
				{
					this.mEditorAtBreakpoint = false;
					this.mBehaviorManager.AtBreakpoint = false;
					return;
				}
			}
			else if (EditorApplication.isPlaying && EditorApplication.isPaused)
			{
				if (this.mBehaviorManager != null && this.mBehaviorManager.AtBreakpoint)
				{
					if (!this.mEditorAtBreakpoint)
					{
						this.mEditorAtBreakpoint = true;
						return;
					}
					this.mEditorAtBreakpoint = false;
					this.mBehaviorManager.AtBreakpoint = false;
					return;
				}
			}
			else if (!EditorApplication.isPlaying)
			{
				this.mBehaviorManager = null;
			}
		}

		private void setBehaviorManager()
		{
			this.mBehaviorManager = BehaviorManager.instance;
			if (this.mBehaviorManager == null)
			{
				return;
			}
			this.mBehaviorManager.onTaskBreakpoint += new BehaviorManager.TaskBreakpointHandler(this.OnTaskBreakpoint);
			if (this.mActiveBehaviorSource != null && this.mActiveBehaviorSource.Owner as Behavior != null && this.mBehaviorManager.setShouldShowExternalTree(this.mActiveBehaviorSource.Owner as Behavior, BehaviorDesignerPreferences.GetBool(BDPreferneces.ShowExternalTrees)))
			{
				this.LoadBehavior(this.mActiveBehaviorSource, true, false, false);
			}
			this.mUpdateNodeTaskMap = true;
		}

		public void OnTaskBreakpoint()
		{
			EditorApplication.isPaused=true;
		}

		private void OnPreferenceChange(BDPreferneces pref, object value)
		{
			if (pref == BDPreferneces.ShowExternalTrees && this.mBehaviorManager != null && this.mActiveBehaviorSource != null && this.mActiveBehaviorSource.Owner as Behavior != null && this.mBehaviorManager.setShouldShowExternalTree(this.mActiveBehaviorSource.Owner as Behavior, (bool)value))
			{
				this.LoadBehavior(this.mActiveBehaviorSource, true);
			}
		}

		public void OnInspectorUpdate()
		{
			if (this.mStepApplication)
			{
				EditorApplication.Step();
				this.mStepApplication = false;
			}
			if (EditorApplication.isPlaying && !EditorApplication.isPaused && this.mActiveBehaviorSource != null && this.mBehaviorManager != null)
			{
				if (this.mUpdateNodeTaskMap)
				{
					this.UpdateNodeTaskMap();
				}
				if (this.mBehaviorManager.AtBreakpoint)
				{
					this.mBehaviorManager.AtBreakpoint = false;
				}
			}
			this.UpdateGraphStatus();
			if (this.mGraphDesigner != null && this.mGraphDesigner.OnInspectorUpdate())
			{
				base.Repaint();
			}
		}

		private void UpdateNodeTaskMap()
		{
			if (this.mUpdateNodeTaskMap && this.mBehaviorManager != null)
			{
				Behavior behavior = this.mActiveBehaviorSource.Owner as Behavior;
				List<Task> taskList = this.mBehaviorManager.getTaskList(behavior);
				if (taskList != null)
				{
					this.mNodeDesignerTaskMap = new Dictionary<NodeDesigner, Task>();
					for (int i = 0; i < taskList.Count; i++)
					{
						NodeDesigner nodeDesigner = taskList[i].NodeData.NodeDesigner as NodeDesigner;
						if (nodeDesigner != null && !this.mNodeDesignerTaskMap.ContainsKey(nodeDesigner))
						{
							this.mNodeDesignerTaskMap.Add(nodeDesigner, taskList[i]);
						}
					}
					this.mUpdateNodeTaskMap = false;
				}
			}
		}

        /// <summary>
        /// ���ƴ���
        /// </summary>
        /// <returns></returns>
		private bool draw()
		{
			bool result = false;
			Color color = GUI.color;
			Color backgroundColor = GUI.backgroundColor;
			GUI.color=Color.white;
			GUI.backgroundColor=Color.white;
			this.drawFileToolbar();
            this.drawPropertiesBox();
            if (this.drawGraphArea())
            {
                result = true;
            }
            this.drawPreferencesPane();
			GUI.color=color;
			GUI.backgroundColor=backgroundColor;
			return result;
		}

		private void drawFileToolbar()
		{
			GUILayout.BeginArea(this.mFileToolBarRect, EditorStyles.toolbar);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (GUILayout.Button("...", EditorStyles.toolbarButton, new GUILayoutOption[]
			{
				GUILayout.Width(22f)
			}))
			{
				this.mBreadcrumbGameObjectBehaviorMenu.ShowAsContext();
			}
			string text = (this.mActiveObject as GameObject != null || this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior != null) ? this.mActiveObject.name : "(None Selected)";
			if (GUILayout.Button(text, EditorStyles.toolbarPopup, new GUILayoutOption[]
			{
				GUILayout.Width(140f)
			}))
			{
				this.mBreadcrumbGameObjectMenu.ShowAsContext();
			}
			string text2 = (this.mActiveBehaviorSource != null) ? this.mActiveBehaviorSource.behaviorName : "(None Selected)";
			if (GUILayout.Button(text2, EditorStyles.toolbarPopup, new GUILayoutOption[]
			{
				GUILayout.Width(140f)
			}) && this.mActiveBehaviorSource != null)
			{
				this.mBreadcrumbBehaviorMenu.ShowAsContext();
			}
			if (GUILayout.Button("-", EditorStyles.toolbarButton, new GUILayoutOption[]
			{
				GUILayout.Width(22f)
			}))
			{
				if (this.mActiveBehaviorSource != null)
				{
					this.removeBehavior();
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to Remove Behavior Tree", "No behavior tree selected.", "OK");
				}
			}
			if (GUILayout.Button("+", EditorStyles.toolbarButton, new GUILayoutOption[]
			{
				GUILayout.Width(22f)
			}))
			{
				if (this.mActiveObject != null)
				{
					this.addBehavior();
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to Add Behavior Tree", "No Game UnityEngine.Object is selected.", "OK");
				}
			}
			if (GUILayout.Button("Lock", this.mLockActiveGameObject ? BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle : EditorStyles.toolbarButton, new GUILayoutOption[]
			{
				GUILayout.Width(42f)
			}))
			{
				if (this.mActiveObject != null)
				{
					this.mLockActiveGameObject = !this.mLockActiveGameObject;
					if (!this.mLockActiveGameObject)
					{
						this.UpdateTree(false);
					}
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to Lock Game UnityEngine.Object", "No Game UnityEngine.Object is selected.", "OK");
				}
			}
			if (GUILayout.Button("Save", EditorStyles.toolbarButton, new GUILayoutOption[]
			{
				GUILayout.Width(42f)
			}))
			{
				if (this.mActiveBehaviorSource != null)
				{
					if (this.mActiveBehaviorSource.Owner.GetObject() as Behavior)
					{
						this.SaveAsAsset();
					}
					else
					{
						this.SaveAsPrefab();
					}
				}
				else
				{
					EditorUtility.DisplayDialog("Unable to Save Behavior Tree", "Select a behavior tree from within the scene.", "OK");
				}
			}
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Preferences", this.mShowPrefPane ? BehaviorDesignerUtility.ToolbarButtonSelectionGUIStyle : EditorStyles.toolbarButton, new GUILayoutOption[]
			{
				GUILayout.Width(80f)
			}))
			{
				this.mShowPrefPane = !this.mShowPrefPane;
			}
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		private void drawPreferencesPane()
		{
			if (this.mShowPrefPane)
			{
				GUILayout.BeginArea(this.mPreferencesPaneRect, BehaviorDesignerUtility.PreferencesPaneGUIStyle);
				BehaviorDesignerPreferences.DrawPreferencesPane(new PreferenceChangeHandler(this.OnPreferenceChange));
				GUILayout.EndArea();
			}
		}
        /// <summary>
        ///���ƶ����˵���
        /// </summary>
		private void drawPropertiesBox()
		{
            //���ƶ����˵�
			GUILayout.BeginArea(this.mPropertyToolbarRect, EditorStyles.toolbar);
			int num = this.mBehaviorToolbarSelection;
			this.mBehaviorToolbarSelection = GUILayout.Toolbar(this.mBehaviorToolbarSelection, this.mBehaviorToolbarStrings, EditorStyles.toolbarButton, new GUILayoutOption[0]);
			GUILayout.EndArea();
            
            //������߲˵�
			GUILayout.BeginArea(this.mPropertyBoxRect, BehaviorDesignerUtility.PropertyBoxGUIStyle);
			if (this.mBehaviorToolbarSelection == 0)//�����˵�ѡ�ŵĲ˵���
			{//��ʾ��Ϊ�����������
				if (this.mActiveBehaviorSource != null)
				{
					GUILayout.Space(3f);
					if (this.mActiveBehaviorSource.Owner as Behavior != null)
					{
						BehaviorInspector.DrawInspectorGUI(this.mActiveBehaviorSource.Owner as Behavior, new SerializedObject(this.mActiveBehaviorSource.Owner as Behavior));
					}
					else
					{
						ExternalBehaviorInspector.DrawInspectorGUI(this.mActiveBehaviorSource);
					}
				}
				else
				{
					GUILayout.Space(5f);
					GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[]
					{
						GUILayout.Width(305f)
					});
				}
			}
			else if (this.mBehaviorToolbarSelection == 1)
            {//��ʾ��Ϊ���ڵ�
				this.mTaskList.drawTaskList(this);
				if (num != 1)
				{
					this.mTaskList.focusSearchField();
				}
			}
			else if (this.mBehaviorToolbarSelection == 2)
			{//��ʾ����
				if (this.mActiveBehaviorSource != null)
				{
					if (this.mVariableInspector.drawVariables(this.mActiveBehaviorSource) && !Application.isPlaying)
					{
						this.saveBehavior();
					}
					if (num != 2)
					{
						this.mVariableInspector.focusNameField();
					}
				}
				else
				{
					GUILayout.Space(5f);
					GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[]
					{
						GUILayout.Width(305f)
					});
				}
			}
			else if (this.mBehaviorToolbarSelection == 3)
			{//��ʾѡ�����Ϊ��ڵ������
				if (this.mGraphDesigner.SelectedNodes.Count == 1 && !this.mGraphDesigner.SelectedNodes[0].IsEntryDisplay)
				{
					Task task = this.mGraphDesigner.SelectedNodes[0].Task;
					if (this.mNodeDesignerTaskMap != null && this.mNodeDesignerTaskMap.Count > 0)
					{
						NodeDesigner nodeDesigner = this.mGraphDesigner.SelectedNodes[0].Task.NodeData.NodeDesigner as NodeDesigner;
						if (nodeDesigner != null && this.mNodeDesignerTaskMap.ContainsKey(nodeDesigner))
						{
							task = this.mNodeDesignerTaskMap[nodeDesigner];
						}
					}
					if (this.mTaskInspector.drawTaskFields(this.mActiveBehaviorSource, task) && !Application.isPlaying)
					{
						this.saveBehavior();
					}
				}
				else
				{
					GUILayout.Space(5f);
					if (this.mGraphDesigner.SelectedNodes.Count > 1)
					{
						GUILayout.Label("Only one task can be selected at a time to view its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[]
						{
							GUILayout.Width(305f)
						});
					}
					else
					{
						GUILayout.Label("Select a task from the tree to view its properties.", BehaviorDesignerUtility.LabelWrapGUIStyle, new GUILayoutOption[]
						{
							GUILayout.Width(305f)
						});
					}
				}
			}
			GUILayout.EndArea();
		}

		private bool drawGraphArea()
		{
			Vector2 vector = GUI.BeginScrollView(new Rect(this.mGraphRect.x, this.mGraphRect.y, this.mGraphRect.width + (float)BehaviorDesignerUtility.ScrollBarSize, this.mGraphRect.height + (float)BehaviorDesignerUtility.ScrollBarSize), this.mGraphScrollPosition, new Rect(0f, 0f, this.mGraphScrollSize.x, this.mGraphScrollSize.y), true, true);
            if (vector != this.mGraphScrollPosition && Event.current.type != EventType.layout && Event.current.type != EventType.ignore)
			{
				this.mGraphOffset -= (vector - this.mGraphScrollPosition) / this.mGraphZoom;
				this.mGraphScrollPosition = vector;
				this.mGraphDesigner.graphDirty();
			}
			GUI.EndScrollView();
			GUI.Box(this.mGraphRect, "", BehaviorDesignerUtility.GraphBackgroundGUIStyle);
			EditorZoomArea.Begin(this.mGraphRect, this.mGraphZoom);
			Vector2 mousePosition;
			if (!this.getMousePositionInGraph(out mousePosition))
			{
				mousePosition = new Vector2(-1f, -1f);
			}
			bool result = false;
			if (this.mGraphDesigner != null && this.mGraphDesigner.drawNodes(mousePosition, this.mGraphOffset, this.mGraphZoom))
			{
				result = true;
			}
			if (this.mIsSelecting)
			{
				GUI.Box(this.getSelectionArea(), "", BehaviorDesignerUtility.SelectionGUIStyle);
			}
			EditorZoomArea.End();
			this.drawGraphStatus();
			this.drawSelectedTaskDescription();
			return result;
		}

		private void drawGraphStatus()
		{
			if (!this.mGraphStatus.Equals(""))
			{
				GUI.Label(new Rect(this.mGraphRect.x + 5f, this.mGraphRect.y + 5f, this.mGraphRect.width, 30f), this.mGraphStatus, BehaviorDesignerUtility.GraphStatusGUIStyle);
			}
		}

		private void drawSelectedTaskDescription()
		{
			TaskDescriptionAttribute[] array;
			if (BehaviorDesignerPreferences.GetBool(BDPreferneces.ShowTaskDescription) && this.mGraphDesigner.SelectedNodes.Count == 1 && (array = (this.mGraphDesigner.SelectedNodes[0].Task.GetType().GetCustomAttributes(typeof(TaskDescriptionAttribute), false) as TaskDescriptionAttribute[])).Length > 0)
			{
				float num;
				float num2;
                BehaviorDesignerUtility.TaskCommentGUIStyle.CalcMinMaxWidth(new GUIContent(array[0].Description), out num, out num2);
				float num3 = Mathf.Min((float)BehaviorDesignerUtility.MaxTaskDescriptionBoxWidth, num2 + (float)BehaviorDesignerUtility.TextPadding);
				float num4 = Mathf.Min((float)BehaviorDesignerUtility.MaxTaskDescriptionBoxHeight, BehaviorDesignerUtility.TaskCommentGUIStyle.CalcHeight(new GUIContent(array[0].Description), num3)) + 3f;
				GUI.Box(new Rect(this.mGraphRect.x + 5f, this.mGraphRect.yMax - num4 - 5f, num3, num4), "");
				GUI.Box(new Rect(this.mGraphRect.x + 2f, this.mGraphRect.yMax - num4 - 5f, num3, num4), array[0].Description, BehaviorDesignerUtility.TaskCommentGUIStyle);
			}
		}

		private void addBehavior()
		{
			if (EditorApplication.isPlaying || EditorApplication.isCompiling)
			{
				return;
			}
			if (Selection.activeGameObject != null)
			{
				GameObject activeGameObject = Selection.activeGameObject;
				this.mActiveObject = Selection.activeObject;
				this.mGraphDesigner = ScriptableObject.CreateInstance<GraphDesigner>();
                //Type type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp");
                //if (type == null)
                //{
                //    type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp-firstpass");
                //}
                Behavior behavior = BehaviorUndo.AddComponent(activeGameObject, typeof(Behavior)) as Behavior;
				Behavior[] components = activeGameObject.GetComponents<Behavior>();
				HashSet<string> hashSet = new HashSet<string>();
				for (int i = 0; i < components.Length; i++)
				{
					string text = components[i].GetBehaviorSource().behaviorName;
					int num = 2;
					while (hashSet.Contains(text))
					{
						text = string.Format("{0} {1}", components[i].GetBehaviorSource().behaviorName, num);
						num++;
					}
					components[i].GetBehaviorSource().behaviorName = text;
					hashSet.Add(components[i].GetBehaviorSource().behaviorName);
				}
				this.LoadBehavior(behavior.GetBehaviorSource(), false);
				this.UpdateBreadcrumbMenus();
				base.Repaint();
			}
		}

		private void removeBehavior()
		{
			if (EditorApplication.isPlaying || EditorApplication.isCompiling)
			{
				return;
			}
			if (this.mActiveObject as GameObject != null && (this.mActiveBehaviorSource.EntryTask == null || (this.mActiveBehaviorSource.EntryTask != null && EditorUtility.DisplayDialog("Remove Behavior Tree", "Are you sure you want to remove this behavior tree?", "Yes", "No"))))
			{
				GameObject gameObject = this.mActiveObject as GameObject;
				int num = this.indexForBehavior(this.mActiveBehaviorSource.Owner);
				BehaviorUndo.DestroyObject(this.mActiveBehaviorSource.Owner.GetObject(), true);
				num--;
				if (num == -1 && gameObject.GetComponents<Behavior>().Length > 0)
				{
					num = 0;
				}
				if (num > -1)
				{
					this.LoadBehavior(gameObject.GetComponents<Behavior>()[num].GetBehaviorSource(), true);
				}
				else
				{
					this.ClearGraph();
				}
				this.UpdateBreadcrumbMenus();
				base.Repaint();
			}
		}

		private int indexForBehavior(IBehavior behavior)
		{
			if (behavior.GetObject() as Behavior)
			{
				Behavior[] components = (behavior.GetObject() as Behavior).gameObject.GetComponents<Behavior>();
				for (int i = 0; i < components.Length; i++)
				{
					if (components[i].Equals(behavior))
					{
						return i;
					}
				}
				return -1;
			}
			return 0;
		}
        /// <summary>
        /// ���һ������
        /// </summary>
        /// <param name="type"></param>
        /// <param name="useMousePosition"></param>
		public void addTask(Type type, bool useMousePosition)
		{
			if ((this.mActiveObject as GameObject == null && this.mActiveObject as BehaviorDesigner.Runtime.ExternalBehavior == null) || EditorApplication.isPlaying || EditorApplication.isCompiling)
			{
				return;
			}
			Vector2 vector = new Vector2(this.mGraphRect.width / (2f * this.mGraphZoom), 150f);
			if (useMousePosition)
			{
				this.getMousePositionInGraph(out vector);
			}
			vector -= this.mGraphOffset;
			BehaviorUndo.RegisterUndo("Add", this.mGraphDesigner, true, true);
			GameObject gameObject = this.mActiveObject as GameObject;
            Debug.Log(gameObject.name);
            if (gameObject != null && gameObject.GetComponent<Behavior>() == null)
            {
                this.addBehavior();
            }
            if (this.mGraphDesigner.addNode(this.mActiveBehaviorSource, type, vector) != null)
            {
                this.saveBehavior();
            }
		}

		public bool isReferencingTasks()
		{
			return this.mTaskInspector.ActiveReferenceTask != null;
		}

		public bool isReferencingField(FieldInfo fieldInfo)
		{
			return fieldInfo.Equals(this.mTaskInspector.ActiveReferenceTaskFieldInfo);
		}

		private void disableReferenceTasks()
		{
			if (this.isReferencingTasks())
			{
				this.toggleReferenceTasks();
			}
		}

		public void toggleReferenceTasks()
		{
			this.toggleReferenceTasks(null, null);
		}

		public void toggleReferenceTasks(Task task, FieldInfo fieldInfo)
		{
			bool flag = !this.isReferencingTasks();
			this.mTaskInspector.setActiveReferencedTasks(flag ? task : null, flag ? fieldInfo : null);
			this.UpdateGraphStatus();
		}

		private void referenceTask(NodeDesigner nodeDesigner)
		{
			if (nodeDesigner != null && this.mTaskInspector.referenceTasks(nodeDesigner.Task))
			{
				this.saveBehavior();
			}
		}

		public void identifyNode(NodeDesigner nodeDesigner)
		{
			this.mGraphDesigner.identifyNode(nodeDesigner);
		}

		private void handleEvents()
		{
			if (EditorApplication.isCompiling)
			{
				return;
			}
			switch (Event.current.type)
			{
                case EventType.mouseDown:
				if (Event.current.button == 0)
				{
					if (this.leftMouseDown(Event.current.clickCount))
					{
						Event.current.Use();
						return;
					}
				}
				else if (Event.current.button == 1 && this.rightMouseDown())
				{
					Event.current.Use();
					return;
				}
				break;
                case EventType.mouseUp:
				if (Event.current.button == 0)
				{
					if (this.leftMouseRelease())
					{
						Event.current.Use();
						return;
					}
				}
				else if (Event.current.button == 1 && this.mShowRightClickMenu)
				{
					this.mShowRightClickMenu = false;
					this.mRightClickMenu.ShowAsContext();
					Event.current.Use();
					return;
				}
				break;
                case EventType.mouseMove:
				if (this.mouseMove())
				{
					Event.current.Use();
					return;
				}
				break;
                case EventType.mouseDrag:
				if (Event.current.button == 0)
				{
					if (this.leftMouseDragged())
					{
						Event.current.Use();
						return;
					}
                    if (Event.current.modifiers == EventModifiers.Alt && this.mousePan())
					{
						Event.current.Use();
						return;
					}
				}
				else if (Event.current.button == 2 && this.mousePan())
				{
					Event.current.Use();
					return;
				}
				break;
                case EventType.keyDown:
				if (this.propertiesInspectorHasFocus() || EditorApplication.isPlaying)
				{
					return;
				}
                if (Event.current.keyCode == KeyCode.Delete || Event.current.commandName.Equals("Delete"))
				{
					this.deleteNodes();
					Event.current.Use();
					return;
				}
                if (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
				{
					this.disableReferenceTasks();
					Event.current.Use();
					return;
				}
				break;
                case EventType.keyUp:
                case EventType.repaint:
                case EventType.layout:
                case EventType.dragUpdated:
                case EventType.dragPerform:
                case EventType.ignore:
                case EventType.used:
                case EventType.DragExited:
                case EventType.ContextClick:
				break;
                case EventType.scrollWheel:
				if (this.mouseZoom())
				{
					Event.current.Use();
					return;
				}
				break;
                case EventType.ValidateCommand:
				if (this.propertiesInspectorHasFocus() || EditorApplication.isPlaying)
				{
					return;
				}
				if (Event.current.commandName.Equals("Copy") || Event.current.commandName.Equals("Paste") || Event.current.commandName.Equals("Cut") || Event.current.commandName.Equals("SelectAll") || Event.current.commandName.Equals("Duplicate"))
				{
					if (this.propertiesInspectorHasFocus() || EditorApplication.isPlaying)
					{
						return;
					}
					Event.current.Use();
					return;
				}
				else if (Event.current.commandName.Equals("UndoRedoPerformed") && this.mActiveBehaviorSource != null)
				{
					this.LoadBehavior(this.mActiveBehaviorSource, false, false, false);
					return;
				}
				break;
                case EventType.ExecuteCommand:
				if (this.propertiesInspectorHasFocus() || EditorApplication.isPlaying)
				{
					return;
				}
				if (Event.current.commandName.Equals("Copy"))
				{
					this.copyNodes();
					Event.current.Use();
					return;
				}
				if (Event.current.commandName.Equals("Paste"))
				{
					this.pasteNodes();
					Event.current.Use();
					return;
				}
				if (Event.current.commandName.Equals("Cut"))
				{
					this.cutNodes();
					Event.current.Use();
					return;
				}
				if (Event.current.commandName.Equals("SelectAll"))
				{
					this.mGraphDesigner.selectAll();
					Event.current.Use();
					return;
				}
				if (Event.current.commandName.Equals("Duplicate"))
				{
					this.duplicateNodes();
					Event.current.Use();
				}
				break;
			default:
				return;
			}
		}

		private bool mouseMove()
		{
			Vector2 point;
			if (!this.getMousePositionInGraph(out point))
			{
				return false;
			}
			NodeDesigner nodeDesigner = this.mGraphDesigner.nodeAt(point, this.mGraphOffset);
			if (this.mGraphDesigner.HoverNode != null && ((nodeDesigner != null && !this.mGraphDesigner.HoverNode.Equals(nodeDesigner)) || !this.mGraphDesigner.HoverNode.hoverBarAreaContains(point, this.mGraphOffset)))
			{
				this.mGraphDesigner.clearHover();
			}
			if (nodeDesigner && !nodeDesigner.IsEntryDisplay)
			{
				this.mGraphDesigner.hover(nodeDesigner);
			}
			return this.mGraphDesigner.HoverNode != null;
		}

		private bool leftMouseDown(int clickCount)
		{
			Vector2 point;
			if (!this.getMousePositionInGraph(out point))
			{
				return false;
			}
			if (this.propertiesInspectorHasFocus())
			{
				this.mTaskInspector.clearFocus();
				this.mVariableInspector.clearFocus();
				base.Repaint();
			}
			NodeDesigner nodeDesigner = this.mGraphDesigner.nodeAt(point, this.mGraphOffset);
            if (Event.current.modifiers == EventModifiers.Alt)
			{
				this.mNodeClicked = this.mGraphDesigner.isSelected(nodeDesigner);
				return false;
			}
			if (this.isReferencingTasks())
			{
				if (nodeDesigner == null)
				{
					this.disableReferenceTasks();
				}
				else
				{
					this.referenceTask(nodeDesigner);
				}
				return true;
			}
			if (nodeDesigner != null)
			{
				if (this.mGraphDesigner.HoverNode != null && !nodeDesigner.Equals(this.mGraphDesigner.HoverNode))
				{
					this.mGraphDesigner.clearHover();
					this.mGraphDesigner.hover(nodeDesigner);
				}
				NodeConnection nodeConnection;
				if ((nodeConnection = nodeDesigner.nodeConnectionRectContains(point, this.mGraphOffset)) != null)
				{
					if (this.mGraphDesigner.nodeCanOriginateConnection(nodeDesigner, nodeConnection))
					{
						this.mGraphDesigner.ActiveNodeConnection = nodeConnection;
					}
					return true;
				}
				if (nodeDesigner.contains(point, this.mGraphOffset, false))
				{
					this.mKeepTasksSelected = false;
					if (this.mGraphDesigner.isSelected(nodeDesigner))
					{
                        if (Event.current.modifiers == EventModifiers.Control)
						{
							this.mKeepTasksSelected = true;
							this.mGraphDesigner.deselect(nodeDesigner);
						}
						else if (BehaviorDesignerPreferences.GetBool(BDPreferneces.OpenInspectorOnTaskDoubleClick) && clickCount == 2)
						{
							this.mBehaviorToolbarSelection = 3;
						}
					}
					else
					{
                        if (Event.current.modifiers != EventModifiers.Shift && Event.current.modifiers != EventModifiers.Control)
						{
							this.mGraphDesigner.clearNodeSelection();
							this.mGraphDesigner.clearConnectionSelection();
							if (BehaviorDesignerPreferences.GetBool(BDPreferneces.OpenInspectorOnTaskSelection))
							{
								this.mBehaviorToolbarSelection = 3;
							}
						}
						else
						{
							this.mKeepTasksSelected = true;
						}
						this.mGraphDesigner.select(nodeDesigner);
					}
					this.mNodeClicked = this.mGraphDesigner.isSelected(nodeDesigner);
					return true;
				}
			}
			if (this.mGraphDesigner.HoverNode != null)
			{
				bool flag = false;
				if (this.mGraphDesigner.HoverNode.hoverBarButtonClick(point, this.mGraphOffset, ref flag))
				{
					if (flag && this.mGraphDesigner.HoverNode.Task.NodeData.Collapsed)
					{
						this.mGraphDesigner.deselectWithParent(this.mGraphDesigner.HoverNode);
					}
					return true;
				}
			}
			List<NodeConnection> list = new List<NodeConnection>();
			this.mGraphDesigner.nodeConnectionsAt(point, this.mGraphOffset, ref list);
			if (list.Count > 0)
			{
                if (Event.current.modifiers != EventModifiers.Shift && Event.current.modifiers != EventModifiers.Control)
				{
					this.mGraphDesigner.clearNodeSelection();
					this.mGraphDesigner.clearConnectionSelection();
				}
				for (int i = 0; i < list.Count; i++)
				{
					if (this.mGraphDesigner.isSelected(list[i]))
					{
                        if (Event.current.modifiers == EventModifiers.Control)
						{
							this.mGraphDesigner.deselect(list[i]);
						}
					}
					else
					{
						this.mGraphDesigner.select(list[i]);
					}
				}
				return true;
			}
            if (Event.current.modifiers != EventModifiers.Shift)
			{
				this.mGraphDesigner.clearNodeSelection();
				this.mGraphDesigner.clearConnectionSelection();
			}
			this.mSelectStartPosition = point;
			this.mIsSelecting = true;
			this.mIsDragging = false;
			this.mNodeClicked = false;
			return true;
		}

		private bool leftMouseDragged()
		{
			Vector2 vector;
			if (!this.getMousePositionInGraph(out vector))
			{
				return false;
			}
            if (Event.current.modifiers != EventModifiers.Alt)
			{
				if (this.isReferencingTasks())
				{
					return true;
				}
				if (this.mIsSelecting)
				{
					this.mGraphDesigner.clearNodeSelection();
					List<NodeDesigner> list = this.mGraphDesigner.nodesAt(this.getSelectionArea(), this.mGraphOffset);
					if (list != null)
					{
						for (int i = 0; i < list.Count; i++)
						{
							this.mGraphDesigner.select(list[i]);
						}
					}
					return true;
				}
				if (this.mGraphDesigner.ActiveNodeConnection != null)
				{
					return true;
				}
			}
			if (this.mNodeClicked)
			{
                bool flag = this.mGraphDesigner.dragSelectedNodes(Event.current.delta / this.mGraphZoom, Event.current.modifiers != EventModifiers.Alt, this.mIsDragging);
				if (flag)
				{
					this.mKeepTasksSelected = true;
					this.mIsDragging = true;
				}
				return flag;
			}
			return false;
		}

		private bool leftMouseRelease()
		{
			this.mNodeClicked = false;
			if (this.isReferencingTasks())
			{
				if (!this.mTaskInspector.isActiveTaskArray() && !this.mTaskInspector.isActiveTaskNull())
				{
					this.disableReferenceTasks();
					base.Repaint();
				}
				Vector2 vector;
				if (!this.getMousePositionInGraph(out vector))
				{
					this.mGraphDesigner.ActiveNodeConnection = null;
					return false;
				}
				return true;
			}
			else
			{
				if (this.mIsSelecting)
				{
					this.mIsSelecting = false;
					return true;
				}
				if (this.mIsDragging)
				{
					this.saveBehavior();
					this.mIsDragging = false;
					return true;
				}
				if (this.mGraphDesigner.ActiveNodeConnection != null)
				{
					Vector2 point;
					if (!this.getMousePositionInGraph(out point))
					{
						this.mGraphDesigner.ActiveNodeConnection = null;
						return false;
					}
					NodeDesigner nodeDesigner = this.mGraphDesigner.nodeAt(point, this.mGraphOffset);
					if (nodeDesigner != null && !nodeDesigner.Equals(this.mGraphDesigner.ActiveNodeConnection.OriginatingNodeDesigner) && this.mGraphDesigner.nodeCanAcceptConnection(nodeDesigner, this.mGraphDesigner.ActiveNodeConnection))
					{
						this.mGraphDesigner.connectNodes(this.mActiveBehaviorSource, nodeDesigner);
						this.saveBehavior();
					}
					else
					{
						this.mGraphDesigner.ActiveNodeConnection = null;
					}
					return true;
				}
				else
				{
                    if (Event.current.modifiers == EventModifiers.Shift || this.mKeepTasksSelected)
					{
						return false;
					}
					Vector2 point2;
					if (!this.getMousePositionInGraph(out point2))
					{
						return false;
					}
					NodeDesigner nodeDesigner2 = this.mGraphDesigner.nodeAt(point2, this.mGraphOffset);
					if (nodeDesigner2 != null && !this.mGraphDesigner.isSelected(nodeDesigner2))
					{
						this.mGraphDesigner.deselectAllExcept(nodeDesigner2);
					}
					return true;
				}
			}
		}

		private bool rightMouseDown()
		{
			if (this.isReferencingTasks())
			{
				this.disableReferenceTasks();
				return false;
			}
			Vector2 point;
			if (!this.getMousePositionInGraph(out point))
			{
				return false;
			}
			NodeDesigner nodeDesigner = this.mGraphDesigner.nodeAt(point, this.mGraphOffset);
			if (nodeDesigner == null || !this.mGraphDesigner.isSelected(nodeDesigner))
			{
				this.mGraphDesigner.clearNodeSelection();
				this.mGraphDesigner.clearConnectionSelection();
				if (nodeDesigner != null)
				{
					this.mGraphDesigner.select(nodeDesigner);
				}
			}
			if (this.mGraphDesigner.HoverNode != null)
			{
				this.mGraphDesigner.clearHover();
			}
			this.buildRightClickMenu(nodeDesigner);
			this.mShowRightClickMenu = true;
			return true;
		}

		private bool mouseZoom()
		{
			Vector2 vector;
			if (!this.getMousePositionInGraph(out vector))
			{
				return false;
			}
			float num = -Event.current.delta.y / BehaviorDesignerUtility.GraphZoomSensitivity;
			this.mGraphZoom += num;
			this.mGraphZoom = Mathf.Clamp(this.mGraphZoom, BehaviorDesignerUtility.GraphZoomMin, BehaviorDesignerUtility.GraphZoomMax);
			Vector2 vector2;
			this.getMousePositionInGraph(out vector2);
			this.mGraphOffset += vector2 - vector;
			this.mGraphScrollPosition += vector2 - vector;
			this.mGraphDesigner.graphDirty();
			return true;
		}

		private bool mousePan()
		{
			Vector2 vector;
			if (!this.getMousePositionInGraph(out vector))
			{
				return false;
			}
			this.mGraphOffset += Event.current.delta / this.mGraphZoom;
			this.mGraphScrollPosition -= Event.current.delta;
			this.mGraphDesigner.graphDirty();
			return true;
		}

		private bool propertiesInspectorHasFocus()
		{
			return this.mTaskInspector.hasFocus() || this.mVariableInspector.hasFocus();
		}

		public void addTaskCallback(object obj)
		{
			this.addTask((Type)obj, true);
		}

		private void behaviorSelectionCallback(object obj)
		{
			Behavior behavior = obj as Behavior;
			this.mActiveObject = behavior.gameObject;
			Selection.activeObject=this.mActiveObject;
			this.LoadBehavior(behavior.GetBehaviorSource(), false);
			this.UpdateGraphStatus();
			this.UpdateBreadcrumbMenus();
			if (EditorApplication.isPaused)
			{
				this.mUpdateNodeTaskMap = true;
				this.UpdateNodeTaskMap();
			}
		}

		private void toggleBreakpoint(object obj)
		{
			NodeDesigner nodeDesigner = obj as NodeDesigner;
			nodeDesigner.toggleBreakpoint();
			this.saveBehavior();
			base.Repaint();
		}

		private void toggleEnableState(object obj)
		{
			NodeDesigner nodeDesigner = obj as NodeDesigner;
			nodeDesigner.toggleEnableState();
			this.saveBehavior();
			base.Repaint();
		}

		private void toggleCollapseState(object obj)
		{
			NodeDesigner nodeDesigner = obj as NodeDesigner;
			if (nodeDesigner.toggleCollapseState())
			{
				this.mGraphDesigner.deselectWithParent(nodeDesigner);
			}
			this.saveBehavior();
			base.Repaint();
		}

		private void copyNodes()
		{
			this.mCopiedTasks = this.mGraphDesigner.copy();
		}

		private void pasteNodes()
		{
			if (this.mActiveObject == null || EditorApplication.isPlaying || EditorApplication.isCompiling)
			{
				return;
			}
			GameObject gameObject = this.mActiveObject as GameObject;
			if (gameObject != null && gameObject.GetComponent<Behavior>() == null)
			{
				this.addBehavior();
			}
			if (this.mCopiedTasks != null && this.mCopiedTasks.Count > 0)
			{
				BehaviorUndo.RegisterUndo("Paste", this.mGraphDesigner, true, true);
			}
			this.mGraphDesigner.paste(this.mActiveBehaviorSource, this.mCopiedTasks);
			this.saveBehavior();
		}

		private void cutNodes()
		{
			this.mCopiedTasks = this.mGraphDesigner.copy();
			if (this.mCopiedTasks != null && this.mCopiedTasks.Count > 0)
			{
				BehaviorUndo.RegisterUndo("Cut", this.mGraphDesigner, true, true);
			}
			this.mGraphDesigner.delete(this.mActiveBehaviorSource);
			this.saveBehavior();
		}

		private void duplicateNodes()
		{
			List<TaskSerializer> list = this.mGraphDesigner.copy();
			if (list != null && list.Count > 0)
			{
				BehaviorUndo.RegisterUndo("Duplicate", this.mGraphDesigner, true, true);
			}
			this.mGraphDesigner.paste(this.mActiveBehaviorSource, list);
			this.saveBehavior();
		}

		private void deleteNodes()
		{
			this.mGraphDesigner.delete(this.mActiveBehaviorSource);
			this.saveBehavior();
		}

        /// <summary>
        /// ���ô����ڲ�����
        /// </summary>
		private void setupSizes()
		{
			if (BehaviorDesignerPreferences.GetBool(BDPreferneces.PropertiesPanelOnLeft))
			{
				this.mFileToolBarRect = new Rect((float)BehaviorDesignerUtility.PropertyBoxWidth, 0f, (float)(Screen.width - BehaviorDesignerUtility.PropertyBoxWidth), (float)BehaviorDesignerUtility.ToolBarHeight);
				this.mPropertyToolbarRect = new Rect(0f, 0f, (float)BehaviorDesignerUtility.PropertyBoxWidth, (float)BehaviorDesignerUtility.ToolBarHeight);
				this.mPropertyBoxRect = new Rect(0f, this.mPropertyToolbarRect.height, (float)BehaviorDesignerUtility.PropertyBoxWidth, (float)Screen.height - this.mPropertyToolbarRect.height - (float)BehaviorDesignerUtility.EditorWindowTabHeight);
				this.mGraphRect = new Rect((float)BehaviorDesignerUtility.PropertyBoxWidth, (float)BehaviorDesignerUtility.ToolBarHeight, (float)(Screen.width - BehaviorDesignerUtility.PropertyBoxWidth - BehaviorDesignerUtility.ScrollBarSize), (float)(Screen.height - BehaviorDesignerUtility.ToolBarHeight - BehaviorDesignerUtility.EditorWindowTabHeight - BehaviorDesignerUtility.ScrollBarSize));
				this.mPreferencesPaneRect = new Rect((float)BehaviorDesignerUtility.PropertyBoxWidth + this.mGraphRect.width - (float)BehaviorDesignerUtility.PreferencesPaneWidth, (float)(BehaviorDesignerUtility.ToolBarHeight + (EditorGUIUtility.isProSkin ? 1 : 2)), (float)BehaviorDesignerUtility.PreferencesPaneWidth, (float)BehaviorDesignerUtility.PreferencesPaneHeight);
			}
			else
			{
				this.mFileToolBarRect = new Rect(0f, 0f, (float)(Screen.width - BehaviorDesignerUtility.PropertyBoxWidth), (float)BehaviorDesignerUtility.ToolBarHeight);
				this.mPropertyToolbarRect = new Rect((float)(Screen.width - BehaviorDesignerUtility.PropertyBoxWidth), 0f, (float)BehaviorDesignerUtility.PropertyBoxWidth, (float)BehaviorDesignerUtility.ToolBarHeight);
				this.mPropertyBoxRect = new Rect((float)(Screen.width - BehaviorDesignerUtility.PropertyBoxWidth), this.mPropertyToolbarRect.height, (float)BehaviorDesignerUtility.PropertyBoxWidth, (float)Screen.height - this.mPropertyToolbarRect.height - (float)BehaviorDesignerUtility.EditorWindowTabHeight);
				this.mGraphRect = new Rect(0f, (float)BehaviorDesignerUtility.ToolBarHeight, (float)(Screen.width - BehaviorDesignerUtility.PropertyBoxWidth - BehaviorDesignerUtility.ScrollBarSize), (float)(Screen.height - BehaviorDesignerUtility.ToolBarHeight - BehaviorDesignerUtility.EditorWindowTabHeight - BehaviorDesignerUtility.ScrollBarSize));
				this.mPreferencesPaneRect = new Rect(this.mGraphRect.width - (float)BehaviorDesignerUtility.PreferencesPaneWidth, (float)(BehaviorDesignerUtility.ToolBarHeight + (EditorGUIUtility.isProSkin ? 1 : 2)), (float)BehaviorDesignerUtility.PreferencesPaneWidth, (float)BehaviorDesignerUtility.PreferencesPaneHeight);
			}
			if (this.mGraphScrollPosition == new Vector2(-1f, -1f))
			{
				this.mGraphScrollPosition = (this.mGraphScrollSize - new Vector2(this.mGraphRect.width, this.mGraphRect.height)) / 2f - 2f * new Vector2((float)BehaviorDesignerUtility.ScrollBarSize, (float)BehaviorDesignerUtility.ScrollBarSize);
			}
		}

		private bool getMousePositionInGraph(out Vector2 mousePosition)
		{
			mousePosition = this.mCurrentMousePosition;
			if (!this.mGraphRect.Contains(mousePosition))
			{
				return false;
			}
			if (this.mShowPrefPane && this.mPreferencesPaneRect.Contains(mousePosition))
			{
				return false;
			}
			mousePosition -= new Vector2(this.mGraphRect.xMin, this.mGraphRect.yMin);
			mousePosition /= this.mGraphZoom;
			return true;
		}

		private Rect getSelectionArea()
		{
			Vector2 vector;
			this.getMousePositionInGraph(out vector);
			float num = (this.mSelectStartPosition.x < vector.x) ? this.mSelectStartPosition.x : vector.x;
			float num2 = (this.mSelectStartPosition.x > vector.x) ? this.mSelectStartPosition.x : vector.x;
			float num3 = (this.mSelectStartPosition.y < vector.y) ? this.mSelectStartPosition.y : vector.y;
			float num4 = (this.mSelectStartPosition.y > vector.y) ? this.mSelectStartPosition.y : vector.y;
			return new Rect(num, num3, num2 - num, num4 - num3);
		}

		private void saveBehavior()
		{
			if (this.mActiveBehaviorSource == null)
			{
				return;
			}
			this.mGraphDesigner.save(this.mActiveBehaviorSource);
			this.mActiveBehaviorSource.Serialization = SerializeJSON.Serialize(this.mActiveBehaviorSource, null);
			if (!AssetDatabase.GetAssetPath(this.mActiveObject).Equals(""))
			{
				this.mActiveBehaviorSource.Variables = null;
				this.mActiveBehaviorSource.EntryTask = null;
				this.mActiveBehaviorSource.RootTask = null;
				this.mActiveBehaviorSource.DetachedTasks = null;
				this.LoadBehavior(this.mActiveBehaviorSource, true, false, false);
			}
			EditorUtility.SetDirty(this.mActiveBehaviorSource.Owner.GetObject());
		}

		private void SaveAsAsset()
		{
			if (this.mActiveBehaviorSource == null)
			{
				return;
			}
			string text = EditorUtility.SaveFilePanel("Save Behavior Tree", "Assets", this.mActiveBehaviorSource.behaviorName + ".asset", "asset");
			if (text.Length != 0 && Application.dataPath.Length < text.Length)
			{
				Type type = Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp");
				if (type == null)
				{
					type = Type.GetType("BehaviorDesigner.Runtime.ExternalBehaviorTree, Assembly-CSharp-firstpass");
				}
				BehaviorDesigner.Runtime.ExternalBehavior externalBehavior = ScriptableObject.CreateInstance(type) as BehaviorDesigner.Runtime.ExternalBehavior;
				BehaviorSource behaviorSource = new BehaviorSource(externalBehavior);
				behaviorSource.behaviorName = this.mActiveBehaviorSource.behaviorName;
				behaviorSource.behaviorDescription = this.mActiveBehaviorSource.behaviorDescription;
				behaviorSource.Serialization = SerializeJSON.Serialize(this.mActiveBehaviorSource, behaviorSource);
				externalBehavior.BehaviorSource = behaviorSource;
				text = string.Format("Assets/{0}", text.Substring(Application.dataPath.Length + 1));
				AssetDatabase.DeleteAsset(text);
				AssetDatabase.CreateAsset(externalBehavior, text);
				AssetDatabase.ImportAsset(text);
				Selection.activeObject=externalBehavior;
			}
		}

		private void SaveAsPrefab()
		{
			if (this.mActiveBehaviorSource == null)
			{
				return;
			}
			string text = EditorUtility.SaveFilePanel("Save Behavior Tree", "Assets", this.mActiveBehaviorSource.behaviorName + ".prefab", "prefab");
			if (text.Length != 0 && Application.dataPath.Length < text.Length)
			{
				GameObject gameObject = new GameObject();
				Type type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp");
				if (type == null)
				{
					type = Type.GetType("BehaviorDesigner.Runtime.BehaviorTree, Assembly-CSharp-firstpass");
				}
				Behavior behavior = gameObject.AddComponent(type) as Behavior;
				BehaviorSource behaviorSource = new BehaviorSource(behavior);
				behaviorSource.behaviorName = this.mActiveBehaviorSource.behaviorName;
				behaviorSource.behaviorDescription = this.mActiveBehaviorSource.behaviorDescription;
				behaviorSource.Serialization = SerializeJSON.Serialize(this.mActiveBehaviorSource, behaviorSource);
				behavior.SetBehaviorSource(behaviorSource);
				text = string.Format("Assets/{0}", text.Substring(Application.dataPath.Length + 1));
				AssetDatabase.DeleteAsset(text);
				GameObject activeObject = PrefabUtility.CreatePrefab(text, gameObject);
				UnityEngine.Object.DestroyImmediate(gameObject, true);
				AssetDatabase.ImportAsset(text);
				Selection.activeObject=activeObject;
			}
		}

		public void LoadBehavior(BehaviorSource behaviorSource, bool loadPrevBehavior)
		{
			this.LoadBehavior(behaviorSource, loadPrevBehavior, false, true, false);
		}

		public void LoadBehavior(BehaviorSource behaviorSource, bool loadPrevBehavior, bool firstLoad, bool reposition)
		{
			this.LoadBehavior(behaviorSource, loadPrevBehavior, firstLoad, reposition, false);
		}

		public void LoadBehavior(BehaviorSource behaviorSource, bool loadPrevBehavior, bool firstLoad, bool reposition, bool inspectorLoad)
		{

            //Debug.Log("111111111111111111111111111");
			if (!firstLoad && !this.mSizesInitialized && inspectorLoad)
			{
				this.mInspectorBehaviorSourceLoad = behaviorSource;
				return;
			}
			this.disableReferenceTasks();
            //Debug.Log("22222222");
			this.mActiveBehaviorSource = behaviorSource;
			if (this.mActiveBehaviorSource.BehaviorID == -1)
			{
				this.mActiveBehaviorSource.BehaviorID = this.mActiveBehaviorSource.Owner.GetInstanceID();
			}
			this.mActiveBehaviorID = this.mActiveBehaviorSource.BehaviorID;
			this.mPrevActiveObject = Selection.activeObject;
			if (EditorApplication.isPlaying && this.mActiveBehaviorSource != null && this.mBehaviorManager != null && this.mActiveBehaviorSource.Owner as Behavior != null)
			{
				this.mBehaviorManager.setShouldShowExternalTree(this.mActiveBehaviorSource.Owner as Behavior, BehaviorDesignerPreferences.GetBool(BDPreferneces.ShowExternalTrees));
			}
			Vector2 vector = new Vector2(this.mGraphRect.width / (2f * this.mGraphZoom), 150f);
			vector -= this.mGraphOffset;
			if (this.mGraphDesigner.Load(this.mActiveBehaviorSource, loadPrevBehavior, vector) && reposition && this.mGraphDesigner.hasEntryNode())
			{
				Vector2 vector2 = this.mGraphDesigner.entryNodePosition();
				this.mGraphOffset = new Vector2(-vector2.x + this.mGraphRect.width / (2f * this.mGraphZoom) - 50f, -vector2.y + 50f);
			}
			if (EditorApplication.isPlaying && this.mActiveBehaviorSource != null)
			{
				this.mRightClickMenu = null;
				this.mUpdateNodeTaskMap = true;
				this.UpdateNodeTaskMap();
			}
            //Debug.Log("3333333333");
			this.UpdateGraphStatus();
			this.UpdateBreadcrumbMenus();
			base.Repaint();
		}

		public void ClearGraph()
		{
			this.mGraphDesigner.clear(true);
			this.mActiveBehaviorSource = null;
			this.UpdateGraphStatus();
			this.UpdateBreadcrumbMenus();
			base.Repaint();
		}
	}
}
