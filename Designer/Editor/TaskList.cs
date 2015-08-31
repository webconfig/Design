using SkillEditor.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


    [Serializable]
    public class TaskList : ScriptableObject
    {
        public enum TaskTypes
        {
            Action,
            Composite,
            Conditional,
            Decorator,
            Last
        }

        private class SearchableType
        {
            private Type mType;

            private bool mVisible = true;

            public Type Type
            {
                get
                {
                    return this.mType;
                }
            }

            public bool Visible
            {
                get
                {
                    return this.mVisible;
                }
                set
                {
                    this.mVisible = value;
                }
            }

            public SearchableType(Type type)
            {
                this.mType = type;
            }
        }

        private class CategoryList
        {
            private string mName = "";

            private string mFullpath = "";

            private List<TaskList.CategoryList> mSubcategories;

            private List<TaskList.SearchableType> mTasks;

            private bool mExpanded = true;

            private bool mVisible = true;

            private int mID;

            public string Name
            {
                get
                {
                    return this.mName;
                }
            }

            public string Fullpath
            {
                get
                {
                    return this.mFullpath;
                }
            }

            public List<TaskList.CategoryList> Subcategories
            {
                get
                {
                    return this.mSubcategories;
                }
            }

            public List<TaskList.SearchableType> Tasks
            {
                get
                {
                    return this.mTasks;
                }
            }

            public bool Expanded
            {
                get
                {
                    return this.mExpanded;
                }
                set
                {
                    this.mExpanded = value;
                }
            }

            public bool Visible
            {
                get
                {
                    return this.mVisible;
                }
                set
                {
                    this.mVisible = value;
                }
            }

            public int ID
            {
                get
                {
                    return this.mID;
                }
            }

            public CategoryList(string name, string fullpath, bool expanded, int id)
            {
                this.mName = name;
                this.mFullpath = fullpath;
                this.mExpanded = expanded;
                this.mID = id;
            }

            public void addSubcategory(TaskList.CategoryList category)
            {
                if (this.mSubcategories == null)
                {
                    this.mSubcategories = new List<TaskList.CategoryList>();
                }
                this.mSubcategories.Add(category);
            }

            public void addTask(Type taskType)
            {
                if (this.mTasks == null)
                {
                    this.mTasks = new List<TaskList.SearchableType>();
                }
                this.mTasks.Add(new TaskList.SearchableType(taskType));
            }
        }

        private List<TaskList.CategoryList> mCategoryList;

        private Vector2 mScrollPosition = Vector2.zero;

        private string mSearchString = "";

        private bool mFocusSearch;

        public void OnEnable()
        {
            base.hideFlags = HideFlags.HideAndDontSave;
        }

        public void init()
        {
            this.mCategoryList = new List<TaskList.CategoryList>();
            List<Type> list = new List<Type>();
            for (int i = 0; i < 4; i++)
            {
                Assembly assembly = null;
                try
                {
                    switch (i)
                    {
                        case 0:
                            assembly = Assembly.Load("Assembly-CSharp");
                            break;
                        case 1:
                            assembly = Assembly.Load("Assembly-CSharp-firstpass");
                            break;
                        case 2:
                            assembly = Assembly.Load("Assembly-UnityScript");
                            break;
                        case 3:
                            assembly = Assembly.Load("Assembly-UnityScript-firstpass");
                            break;
                    }
                }
                catch (Exception)
                {
                    assembly = null;
                }
                if (assembly != null)
                {
                    Type[] types = assembly.GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        if (!types[j].IsAbstract && types[j].IsSubclassOf(typeof(Skill_Time)))
                        {
                            list.Add(types[j]);
                        }
                    }
                }
            }

            list.Sort(new AlphanumComparator<Type>());


            //形成树状结构
            Dictionary<string, TaskList.CategoryList> dictionary = new Dictionary<string, TaskList.CategoryList>();
            int id = 0;
            for (int k = 0; k < list.Count; k++)
            {
                string text = "";
                TaskCategoryAttribute[] array;
                if ((array = (list[k].GetCustomAttributes(typeof(TaskCategoryAttribute), false) as TaskCategoryAttribute[])).Length > 0)
                {
                    text = text + "/" + array[0].Category;
                }
                else
                {
                    continue;
                }
                string text2 = "";
                string[] array2 = text.Split(new char[]
				{
					'/'
				});
                TaskList.CategoryList categoryList = null;
                TaskList.CategoryList categoryList2;
                for (int l = 0; l < array2.Length; l++)
                {
                    if (l > 0)
                    {
                        text2 += "/";
                    }
                    text2 += array2[l];
                    if (!dictionary.ContainsKey(text2))
                    {
                        categoryList2 = new TaskList.CategoryList(array2[l], text2, this.PreviouslyExpanded(id), id++);
                        if (categoryList == null)
                        {
                            this.mCategoryList.Add(categoryList2);
                        }
                        else
                        {
                            categoryList.addSubcategory(categoryList2);
                        }
                        dictionary.Add(text2, categoryList2);
                    }
                    else
                    {
                        categoryList2 = dictionary[text2];
                    }
                    categoryList = categoryList2;
                }
                categoryList2 = dictionary[text2];
                categoryList2.addTask(list[k]);
            }
            this.Search(this.mSearchString.ToLower().Replace(" ", ""), this.mCategoryList);
        }

        public void addTasksToMenu(SkillWindow window, ref GenericMenu genericMenu)
        {
            this.addCategoryTasksToMenu(window, ref genericMenu, this.mCategoryList);
        }

        private void addCategoryTasksToMenu(SkillWindow window, ref GenericMenu genericMenu, List<TaskList.CategoryList> categoryList)
        {
            for (int i = 0; i < categoryList.Count; i++)
            {
                if (categoryList[i].Subcategories != null)
                {
                    this.addCategoryTasksToMenu(window, ref genericMenu, categoryList[i].Subcategories);
                }
                if (categoryList[i].Tasks != null)
                {
                    for (int j = 0; j < categoryList[i].Tasks.Count; j++)
                    {
                        genericMenu.AddItem(new GUIContent(string.Format("Add Task/{0}/{1}", categoryList[i].Fullpath,categoryList[i].Tasks[j].Type.Name.ToString())), false, new GenericMenu.MenuFunction2(window.addTaskCallback), categoryList[i].Tasks[j].Type);
                    }
                }
            }
        }

        public void focusSearchField()
        {
            this.mFocusSearch = true;
        }
        /// <summary>
        /// 绘制行为树所有的行为节点
        /// </summary>
        /// <param name="window"></param>
        public void drawTaskList(SkillWindow window)
        {
            //搜索
            GUILayout.BeginHorizontal(new GUILayoutOption[0]);
            GUI.SetNextControlName("Search");
            string value = GUILayout.TextField(this.mSearchString, GUI.skin.FindStyle("ToolbarSeachTextField"), new GUILayoutOption[0]);
            if (this.mFocusSearch)
            {
                GUI.FocusControl("Search");
                this.mFocusSearch = false;
            }
            if (!this.mSearchString.Equals(value))
            {
                this.mSearchString = value;
                this.Search(this.mSearchString.ToLower().Replace(" ", ""), this.mCategoryList);
            }
            if (GUILayout.Button("", this.mSearchString.Equals("") ? GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty") : GUI.skin.FindStyle("ToolbarSeachCancelButton"), new GUILayoutOption[0]))
            {
                this.mSearchString = "";
                this.Search("", this.mCategoryList);
                GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4f);
            this.mScrollPosition = GUILayout.BeginScrollView(this.mScrollPosition, new GUILayoutOption[0]);

            for (int i = 0; i < mCategoryList.Count; i++)
            {
                this.DrawCategory(window, this.mCategoryList[i]);
            }
            GUILayout.EndScrollView();
        }
        /// <summary>
        /// 绘制树的叶子结点
        /// </summary>
        /// <param name="window"></param>
        /// <param name="category"></param>
        private void DrawCategory(SkillWindow window, TaskList.CategoryList category)
        {
            if (category.Visible)
            {
                category.Expanded = EditorGUILayout.Foldout(category.Expanded, category.Name);
                this.SetExpanded(category.ID, category.Expanded);
                if (category.Expanded)
                {
                    EditorGUI.indentLevel = EditorGUI.indentLevel + 1;
                    if (category.Tasks != null)
                    {
                        for (int i = 0; i < category.Tasks.Count; i++)
                        {
                            if (category.Tasks[i].Visible)
                            {
                                GUILayout.BeginHorizontal(new GUILayoutOption[0]);
                                GUILayout.Space((float)(EditorGUI.indentLevel * 10));

                                TaskNameAttribute[] array = category.Tasks[i].Type.GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[];
                                if (GUILayout.Button(array[0].Name, EditorStyles.toolbarButton, new GUILayoutOption[0]))
                                {//点击按钮，添加一个任务
                                    window.addTask(category.Tasks[i].Type, false);
                                }


                                GUILayout.Space(3f);
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    if (category.Subcategories != null)
                    {
                        this.DrawCategoryTaskList(window, category.Subcategories);
                    }
                    EditorGUI.indentLevel = EditorGUI.indentLevel - 1;
                }
            }
        }

        private void DrawCategoryTaskList(SkillWindow window, List<TaskList.CategoryList> categoryList)
        {
            for (int i = 0; i < categoryList.Count; i++)
            {
                this.DrawCategory(window, categoryList[i]);
            }
        }

        private bool Search(string searchString, List<TaskList.CategoryList> categoryList)
        {
            bool result = searchString.Equals("");
            for (int i = 0; i < categoryList.Count; i++)
            {
                bool flag = false;
                categoryList[i].Visible = false;
                if (categoryList[i].Subcategories != null && this.Search(searchString, categoryList[i].Subcategories))
                {
                    categoryList[i].Visible = true;
                    result = true;
                }
                if (categoryList[i].Name.ToLower().Replace(" ", "").Contains(searchString))
                {
                    result = true;
                    flag = true;
                    categoryList[i].Visible = true;
                    if (categoryList[i].Subcategories != null)
                    {
                        this.markVisible(categoryList[i].Subcategories);
                    }
                }
                if (categoryList[i].Tasks != null)
                {
                    for (int j = 0; j < categoryList[i].Tasks.Count; j++)
                    {
                        categoryList[i].Tasks[j].Visible = searchString.Equals("");
                        if (flag || categoryList[i].Tasks[j].Type.Name.ToLower().Replace(" ", "").Contains(searchString))
                        {
                            categoryList[i].Tasks[j].Visible = true;
                            result = true;
                            categoryList[i].Visible = true;
                        }
                    }
                }
            }
            return result;
        }

        private void markVisible(List<TaskList.CategoryList> categoryList)
        {
            for (int i = 0; i < categoryList.Count; i++)
            {
                categoryList[i].Visible = true;
                if (categoryList[i].Subcategories != null)
                {
                    this.markVisible(categoryList[i].Subcategories);
                }
                if (categoryList[i].Tasks != null)
                {
                    for (int j = 0; j < categoryList[i].Tasks.Count; j++)
                    {
                        categoryList[i].Tasks[j].Visible = true;
                    }
                }
            }
        }

        private bool PreviouslyExpanded(int id)
        {
            return EditorPrefs.GetBool("BehaviorDesignerTaskList" + id, true);
        }

        private void SetExpanded(int id, bool visible)
        {
            EditorPrefs.SetBool("BehaviorDesignerTaskList" + id, visible);
        }
    }

