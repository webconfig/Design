using System.Collections.Generic;
using UnityEngine;

using System.Xml;
/// <summary>
/// 技能的所有属性
/// </summary>
public class Skill_Data
{

    /// <summary>
    /// 技能等级
    /// </summary>
    public int Level;

    #region 基础数据
    /// <summary>
    /// 技能名称
    /// </summary>
    public string Name;
    /// <summary>
    /// 技能描述
    /// </summary>
    public string Description;
    /// <summary>
    /// 技能图标
    /// </summary>
    public int Icon;
    /// <summary>
    /// 是否能被打断
    /// </summary>
    public string Interrupt;
    /// <summary>
    /// 射程
    /// </summary>
    public float Range;
    /// <summary>
    /// 技能CD
    /// </summary>
    public float CD;
    ///// <summary>
    ///// 技能类型(无，1：普攻 3:天生)
    ///// </summary>
    //public int SkillType;

    /// <summary>
    /// 愤怒值
    /// </summary>
    public float Ange;
    /// <summary>
    /// 仇恨值
    /// </summary>
    public float Threat;

    /// <summary>
    /// 等级
    /// </summary>
    public int Rank;
    #endregion

    public Skill_Data()
    { }

    
}