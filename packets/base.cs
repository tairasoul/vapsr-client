using MessagePack;
using VapSRClient.Networking.PacketTypes;

namespace VapSRClient.Networking.Base;

[MessagePackObject(true)]
public struct ClientRequest {
  public C2STypes type;
  public object? data;
  public byte[] Bytes() {
		MessagePackSerializerOptions opts = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
		return MessagePackSerializer.Serialize(this, opts);
  }
}

[MessagePackObject(true)]
public struct ServerResponse {
  public S2CTypes type;
  public object? data;
  public byte[] Bytes() {
		MessagePackSerializerOptions opts = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
		return MessagePackSerializer.Serialize(this, opts);
  }
}