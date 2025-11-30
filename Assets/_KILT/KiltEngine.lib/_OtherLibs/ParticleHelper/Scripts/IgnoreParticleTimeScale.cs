using UnityEngine;
using System.Collections;

public class IgnoreParticleTimeScale : MonoBehaviour {

	private ParticleSystem particle;

	/*private void Awake()
	{
		particle = GetComponent<ParticleSystem>();
	}

	protected virtual void Start()
	{
		if(particle != null && particle.IsAlive(true) && PauseManager.IsPaused())
			particle.Simulate(1f, true, false); //last must be false!!
	}
	
	// Update is called once per frame
	void Update () 
	{
		//if(particle != null && particle.IsAlive(true) && PauseManager.IsPaused())
		//	particle.Simulate(Time.unscaledDeltaTime, true, false); //last must be false!!
	}*/

	private void Awake()
	{
		particle = GetComponent<ParticleSystem>();
		//for (float i = 0.0f; i < initTime; i += Time.unscaledDeltaTime) {
		//	particle.Simulate(i);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(particle != null)
		{
			particle.Simulate(Time.unscaledDeltaTime, true, false); //last must be false!!
			if(particle.time == particle.duration)
				particle.Stop();


			/*if(PauseManager.IsPaused())
			{
				if(!particle.isStopped)
				{
					particle.Simulate(Time.unscaledDeltaTime, true, false); //last must be false!!
					if(particle.time == particle.duration)
						particle.Stop();
				}
			}
			else if(particle.isPaused)
				particle.Play(true);*/
		}
	}
}