using MessagePack;

namespace VapSRClient.Networking.Common;

[MessagePackObject(true)]
public struct RoomDataCommon {
  public string code;
}

[MessagePackObject(true)]
public struct RngDataCommon {
  public int seed;
}

[MessagePackObject(true)]
public struct PlayerResultCommon {
  public string playerName;
}