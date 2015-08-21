using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;



/// <summary>
/// 动画模块
/// </summary>
[TaskCategory("Basic")]
[TaskName("anim")]
[TaskDescription("Plays animation")]
public class Skill_Anim : Skill_Time,IDeepCopy
{
    /// <summary>
    /// 动画
    /// </summary>
    public string Anim;
    /// <summary>
    /// 动画播放时间
    /// </summary>
    public float AnimSpeed;
    /// <summary>
    /// 动画联动--无，2:本队
    /// </summary>
    private int Relation;
    private List<GameObject> AnimCharacters = new List<GameObject>();

    private GameObject player;


    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init(Skill Skill, XmlNode data)
    {
        base.Init(Skill, data);
        Anim = data.Attributes["anim"].InnerText;

        State = SkillState.Init;
    }

    /// <summary>
    /// 运行
    /// </summary>
    public override void Run(object Data)
    {
        
    }

    void Skill_Anim_TimeLifeTime()
    {
        RunNext(Prev_Data);
        State = SkillState.Over;
    }
    void Skill_Anim_TimeDealy()
    {

    }

    /// <summary>
    /// 强行结束
    /// </summary>
    public override void End()
    {
        if (State != SkillState.Over)
        {
            //playerscript.component.animator.CrossFade("playerAni.player_gethit01", 0.03f, -1, 0f);
            State = SkillState.Over;
        }
    }

    #region 拷贝对象
    public Skill_Base DeepCopy()
    {
        Skill_Anim data = new Skill_Anim(this);
        return data;
    }
    public Skill_Anim() { }
    public Skill_Anim(Skill_Anim data)
        : base(data)
    {
        this.Anim = data.Anim;
        this.AnimSpeed = data.AnimSpeed;
        this.Relation = data.Relation;
    }
    #endregion



    //=============new================
    public override void Serialize(Dictionary<string, string> dictionary)
    {
        base.Serialize(dictionary);
        dictionary.Add("Anim", Anim);
        dictionary.Add("AnimSpeed", AnimSpeed.ToString());
    }

}
public delegate void PlayAnim(AnimatorStateInfo anim_state, string anim);


