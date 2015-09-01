using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Xml;

[Serializable]
public class GraphDesigner : ScriptableObject
{
    /// <summary>
    /// 当前选择的节点
    /// </summary>
    public List<Task> SelectedNodes = new List<Task>();
    /// <summary>
    /// 所有的节点
    /// </summary>
    public List<Task> DetachedNodes = new List<Task>();
    /// <summary>
    /// 正在连接的线
    /// </summary>
    public NodeConnection ActiveNodeConnection;

    #region 数据操作
    /// <summary>
    /// 加载一个数据源
    /// </summary>
    /// <param name="data"></param>
    public void Load(SkillData data)
    {
        clear();
        DetachedNodes.Clear();
        List<Task> list = data.Datas;
        if (list != null)
        {
            for (int j = 0; j < list.Count; j++)
            {
                list[j].Init();
                DetachedNodes.Add(list[j]);
            }
        }
    }
    /// <summary>
    /// 添加一个节点
    /// </summary>
    /// <param name="behaviorSource"></param>
    /// <param name="type"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public Task addNode(Type type, Vector2 position)
    {
        Task task;
        task = (ScriptableObject.CreateInstance(type) as Task);
        if (task == null)
        {
            Debug.LogError(string.Format("Unable to create task of type {0}. Is the class name the same as the file name?", type));
            return null;
        }
        task.loadNode(position);
        DetachedNodes.Add(task);
        return task;
    }
    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="behaviorSource"></param>
    public void save(SkillData _data)
    {
        if (DetachedNodes == null || DetachedNodes.Count <= 0)
        {
            return;
        }
        List<Task> list = new List<Task>();
        for (int i = 0; i < DetachedNodes.Count; i++)
        {
            list.Add(DetachedNodes[i]);
        }
        _data.save(list);
    }
    #endregion

    #region GUI
    public bool drawNodes(Vector2 mousePosition, Vector2 offset, float graphZoom)
    {
        bool result = false;
        if (DetachedNodes == null || DetachedNodes.Count <= 0) { return false; }

        //绘制正在连接的线（鼠标拖动的连线）
        if (mousePosition != new Vector2(-1f, -1f) && ActiveNodeConnection != null)
        {
            //获取连线的起点
            Vector2 position_begin = ActiveNodeConnection.Originating.GetConnectionPosition(offset);

            ActiveNodeConnection.HorizontalHeight = (position_begin.y + mousePosition.y) / 2f;
            ActiveNodeConnection.drawConnection(position_begin, mousePosition, graphZoom,true);
        }


        //绘制节点
        for (int i = 0; i < DetachedNodes.Count; i++)
        {
            //绘制自己和自己的子节点
            if(this.drawNodeChildren(DetachedNodes[i], offset, graphZoom, DetachedNodes[i].NodeData.Disabled))
            {
                result = true;
            }
        }
        return result;

    }
    public bool drawNodeChildren(Task task, Vector2 offset, float graphZoom, bool disabled)
    {
        //绘制自己
        task.DrawNode(offset,  disabled);

        //绘制外接口
        foreach (var item in task.OutLinks)
        {
            item.Value.DrawLine(offset, graphZoom);//绘制外接口的连线

            foreach (var child in item.Value.Childs)
            {//绘制子模块
                drawNodeChildren(child, offset, graphZoom, disabled);
            }
        }
        return true;

    }
    #endregion

    #region 查找结点
    /// <summary>
    /// 寻找点所在的任务节点
    /// </summary>
    /// <param name="point"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public Task nodeAt(Vector2 point, Vector2 offset)
    {
        if (DetachedNodes == null || DetachedNodes.Count <= 0)
        {
            return null;
        }
        Task result;
        for (int j = DetachedNodes.Count - 1; j > -1; j--)
        {
            if (DetachedNodes[j] != null && (result = nodeChildrenAt(DetachedNodes[j], point, offset)) != null)
            {
                return result;
            }
        }
        return null;
    }

