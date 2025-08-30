using BepInEx.Configuration;
using TcpSharp;
using System.Reflection;
using System;
using UnityEngine;
using System.Threading.Tasks;
using MessagePack;
using MessagePack.Formatters;
using System.Text;
using BepInEx;
using System.Collections.Generic;
using VapSRClient.Networking.PacketTypes;
using VapSRClient.Networking.C2S;
using VapSRClient.Attributes;
using VapSRClient.Networking.Common;
using System.Linq;
using VapSRClient.Networking.Base;

namespace VapSRClient.Client;

public class Client {
  readonly SocketWrapper client;
  readonly ConfigEntry<string> host;
  readonly ConfigEntry<int> port;
  internal readonly ConfigEntry<string> username;
  internal static bool MatchFound = false;
  internal static bool MatchLoaded = false;
  internal static bool Matchmaking = false;
  internal static bool InPrivateRoom = false;
  internal static bool RunStarted = false;
  Dictionary<S2CTypes, MethodInfo> info = [];
  public Client(ConfigFile config) {
    client = new();
		host = config.Bind("Server", "Host", "127.0.0.1", "The server to connect to.");
		port = config.Bind("Server", "Port", 7777, "Port to connect to. VapSR servers run on port 7777 by default, if unspecified leave as 7777.");
		username = config.Bind("User", "Username", "vapsr-user", "Username to identify as.");
		client.host = host.Value;
		client.port = port.Value;
    GrabHandlers();
    client.client.OnConnected += (_, _) =>
    {
      SendUserInfo();
      Plugin.Log.LogDebug("TCP client connected, sent user info.");
    };
    client.client.OnDataReceived += OnDataReceived;
  }

  private void OnDataReceived(object sender, OnClientDataReceivedEventArgs args) {
    MessagePackSerializerOptions opts = MessagePackSerializerOptions.Standard.WithResolver(
      MessagePack.Resolvers.CompositeResolver.Create(
        [new ClientRequestFormatter(), new ServerResponseFormatter()],
        [MessagePack.Resolvers.StandardResolver.Instance]
      )
    ).WithCompression(MessagePackCompression.Lz4Block);
    ServerResponse response = MessagePackSerializer.Deserialize<ServerResponse>(args.Data, opts);
    Plugin.Log.LogDebug($"Received S2C packet of type {response.type}");
    if (info.TryGetValue(response.type, out MethodInfo method)) {
      method.Invoke(null, [response.data]);
    }
  }

  public void Disconnect() {
    client.Disconnect();
  }

  public void Connect() {
    client.client.Connect();
  }

  public void Reconnect() {
    client.Reconnect();
  }

  public void StartMatchmaking() {
    client.SendRequest(C2STypes.StartMatchmaking);
  }

  public void Loaded() {
    MatchLoaded = true;
    client.SendRequest(C2STypes.LoadingFinished);
  }

  public void SplitCompleted(string split) {
    if (!MatchFound)
      return;
    EventLog.Log($"{username.Value} completed split {split}");
    client.SendRequest(C2STypes.RouteStageFinished, new PlayerCompletedStageC2S() { stage = split });
  }

  public void Forfeit() {
    client.SendRequest(C2STypes.LeftToMenu);
  }

  public void RunCompleted() {
    Plugin.timer.StopTimer();
    float time = Plugin.timer.GetTime();
		if (InPrivateRoom) 
		{
			PlacementScreen screen = GameObject.Find("PlacementScreen").GetComponent<PlacementScreen>();
			screen.StartCoroutine(screen.RunFinished(time));
		}
		else 
		{
			EndScreen end = GameObject.Find("EndScreen").GetComponent<EndScreen>();
			end.RunFinished(time);
		}
		if (!MatchFound)
			return;
    client.SendRequest(C2STypes.RunFinished, new RunFinishedC2S() { time = time });
  }

  public void RespondWithSeed() {
    int rngSeed = new System.Random().Next();
    UnityEngine.Random.InitState(rngSeed);
    client.SendRequest(C2STypes.RngSeed, new RngDataCommon() { seed = rngSeed });
  }

  public void CancelMatchmaking() {
    client.SendRequest(C2STypes.CancelMatchmaking);
  }

  public void CreatePrivateRoom() {
    if (InPrivateRoom)
      return;
    client.SendRequest(C2STypes.CreatePrivateRoom);
  }

  public void JoinPrivateRoom(string code) {
    if (InPrivateRoom)
      return;
    client.SendRequest(C2STypes.JoinPrivateRoom, new RoomDataCommon() { code = code });
  }

  public void StartPrivateRoom() {
    if (!InPrivateRoom)
      return;
    client.SendRequest(C2STypes.PrivateRoomStart);
  }

  public void LeavePrivateRoom() {
    if (!InPrivateRoom)
      return;
    client.SendRequest(C2STypes.LeavePrivateRoom);
    InPrivateRoom = false;
    RoomScreen.DeactivateScreen();
  }

  public void RequestCurrentHost() {
    if (!InPrivateRoom)
      return;
    client.SendRequest(C2STypes.RequestCurrentHost);
  }

  private void SendUserInfo() {
    UserInfoC2S info = new()
    {
      username = username.Value
    };
    client.SendRequest(C2STypes.UserInfo, info);
  }

  private void GrabHandlers() {
    Type[] types = Assembly.GetExecutingAssembly().GetTypes();
    foreach (Type type in types) {
			MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
      foreach (MethodInfo method in methods) {
        MessageHandler? attribute = method.GetCustomAttribute<MessageHandler>();
        if (attribute != null) {
          info.Add(attribute.type, method);
        }
      }
    }
  }
}