﻿using SkillEditor.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[TaskCategory("动画")]
[TaskName("anim")]
[TaskDescription("播放动画-Triger方式触发")]
public class Skill_Anim : Skill_Time,IDeepCopy
{
    /// <summary>
    /// 动画
    /// </summary>
    public string Anim;
    /// <summary>
    /// 动画播放速度
    /// </summary>
    public float AnimSpeed;



    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init(Skill Skill, XmlNode data)
    {
        base.Init(Skill, data);
        Anim = data.Attributes["anim"].InnerText;
        AnimSpeed = Skill_Manager.GetXmlAttrFloat(data, "speed");
        if (AnimSpeed < 0) { AnimSpeed = 1; }
        State = SkillState.Init;
    }

    /// <summary>
    /// 运行
    /// </summary>
    public override void Run(object Data)
    {
        //if (Skill_Manager.Instance.CheckDead(skill.owner.gameObject)) { return; }


        //base.Run(Data);

        //if (Prev_Data != null && Prev_Data is GameObject)
        //{
        //    player = Prev_Data as GameObject;
        //}
        //else
        //{
        //    player = skill.owner.gameObject;
        //}
        //if (Skill_Manager.Instance.CheckDead(player)) { return; }


        //playerscript = player.GetComponent<Character>();
        //TimeDealy += Skill_Anim_TimeDealy;
        //TimeLifeTime += Skill_Anim_TimeLifeTime;
        State = SkillState.Init;
    }

    void Skill_Anim_TimeLifeTime()
    {
        RunNext(Prev_Data);
        State = SkillState.Over;
    }
    void Skill_Anim_TimeDealy()
    {
        PlayAnim();
    }
    /// <summary>
    /// 播放动画
    /// </summary>
    public void PlayAnim()
    {
    }
    /// <summary>
    /// 强行结束
    /// </summary>
    public override void End()
    {
        State = SkillState.Over;
    }

    #region 拷贝对象
    public void CopyData(Skill_Base obj)
    {
        Copy(obj as Skill_Anim);
    }
    public Skill_Base CopyObj()
    {
        return ScriptableObject.CreateInstance(typeof(Skill_Anim)) as Skill_Anim;
    }
    public Skill_Base DeepCopy()
    {
        Skill_Anim data = ScriptableObject.CreateInstance(typeof(Skill_Anim)) as Skill_Anim;// new Skill_Effect(this);
        this.Copy(data);
        return data;
    }

    public  void Copy(Skill_Anim data)
    {
        base.Copy(data);
        data.Anim = this.Anim;
        data.AnimSpeed = this.AnimSpeed;
    }
    #endregion
    #region 编辑器
    public override void Serialize(Dictionary<string, string> dictionary)
    {
        base.Serialize(dictionary);
        dictionary.Add("anim", Anim);
        dictionary.Add("speed", AnimSpeed.ToString());
    }
    #endregion

}
public delegate void PlayAnim(AnimatorStateInfo anim_state, string anim);


