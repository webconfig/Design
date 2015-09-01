using SkillEditor.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[TaskCategory("Test")]
[TaskName("test")]
[TaskDescription("播放动画-Triger方式触发")]
public class Skill_Test : Skill_Time, IDeepCopy
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

        State = SkillState.Init;
    }

    /// <summary>
    /// 运行
    /// </summary>
    public override void Run(object Data)
    {
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
    public override void GetOutLinks(List<TaskOutLink> datas)
    {
        TaskOutLink tol = new TaskOutLink();
        tol.Index = 1;
        tol.name = "ok";
        datas.Add(tol);
        TaskOutLink tol2 = new TaskOutLink();
        tol2.Index = 2;
        tol2.name = "no";
        datas.Add(tol2);
    }
    #endregion

}


