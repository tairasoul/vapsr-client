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

namespace VapSRClient;

public struct HandlerClassInfo 
{
	public MethodInfo handler;
	public MessageHandler attribute;
}

public class SRComms
{
	readonly TcpSharpSocketClient client;
	readonly ConfigEntry<string> host;
	readonly ConfigEntry<int> port;
	internal readonly ConfigEntry<string> username;
	internal static bool MatchFound = false;
	internal static bool MatchLoaded = false;
	internal static bool Matchmaking = false;
	internal static bool InPrivateRoom = false;
	internal static bool RunStarted = false;
	HandlerClassInfo[] info = [];
	
	public SRComms(ConfigFile config) 
	{
		client = new();
		host = config.Bind("Server", "Host", "127.0.0.1", "The server to connect to.");
		port = config.Bind("Server", "Port", 7777, "Port to connect to. VapSR servers run on port 7777 by default, if unspecified leave as 7777.");
		username = config.Bind("User", "Username", "vapsr-user", "Username to identify as.");
		client.Host = host.Value;
		client.Port = port.Value;
		client.OnConnected += OnConnected;
		client.OnDataReceived += OnDataReceived;
		client.OnError += OnError;
		client.Connect();
	}
	
	public async Task MatchmakingCancelBind() 
	{
		if (Matchmaking && UnityInput.Current.GetKeyDown(KeyCode.Escape)) 
		{
			Matchmaking = false;
			CancelMatchmaking();
		}
	}
	
	public void Disconnect() 
	{
		if (!client.Connected)
			return;
		client.SendBytes(new Request(SendingMessageType.Disconnect).Bytes());
		client.Disconnect();
	}
	
	public void Reconnect() 
	{
		Task.Run(ReconnectT);
	}
	
	private async Task ReconnectT() 
	{
		Disconnect();
		await Task.Delay(500);
		client.Connect();
	}
	
	public void Connect() 
	{
		if (client.Connected) 
			Task.Run(ReconnectT);
		else
			client.Connect();
	}
	
	private void SendUserInfo() 
	{
		UserInfo info = new() 
		{
			username = username.Value
		};
		Request request = new(SendingMessageType.UserInfo, info);
		client.SendBytes(request.Bytes());
	}
	
	private void GrabHandlers() 
	{
		Type[] types = Assembly.GetExecutingAssembly().GetTypes();
		foreach (Type type in types) 
		{
			MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

			foreach (MethodInfo method in methods)
			{
				MessageHandler? attribute = method.GetCustomAttribute<MessageHandler>();
				if (attribute != null)
				{
					info = [ .. info, new HandlerClassInfo() 
					{
						handler = method,
						attribute = attribute
					}];
				}
			}
		}
	}
	
	private void OnError(object sender, OnClientErrorEventArgs args) 
	{
		Plugin.Log.LogError(args.Exception.Source);
		Plugin.Log.LogError(args.Exception.Message);
		Plugin.Log.LogError(args.Exception.StackTrace);
		Plugin.Log.LogError(args.Exception.InnerException.Source);
		Plugin.Log.LogError(args.Exception.InnerException.Message);
		Plugin.Log.LogError(args.Exception.InnerException.StackTrace);
		Plugin.Log.LogError(args.Exception.InnerException.InnerException.Source);
		Plugin.Log.LogError(args.Exception.InnerException.InnerException.Message);
		Plugin.Log.LogError(args.Exception.InnerException.InnerException.StackTrace);
	}
	
	private void OnConnected(object sender, OnClientConnectedEventArgs args) 
	{
		Plugin.Log.LogDebug("TCP client connected.");
		SendUserInfo();
		GrabHandlers();
	}
	
