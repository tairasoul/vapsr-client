using System;
using VapSRClient.Networking.PacketTypes;

namespace VapSRClient.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class MessageHandler : Attribute 
{
	public S2CTypes type;
	
	public MessageHandler(S2CTypes type) 
	{
		this.type = type;
	}
}