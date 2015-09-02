using SkillEditor.Runtime.Tasks;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

[TaskCategory("选择目标")]
[TaskName("findtarget_area")]
[TaskDescription("选择目标")]
public class Skill_FindTarget_Area : Skill_Time, IDeepCopy
{

    /// <summary>
    /// 基础目标
    /// </summary>
    private string  Base_Obj;
    /// <summary>
    /// 位置偏移
    /// </summary>
    public Vector3 position;

    /// <summary>
    /// 范围
    /// </summary>
    public float Range;

    /// <summary>
    /// 角度
    /// </summary>
    public float Angle;

    /// <summary>
    /// 目标tag
    /// </summary>
    public List<string> Tag;

    /// <summary>
    /// 避免重复
    /// </summary>
    public int Aword_Repeat;


    public int Random_Num = 0;


    private List<GameObject> OldGamgeObjs = new List<GameObject>();

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Init(Skill Skill, XmlNode data)
    {
        base.Init(Skill, data);
        Base_Obj = data.Attributes["base_obj"].Value;
        position = Skill_Manager.GetXmlAttrVector(data, "position");
        Range = Skill_Manager.GetXmlAttrFloat(data, "range");
        if (Range < 0) { Range = 0; }
        Angle = Skill_Manager.GetXmlAttrFloat(data, "angle");
        if (Angle < 0) { Angle = 360; }
        Tag = Skill_Manager.GetXmlAttrStrings(data, "tag");// data.Attributes["tag"].InnerText;
        Aword_Repeat = Skill_Manager.GetXmlAttrInt(data, "aword_repeat");
        if (data.Attributes["random"]!=null)
        {
            Random_Num = Skill_Manager.GetXmlAttrInt(data, "random");
        }
        State = SkillState.Init;
    }

    /// <summary>
    /// 运行
    /// </summary>
    public override void Run(object Data)
    {
        //Debug.Log("sssssssssssssssss");
        base.Run(Data);
        OldGamgeObjs.Clear();
        //Debug.Log("CD:" + CD);
        if (CD > 0)
        {//有CD
            TimeCD += Skill_Area_Fan_Target_TimeCD;
            TimeLifeTime += Skill_Area_Fan_Target_TimeLifeTime;
        }
        else
        {//无CD
            TimeDealy += Skill_Target_TimeDealy;
        }
        State = SkillState.Init;
    }

    void Skill_Area_Fan_Target_TimeLifeTime()
    {
        //Debug.Log("生命周期到！");
        action(NextIds);
        End();
    }

    void Skill_Area_Fan_Target_TimeCD()
    {
        //Debug.Log("11111111111111:"+State+"--"+ID);
        action(NextIds);
    }

    void Skill_Target_TimeDealy()
    {
        action(NextIds);
        End();
    }

    private void action(List<int> datas)
    {
      
    }

    /// <summary>
    /// 强行结束
    /// </summary>
    public override void End()
    {
        //Debug.Log("End!");
        OldGamgeObjs.Clear();
        State = SkillState.Over;
    }

    #region 拷贝对象
    public void CopyData(Skill_Base obj)
    {
        Copy(obj as Skill_FindTarget_Area);
    }
    public Skill_Base CopyObj()
    {
        return ScriptableObject.CreateInstance(typeof(Skill_FindTarget_Area)) as Skill_FindTarget_Area;
    }
    public Skill_Base DeepCopy()
    {
        Skill_FindTarget_Area data = ScriptableObject.CreateInstance(typeof(Skill_FindTarget_Area)) as Skill_FindTarget_Area;// new Skill_Effect(this);
        this.Copy(data);
        return data;
    }

    public  void Copy(Skill_FindTarget_Area data)
    {
        base.Copy(data);
        data.Base_Obj = this.Base_Obj;
        data.position = this.position;
        data.Range = this.Range;
        data.Angle = this.Angle;
        data.Tag = this.Tag;
        data.Aword_Repeat = this.Aword_Repeat;
        data.Random_Num = this.Random_Num;
    }
    #endregion

    #region 编辑器
    public SharedGameObject BaseObj;
    public override void Serialize(Dictionary<string, string> dictionary)
    {
        base.Serialize(dictionary);
        Debug.Log(BaseObj.name);
        //dictionary.Add("ids", ids.ToString());
        dictionary.Add("base_obj", (BaseObj.IsShared ? BaseObj.ValueType+"-" : "") + BaseObj.name);
        dictionary.Add("position",Skill_Manager.ToString(position));
        dictionary.Add("range", Range.ToString());
        dictionary.Add("angle",Angle.ToString());
        dictionary.Add("tag", Skill_Manager.ToString(Tag));
        dictionary.Add("aword_repeat", Aword_Repeat.ToString());
        dictionary.Add("random", Random_Num.ToString());
    }
    public override void InitValue()
    {
        if (BaseObj == null)
        {
            BaseObj = new SharedGameObject();
            BaseObj.Value = null;
        }
        base.InitValue();
    }
    public override void Deserialize(XmlNode node, SkillData _data)
    {
        base.Deserialize(node, _data);

        string[] data = Base_Obj.Split('-');
        if (data!=null&&data.Length>0)
        {
            Base_Obj = data[1];
            BaseObj = _data.GetVariable(data[1]) as SharedGameObject;
            BaseObj.IsShared = true;

        }
    }

    #endregion
}



