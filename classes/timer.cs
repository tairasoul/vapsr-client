using System.Diagnostics;
using UnityEngine;

namespace VapSRClient;

public class Timer : MonoBehaviour 
{
	private Stopwatch stopwatch;
	
	public void Awake() 
	{
		stopwatch = new();
	}
	
	public void StartTimer() 
	{
		if (stopwatch == null) 
		{
			Awake();
		}
		if (!stopwatch.IsRunning)
		{
			Plugin.Log.LogInfo("Starting timer");
			stopwatch.Start();
		}
	}
	
	public void StopTimer() 
	{
		if (stopwatch == null) 
		{
			Awake();
		}
		if (stopwatch.IsRunning)
		{
			Plugin.Log.LogInfo("Stopping timer");
			stopwatch.Stop();
		}
	}
	
	public void ResetTime() 
	{
		if (stopwatch == null) 
		{
			Awake();
		}
		stopwatch.Reset();
	}
	
	public bool Running() {
		return stopwatch.IsRunning;
	}
	
	public float GetTime() 
	{
		return (float)stopwatch.Elapsed.TotalMilliseconds;
	}
}