using SkillEditor.Runtime.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public abstract class Task : ScriptableObject
{
    [System.NonSerialized]
    public DesignerNodeData NodeData;
    public int ID;
    [System.NonSerialized]
    public int ReferenceID;

    

    #region 编辑器相关
    public bool IsInit = false;

    /// <summary>
    /// 节点的任务名称
    /// </summary>
    [System.NonSerialized]
    public string taskName = "";
    /// <summary>
    /// 连接外部的线
    /// </summary>
    [System.NonSerialized]
    public List<NodeConnection> InConnections = new List<NodeConnection>();

    #region 数据
    /// <summary>
    /// 外接口
    /// </summary>
    public Dictionary<string, TaskOutLink> OutLinks = new Dictionary<string, TaskOutLink>();

    /// <summary>
    /// 序列化节点内容
    /// </summary>
    /// <returns></returns>
    public virtual void Serialize(Dictionary<string, string> dictionary)
    {
        return;
    }

    /// <summary>
    /// 序列化节点内容的子节点
    /// </summary>
    /// <returns></returns>
    public virtual string SerializeChild()
    {
        return string.Empty;
    }

    /// <summary>
    /// 序列化节点内容
    /// </summary>
    /// <returns></returns>
    public virtual void SerializeUI(Dictionary<string, string> dictionary)
    {
        return;
    }

    public virtual void Deserialize(XmlNode node)
    {

    }
    public virtual void GetOutLinks(List<TaskOutLink> datas)
    {
        return;
    }
    #endregion


    #region GUI
    /// <summary>
    /// 宽度
    /// </summary>
    [System.NonSerialized]
    public float Width = 100;

    /// <summary>
    /// 高度
    /// </summary>
    [System.NonSerialized]
    public float Height = 40;
    public static readonly int TopConnectionHeight = 14;
    public static readonly int BottomConnectionHeight = 16;
    public static readonly int ConnectionWidth = 21;
    public static readonly int TaskBackgroundShadowSize = 3;
    public static readonly int TitleHeight = 20;

    /// <summary>
    /// 是否选中
    /// </summary>
    [System.NonSerialized]
    public bool mSelected;

    public virtual void DrawNode(Vector2 offset,  bool disabled)
    {
        Rect rect = this.rectangle(offset, false);
        GUI.color = (disabled || NodeData.Disabled) ? new Color(0.7f, 0.7f, 0.7f) : Color.white;

        Color k = GUI.backgroundColor;
        if (this.mSelected)
        {
            GUI.backgroundColor = Color.green;
        }
        else
        {
            GUI.backgroundColor = Color.yellow;
        }

        //绘制模块上端连接父节点处
        GUI.Box(new Rect(
            rect.x + (rect.width - ConnectionWidth) / 2f,
            rect.y - TopConnectionHeight - TaskBackgroundShadowSize + 3f,
            ConnectionWidth, TopConnectionHeight + TaskBackgroundShadowSize),
            "");

        //绘制模块的下端连接处
        DrawOutLink(rect);
        //绘制模块中间背景
        GUI.Box(rect, "");
        GUI.backgroundColor = k;

        //绘制模块的行为名称
        GUI.Label(new Rect(rect.x, rect.center.y - 5, rect.width, TitleHeight), this.ToString());
    }
    private void DrawOutLink(Rect rect)
    {
        //Debug.Log(OutLinks.Count);
        foreach (var item in OutLinks)
        {
            item.Value.Draw(rect);
        }
    }
    /// <summary>
    /// 是否包含一点
    /// </summary>
    /// <param name="point"></param>
    /// <param name="offset"></param>
    /// <param name="includeConnections"></param>
    /// <returns></returns>
    public bool contains(Vector2 point, Vector2 offset, bool includeConnections)
    {
        return this.rectangle(offset, includeConnections).Contains(point);
    }
    public Rect rectangle(Vector2 offset, bool includeConnections)
    {

        float x = NodeData.Position.x + offset.x - Width / 2f;
        float y = NodeData.Position.y + offset.y;

        Rect result = new Rect(x, y, Width, Height);

        if (includeConnections)
        {
            result.yMin = result.yMin - TopConnectionHeight;
            result.yMax = result.yMax + BottomConnectionHeight;
        }
        return result;
    }

    /// <summary>
    /// 判断点是否在连线区域内
    /// </summary>
    /// <param name="point"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public NodeConnection nodeConnectionRectContains(Vector2 point, Vector2 offset)
    {
        TaskOutLink ol = IsIncomingConnectionRect(offset, point);
        if (ol != null)
        {
            NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
            nodeConnection.loadConnection(this, NodeConnectionType.Outgoing);
            nodeConnection.Originating = ol;
            return nodeConnection;
        }


        return null;
    }
    /// <summary>
    /// 判断是否在连线区域内
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    private TaskOutLink IsIncomingConnectionRect(Vector2 offset, Vector2 point)
    {
        Rect rect = this.rectangle(offset, false);

        foreach (var item in OutLinks)
        {
            if (item.Value.IsInRect(rect, point))
            {
                return item.Value;
            }
        }
        return null;
    }
    #endregion
    public Vector2 getIncomingConnectionRect(Vector2 offset, NodeConnectionType connectionType)
    {
        Vector2 result;

        Rect rect = this.IncomingConnectionRect(offset);
        result = new Vector2(rect.center.x, rect.y +Task.TopConnectionHeight / 2);

        return result;
    }
    public Rect IncomingConnectionRect(Vector2 offset)
    {
        Rect rect = this.rectangle(offset, false);
        return new Rect(rect.x + (rect.width - Task.ConnectionWidth) / 2f, rect.y - Task.TopConnectionHeight, Task.ConnectionWidth, Task.TopConnectionHeight);
    }
    public string GetOutLinkIds(string key)
    {
        if (!OutLinks.ContainsKey(key)) { return string.Empty; }

        string ok_str = string.Empty;
        for (int i = 0; i < OutLinks[key].Childs.Count; i++)
        {
            ok_str += OutLinks[key].Childs[i].ID + ",";
        }
        if (ok_str != string.Empty)
        {
            ok_str = ok_str.TrimEnd(',');
        }
        return ok_str;
    }

    /// <summary>
    /// 初始化该节点
    /// </summary>
    /// <param name="task"></param>
    /// <param name="behaviorSource"></param>
    /// <param name="position"></param>
    /// <param name="id"></param>
    public void loadNode(Vector2 position)
    {

        NodeData = new DesignerNodeData();
        NodeData.Position = position;
        SetName();
        NodeData.FriendlyName = this.taskName;


        //初始化向外部接口
        List<TaskOutLink> datas = new List<TaskOutLink>();
        GetOutLinks(datas);
        float avg_widht = Width / (datas.Count + 1);
        float width = ConnectionWidth;
        float height = BottomConnectionHeight +Task.TaskBackgroundShadowSize;

        for (int i = 0; i < datas.Count; i++)
        {
            datas[i].Parent = this;
            datas[i].Index = i + 1;
            datas[i].AverageWidth = avg_widht;
            datas[i].Width = width;
            datas[i].Height = height;
            OutLinks.Add(datas[i].name, datas[i]);
        }
    }
    private void SetName()
    {
        TaskNameAttribute[] array =this.GetType().GetCustomAttributes(typeof(TaskNameAttribute), false) as TaskNameAttribute[];
        //获取名称
        this.taskName = array[0].Name.ToString();
    }
    /// <summary>
    /// 添加一个连接
    /// </summary>
    /// <param name="childNodeDesigner"></param>
    /// <param name="nodeConnection"></param>
    public void AddConnection(Task child, NodeConnection Connection, bool addtodata)
    {
        child.InConnections.Add(Connection);
        Connection.DestinationNodeDesigner = child;
        Connection.Originating.AddNodeConnection(Connection, addtodata);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public void Init()
    {
        if (!IsInit)
        {
            IsInit = true;
            float avg_widht = Width / (OutLinks.Count + 1);
            float width = ConnectionWidth;
            float height = BottomConnectionHeight + Task.TaskBackgroundShadowSize;

            foreach (var item in OutLinks)
            {
                item.Value.Parent = this;
                item.Value.AverageWidth = avg_widht;
                item.Value.Width = width;
                item.Value.Height = height;


                foreach(var data in item.Value.Childs)
                {

                    NodeConnection nodeConnection = ScriptableObject.CreateInstance<NodeConnection>();
                    nodeConnection.loadConnection(this, NodeConnectionType.Outgoing);
                    nodeConnection.Originating = item.Value;
                    nodeConnection.DestinationNodeDesigner = data;
                    item.Value.AddNodeConnection(nodeConnection, false);
                    data.InConnections.Add(nodeConnection);
                    data.Init();
                }
            }
        }
    }

    #endregion
}

    /// <summary>
    /// 外接口
    /// </summary>
public class TaskOutLink
{
    /// <summary>
    /// 名称
    /// </summary>
    public string name;

    /// <summary>
    /// 儿子节点
    /// </summary>
    public List<Task> Childs = new List<Task>();

    /// <summary>
    /// 外接口设UI
    /// </summary>
    public object OutLinkDesigner;

    #region 编辑器
    /// <summary>
    /// 父节点
    /// </summary>
    public Task Parent;
    /// <summary>
    /// 连接到子节点的连线
    /// </summary>
    public List<NodeConnection> OutgoingNodeConnections=new List<NodeConnection>();
    /// <summary>
    /// 序号
    /// </summary>
    public int Index;
    /// <summary>
    /// 平均宽度
    /// </summary>
    public float AverageWidth;

    /// <summary>
    /// 模块宽度
    /// </summary>
    public float Width;

    /// <summary>
    /// 模块高度
    /// </summary>
    public float Height;

    /// <summary>
    /// 点是否在当前矩形范围内
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool IsInRect(Rect rect, Vector2 point)
    {
        float x = Index * AverageWidth - Width / 2 + rect.x;
        float y = rect.yMax - 3f;
        return new Rect(x, y, Width, Height).Contains(point);
    }

    /// <summary>
    /// 绘制
    /// </summary>
    /// <param name="rect"></param>
    public void Draw(Rect rect)
    {
        float x = Index * AverageWidth - Width / 2 + rect.x;
        float y = rect.yMax - 3f;
        Rect r = new Rect(x, y, Width, Height);
        GUI.Box(r, "");
        GUI.Label(r, name);
    }
    /// <summary>
    /// 绘制连线
    /// </summary>
    public void DrawLine(Vector2 offset, float graphZoom)
    {
        for (int i = 0; i < OutgoingNodeConnections.Count; i++)
        {
            //获取连线的起点
            Vector2 position_begin = GetConnectionPosition(offset);
            OutgoingNodeConnections[i].drawConnection(position_begin, OutgoingNodeConnections[i].DestinationNodeDesigner.getIncomingConnectionRect(offset, NodeConnectionType.Incoming), graphZoom, true);
        }
    }
    /// <summary>
    /// 获取外联线的起点位置
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public Vector2 GetConnectionPosition(Vector2 offset)
    {
        Rect parent_rect = Parent.rectangle(offset, false);
        float x = Index * AverageWidth - Width / 2 + parent_rect.x;
        float y = parent_rect.yMax - 3f;
        Rect rect_self = new Rect(x, y, Width, Height);

        return new Vector2(rect_self.center.x, rect_self.yMax - Task.BottomConnectionHeight / 2);
    }

    /// <summary>
    /// 添加一条连线
    /// </summary>
    /// <param name="Conn"></param>
    public void AddNodeConnection(NodeConnection Conn, bool addtodata)
    {
        OutgoingNodeConnections.Add(Conn);
        if (addtodata)
        {
            Childs.Add(Conn.DestinationNodeDesigner);
        }
    }


    /// <summary>
    ///移除一条连线
    /// </summary>
    /// <param name="conn"></param>
    public void RemoveConnection(NodeConnection conn)
    {
        OutgoingNodeConnections.Remove(conn);
        Childs.Remove(conn.DestinationNodeDesigner);
    }
    #endregion
}



