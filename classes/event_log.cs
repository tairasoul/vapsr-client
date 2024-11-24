using BepInEx.Logging;
using MainMenuSettings.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace VapSRClient;

public class EventLog
{
	private static GameObject LogObject;
	private static ManualLogSource Logger = new("EventLog");
	public static void CreateContainer() 
	{
		GameObject EventLog = GameObject.Find("UI").Find("LogPickups");
		LogObject = EventLog;
	}
	
	public static void Log(string text) 
	{
		Logger.LogInfo($"EventLog: {text}");
		LogObject.GetComponent<Logger>().Empty(text);
	}
}