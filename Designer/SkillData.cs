using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 数据源
/// </summary>
[Serializable]
public class SkillData
{
    public int Id=0;
    public string Name="";


    /// <summary>
    /// 数据节点
    /// </summary>
    public List<Task> Datas;
}

