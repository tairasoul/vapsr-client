using MessagePack;

namespace VapSRClient;

public enum SendingMessageType 
{
	UserInfo,
	StartMatchmaking,
	LoadingFinished,
	RouteStageFinished,
	RunFinished,
	LeftToMenu,
	RngSeed,
	Disconnect,
	CancelMatchmaking,
	CreatePrivateRoom,
	JoinPrivateRoom,
	PrivateRoomStart,
	LeavePrivateRoom,
	RequestCurrentHost
}

public enum ReceivingMessageType 
{
	MatchmakingStarted,
	MatchFound,
	StartRun,
	PlayerFinishedStage,
	RunStopped,
	OtherPlayerForfeit,
	RequestSeed,
	RngSeedSet,
	PrivateRoomCreated,
	PrivateRoomStarted,
	PrivateRoomJoinAttempt,
	ReplicateRoomData,
	OpponentForfeit,
	PrivateRoomRunFinished,
	PrivateRoomBatchRunsFinished,
	PrivateRoomEveryoneCompleted,
	PrivateRoomNewHost
}

[MessagePackObject(true)]
public class RngData 
{
	public RngData() {}
	public int seed;
}

[MessagePackObject(true)]
public class Request 
{
	public Request() {}
	public Request(SendingMessageType sendType, object? body) 
	{
		type = sendType.ToString();
		data = body;
	}
	public Request(SendingMessageType sendType) 
	{
		type = sendType.ToString();
		data = null;
	}
	
	public string type;
	
	public object? data;
	
	public byte[] Bytes() 
	{
		MessagePackSerializerOptions opts = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
		return MessagePackSerializer.Serialize(this, opts);
	}
}

[MessagePackObject(true)]
public class Run 
{
	public Run() {}
	public string name;
	
	public float time;
}

[MessagePackObject(true)]
public class BatchRoomRunsFinished 
{
	public BatchRoomRunsFinished() {}
	public Run[] times;
}

[MessagePackObject(true)]
public class RoomRunFinished 
{
	public RoomRunFinished() {}
	public string player;
	
	public float time;
}

[MessagePackObject(true)]
public class Response 
{
	public Response() {}
	public string type;
	
	public object? data;
}

[MessagePackObject(true)]
public class PlayerCompletedStageInfo 
{
	public PlayerCompletedStageInfo() {}
	public string playerName;
	
	public string stage;
}

[MessagePackObject(true)]
public class PlayerCompletedStage
{
	public PlayerCompletedStage() {}
	public string stage;
}

[MessagePackObject(true)]
public class UserInfo 
{
	public UserInfo() {}
	public string username;
	//[Key(1)]
	//public string id;
}

[MessagePackObject(true)]
public class PlayerResult 
{
	public PlayerResult() {}
	public string playerName;
}

[MessagePackObject(true)]
public class RunFinishedInfo 
{
	public RunFinishedInfo() {}
	public float time;
}

[MessagePackObject(true)]
public class PrivateRoomNewHost 
{
	public PrivateRoomNewHost() {}
	public bool youAreNewHost;
}

[MessagePackObject(true)]
public class RunFinishedRelayInfo 
{
	public RunFinishedRelayInfo() {}
	public string playerName;
	
	public float time;
	
	public bool youWon;
}

[MessagePackObject(true)]
public class RoomData 
{
	public RoomData() {}
	public string code;
}

[MessagePackObject(true)]
public class RoomReplicationData 
{
	public RoomReplicationData() {}
	public string host;
	
	public string[] opponents;
	
	public string code;
}

[MessagePackObject(true)]
public class RoomJoinAttempt 
{
	public RoomJoinAttempt() {}
	public bool RoomJoined;
	
	public RoomReplicationData? replicationData;
}
