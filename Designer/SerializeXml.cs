using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
            for (int i = 0; i < origBehaviorSource.DetachedTasks.Count; i++)
            {
                _str.Append(origBehaviorSource.DetachedTasks[i].Serialize());//序列化内容
                _str_ui.Append(SerializeHelp.ToString(SerializeTask_UI(origBehaviorSource.DetachedTasks[i])));//序列化UI相关
            }
            _str_ui.Append("</skill>");

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

