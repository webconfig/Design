using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Basic")]
[TaskDescription("Plays animation without any blending. Returns Success.")]
public class SkillTest : BehaviorDesigner.Runtime.Tasks.Task
{


    public override string Serialize()
    {
        return @"<set_user_state id='10' play='auto' delay='' lifetime='' nextids='' state='noaction'/>";
    }
}

