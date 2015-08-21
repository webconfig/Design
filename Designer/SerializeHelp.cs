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

