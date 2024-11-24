using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using SpeedrunningUtils;
using MainMenuSettings.Extensions;
using BepInEx;

namespace VapSRClient;

public class Handlers 
{
	internal static int seed;
	static AsyncOperation? loadOperation;
	[MessageHandler(ReceivingMessageType.PlayerFinishedStage)]
	public static void PlayerFinishedStage(object? data) 
	{
		PlayerCompletedStageInfo info = (PlayerCompletedStageInfo)data;
		if (!info.stage.IsNullOrWhiteSpace())
			EventLog.Log($"{info.playerName} completed split {info.stage}");
	}
	
	[MessageHandler(ReceivingMessageType.StartRun)]
	public static void StartRun(object? data) 
	{
		if (SRComms.RunStarted)
			return;
		SRComms.RunStarted = true;
		ReadySync readySync = GameObject.Find("ReadySync").GetComponent<ReadySync>();
		Plugin.Log.LogInfo("Starting our run!");
		readySync.RunStarted();
	}
	
	[MessageHandler(ReceivingMessageType.MatchmakingStarted)]
	public static void MatchmakingStarted(object? data) 
	{
		SRComms.Matchmaking = true;
	}
	
	[MessageHandler(ReceivingMessageType.RunStopped)]
	public static void RunStopped(object? data) 
	{
		EndScreen end = GameObject.Find("EndScreen").GetComponent<EndScreen>();
		Plugin.Log.LogInfo("Run stopped.");
		RunFinishedRelayInfo info = (RunFinishedRelayInfo)data;
		Task.Run(() => end.OpponentFinished(info.playerName, info.time, info.youWon));
	}
	
	[MessageHandler(ReceivingMessageType.OtherPlayerForfeit)]
	public static void OtherPlayerForfeit(object? data) 
	{
		Plugin.Log.LogDebug($"Other player forfeit, data {data}");
		PlayerResult result = (PlayerResult)data;
		EventLog.Log($"{result.playerName} forfeited run");
		SRComms.MatchFound = false;
	}
	
	[MessageHandler(ReceivingMessageType.OpponentForfeit)]
	public static void OpponentForfeit(object? data) 
	{
		Plugin.Log.LogDebug($"Other player forfeit, data {data}");
		PlayerResult result = (PlayerResult)data;
		EventLog.Log($"{result.playerName} forfeited");
	}
	
	[MessageHandler(ReceivingMessageType.RequestSeed)]
	public static void SeedRequest(object? data) 
	{
		RngData rng = new()
		{
			seed = Random.seed
		};
		Plugin.comms.RespondWithSeed(rng);
	}
	
	[MessageHandler(ReceivingMessageType.RngSeedSet)]
	public static void SetRngSeed(object? data) 
	{
		seed = Random.seed;
		RngData rng = (RngData)data;
		Random.InitState(rng.seed);
	}
	
	[MessageHandler(ReceivingMessageType.PrivateRoomCreated)]
	public static void PrivateRoomCreated(object? data) 
	{
		if (SRComms.InPrivateRoom)
			return;
		RoomData roomData = (RoomData)data;
		ClientsideRoomData.Host = Plugin.comms.username.Value;
		ClientsideRoomData.RoomCode = roomData.code;
		ClientsideRoomData.isHost = true;
		SRComms.InPrivateRoom = true;
		RoomScreen.CreateBaseCanvas(roomData.code, Plugin.comms.StartPrivateRoom, Plugin.comms.LeavePrivateRoom, true);
		ClientsideRoomData.UpdatePlayers();
		/*RoomData roomData = (RoomData)data;
		ClientsideRoomData.LocalPlayerName = Plugin.comms.username.Value;
		ClientsideRoomData.RoomCode = roomData.code;
		SRComms.InPrivateRoom = true;
		RoomScreen.CreateBaseCanvas(roomData.code, Plugin.comms.StartPrivateRoom, Plugin.comms.LeavePrivateRoom);
		RoomScreen.CreatePlayer(ClientsideRoomData.LocalPlayerName);*/
	}
	
