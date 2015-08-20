using System.Collections.Generic;
using System.Text;
using System.Xml;
using UnityEngine;
public class SerializeHelp
{
    public static string ToString(List<string> strs)
    {
        StringBuilder str = new StringBuilder();
        for(int i=0;i<strs.Count;i++)
        {
            str.Append(strs[i] + ",");
        }
        return str.ToString().TrimEnd(',');
    }

    public static string ToString(Dictionary<string, string> strs)
    {
        StringBuilder str = new StringBuilder();
       foreach(var item in strs)
       {
           str.Append(item.Key + ":" + item.Value+" ");
       }
        return str.ToString().TrimEnd(' ');
    }


    public static string ToString(XmlData data)
    {
        StringBuilder str = new StringBuilder();
        str.Append("<item ");
        foreach(var item in data.property)
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
            for(int i=0;i<data.childs.Count;i++)
            {
                str.Append(ToString(data.childs[i]));
            }
            str.Append("</item>");
        }
        return str.ToString();
    }
    public static Vector2 GetXmlAttrVector2(XmlNode node, string key)
    {
        //Debug.Log(key);
        if (node.Attributes[key] != null)
        {
            string str = node.Attributes[key].InnerText.TrimStart('(').TrimEnd(')');
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(',');
                return new Vector2(float.Parse(strs[0]), float.Parse(strs[1]));
            }
        }
        return Vector2.zero;
    }
}

