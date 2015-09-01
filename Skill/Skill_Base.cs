using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
/// <summary>
/// 技能模块基类
/// </summary>
public class Skill_Base : Task
{
    [System.NonSerialized]
    public Skill skill;
    ///// <summary>
    ///// 模块ID
    ///// </summary>
    //public int ID;
    [System.NonSerialized]
    /// <summary>
    /// 模块下一阶段的id
    /// </summary>
    public List<int> NextIds;
    /// <summary>
    /// 技能状态
    /// </summary>
    [System.NonSerialized]
    public SkillState State = SkillState.None;
    /// <summary>
    /// 模块的类型
    /// </summary>
    public int Kind;

    public bool Play = false;
    [System.NonSerialized]
    public bool ready_end = false;
    [System.NonSerialized]
    public object Prev_Data;
    [System.NonSerialized]
    public bool SkillOver = false;
    [System.NonSerialized]
    public string class_type;

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init(Skill Skill, XmlNode data)
    {
        ID = Skill_Manager.GetXmlAttrInt(data, "id");
        skill = Skill;
        Play = false;
        if (data.Attributes["play"].InnerText == "auto")
        {
            Play = true;
        }
        else
        {
            Play = false;
        }
        if (data.Attributes["nextids"].InnerText == "over")
        {
            SkillOver = true;
        }
        Kind = Skill_Manager.GetXmlAttrInt(data, "kind");

        NextIds = Skill_Manager.GetXmlAttrInts(data, "nextids");
      
    }

    /// <summary>
    /// 运行
    /// </summary>
    public virtual void Run(object Data)
    {
        Prev_Data = Data;
        ready_end = false;
        //skill.running_count_max++;
        //skill.Runing_Add.Add(this);
    }

    /// <summary>
    /// 更新
    /// </summary>
    public virtual void SkillUpdate(object data)
    {

    }
    /// <summary>
    /// 强行结束
    /// </summary>
    public virtual void End()
    {

    }

    /// <summary>
    /// 运行一个模块
    /// </summary>
    /// <param name="id"></param>
    public Skill_Base RunModule(int id, object Data)
    {
        //if (id > 0)
        //{
        //    if (!skill.Items.ContainsKey(id)) { Debug.Log("===========技能不包含模块ID："+id); }
        //    Skill_Base sb = Skill_Manager.Instance.SkillClass.GetObj(id, skill);//(skill.Items[id]  as IDeepCopy).DeepCopy();
        //    sb.skill = skill;
        //    sb.Run(Data);
        //    return sb;
        //}
        return null;
    }
    /// <summary>
    /// 运行一个模块
    /// </summary>
    /// <param name="id"></param>
    public void RunNext(object Data)
    {

        //if ((!SkillOver) && (NextIds != null) && (NextIds.Count > 0))
        //{
        //    foreach (int num in NextIds)
        //    {
        //        //Debug.Log(num);
        //        Skill_Base sb = Skill_Manager.Instance.SkillClass.GetObj(num, skill);// (skill.Items[num] as IDeepCopy).DeepCopy(); 
        //        sb.skill = skill;
        //        sb.Run(Data);
        //    }
        //}

    }
    
    public void Copy(Skill_Base data)
    {
        data.class_type = this.class_type;
        data.skill=this.skill;
        data.ID=this.ID;
        data.NextIds=new List<int>(this.NextIds);
        data.Play = this.Play;
        data.SkillOver = this.SkillOver;
        data.State = SkillState.None;
        data.Kind = this.Kind;
    }



    #region 编辑器
    public override void Serialize(Dictionary<string, string> dictionary)
    {
        dictionary.Add("id", ID.ToString());

        dictionary.Add("nextids", GetOutLinkIds("n"));
        if (Kind>0)
        {
            dictionary.Add("kind", Kind.ToString());
        }

        dictionary.Add("play", Play ? "auto" : "");
    }
    public override void SerializeUI(Dictionary<string, string> dictionary)
    {
        dictionary.Add("ID", ID.ToString());
    }
    public override void Deserialize(XmlNode node)
    {
        Init(null, node);
    }
    public override void GetOutLinks(List<TaskOutLink> datas)
    {
        TaskOutLink tol = new TaskOutLink();
        tol.Index = 1;
        tol.name = "n";
        datas.Add(tol);
    }
    #endregion
}


public enum SkillState
{
    None=0,
    Init=1,
    Start=2,
    OverDelay=3,
    Running=4,
    Waiting=5,
    OverLifeTime=6,
    Over=6
}