	[MessageHandler(ReceivingMessageType.PrivateRoomJoinAttempt)]
	public static void PrivateRoomJoinAttempt(object? data) 
	{
		RoomJoinAttempt attempt = (RoomJoinAttempt)data;
		Plugin.Log.LogInfo($"did we join room? {attempt.RoomJoined}");
		if (attempt.RoomJoined && !SRComms.InPrivateRoom) 
		{
			GameObject.Destroy(GameObject.Find("CodeScreen"));
			ClientsideRoomData.RoomCode = attempt.replicationData.code;
			ClientsideRoomData.Host = attempt.replicationData.host;
			ClientsideRoomData.OtherPlayers = attempt.replicationData.opponents;
			SRComms.InPrivateRoom = true;
			RoomScreen.CreateBaseCanvas(ClientsideRoomData.RoomCode, Plugin.comms.StartPrivateRoom, Plugin.comms.LeavePrivateRoom, false);
			ClientsideRoomData.UpdatePlayers();
		}
	}
	
	[MessageHandler(ReceivingMessageType.ReplicateRoomData)]
	public static void ReplicateRoomData(object? data) 
	{
		RoomReplicationData replicationData = (RoomReplicationData)data;
		ClientsideRoomData.Host = replicationData.host;
		ClientsideRoomData.OtherPlayers = replicationData.opponents;
		ClientsideRoomData.RoomCode = replicationData.code;
		ClientsideRoomData.UpdatePlayers();
		//RoomScreen.CreatePlayer(ClientsideRoomData.Host, true);
		//foreach (string player in ClientsideRoomData.OtherPlayers)
		//	RoomScreen.CreatePlayer(player, false);
		//ClientsideRoomData.OpponentName = replicationData.opponentName;
		//RoomScreen.CreatePlayer(ClientsideRoomData.OpponentName);
	}
	
	[MessageHandler(ReceivingMessageType.PrivateRoomNewHost)]
	public static void NewHostResponse(object? data) 
	{
		PrivateRoomNewHost host = (PrivateRoomNewHost)data;
		ClientsideRoomData.isHost = host.youAreNewHost;
	}
	
	[MessageHandler(ReceivingMessageType.PrivateRoomRunFinished)]
	public static void PrivateRoomRunFinished(object? data) 
	{
		RoomRunFinished finished = (RoomRunFinished)data;
		if (SRComms.MatchLoaded) 
		{
			Plugin.Log.LogInfo($"Player {finished.player} finished with time {PlacementScreen.FormatTime(finished.time)} ({finished.time})");
			Placement placement = new()
			{
				name = finished.player,
				time = finished.time
			};
			Task.Run(() => RunFinished(placement));
		}
	}
	
	static async Task RunFinished(Placement runs) 
	{
		while (true) 
		{
			if (PlacementUpdater.instance != null)
				break;
			await Task.Delay(1);
		}
		Plugin.Log.LogInfo(PlacementUpdater.instance);
		PlacementUpdater updater = PlacementUpdater.instance;
		updater.PlayerFinished(runs);
	}
	
	[MessageHandler(ReceivingMessageType.PrivateRoomBatchRunsFinished)]
	public static void PrivateRoomBatch(object? data) 
	{
		BatchRoomRunsFinished finished = (BatchRoomRunsFinished)data;
		if (SRComms.MatchLoaded) 
		{
			Task.Run(() => BatchUpdate(finished.times));
		}
	}
	
	static async Task BatchUpdate(Run[] runs) 
	{
		while (true) 
		{
			if (PlacementUpdater.instance != null)
				break;
			await Task.Delay(1);
		}
		PlacementUpdater updater = PlacementUpdater.instance;
		foreach (Run run in runs) 
		{
			Placement placement = new() 
			{
				name = run.name,
				time = run.time
			};
			updater.PlayerFinished(placement);
		}
	}
	
