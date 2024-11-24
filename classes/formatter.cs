using MessagePack;
using MessagePack.Formatters;

namespace VapSRClient;

public class ResponseFormatter : IMessagePackFormatter<Response> 
{
	public Response Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) 
	{
		Response request = new();
		reader.ReadMapHeader();
		reader.ReadString();
		request.type = reader.ReadString();
		reader.ReadString();

		request.data = request.type switch
		{
			"OpponentForfeit" => MessagePackSerializer.Deserialize<PlayerResult>(ref reader, options),
			"MatchFound" => MessagePackSerializer.Deserialize<PlayerResult>(ref reader, options),
			"PlayerFinishedStage" => MessagePackSerializer.Deserialize<PlayerCompletedStageInfo>(ref reader, options),
			"PrivateRoomJoinAttempt" => MessagePackSerializer.Deserialize<RoomJoinAttempt>(ref reader, options),
			"RngSeedSet" =>  MessagePackSerializer.Deserialize<RngData>(ref reader, options),
			"PrivateRoomCreated" => MessagePackSerializer.Deserialize<RoomData>(ref reader, options),
			"ReplicateRoomData" => MessagePackSerializer.Deserialize<RoomReplicationData>(ref reader, options),
			"OtherPlayerForfeit" => MessagePackSerializer.Deserialize<PlayerResult>(ref reader, options),
			"PrivateRoomRunFinished" => MessagePackSerializer.Deserialize<RoomRunFinished>(ref reader, options),
			"PrivateRoomBatchRunsFinished" => MessagePackSerializer.Deserialize<BatchRoomRunsFinished>(ref reader, options),
			"PrivateRoomNewHost" => MessagePackSerializer.Deserialize<PrivateRoomNewHost>(ref reader, options),
			_ => null,
		};
		return request;
	}
	
	public void Serialize(ref MessagePackWriter writer, Response value, MessagePackSerializerOptions options) 
	{
		MessagePackSerializer.Serialize(ref writer, value, options);
	}
}