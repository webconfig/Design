#define debug_skill

using System.Collections.Generic;
using UnityEngine;

using System.Xml;
/// <summary>
/// 时间控制器模块
/// </summary>
[System.Serializable]
public class Skill_Time : Skill_Base
{
    /// <summary>
    /// Dealy时间到
    /// </summary>
    public event Time_Event TimeDealy;
    /// <summary>
    /// CD时间到
    /// </summary>
    public event Time_Event TimeCD;
    /// <summary>
    /// 生命周期完成
    /// </summary>
    public event Time_Event TimeLifeTime;
    /// <summary>
    /// 延迟
    /// </summary>
    public float Dealy = 0;
    /// <summary>
    /// 运行时间
    /// </summary>
    public float LifeTime;
    public float CD;

    private float time_begin = 0, time_run = 0, _lifetime;
    private bool run = false, over = false;

    private float t1 = 0, t2 = 0;

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init(Skill Skill, XmlNode data)
    {
        base.Init(Skill, data);
        Dealy = Skill_Manager.GetXmlAttrFloat(data, "delay");
        LifeTime = Skill_Manager.GetXmlAttrFloat(data, "lifetime");
        CD = Skill_Manager.GetXmlAttrFloat(data, "cd");
    }

    /// <summary>
    /// 运行
    /// </summary>
    public override void Run(object Data)
    {
        base.Run(Data);
        time_begin = Time.time;
        time_run = time_begin;
        _lifetime = 0;
        t1 = 0;
        t2 = 0;
        TimeCD = null;
        TimeDealy = null;
        TimeLifeTime = null;
        run = false;
        over = false;
    }

    /// <summary>
    /// 更新
    /// </summary>
    public override void SkillUpdate(object data)
    {

        if (ready_end) { return; }
        //if (ID == 903)
        //{
        //    //Debug.Log("state:" + (State != SkillState.Over));
        //}
        if (State != SkillState.Over)
        {
            if (!over)
            {
                if (!run)
                {
                    time_run = Time.time;
                    if ((time_run - time_begin) >= Dealy)
                    {//运行
                        run = true;
                        if (TimeDealy != null)
                        {
                            TimeDealy();
                            TimeDealy = null;
                        }
                        time_begin = Time.time;
                        time_run = time_begin;
                    }
                }
                else
                {
                    if (LifeTime > 0)
                    {
                        time_run = Time.time;
                        _lifetime = time_run - time_begin;
                        if (_lifetime >= LifeTime)
                        {//超过生命周期
                            //Debug.Log("========超过生命周期========");
                            if (TimeLifeTime != null)
                            {
                                TimeLifeTime();
                                TimeLifeTime = null;
                                over = true;
                            }
                        }
                        else if (CD > 0)
                        {

                            t1 = _lifetime % CD;
                            if (t1 < t2)
                            {
                                //========到达触发条件========
                                if (TimeCD != null)
                                {
                                    //Debug.Log("11111111111:"+State);
                                    TimeCD();
                                }
                            }
                            t2 = t1;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 强行结束
    /// </summary>
    public override void End()
    {

    }


    public void Copy(Skill_Time data)
    {
        base.Copy(data);
        data.Dealy = this.Dealy;
        data.LifeTime = this.LifeTime;
        data.CD = this.CD;
    }




    #region 编辑器
    public override void Serialize(Dictionary<string, string> dictionary)
    {
        base.Serialize(dictionary);
        dictionary.Add("delay", Dealy.ToString());
        dictionary.Add("lifetime", LifeTime.ToString());
        if (CD > 0)
        {
            dictionary.Add("cd", CD.ToString());
        }
    }
    #endregion

}
public delegate void Time_Event();

