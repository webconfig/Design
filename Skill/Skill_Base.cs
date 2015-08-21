using System.Collections.Generic;
using UnityEngine;

using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
/// <summary>
/// 技能模块基类
/// </summary>
[System.Serializable]
public class Skill_Base : BehaviorDesigner.Runtime.Tasks.Task
{
    [System.NonSerialized]
    public Skill skill;
    public int ID;
    [System.NonSerialized]
    public List<int> NextIds;
    [System.NonSerialized]
    public bool SkillOver = false;
    /// <summary>
    /// 技能状态
    /// </summary>
    [System.NonSerialized]
    public SkillState State = SkillState.None;
    public bool Play = false;
    [System.NonSerialized]
    public bool ready_end = false;
    [System.NonSerialized]
    public object Prev_Data;

    public Skill_Base() { }
    public Skill_Base(Skill_Base data)
    {
        this.skill = data.skill;
        this.ID = data.ID;
        this.NextIds = new List<int>(data.NextIds);
        this.Play = data.Play;
        this.SkillOver = data.SkillOver;
    }
    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init(Skill Skill, XmlNode data)
    {
       
    
    }

    /// <summary>
    /// 运行
    /// </summary>
    public virtual void Run(object Data)
    {
        Prev_Data = Data;
        ready_end = false;
        skill.running_count_max++;
        skill.Runing_Add.Add(this);
    }

    ///// <summary>
    ///// 更新
    ///// </summary>
    //public virtual void Update(object data)
    //{

    //}
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
        if (id > 0)
        {
            if (!skill.Items.ContainsKey(id)) { Debug.Log("===========技能不包含模块ID："+id); }
            Skill_Base sb = (skill.Items[id]  as IDeepCopy).DeepCopy();
            sb.Run(Data);
            return sb;
        }
        return null;
    }
    /// <summary>
    /// 运行一个模块
    /// </summary>
    /// <param name="id"></param>
    public void RunNext(object Data)
    {

        if ((!SkillOver) && (NextIds != null) && (NextIds.Count > 0))
        {
            foreach (int num in NextIds)
            {
                //Debug.Log(num);
                Skill_Base sb = (skill.Items[num] as IDeepCopy).DeepCopy(); 
                sb.Run(Data);
            }
        }

    }
    

    //=============new================
    public override void Serialize(Dictionary<string, string> dictionary)
    {
        dictionary.Add("ID", ID.ToString());
        dictionary.Add("NextIds", NextIds.ToString());
        dictionary.Add("Play", Play.ToString());
    }
    public override void SerializeUI(Dictionary<string, string> dictionary)
    {
        dictionary.Add("ID", ID.ToString());
    }
    public override void Deserialize(XmlNode node)
    {
        Init(null, node);
    }
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
