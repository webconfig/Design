using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;
public class SkillWindow : EditorWindow
{
    [SerializeField]
    public static SkillWindow Instance;
    [MenuItem("Window/Skill Editor")]
    public static void ShowWindow()
    {
        Instance = EditorWindow.GetWindow(typeof(SkillWindow)) as SkillWindow;//显示窗体
        Instance.wantsMouseMove = true;
        Instance.minSize = new Vector2(600f, 500f);
        UnityEngine.Object.DontDestroyOnLoad(Instance);
        //BehaviorDesignerPreferences.InitPrefernces();
    }

    #region GUI
    /// <summary>
    /// 任务属性
    /// </summary>
    private TaskInspector mTaskInspector = ScriptableObject.CreateInstance<TaskInspector>();
    public static readonly float PropertyBoxWidth = 320;
    public static readonly float ToolBarHeight = 18;
    public static readonly int EditorWindowTabHeight = 21;
    public static readonly int ScrollBarSize = 15;


    private Rect TopRect,LeftRect,LeftMidRect,MidRect;
    private TaskList tasklist;
    private  void SetSizes()
    {

        TopRect = new Rect(PropertyBoxWidth, 0f, (float)(Screen.width - PropertyBoxWidth), ToolBarHeight);
        LeftRect = new Rect(0f, 0f, PropertyBoxWidth, ToolBarHeight);
        LeftMidRect = new Rect(0f, LeftRect.height, PropertyBoxWidth, (float)Screen.height - LeftRect.height - EditorWindowTabHeight);
        MidRect = new Rect(PropertyBoxWidth, ToolBarHeight, (float)(Screen.width - PropertyBoxWidth - ScrollBarSize), (float)(Screen.height - ToolBarHeight - EditorWindowTabHeight - ScrollBarSize));
        //this.mPreferencesPaneRect = new Rect(PropertyBoxWidth + MidRect.width - PreferencesPaneWidth, (float)(ToolBarHeight + (EditorGUIUtility.isProSkin ? 1 : 2)), PreferencesPaneWidth, PreferencesPaneHeight);


        if (this.mGraphScrollPosition == new Vector2(-1f, -1f))
        {
            this.mGraphScrollPosition = (this.mGraphScrollSize - new Vector2(MidRect.width, MidRect.height)) / 2f - 2f * new Vector2(ScrollBarSize, ScrollBarSize);
        }
    }
    public void OnGUI()
    {
        this.mCurrentMousePosition = Event.current.mousePosition;
        this.handleEvents();
        SetSizes();
        DrawTop();
        DrawLeft();
        if (DrawMiddle())
        {
            base.Repaint();
        }
    }

