 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEngine;

[System.Serializable]
public class Skill
{
    /// <summary>
    /// 技能ID
    /// </summary>
    public int SkillID = -100;
    /// <summary>
    /// 基础数据
    /// </summary>
    public Skill_Data property;
    /// <summary>
    /// 技能模块
    /// </summary>
   [System.NonSerialized]
    public Dictionary<int, Skill_Base> Items = new Dictionary<int, Skill_Base>();
    /// <summary>
    /// 运行中的模块
    /// </summary>
    [System.NonSerialized]
    public List<Skill_Base> Runing_Add = new List<Skill_Base>();
    /// <summary>
    /// 运行中的模块
    /// </summary>
    public List<Skill_Base> Runing = new List<Skill_Base>();
    /// <summary>
    /// 运行结束的模块
    /// </summary>
   [System.NonSerialized]
    public List<Skill_Base> Runing_End = new List<Skill_Base>();

   

    /// <summary>
    /// 技能的射程
    /// </summary>
    public float Range
    {
        get
        {
            return property.Range;
        }
    }

    //public Skill_Network network;

    public int running_count_max = 0;

    public Skill(int id)
    {
       
    }

    public void Use()
    {


    }

    public void Run(bool isUsing)
    {
        
    }


    public void Update()
    {
        if (Runing_Add.Count > 0)
        {
            Runing.AddRange(Runing_Add);
            Runing_Add.Clear();
        }
        if (Runing.Count > 0)
        {
            foreach (Skill_Base item in Runing)
            {
                if (item.State!=SkillState.Over)
                {
                    item.Update(null);
                }
                else
                {
                    Runing_End.Add(item);
                }
            }
            //===移除运行结束的模块====
            if (Runing_End.Count > 0)
            {
                foreach (Skill_Base item in Runing_End)
                {
                    Runing.Remove(item);
                }
                Runing_End.Clear();
            }
        }
    }


    public void RunModle(List<int> ids)
    {
        foreach (int id in ids)
        {
            Skill_Base sb = (Items[id] as IDeepCopy).DeepCopy();
            sb.Run(null);
        }
    }


    /// <summary>
    /// 强行终止运行的一个模块
    /// </summary>
    /// <param name="modle_id"></param>
    public void EndSkillModle(int modle_id)
    {
        foreach(Skill_Base _skill_base in Runing)
        {
            if(_skill_base.ID==modle_id)
            {
                _skill_base.End();
                return;
            }
        }
        foreach (Skill_Base _skill_base in Runing_Add)
        {
            if (_skill_base.ID == modle_id)
            {
                _skill_base.End();
                return;
            }
        }
    }



}


