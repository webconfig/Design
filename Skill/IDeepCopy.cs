/// <summary>
/// 深拷贝接口
/// </summary>
interface IDeepCopy
{
    Skill_Base DeepCopy();

    Skill_Base CopyObj();

    void CopyData(Skill_Base obj);
}