    #region Top
    /// <summary>
    /// 绘制顶部菜单
    /// </summary>
    private void DrawTop()
    {
        GUILayout.BeginArea(TopRect, EditorStyles.toolbar);
        GUILayout.BeginHorizontal(new GUILayoutOption[0]);

        //string text2 = (this.mActiveBehaviorSource != null) ? this.mActiveBehaviorSource.behaviorName : "(None Selected)";
        //if (GUILayout.Button(text2, EditorStyles.toolbarPopup, new GUILayoutOption[] { GUILayout.Width(140f) }))
        //{//技能选择
        //    GenericMenu menu = new GenericMenu();
        //    for (int j = 0; j < BehaviorSources.Count; j++)
        //    {
        //        menu.AddItem(new GUIContent(BehaviorSources[j].behaviorName), false, (obj) => { SaveCurrent(); LoadDataSource(obj as BehaviorSource); }, BehaviorSources[j]);
        //    }
        //    menu.ShowAsContext();
        //}

        if (GUILayout.Button("New", EditorStyles.toolbarButton, new GUILayoutOption[] { GUILayout.Width(42f) }))
        {//加载按钮
            //SaveCurrent();
            //NewBehavior();
        }

        if (GUILayout.Button("Load", EditorStyles.toolbarButton, new GUILayoutOption[] { GUILayout.Width(42f) }))
        {//加载按钮
            //EditorApplication.delayCall += LoadBtnClick;
        }

        if (GUILayout.Button("Save", EditorStyles.toolbarButton, new GUILayoutOption[] { GUILayout.Width(42f) }))
        {//加载按钮
            //if (this.mActiveBehaviorSource != null)
            //{
            //    SaveData();
            //}
            //else
            //{
            //    EditorUtility.DisplayDialog("Unable to Save Behavior Tree", "Select a behavior tree from within the scene.", "OK");
            //}
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    #endregion

    #region Left
    private string[] LeftToolbarStrings = new string[]{"Skill","Tasks","Variables","Inspector"};
    private int LeftSelect = 1;
    private void DrawLeft()
    {
        //绘制顶部菜单
        GUILayout.BeginArea(LeftRect, EditorStyles.toolbar);
        int num = LeftSelect;
        LeftSelect = GUILayout.Toolbar(LeftSelect, LeftToolbarStrings, EditorStyles.toolbarButton, new GUILayoutOption[0]);
        GUILayout.EndArea();

        //绘制内容菜单
        GUILayout.BeginArea(LeftMidRect, EditorStyles.textArea);
        if (LeftSelect == 0)//顶部菜单选着的菜单项
        {//显示行为树本身的属性
            //if (this.mActiveBehaviorSource != null)
            //{
            //    GUILayout.Space(3f);
            //    DrawDataSource();
            //}
            //else
            //{
            //    GUILayout.Space(5f);
            //    GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", LabelWrapGUIStyle, new GUILayoutOption[]
            //        {
            //            GUILayout.Width(305f)
            //        });
            //}
        }
        else if (LeftSelect == 1)
        {//显示行为数节点
            tasklist.drawTaskList(this);
            if (num != 1)
            {
                tasklist.focusSearchField();
            }
        }
        else if (LeftSelect == 2)
        {//显示变量
            //if (this.mActiveBehaviorSource != null)
            //{
            //    if (this.mVariableInspector.drawVariables(this.mActiveBehaviorSource) && !Application.isPlaying)
            //    {
            //        //this.saveBehavior();
            //    }
            //    if (num != 2)
            //    {
            //        this.mVariableInspector.focusNameField();
            //    }
            //}
            //else
            //{
            //    GUILayout.Space(5f);
            //    GUILayout.Label("No behavior tree selected. Create a new behavior tree or select one from the hierarchy.", LabelWrapGUIStyle, new GUILayoutOption[]
            //        {
            //            GUILayout.Width(305f)
            //        });
            //}
        }
        else if (LeftSelect == 3)
        {//显示选择的行为书节点的属性
            if (this.mGraphDesigner.SelectedNodes.Count == 1)
            {
                Task task = this.mGraphDesigner.SelectedNodes[0];
                //if (this.mNodeDesignerTaskMap != null && this.mNodeDesignerTaskMap.Count > 0)
                //{
                //    NodeDesigner nodeDesigner = this.mGraphDesigner.SelectedNodes[0].Task.NodeData.NodeDesigner as NodeDesigner;
                //    if (nodeDesigner != null && this.mNodeDesignerTaskMap.ContainsKey(nodeDesigner))
                //    {
                //        task = this.mNodeDesignerTaskMap[nodeDesigner];
                //    }
                //}
                if (this.mTaskInspector.drawTaskFields(task) && !Application.isPlaying)
                {
                    //this.saveBehavior();
                }
            }
            else
            {
                GUILayout.Space(5f);
                if (this.mGraphDesigner.SelectedNodes.Count > 1)
                {
                    GUILayout.Label("Only one task can be selected at a time to view its properties.", new GUILayoutOption[]
                        {
                            GUILayout.Width(305f)
                        });
                }
                else
                {
                    GUILayout.Label("Select a task from the tree to view its properties.", new GUILayoutOption[]
                        {
                            GUILayout.Width(305f)
                        });
                }
            }
        }
        GUILayout.EndArea();
    }
    #endregion

    #region Middle
    /// <summary>
    /// 当前鼠标位置
    /// </summary>
    private Vector2 mCurrentMousePosition = Vector2.zero;
    private Vector2 mGraphScrollPosition = new Vector2(-1f, -1f);
    private Vector2 mGraphScrollSize = new Vector2(20000f, 20000f);
    private Vector2 mGraphOffset = Vector2.zero;
    private float mGraphZoom = 1f;
    private GraphDesigner mGraphDesigner = ScriptableObject.CreateInstance<GraphDesigner>();
    private bool DrawMiddle()
    {
        Vector2 vector = GUI.BeginScrollView(new Rect(MidRect.x, MidRect.y, MidRect.width + (float)ScrollBarSize, MidRect.height + (float)ScrollBarSize), this.mGraphScrollPosition, new Rect(0f, 0f, this.mGraphScrollSize.x, this.mGraphScrollSize.y), true, true);
        if (vector != this.mGraphScrollPosition && Event.current.type != EventType.layout && Event.current.type != EventType.ignore)
        {
            this.mGraphOffset -= (vector - this.mGraphScrollPosition) / this.mGraphZoom;
            this.mGraphScrollPosition = vector;
            //this.mGraphDesigner.graphDirty();
        }
        GUI.EndScrollView();
        GUI.Box(MidRect, "");


        EditorZoomArea.Begin(MidRect, this.mGraphZoom);
        Vector2 mousePosition;
        if (!this.getMousePositionInGraph(out mousePosition))
        {//获取鼠标在绘图区域的位置
            mousePosition = new Vector2(-1f, -1f);
        }
        bool result = false;
        if (this.mGraphDesigner != null &&this.mGraphDesigner.drawNodes(mousePosition, this.mGraphOffset, this.mGraphZoom))
        {
            result = true;
        }
        EditorZoomArea.End();

        return result;
    }
    /// <summary>
    /// 点击点是否在绘图区域内
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <returns></returns>
    private bool getMousePositionInGraph(out Vector2 mousePosition)
    {
        mousePosition = this.mCurrentMousePosition;
        if (!this.MidRect.Contains(mousePosition))
        {
            return false;
        }
        if (LeftRect.Contains(mousePosition))
        {
            return false;
        }
        mousePosition -= new Vector2(this.MidRect.xMin, this.MidRect.yMin);
        mousePosition /= this.mGraphZoom;
        return true;
    }
    #endregion

    #endregion

    #region 初始化
    public void OnFocus()
    {
        base.wantsMouseMove = true;
        this.init();
    }
    private void init()
    {
        if (tasklist == null)
        {
            tasklist = ScriptableObject.CreateInstance<TaskList>();
        }
        tasklist.init();
    }
    #endregion


    #region 鼠标操作
    private void handleEvents()
    {
        if (EditorApplication.isCompiling)
        {//正在编译脚本
            return;
        }
        switch (Event.current.type)
        {
            case EventType.mouseDown:
                if (Event.current.button == 0)
                {//按下鼠标左键
                    if (leftMouseDown(Event.current.clickCount))
                    {
                        Event.current.Use();
                        return;
                    }
                }
                break;
            case EventType.mouseUp:
                if (Event.current.button == 0)
                {//鼠标左键抬起
                    if (this.leftMouseRelease())
                    {
                        Event.current.Use();
                        return;
                    }
                }
                break;
            //case EventType.mouseMove:
            //    if (this.mouseMove())
            //    {//鼠标移动
            //        Event.current.Use();
            //        return;
            //    }
            //    break;
            case EventType.mouseDrag:
                if (Event.current.button == 0)
                {
                    if (this.leftMouseDragged())
                    {
                        Event.current.Use();
                        return;
                    }
                }
                break;
            //case EventType.keyDown:
            //    if (this.propertiesInspectorHasFocus() || EditorApplication.isPlaying)
            //    {
            //        return;
            //    }
            //    if (Event.current.keyCode == KeyCode.Delete || Event.current.commandName.Equals("Delete"))
            //    {
            //        this.deleteNodes();
            //        Event.current.Use();
            //        return;
            //    }
            //    if (Event.current.keyCode == KeyCode.Escape || Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
            //    {
            //        this.disableReferenceTasks();
            //        Event.current.Use();
            //        return;
            //    }
            //    break;
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
            //case EventType.scrollWheel:
            //    if (this.mouseZoom())
            //    {
            //        Event.current.Use();
            //        return;
            //    }
            //    break;
            //case EventType.ValidateCommand:
            //    if (this.propertiesInspectorHasFocus() || EditorApplication.isPlaying)
            //    {
            //        return;
            //    }
            //    if (Event.current.commandName.Equals("Copy") || Event.current.commandName.Equals("Paste") || Event.current.commandName.Equals("Cut") || Event.current.commandName.Equals("SelectAll") || Event.current.commandName.Equals("Duplicate"))
            //    {
            //        if (this.propertiesInspectorHasFocus() || EditorApplication.isPlaying)
            //        {
            //            return;
            //        }
            //        Event.current.Use();
            //        return;
            //    }
            //    break;
            //case EventType.ExecuteCommand:
            //    if (this.propertiesInspectorHasFocus() || EditorApplication.isPlaying)
            //    {
            //        return;
            //    }
            //    if (Event.current.commandName.Equals("Copy"))
            //    {
            //        this.copyNodes();
            //        Event.current.Use();
            //        return;
            //    }
            //    if (Event.current.commandName.Equals("Paste"))
            //    {
            //        this.pasteNodes();
            //        Event.current.Use();
            //        return;
            //    }
            //    if (Event.current.commandName.Equals("Cut"))
            //    {
            //        this.cutNodes();
            //        Event.current.Use();
            //        return;
            //    }
            //    if (Event.current.commandName.Equals("SelectAll"))
            //    {
            //        this.mGraphDesigner.selectAll();
            //        Event.current.Use();
            //        return;
            //    }
            //    if (Event.current.commandName.Equals("Duplicate"))
            //    {
            //        this.duplicateNodes();
            //        Event.current.Use();
            //    }
            //    break;
            default:
                return;
        }
    }
    /// <summary>
    /// 鼠标左键按下
    /// </summary>
    /// <param name="clickCount"></param>
    /// <returns></returns>
    private bool leftMouseDown(int clickCount)
    {
        Vector2 point;
        //点击点是否在绘图区域内
        if (!this.getMousePositionInGraph(out point))
        {
            return false;
        }

        //获取点击的任务
        Task node = mGraphDesigner.nodeAt(point, this.mGraphOffset);
        if (Event.current.modifiers == EventModifiers.Alt)
        {//是否按住了alt按键
            if (node != null)
            {//点击了一个任务
                mGraphDesigner.select(node);
            }
        }
        else
        {
            mGraphDesigner.clearNodeSelection();//清空以前选中的节点
            if (node != null)
            {//点击了一个任务
                NodeConnection nodeConnection;
                if ((nodeConnection = node.nodeConnectionRectContains(point, this.mGraphOffset)) != null)
                {//点击到连线区域
                    this.mGraphDesigner.ActiveNodeConnection = nodeConnection;
                    return true;
                }
                else
                {
                    mGraphDesigner.select(node);
                }
            }
        }
        return true;
    }
    /// <summary>
    /// 鼠标左键拖动
    /// </summary>
    /// <returns></returns>
    private bool leftMouseDragged()
    {
        Vector2 vector;
        if (!this.getMousePositionInGraph(out vector))
        {
            return false;
        }
        if (Event.current.modifiers != EventModifiers.Alt)
        {//不是拖动
            //if (this.isReferencingTasks())
            //{
            //    return true;
            //}
            //if (this.mIsSelecting)
            //{//先点击了绘图区域
            //    this.mGraphDesigner.clearNodeSelection();//清理掉选中的节点
            //    List<NodeDesigner> list = this.mGraphDesigner.nodesAt(this.getSelectionArea(), this.mGraphOffset);
            //    if (list != null)
            //    {//区域选择
            //        for (int i = 0; i < list.Count; i++)
            //        {
            //            this.mGraphDesigner.select(list[i]);
            //        }
            //    }
            //    return true;
            //}
            //if (this.mGraphDesigner.ActiveNodeConnection != null)
            //{
            //    return true;
            //}
        }
        if( this.mGraphDesigner.dragSelectedNodes(Event.current.delta / this.mGraphZoom))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 鼠标左键抬起
    /// </summary>
    /// <returns></returns>
    private bool leftMouseRelease()
    {
        if (this.mGraphDesigner.ActiveNodeConnection != null)
        {//存在连接的节点
            Vector2 point;
            if (!this.getMousePositionInGraph(out point))
            {
                this.mGraphDesigner.ActiveNodeConnection = null;
                return false;
            }
            Task nodeDesigner = this.mGraphDesigner.nodeAt(point, this.mGraphOffset);
            if (nodeDesigner != null && !nodeDesigner.Equals(this.mGraphDesigner.ActiveNodeConnection.OriginatingNodeDesigner))
            {//连接节点
                this.mGraphDesigner.connectNodes(nodeDesigner);
            }
            else
            {
                this.mGraphDesigner.ActiveNodeConnection = null;
            }
            return true;
        }
        return false;

    }
    #endregion

    public void addTaskCallback(object obj)
    {

    }


    #region 节点操作
    /// <summary>
    /// 添加一个任务
    /// </summary>
    /// <param name="type"></param>
    /// <param name="useMousePosition"></param>
    public void addTask(Type type, bool useMousePosition)
    {
        //得到任务显示的坐标
        Vector2 vector = new Vector2(MidRect.width / (2f * this.mGraphZoom), 150f);
        if (useMousePosition)
        {
            this.getMousePositionInGraph(out vector);
        }
        vector -= this.mGraphOffset;
        //====添加节点
        this.mGraphDesigner.addNode(type, vector);
    }

    #endregion


    #region 数据操作
    private List<SkillData> Datas = new List<SkillData>();
    private SkillData CurrentData = null;
    /// <summary>
    /// 新建
    /// </summary>
    private void NewBehavior()
    {
        SkillData sd = new SkillData();
        Datas.Add(sd);
        LoadDataSource(sd);
        base.Repaint();
    }
    /// <summary>
    /// 加载选择的数据源
    /// </summary>
    /// <param name="behaviorSource"></param>
    private void LoadDataSource(SkillData _data)
    {
        CurrentData = _data;//给当前行为树赋值

        //设计器加载数据
        this.mGraphDesigner.Load(CurrentData);

        //Vector2 vector2 = this.mGraphDesigner.rootNodePosition();
        //this.mGraphOffset = new Vector2(-vector2.x +this.MidRect.width / (2f * this.mGraphZoom) - 50f, -vector2.y + 50f);

    }
    #endregion
}