	private void OnDataReceived(object sender, OnClientDataReceivedEventArgs args) 
	{
		if (Encoding.UTF8.GetString(args.Data) == "k") 
		{
			Plugin.Log.LogDebug("Responding to keepalive request");
			client.SendString("k");
			return;
		}
		MessagePackSerializerOptions opts = MessagePackSerializerOptions.Standard.WithResolver(
			MessagePack.Resolvers.CompositeResolver.Create(
				[new ResponseFormatter()],
				[MessagePack.Resolvers.StandardResolver.Instance]
			)
		).WithCompression(MessagePackCompression.Lz4Block);
		//MessagePackSerializerOptions opts = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
		Response response = MessagePackSerializer.Deserialize<Response>(args.Data, opts);
		Plugin.Log.LogDebug($"Response type: {response.type}");
		Plugin.Log.LogDebug($"Response data: {Newtonsoft.Json.JsonConvert.SerializeObject(response.data)}");
		foreach (HandlerClassInfo classInfo in info) 
		{
			if (classInfo.attribute.type.ToString() == response.type) 
			{
				classInfo.handler.Invoke(null, [response.data]);
				break;
			}
		}
	}
	
	public void StartMatchmaking() 
	{
		Request request = new(SendingMessageType.StartMatchmaking);
		client.SendBytes(request.Bytes());
	}
	
	public void Loaded() 
	{
		MatchLoaded = true;
		Request request = new(SendingMessageType.LoadingFinished);
		client.SendBytes(request.Bytes());
	}
	
	public void SplitCompleted(string split) 
	{
		Plugin.Log.LogDebug($"splitcompleted matchfound: {MatchFound}");
		if (!MatchFound)
			return;
		EventLog.Log($"{Plugin.comms.username.Value} completed split {split}");
		Request request = new(SendingMessageType.RouteStageFinished, new PlayerCompletedStage() { stage = split });
		client.SendBytes(request.Bytes());
	}
	
	public void Forfeit() 
	{
		Request request = new(SendingMessageType.LeftToMenu);
		client.SendBytes(request.Bytes());
	}
	
	public void RunCompleted() 
	{
		Plugin.timer.StopTimer();
		float time = Plugin.timer.GetTime();
		if (InPrivateRoom) 
		{
			PlacementScreen screen = GameObject.Find("PlacementScreen").GetComponent<PlacementScreen>();
			screen.StartCoroutine(screen.RunFinished(time));
			UnityEngine.Random.InitState(Handlers.seed);
		}
		else 
		{
			EndScreen end = GameObject.Find("EndScreen").GetComponent<EndScreen>();
			end.RunFinished(time);
			UnityEngine.Random.InitState(Handlers.seed);
		}
		if (!MatchFound)
			return;
		RunFinishedInfo info = new() 
		{
			time = time
		};
		Request request = new(SendingMessageType.RunFinished, info);
		client.SendBytes(request.Bytes());
	}
	
	public void RespondWithSeed(RngData seed) 
	{
		Request request = new(SendingMessageType.RngSeed, seed);
		client.SendBytes(request.Bytes());
	}
	
	public void CancelMatchmaking() 
	{
		Request request = new(SendingMessageType.CancelMatchmaking);
		client.SendBytes(request.Bytes());
	}
	
	public void CreatePrivateRoom() 
	{
		if (InPrivateRoom)
			return;
		Request request = new(SendingMessageType.CreatePrivateRoom);
		client.SendBytes(request.Bytes());
	}
	
	public void JoinPrivateRoom(string code) 
	{
		if (InPrivateRoom)
			return;
		Request request = new(SendingMessageType.JoinPrivateRoom, new RoomData() { code = code });
		client.SendBytes(request.Bytes());
	}
	
	public void StartPrivateRoom() 
	{
		if (!InPrivateRoom)
			return;
		Request request = new(SendingMessageType.PrivateRoomStart);
		client.SendBytes(request.Bytes());
	}
	
	public void LeavePrivateRoom() 
	{
		if (!InPrivateRoom)
			return;
		Request request = new(SendingMessageType.LeavePrivateRoom);
		client.SendBytes(request.Bytes());
		InPrivateRoom = false;
		RoomScreen.DeactivateScreen();
	}
	
	public void RequestCurrentHost() 
	{
		if (!InPrivateRoom)
			return;
		Request request = new(SendingMessageType.RequestCurrentHost);
		client.SendBytes(request.Bytes());
	}
}