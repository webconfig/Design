using BehaviorDesigner.Runtime.Tasks;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
    public class DeserializeXml : UnityEngine.Object
	{
        public static  BehaviorSource Deserialize(XmlNode dataui,XmlNode  data)
        {
            BehaviorSource behaviorSource = new BehaviorSource();
            behaviorSource.behaviorName = dataui.Attributes["name"].Value;
            behaviorSource.BehaviorID = int.Parse(dataui.Attributes["id"].Value);

            List<Task> list = new List<Task>();
            XmlNodeList childs = dataui.SelectNodes("item");
            foreach (XmlNode child in childs)
            {
                string k = child.Attributes["ID"].Value;
                XmlNode k1 = data.SelectSingleNode(@"/*[@ID='" + k + "']");
                foreach(XmlNode xn in  data.ChildNodes)
                {
                    if(xn.Attributes["ID"].Value==child.Attributes["ID"].Value)
                    {
                        Task tk = DeserializeTask(child, xn);
                        list.Add(tk);
                    }
                }

                
            }
            behaviorSource.DetachedTasks = list;
            return behaviorSource;
        }
        private static Task DeserializeTask(XmlNode dataui, XmlNode data)
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
            DeserializeObject(task, task,data);
            XmlNodeList childs = dataui.SelectNodes("item");
            if (childs != null && childs.Count>0)
            {

                foreach (XmlNode child in childs)
                {
                    foreach (XmlNode xn in data.ParentNode.ChildNodes)
                    {
                        if (xn.Attributes["ID"].Value == child.Attributes["ID"].Value)
                        {
                            Task tk = DeserializeTask(child, xn);
                            task.AddChild(tk, 0);
                            break;
                        }
                    }
                }
            }
            return task;
        }

        private static void DeserializeObject(Task task, object obj,XmlNode data)
        {
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < fields.Length; i++)
            {
                if (data.Attributes[fields[i].Name] != null)
                {
                    if (fields[i].FieldType.IsArray)
                    {
                    }
                    else
                    {
                        Type fieldType = fields[i].FieldType;
                        fields[i].SetValue(obj, ValueToObject(task, fieldType, data.Attributes[fields[i].Name].Value));
                    }
                }
            }
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
}
