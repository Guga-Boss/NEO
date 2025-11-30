using UnityEngine;
using System.Collections;

public class AutoDestroyWhenNoChildren : AutoDestroyParticle 
{
	#region Coroutines
	
	protected override IEnumerator CheckIfMustDestroy ()
	{
		while(true)
		{
			float start = Time.realtimeSinceStartup;
			float time = 0.5f;
			while (Time.realtimeSinceStartup < start + time)
				yield return null;
			
			if(transform.childCount == 0)
			{
				if(OnlyDeactivate)
				{
					#if UNITY_3_5
					this.gameObject.SetActiveRecursively(false);
					#else
					this.gameObject.SetActive(false);
					#endif
				}
				else
					GameObject.Destroy(this.gameObject);
				break;
			}
		}
	}
	
	#endregion
}
