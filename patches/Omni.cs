using HarmonyLib;
using UnityEngine;
using PixelCrushers.DialogueSystem;

namespace VapSRClient;

[HarmonyPatch(typeof(Omni))]
static class OmniPatches 
{
	[HarmonyPatch("Start")]
	[HarmonyPrefix]
	static bool Start(Omni __instance) 
	{
		if (SRComms.MatchFound && !ReadySync.StartBypass) 
		{
			__instance.first = false;
			GameObject readySyncObj = new("ReadySync");
			readySyncObj.AddComponent<ReadySync>();
			if (SRComms.InPrivateRoom) 
			{
				GameObject placementScreen = new("PlacementScreen");
				placementScreen.AddComponent<PlacementScreen>();
			}
			else 
			{
				GameObject endScreenObj = new("EndScreen");
				endScreenObj.AddComponent<EndScreen>();
			}
			return false;
		}
		return true;
	}
}