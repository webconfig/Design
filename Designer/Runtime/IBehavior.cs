using System;
using UnityEngine;

namespace BehaviorDesigner.Runtime
{
	public interface IBehavior
	{
		string GetOwnerName();

		int GetInstanceID();

		BehaviorSource GetBehaviorSource();

		void SetBehaviorSource(BehaviorSource behaviorSource);

		UnityEngine.Object GetObject();

		SharedVariable GetVariable(string name);

		void SetVariable(string name, SharedVariable item);

		void ClearUnityObjects();

		int SerializeUnityObject(UnityEngine.Object unityObject);

		UnityEngine.Object DeserializeUnityObject(int id);
	}
}
