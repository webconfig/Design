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
        public static  BehaviorSource Deserialize(XmlNode data)
        {
            BehaviorSource behaviorSource = new BehaviorSource();
            behaviorSource.behaviorName = data.Attributes["name"].Value;
            behaviorSource.BehaviorID =int.Parse(data.Attributes["id"].Value);

            List<Task> list = new List<Task>();
            XmlNodeList childs = data.SelectNodes("item");
            foreach (XmlNode child in childs)
            {
                Task tk = DeserializeTask(child);
                list.Add(tk);
            }
            behaviorSource.DetachedTasks = list;
            return behaviorSource;
        }
        private static Task DeserializeTask(XmlNode data)
        {
            string type_str = data.Attributes["ObjectType"].Value;

            Type type = Type.GetType(type_str);
            if (type == null) { type = Type.GetType(string.Format("{0}, Assembly-CSharp", type_str as string)); }
            if (type == null) { type = Type.GetType(string.Format("{0}, Assembly-CSharp-firstpass", type_str as string)); }
            if (type == null) { type = Type.GetType(string.Format("{0}, Assembly-UnityScript", type_str as string)); }
            if (type == null) { type = Type.GetType(string.Format("{0}, Assembly-UnityScript-firstpass", type_str as string)); }
            Task task = (ScriptableObject.CreateInstance(type) as Task);
            task.hideFlags = HideFlags.HideAndDontSave;


            task.NodeData = new DesignerNodeData();
            task.NodeData.deserialize_ui(data);

            XmlNodeList childs=data.SelectNodes("item");
            if (childs != null && childs.Count>0)
            {

                foreach (XmlNode child in childs)
                {
                    Task tk = DeserializeTask(child);

                    task.AddChild(tk, 0);
                }

            }
            return task;
        }

	}
}