	[MessageHandler(ReceivingMessageType.PrivateRoomStarted)]
	public static void PrivateRoomStarted(object? data) 
	{
		Plugin.Log.LogDebug($"prs matchfound: {SRComms.MatchFound}");
		if (SRComms.MatchFound)
			return;
		Plugin.Log.LogInfo("our private room has started.");
		SRComms.MatchFound = true;
		RoomScreen.DeactivateScreenAlternate();
		SpeedrunningUtils.OBS.ObsWebsocket.StartRecording();
		MainMenuLoadscreen loadscreen = GameObject.FindFirstObjectByType<MainMenuLoadscreen>();
		loadscreen.canCancel = false;
		AsyncOperation operation = SceneManager.LoadSceneAsync(2);
		loadOperation = operation;
		Task.Run(() => KeepTabsOnProgress(ClientsideRoomData.OtherPlayers.Length));
	}
	
	[MessageHandler(ReceivingMessageType.PrivateRoomEveryoneCompleted)]
	public static void EveryoneCompleted(object? data) 
	{
		PlacementScreen placement = GameObject.FindFirstObjectByType<PlacementScreen>();
		placement.CanReturnToMenu = true;
	}
	
	[MessageHandler(ReceivingMessageType.MatchFound)]
	public static void MatchFound(object? data) 
	{
		if (SRComms.MatchFound)
			return;
		Plugin.Log.LogInfo(data);
		MainMenuLoadscreen loadscreen = GameObject.FindFirstObjectByType<MainMenuLoadscreen>();
		loadscreen.canCancel = false;
		SpeedrunningUtils.OBS.ObsWebsocket.StartRecording();
		PlayerResult result = (PlayerResult)data;
		SRComms.Matchmaking = false;
		SRComms.MatchFound = true;
		AsyncOperation operation = SceneManager.LoadSceneAsync(2);
		loadOperation = operation;
		Plugin.Log.LogInfo($"found match, we are up against {result.playerName}");
		Task.Run(() => KeepTabsOnProgress(result.playerName));
	}
	
	private static async void KeepTabsOnProgress(int playerCount) 
	{
		PlayerPrefs.SetInt("Slot", Plugin.SpeedrunSlot);
		HomeScreen screen = GameObject.FindFirstObjectByType<HomeScreen>();
		MainMenuLoadscreen loadscreen = GameObject.FindFirstObjectByType<MainMenuLoadscreen>();
		GameObject Electric = GameObject.Find("3Dstuff").Find("Loading");
		bool invoked = false;
		while (true) 
		{
			Electric.SetActive(false);
			if (SRComms.MatchLoaded) 
				break;
			int progress = (int)(loadOperation.progress*100);
			if (progress > 60 && !invoked) 
			{
				invoked = true;
				screen._Start.Invoke();
			}
			loadOperation.completed += (AsyncOperation _) =>
			{
				Plugin.Log.LogInfo("Loaded scene, starting up ReadySync");
			};
			loadscreen.SetText($"Loading into private match against {playerCount} opponents. ({progress}%)");
			await Task.Delay(10);
		}
	}
	
	private static async void KeepTabsOnProgress(string playerName) 
	{
		PlayerPrefs.SetInt("Slot", Plugin.SpeedrunSlot);
		HomeScreen screen = GameObject.FindFirstObjectByType<HomeScreen>();
		MainMenuLoadscreen loadscreen = GameObject.FindFirstObjectByType<MainMenuLoadscreen>();
		GameObject Electric = GameObject.Find("3Dstuff").Find("Loading");
		bool invoked = false;
		while (true) 
		{
			Electric.SetActive(false);
			if (SRComms.MatchLoaded) 
				break;
			int progress = (int)(loadOperation.progress*100);
			if (progress > 60 && !invoked) 
			{
				invoked = true;
				screen._Start.Invoke();
			}
			loadOperation.completed += (AsyncOperation _) =>
			{
				Plugin.Log.LogInfo("Loaded scene, starting up ReadySync");
			};
			loadscreen.SetText($"Match found. Opponent: {playerName}\n Loading. ({progress}%)");
			await Task.Delay(10);
		}
	}
}
