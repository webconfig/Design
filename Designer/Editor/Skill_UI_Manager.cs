using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BehaviorDesigner.Runtime;
using System.Xml;
public static class Skill_UI_Manager
{
    /// <summary>
    /// xml 路径
    /// </summary>
    public static string XmlPath;

    public static XmlDocument Root;

    public static List<Skill_UI> skills = new List<Skill_UI>();


    public static void Reload()
    {
        if(Root==null)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(XmlPath);
            Root = xml;
        }

        XmlNodeList nodes = Root.SelectNodes(@"/skills/skill");
        foreach(XmlNode  node in nodes)
        {
            skills.Add(new Skill_UI(node));
        }
    }

}

