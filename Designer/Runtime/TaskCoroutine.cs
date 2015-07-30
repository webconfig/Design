using System;
using System.Collections;

namespace BehaviorDesigner.Runtime
{
	public class TaskCoroutine
	{
		private IEnumerator mCoroutine;

		private Behavior mParent;

		private string mCoroutineName;

		private bool mStop;

		public void Stop()
		{
			this.mStop = true;
		}

		public TaskCoroutine(Behavior parent, IEnumerator coroutine, string coroutineName)
		{
			this.mParent = parent;
			this.mCoroutine = coroutine;
			this.mCoroutineName = coroutineName;
			parent.StartCoroutine(this.RunCoroutine());
		}

		public IEnumerator RunCoroutine()
		{
			yield return null;
			while (!this.mStop && this.mCoroutine != null && this.mCoroutine.MoveNext())
			{
				yield return this.mCoroutine.Current;
			}
			this.mParent.TaskCoroutineEnded(this, this.mCoroutineName);
			yield break;
		}
	}
}