    public Task nodeChildrenAt(Task nodeDesigner, Vector2 point, Vector2 offset)
    {
        if (nodeDesigner.contains(point, offset, true))
        {
            return nodeDesigner;
        }
        Task result = null;
        if (!nodeDesigner.NodeData.Collapsed && nodeDesigner.OutLinks.Count > 0)
        {
            foreach (var item in nodeDesigner.OutLinks)
            {
                foreach (var child in item.Value.Childs)
                {
                    result = nodeChildrenAt(child, point, offset);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
        }
        return result;
    }

    #endregion

    #region 选中节点
     /// <summary>
     /// 是否选中了节点
     /// </summary>
     /// <param name="node"></param>
     /// <returns></returns>
    public bool isSelected(Task node)
    {
        return SelectedNodes.Contains(node);
    }
    /// <summary>
    /// 让节点处于选中状态
    /// </summary>
    /// <param name="nodeDesigner"></param>
    public void select(Task node)
    {
        SelectedNodes.Add(node);
        node.mSelected = true;
        if (SelectedNodes.Count == 1)
        {//属性栏显示节点属性
            ReferencedTask(SelectedNodes[0], true);
        }
    }
    /// <summary>
    /// 清空所有选中的节点
    /// </summary>
    public void clearNodeSelection()
    {
        for (int i = 0; i < SelectedNodes.Count; i++)
        {
            SelectedNodes[i].mSelected = false;
        }
        SelectedNodes.Clear();
        ClearReferencedTask();
    }
    #endregion

    #region 属性栏
    /// <summary>
    /// 属性栏显示一个任务节点的属性
    /// </summary>
    /// <param name="task"></param>
    /// <param name="indicate"></param>
    private void ReferencedTask(Task task, bool indicate)
    {
        List<Task> referencedTasks = TaskInspector.GetReferencedTasks(task);
        //if (referencedTasks != null && referencedTasks.Count > 0)
        //{
        //    for (int i = 0; i < referencedTasks.Count; i++)
        //    {
        //        if (referencedTasks[i] != null && referencedTasks[i].NodeData != null)
        //        {
        //            Task nodeDesigner = referencedTasks[i].NodeData.NodeDesigner as NodeDesigner;
        //            if (nodeDesigner != null)
        //            {
        //                nodeDesigner.ShowReferenceIcon = indicate;
        //            }
        //        }
        //    }
        //}
    }
    /// <summary>
    /// 属性栏显示空
    /// </summary>
    /// <param name="task"></param>
    /// <param name="indicate"></param>
    private void ClearReferencedTask()
    {
        
    }

    #endregion

    #region 拖动节点
    /// <summary>
    /// 拖动节点
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="dragChildren"></param>
    /// <param name="hasDragged"></param>
    /// <returns></returns>
    public bool dragSelectedNodes(Vector2 delta)
    {
        if (SelectedNodes.Count == 0)
        {
            return false;
        }
        for (int i = 0; i < SelectedNodes.Count; i++)
        {
           movePosition(SelectedNodes[i],delta);
        }
        return true;
    }
    //---------------------
    /// <summary>
    /// 移动模块
    /// </summary>
    /// <param name="delta"></param>
    public void movePosition(Task task, Vector2 delta)
    {
        task.NodeData.Position += delta;

        //====子节点也动
        foreach (var item in task.OutLinks)
        {
            for (int j = 0; j < item.Value.Childs.Count; j++)
            {
                movePosition(item.Value.Childs[j], delta);
            }
        }
    }
    #endregion

    /// <summary>
    /// 连接两个节点
    /// </summary>
    /// <param name="behaviorSource"></param>
    /// <param name="nodeDesigner">结束模块</param>
    public void connectNodes(Task node)
    {
        NodeConnection nodeConnection = ActiveNodeConnection;
        ActiveNodeConnection = null;
        if (nodeConnection != null && !nodeConnection.OriginatingNodeDesigner.Equals(node))
        {//不是连接到自己
            Task originating = nodeConnection.OriginatingNodeDesigner;
            if (nodeConnection.NodeConnectionType == NodeConnectionType.Outgoing)
            {
                originating.AddConnection(node, nodeConnection, true);
            }
            DetachedNodes.Remove(nodeConnection.DestinationNodeDesigner);
        }
    }

    /// <summary>
    /// 删除节点
    /// </summary>
    /// <param name="behaviorSource"></param>
    /// <returns></returns>
    public bool delete(SkillData _data)
    {
        bool flag = false;
        //if (SelectedNodeConnections != null)
        //{
        //    for (int i = 0; i < SelectedNodeConnections.Count; i++)
        //    {
        //        this.removeConnection(SelectedNodeConnections[i], "Delete");
        //    }
        //    SelectedNodeConnections.Clear();
        //    flag = true;
        //}
        if (SelectedNodes != null)
        {
            //删除选中的节点
            for (int j = 0; j < SelectedNodes.Count; j++)
            {
                this.removeNode(SelectedNodes[j]);
            }
            SelectedNodes.Clear();
            flag = true;
        }
        if (flag)
        {
            this.save(_data);
        }
        return flag;
    }
    /// <summary>
    /// 删除一个节点
    /// </summary>
    /// <param name="nodeDesigner"></param>
    public void removeNode(Task node)
    {
        if (node.InConnections.Count > 0)
        {//有连接到外部的线
            for (int i = 0; i < node.InConnections.Count; i++)
            {//移除连线
                node.InConnections[i].Originating.RemoveConnection(node.InConnections[i]);
            }
        }
        else
        {
            DetachedNodes.Remove(node);
        }
    }
    public void clear()
    {

        //this.mPrevNodeSelectedID = null;

        //this.mNodeSelectedID.Clear();
        //SelectedNodes.Clear();
        //SelectedNodeConnections.Clear();
        //DetachedNodes = new List<NodeDesigner>();
    }


    #region 复制粘贴
    public List<Task> copy()
    {
        List<Task> result = new List<Task>();
        for (int i = 0; i < SelectedNodes.Count; i++)
        {
            string str_ui = SerializeXml.UI_ToString(SerializeXml.SerializeTask_UI(SelectedNodes[i]));
            string str_data = "<skill>" + SerializeXml.Data_ToString(SerializeXml.SerializeTask_Data(SelectedNodes[i])) + "</skill>";

            XmlDocument xml_ui = new XmlDocument();
            xml_ui.LoadXml(str_ui);
            XmlDocument xml_data = new XmlDocument();
            xml_data.LoadXml(str_data);
            Task item = DeserializeXml.DeserializeTask(xml_ui.FirstChild, xml_data.FirstChild.SelectSingleNode(@"*[@id=" + SelectedNodes[i].ID + "]"));
            result.Add(item);

        }
        return result;
    }
    #endregion
}

