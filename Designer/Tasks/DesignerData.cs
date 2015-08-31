using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using UnityEngine;

[Serializable]
public class DesignerNodeData
{
    [SerializeField]
    private List<string> watchedFieldNames;
    /// <summary>
    /// UI 坐标
    /// </summary>
    public Vector2 Position;
    public string FriendlyName = "";
    public string Comment = "";
    public bool IsBreakpoint;
    public Texture Icon;
    public bool Collapsed;
    public bool Disabled;
    public float PushTime;
    public float PopTime = -1f;

    public void deserialize_ui(XmlNode node)
    {
        Position = Skill_Manager.GetXmlAttrVector(node, "Position");

        FriendlyName = node.Attributes["FriendlyName"].Value;
        if (node.Attributes["Comment"] != null)
        {
            Comment = node.Attributes["Comment"].Value;
        }
        if (node.Attributes["Collapsed"] != null)
        {
            Collapsed = node.Attributes["Collapsed"].Value == "true";
        }
        if (node.Attributes["Disabled"] != null)
        {
            Disabled = node.Attributes["Disabled"].Value == "true";
        }
    }

    public void serialize_ui(Dictionary<string, string> dictionary)
    {
        dictionary.Add("Position", Position.ToString());
        if (this.FriendlyName.Length > 0)
        {
            dictionary.Add("FriendlyName", this.FriendlyName);
        }
        if (this.Comment.Length > 0)
        {
            dictionary.Add("Comment", this.Comment);
        }
        if (this.Collapsed)
        {
            dictionary.Add("Collapsed", this.Collapsed.ToString());
        }
        if (this.Disabled)
        {
            dictionary.Add("Disabled", this.Disabled.ToString());
        }

    }
}