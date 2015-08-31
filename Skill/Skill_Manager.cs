using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
public class Skill_Manager : MonoBehaviour
{
    private static Skill_Manager _instance;
    public static Skill_Manager Instance
    {
        get
        {
            return _instance;
        }
    }
    #region xml 读取
    private static int XmlNullValu = 0;
    public static int GetXmlAttrInt(XmlNode node, string key)
    {
        //Debug.Log(key + "==" + node);
        if (node.Attributes[key] != null)
        {
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                return int.Parse(str);
            }
        }
        return XmlNullValu;
    }
    public static float GetXmlAttrFloat(XmlNode node, string key)
    {
        //Debug.Log(key);
        if (node.Attributes[key] != null)
        {
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                return float.Parse(str);
            }
        }
        return XmlNullValu;
    }
    public static Vector3 GetXmlAttrVector(XmlNode node, string key)
    {
        //Debug.Log(key);
        if (node.Attributes[key] != null)
        {
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                string[] strs = str.Split(',');
                return new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
            }
        }
        return Vector3.zero;
    }
    public static List<Vector3> GetXmlAttrVectors(XmlNode node, string key)
    {
        List<Vector3> result = new List<Vector3>();
        if (node.Attributes[key] != null)
        {
            string str = node.Attributes[key].InnerText;
            if (!string.IsNullOrEmpty(str))
            {
                string[] datas = str.Split('|');

                foreach (string pp in datas)
                {
                    string[] strs = pp.Split(',');

                    result.Add(new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2])));
                }
            }
        }
        return result;
    }
    public static List<int> GetXmlAttrInts(XmlNode node, string key)
    {
        //Debug.Log(key);
        int num = 0;
        List<int> result = new List<int>();
        string str = node.Attributes[key].InnerText;
        if (!string.IsNullOrEmpty(str))
        {
            string[] strs = str.Split(',');
            foreach (string item in strs)
            {
                if (int.TryParse(item, out num))
                {
                    result.Add(num);
                }
            }
        }
        return result;
    }
    public static List<string> GetXmlAttrStrings(XmlNode node, string key)
    {
        List<string> result = new List<string>();
        string str = node.Attributes[key].InnerText;
        if (!string.IsNullOrEmpty(str))
        {
            string[] strs = str.Split(',');
            foreach (string item in strs)
            {
                result.Add(item);
            }
        }
        return result;
    }
    public static List<List<int>> GetXmlAttrIntss(XmlNode node, string key)
    {
        //Debug.Log(key);
        int num = 0;
        List<List<int>> result = new List<List<int>>();
        string str = node.Attributes[key].InnerText;
        if (!string.IsNullOrEmpty(str))
        {
            string[] strs = str.Split(',');
            foreach (string item in strs)
            {
                List<int> r = new List<int>();
                string[] items = item.Split('|');
                foreach (string s in items)
                {
                    if (int.TryParse(s, out num))
                    {
                        r.Add(num);
                    }
                }
                result.Add(r);
            }
        }
        return result;
    }
    public static int GetXmlInt(XmlNode node)
    {
        if (node != null)
        {
            if (!string.IsNullOrEmpty(node.InnerText))
            {
                return int.Parse(node.InnerText);
            }
        }
        return XmlNullValu;
    }

    public static string ToString(Vector3 v)
    {
        if (v != Vector3.zero)
        {
            return v.x + "," + v.y + "," + v.z;
        }
        return "";
    }

    public static string ToString(bool b)
    {
        return b ? "yes" : "";
    }

    public static string ToString(List<int> datas)
    {
        if (datas == null || datas.Count <= 0) { return ""; }
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        for (int i = 0; i < datas.Count; i++)
        {
            str.Append(datas[i] + ",");
        }
        return str.ToString().TrimEnd(',');
    }

    public static string ToString(List<string> datas)
    {
        if (datas == null || datas.Count <= 0) { return ""; }
        System.Text.StringBuilder str = new System.Text.StringBuilder();
        for (int i = 0; i < datas.Count; i++)
        {
            str.Append(datas[i] + ",");
        }
        return str.ToString().TrimEnd(',');
    }

    #endregion
}
