using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using SpeedrunningUtils;
using MainMenuSettings.Extensions;
using BepInEx;
using VapSRClient.Networking.PacketTypes;
using VapSRClient.Attributes;
using VapSRClient.Networking.S2C;
using VapSRClient.Networking.Common;

namespace VapSRClient.Client;

public static class Handlers 
{
  static AsyncOperation? loadOperation;
  [MessageHandler(S2CTypes.PlayerFinishedStage)]
  public static void PlayerFinishedStage(object? data) {
    PlayerCompletedStageS2C info = (PlayerCompletedStageS2C)data;
    if (!info.stage.IsNullOrWhiteSpace())
      EventLog.Log($"{info.playerName} completed split {info.stage}");
  }

  [MessageHandler(S2CTypes.StartRun)]
  public static void StartRun(object? data) {
    if (Client.RunStarted)
      return;
    Client.RunStarted = true;
    ReadySync sync = GameObject.Find("ReadySync").GetComponent<ReadySync>();
    Plugin.Log.LogInfo("Starting our run.");
    sync.RunStarted();
  }

  [MessageHandler(S2CTypes.MatchmakingStarted)]
  public static void MatchmakingStarted(object? data) {
    Client.Matchmaking = true;
  }

  [MessageHandler(S2CTypes.RunStopped)]
  public static void RunStopped(object? data) {
    EndScreen end = GameObject.Find("EndScreen").GetComponent<EndScreen>();
    Plugin.Log.LogInfo("Run stopped.");
    RunFinishedS2C info = (RunFinishedS2C)data;
    Task.Run(() => end.OpponentFinished(info.playerName, info.time, info.youWon));
  }

  [MessageHandler(S2CTypes.OtherPlayerForfeit)]
  public static void OtherPlayerForfeit(object? data) {
    PlayerResultCommon result = (PlayerResultCommon)data;
    EventLog.Log($"{result.playerName} forfeited run");
    Client.MatchFound = false;
  }

  [MessageHandler(S2CTypes.OpponentForfeit)]
  public static void OpponentForfeit(object? data) {
    PlayerResultCommon result = (PlayerResultCommon)data;
    EventLog.Log($"{result.playerName} forfeited");
  }

  [MessageHandler(S2CTypes.RequestSeed)]
  public static void RequestSeed(object? data) {
    Plugin.comms.RespondWithSeed();
  }

  [MessageHandler(S2CTypes.RngSeed)]
  public static void SetRngSeed(object? data) {
    RngDataCommon rng = (RngDataCommon)data;
    Random.InitState(rng.seed);
  }

  [MessageHandler(S2CTypes.PrivateRoomCreated)]
  public static void PrivateRoomCreated(object? data) {
    if (Client.InPrivateRoom)
      return;
    RoomDataCommon roomData = (RoomDataCommon)data;
    ClientsideRoomData.Host = Plugin.comms.username.Value;
    ClientsideRoomData.RoomCode = roomData.code;
    ClientsideRoomData.isHost = true;
    Client.InPrivateRoom = true;
    RoomScreen.CreateBaseCanvas(roomData.code, Plugin.comms.StartPrivateRoom, Plugin.comms.LeavePrivateRoom, true);
    ClientsideRoomData.UpdatePlayers();
  }

  [MessageHandler(S2CTypes.PrivateRoomJoinAttempt)]
  public static void PrivateRoomJoinAttempt(object? data) {
    RoomJoinAttemptS2C attempt = (RoomJoinAttemptS2C)data;
    if (attempt.RoomJoined && !Client.InPrivateRoom && attempt.replicationData.HasValue) {
      GameObject.Destroy(GameObject.Find("CodeScreen"));
      ClientsideRoomData.RoomCode = attempt.replicationData.Value.code;
      ClientsideRoomData.Host = attempt.replicationData.Value.host;
      ClientsideRoomData.OtherPlayers = attempt.replicationData.Value.opponents;
      Client.InPrivateRoom = true;
			RoomScreen.CreateBaseCanvas(ClientsideRoomData.RoomCode, Plugin.comms.StartPrivateRoom, Plugin.comms.LeavePrivateRoom, false);
			ClientsideRoomData.UpdatePlayers();
    }
  }
  
  [MessageHandler(S2CTypes.ReplicateRoomData)]
  public static void ReplicateRoomData(object? data) {
    RoomReplicationDataS2C replicationData = (RoomReplicationDataS2C)data;
    ClientsideRoomData.Host = replicationData.host;
    ClientsideRoomData.OtherPlayers = replicationData.opponents;
    ClientsideRoomData.RoomCode = replicationData.code;
    ClientsideRoomData.UpdatePlayers();
  }

