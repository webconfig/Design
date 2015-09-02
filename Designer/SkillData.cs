using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 数据源
/// </summary>
[Serializable]
public class SkillData : UnityEngine.ScriptableObject
{
    public int Id=0;
    public string Name="Skill";
    public float CD=0;

    [System.NonSerialized]
    public int index=0;

    /// <summary>
    /// 数据节点
    /// </summary>
    [System.NonSerialized]
    public List<Task> Datas=new List<Task>();


    #region 变量
    /// <summary>
    /// 变量
    /// </summary>
    public Dictionary<string, SharedVariable> Variables=new Dictionary<string,SharedVariable>() ;


    public SharedVariable GetVariable(string name)
    {

        if (Variables!=null&&Variables.ContainsKey(name))
        {
            return Variables[name];
        }

        return null;
    }

    public void SetVariable(string name, SharedVariable item)
    {
        if (!Variables.ContainsKey(name))
        {
            Variables.Add(name, item);
        }
    }
    #endregion

}

