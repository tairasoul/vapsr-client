using HarmonyLib;
using VapSRClient.Reflection;
using VapSRClient.Extensions;
using SpeedrunningUtils;
using UnityEngine;

namespace VapSRClient;
[HarmonyPatch(typeof(SpeedrunnerUtils))]
static class UtilsPatch 
{
	[HarmonyPatch("Update")]
	[HarmonyPrefix]
	static bool Update(SpeedrunnerUtils __instance) 
	{
		ReflectionHelper helper = new(__instance);
		string CurrentScene = helper.GetField<string>("CurrentScene");
		int SplitIndex = helper.GetField<int>("SplitIndex");
		CustomSplit[] splits = helper.GetField<CustomSplit[]>("splits");
		if (CurrentScene != "Intro" && CurrentScene != "Menu")
		{
			if (!Client.Client.MatchLoaded)
				return true;
			if (SplitIndex < splits.Length) 
			{
				bool isLastSplit = SplitIndex == splits.Length - 1;
				CustomSplit split = splits[SplitIndex];
				bool splitFulfilled = split.Fulfilled();
				if (splitFulfilled)
				{
					Plugin.Log.LogInfo($"comms: {Plugin.comms}");
					Plugin.comms.SplitCompleted(split.SplitName);
					if (isLastSplit)
						Plugin.comms.RunCompleted();
					if (split.Command != null)
					{
						SpeedrunningUtils.Plugin.Log.LogInfo($"Executing command {split.Command} at split {split.SplitName}");
						if (split.Command == "pause") 
							Plugin.timer.StopTimer();
						if (split.Command == "resume")
							Plugin.timer.StartTimer();
						if (split.Command == "startorsplit")
							Plugin.timer.StartTimer();
						Livesplit.SendCommand(split.Command);
					}
					helper.SetField("SplitIndex", SplitIndex + 1);
				}
			}
			else if (typeof(SpeedrunningUtils.Plugin).GetF<bool>("Recording", null) && typeof(SpeedrunningUtils.Plugin).GetF<bool>("WebsocketConnected", null)) 
			{
				SpeedrunningUtils.OBS.ObsWebsocket.StopRecording();
			}
		}
		return false;
	}
}