  [MessageHandler(S2CTypes.PrivateRoomNewHost)]
  public static void NewHostResponse(object? data) {
    PrivateRoomNewHostS2C host = (PrivateRoomNewHostS2C)data;
    ClientsideRoomData.isHost = host.youAreNewHost;
  }

  [MessageHandler(S2CTypes.PrivateRoomRunFinished)]
  public static void PrivateRoomRunFinished(object? data) {
    RunFinishedS2C finished = (RunFinishedS2C)data;
    if (Client.MatchLoaded) {
      Plugin.Log.LogInfo($"Player {finished.playerName} finished with time finished with time {PlacementScreen.FormatTime(finished.time)} ({finished.time})");
      Placement placement = new()
      {
        name = finished.playerName,
        time = finished.time
      };
      Task.Run(async () => await RunFinished(placement));
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

  [MessageHandler(S2CTypes.PrivateRoomBatchRunsFinished)]
  public static void PrivateRoomBatch(object? data) {
    BatchRoomRunsFinishedS2C finished = (BatchRoomRunsFinishedS2C)data;
    if (Client.MatchLoaded) {
      Task.Run(async () => await BatchUpdate(finished.times));
    }
  }
	
	static async Task BatchUpdate(RunS2C[] runs) 
	{
		while (true) 
		{
			if (PlacementUpdater.instance != null)
				break;
			await Task.Delay(1);
		}
		PlacementUpdater updater = PlacementUpdater.instance;
		foreach (RunS2C run in runs) 
		{
			Placement placement = new() 
			{
				name = run.name,
				time = run.time
			};
			updater.PlayerFinished(placement);
		}
	}

  [MessageHandler(S2CTypes.PrivateRoomStarted)]
  public static void PrivateRoomStarted(object? data) {
    if (Client.MatchFound)
      return;
    Plugin.Log.LogInfo("our private room has started.");
    Client.MatchFound = true;
    RoomScreen.DeactivateScreenAlternate();
    SpeedrunningUtils.OBS.ObsWebsocket.StartRecording();
    MainMenuLoadscreen loadscreen = GameObject.FindFirstObjectByType<MainMenuLoadscreen>();
    loadscreen.canCancel = false;
    AsyncOperation operation = SceneManager.LoadSceneAsync(2);
    loadOperation = operation;
		Task.Run(() => KeepTabsOnProgress(ClientsideRoomData.OtherPlayers.Length));
  }
	
	[MessageHandler(S2CTypes.PrivateRoomEveryoneCompleted)]
	public static void EveryoneCompleted(object? data) 
	{
		PlacementScreen placement = GameObject.FindFirstObjectByType<PlacementScreen>();
		placement.CanReturnToMenu = true;
	}

  [MessageHandler(S2CTypes.MatchFound)]
  public static void MatchFound(object? data) {
    if (Client.MatchFound)
      return;
    MainMenuLoadscreen loadscreen = GameObject.FindFirstObjectByType<MainMenuLoadscreen>();
    loadscreen.canCancel = true;
    SpeedrunningUtils.OBS.ObsWebsocket.StartRecording();
    PlayerResultCommon result = (PlayerResultCommon)data;
    Client.Matchmaking = false;
    Client.MatchFound = true;
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
    bool logged = false;
		while (true) 
		{
			Electric.SetActive(false);
			if (Client.MatchLoaded) 
				break;
			int progress = (int)(loadOperation.progress*100);
			if (progress > 60 && !invoked) 
			{
				invoked = true;
				screen._Start.Invoke();
			}
      loadOperation.completed += (AsyncOperation _) =>
			{
        if (logged) return;
        logged = true;
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
    bool logged = false;
		while (true) 
		{
			Electric.SetActive(false);
			if (Client.MatchLoaded) 
				break;
			int progress = (int)(loadOperation.progress*100);
			if (progress > 60 && !invoked) 
			{
				invoked = true;
				screen._Start.Invoke();
			}
			loadOperation.completed += (AsyncOperation _) =>
			{
        if (logged) return;
        logged = true;
				Plugin.Log.LogInfo("Loaded scene, starting up ReadySync");
			};
			loadscreen.SetText($"Match found. Opponent: {playerName}\n Loading. ({progress}%)");
			await Task.Delay(10);
		}
	}
}