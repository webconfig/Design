using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace BehaviorDesigner.Editor
{
    /// <summary>
    /// 序列化Xml
    /// </summary>
    public class SerializeXml : UnityEngine.Object
    {
        /// <summary>
        /// 序列化行为树
        /// </summary>
        /// <param name="origBehaviorSource"></param>
        /// <param name="newBehaviorSource"></param>
        /// <returns></returns>
        public static void Serialize(BehaviorSource origBehaviorSource, out string str, out string str_ui)
        {
            System.Text.StringBuilder _str = new System.Text.StringBuilder();
            System.Text.StringBuilder _str_ui = new System.Text.StringBuilder();
            _str_ui.AppendFormat("<skill id=\"{0}\" name=\"{1}\">", origBehaviorSource.BehaviorID, origBehaviorSource.behaviorName);
            _str.AppendFormat("<skill id=\"{0}\" name=\"{1}\">", origBehaviorSource.BehaviorID, origBehaviorSource.behaviorName);
            for (int i = 0; i < origBehaviorSource.DetachedTasks.Count; i++)
            {
                _str.Append(Data_ToString(SerializeTask_Data(origBehaviorSource.DetachedTasks[i])));//序列化内容
                _str_ui.Append(UI_ToString(SerializeTask_UI(origBehaviorSource.DetachedTasks[i])));//序列化UI相关
            }
            _str_ui.Append("</skill>");
            _str.Append("</skill>");
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
        private static XmlData SerializeTask_UI(Task task)
        {
            XmlData xd = new XmlData();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("ObjectType", task.GetType().ToString());
            task.NodeData.serialize_ui(dictionary);
            task.SerializeUI(dictionary);
            xd.property = dictionary;
            //SerializeJSON.SerializeFields(task, ref dictionary, behavior);

            if (task.Children != null && task.Children.Count > 0)
            {
                for (int i = 0; i < task.Children.Count; i++)
                {
                    XmlData xd_child = SerializeTask_UI(task.Children[i]);
                    xd.childs.Add(xd_child);
                }
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
        private static XmlData SerializeTask_Data(Task task)
        {
            XmlData xd = new XmlData();
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary.Add("Name", task.NodeData.FriendlyName);
            task.Serialize(dictionary);
            xd.property = dictionary;
            //SerializeJSON.SerializeFields(task, ref dictionary, behavior);

            if (task.Children != null && task.Children.Count > 0)
            {
                for (int i = 0; i < task.Children.Count; i++)
                {
                    XmlData xd_child = SerializeTask_Data(task.Children[i]);
                    xd.childs.Add(xd_child);
                }
            }

            return xd;
        }

        public static string UI_ToString(XmlData data)
        {
            StringBuilder str = new StringBuilder();
            str.Append("<item ");
            foreach (var item in data.property)
            {
                str.AppendFormat("{0}=\"{1}\" ", item.Key, item.Value);
            }
            if (data.childs.Count == 0)
            {
                str.Append("/>");
            }
            else
            {
                str.Append(">");
                for (int i = 0; i < data.childs.Count; i++)
                {
                    str.Append(UI_ToString(data.childs[i]));
                }
                str.Append("</item>");
            }
            return str.ToString();
        }
        public static string Data_ToString(XmlData data)
        {
            StringBuilder str = new StringBuilder();
            str.AppendFormat("<{0} ", data.property["Name"]);
            data.property.Remove("Name");
            foreach (var item in data.property)
            {
                str.AppendFormat("{0}=\"{1}\" ", item.Key, item.Value);
            }

            str.Append("/>");


            for (int i = 0; i < data.childs.Count; i++)
            {
                str.Append(Data_ToString(data.childs[i]));
            }

            return str.ToString();
        }
    }
}

public class XmlData
{
    /// <summary>
    /// 属性数据
    /// </summary>
    public Dictionary<string, string> property;
    /// <summary>
    /// 儿子节点
    /// </summary>
    public List<XmlData> childs=new List<XmlData>();
}

