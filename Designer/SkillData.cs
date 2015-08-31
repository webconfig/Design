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

    /// <summary>
    /// 数据节点
    /// </summary>
    [System.NonSerialized]
    public List<Task> Datas=new List<Task>();

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="rootTask"></param>
    /// <param name="detachedTasks"></param>
    public void save(List<Task> detachedTasks)
    {
        Datas = detachedTasks;
    }
}

