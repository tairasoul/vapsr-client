using System.Threading.Tasks;
using BepInEx.Logging;
using Invector;
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
		LogObject = EventLog.Instantiate();
    LogObject.name = "SplitLog";
    LogObject.transform.parent = EventLog.transform.parent;
    RectTransform t = LogObject.GetComponent<RectTransform>();
    t.anchoredPosition = new(-280.5f, -95.5f);
    ModifyLogPrefab();
  }

	private static void ModifyLogPrefab() {
    Logger l = LogObject.GetComponent<Logger>();
    l.LogCount = 4;
    GameObject LogPrefab = l.log.Instantiate();
    LogPrefab.name = "SplitLogPrefab";
    LogPrefab.transform.parent = l.log.transform.parent;
    l.log = LogPrefab;
    GameObject.Destroy(LogPrefab.GetComponent<vDestroyGameObject>());
  }
	
	public static void Log(string text) 
	{
		Logger.LogInfo($"EventLog: {text}");
		LogObject.GetComponent<Logger>().Empty(text);
	}
}