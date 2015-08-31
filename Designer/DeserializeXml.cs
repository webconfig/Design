using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;
/// <summary>
/// 反序列化数据
/// </summary>
public class DeserializeXml : UnityEngine.Object
{
    public static SkillData Deserialize(XmlNode dataui, XmlNode data)
    {
        SkillData SkillData = ScriptableObject.CreateInstance<SkillData>();
        SkillData.Name = dataui.Attributes["name"].Value;
        SkillData.Id = int.Parse(dataui.Attributes["id"].Value);

        List<Task> list = new List<Task>();
        XmlNodeList childs = dataui.SelectNodes("item");
        foreach (XmlNode child in childs)
        {
            string k = child.Attributes["ID"].Value;

            XmlNode k1 = data.SelectSingleNode(@"*[@id=" + k + "]");

            Task tk = DeserializeTask(child, k1);
            list.Add(tk);

        }
        SkillData.Datas = list;
        return SkillData;
    }
    public static Task DeserializeTask(XmlNode dataui, XmlNode data)
    {
        string type_str = dataui.Attributes["ObjectType"].Value;

        Type type = Type.GetType(type_str);
        if (type == null) { type = Type.GetType(string.Format("{0}, Assembly-CSharp", type_str as string)); }
        if (type == null) { type = Type.GetType(string.Format("{0}, Assembly-CSharp-firstpass", type_str as string)); }
        if (type == null) { type = Type.GetType(string.Format("{0}, Assembly-UnityScript", type_str as string)); }
        if (type == null) { type = Type.GetType(string.Format("{0}, Assembly-UnityScript-firstpass", type_str as string)); }
        Task task = (ScriptableObject.CreateInstance(type) as Task);
        task.hideFlags = HideFlags.HideAndDontSave;


        task.NodeData = new DesignerNodeData();
        task.NodeData.deserialize_ui(dataui);
        task.Deserialize(data);



        List<TaskOutLink> datas = new List<TaskOutLink>();
        task.GetOutLinks(datas);

        task.OutLinks = new Dictionary<string, TaskOutLink>();
        for (int i = 0; i < datas.Count; i++)
        {
            task.OutLinks.Add(datas[i].name, datas[i]);
        }

        for (int i = 0; i < dataui.ChildNodes.Count; i++)
        {
            TaskOutLink item = task.OutLinks[dataui.ChildNodes[i].Name];
            for (int j = 0; j < dataui.ChildNodes[i].ChildNodes.Count; j++)
            {
                XmlNode k = data.ParentNode.SelectSingleNode(@"*[@id=" + dataui.ChildNodes[i].ChildNodes[j].Attributes["ID"].Value + "]");
                item.Childs.Add(DeserializeTask(dataui.ChildNodes[i].ChildNodes[j], k));
            }
        }

        return task;
    }


    private static object ValueToObject(Task task, Type type, object obj)
    {
        if (type.IsPrimitive || type.Equals(typeof(string)))
        {
            return Convert.ChangeType(obj, type);
        }
        return null;
    }

}