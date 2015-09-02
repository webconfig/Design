using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

/// <summary>
/// 序列化Xml
/// </summary>
public class SerializeXml : UnityEngine.Object
{
    /// <summary>
    /// 序列化行为树
    /// </summary>
    /// <param name="data"></param>
    /// <param name="newBehaviorSource"></param>
    /// <returns></returns>
    public static void Serialize(SkillData data, out string str, out string str_ui)
    {
        System.Text.StringBuilder _str = new System.Text.StringBuilder();
        System.Text.StringBuilder _str_ui = new System.Text.StringBuilder();
        _str_ui.AppendFormat("<skill id=\"{0}\" name=\"{1}\">\r\n", data.Id, data.Name);
        _str.AppendFormat("<skill id=\"{0}\" name=\"{1}\" cd=\"{2}\">\r\n", data.Id, data.Name, data.CD.ToString());

        System.Text.StringBuilder _str_var = new StringBuilder();
        _str_var.Append("<variables>\r\n");
        foreach (var item in data.Variables)
        {
            _str_var.AppendFormat("<var type=\"{0}\" name=\"{1}\" />\r\n", item.Value.ValueType.ToString(), item.Key);
        }

        //for (int i = 0; i < data.Variables.Count; i++)
        //{
        //    _str_var.AppendFormat("<var type=\"{0}\" name=\"{1}\" />\r\n", data.Variables[i].ValueType.ToString(), data.Variables[i].name);
        //}
        _str_var.Append("</variables>\r\n");
        _str.Append(_str_var.ToString());


        for (int i = 0; i < data.Datas.Count; i++)
        {
            _str.Append(Data_ToString(SerializeTask_Data(data.Datas[i])));//序列化内容
            _str_ui.Append(UI_ToString(SerializeTask_UI(data.Datas[i])));//序列化UI相关
        }
        _str_ui.Append("</skill>\r\n");
        _str.Append("</skill>\r\n");
        str = _str.ToString();
        str_ui = _str_ui.ToString();



    }

    /// <summary>
    /// 序列化任务
    /// </summary>
    /// <param name="task"></param>
    /// <param name="behavior"></param>
    /// <param name="taskCount"></param>
    /// <returns></returns>
    public static SerializeData SerializeTask_UI(Task task)
    {
        SerializeData xd = new SerializeData();
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary.Add("ObjectType", task.GetType().ToString());
        task.NodeData.serialize_ui(dictionary);
        task.SerializeUI(dictionary);
        xd.property = dictionary;


        foreach (var item in task.OutLinks)
        {
            List<SerializeData> sds = new List<SerializeData>();
            for (int i = 0; i < item.Value.Childs.Count; i++)
            {
                SerializeData xd_child = SerializeTask_UI(item.Value.Childs[i]);
                sds.Add(xd_child);
            }
            xd.childs.Add(item.Key, sds);
        }
        return xd;
    }

    /// <summary>
    /// 序列化任务
    /// </summary>
    /// <param name="task"></param>
    /// <param name="behavior"></param>
    /// <param name="taskCount"></param>
    /// <returns></returns>
    public static SerializeData SerializeTask_Data(Task task)
    {
        SerializeData xd = new SerializeData();
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary.Add("Name", task.NodeData.FriendlyName);
        task.Serialize(dictionary);
        xd.childs_self = task.SerializeChild();
        xd.property = dictionary;

        foreach (var item in task.OutLinks)
        {
            List<SerializeData> sds = new List<SerializeData>();
            for (int i = 0; i < item.Value.Childs.Count; i++)
            {
                SerializeData xd_child = SerializeTask_Data(item.Value.Childs[i]);
                sds.Add(xd_child);
            }
            xd.childs.Add(item.Key, sds);
        }
        return xd;
    }

    public static string UI_ToString(SerializeData data)
    {
        StringBuilder str = new StringBuilder();
        str.Append("<item ");
        foreach (var item in data.property)
        {
            str.AppendFormat("{0}=\"{1}\" ", item.Key, item.Value);
        }

        str.Append(">");
        bool havedata = false;
        foreach (var item in data.childs)
        {
            if (item.Value.Count > 0)
            {
                havedata = true;
                str.AppendFormat("\r\n<{0}>\r\n", item.Key);
                for (int i = 0; i < item.Value.Count; i++)
                {
                    str.Append(UI_ToString(item.Value[i]));
                }
                str.AppendFormat("</{0}>", item.Key);
            }
        }
        if (havedata)
        {
            str.Append("\r\n");
        }
        str.Append("</item>\r\n");

        return str.ToString();
    }
    public static string Data_ToString(SerializeData data)
    {
        StringBuilder str = new StringBuilder();
        string name = data.property["Name"];
        str.AppendFormat("<{0} ", name);
        data.property.Remove("Name");
        foreach (var item in data.property)
        {
            str.AppendFormat("{0}=\"{1}\" ", item.Key, item.Value);
        }
        if (string.IsNullOrEmpty(data.childs_self))
        {
            str.Append("/>\r\n");
        }
        else
        {
            str.AppendFormat(">\r\n{0}</{1}>\r\n", data.childs_self, name);
        }

        foreach (var item in data.childs)
        {
            for (int i = 0; i < item.Value.Count; i++)
            {
                str.Append(Data_ToString(item.Value[i]));
            }
        }
        return str.ToString();
    }
}
public class SerializeData
{
    /// <summary>
    /// 属性数据
    /// </summary>
    public Dictionary<string, string> property;


    /// <summary>
    /// 自己内部儿子节点
    /// </summary>
    public string childs_self;



    /// <summary>
    /// 儿子节点
    /// </summary>
    public Dictionary<string, List<SerializeData>> childs = new Dictionary<string, List<SerializeData>>();
}