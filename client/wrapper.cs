using System;
using System.Threading.Tasks;
using TcpSharp;
using VapSRClient.Networking.Base;
using VapSRClient.Networking.PacketTypes;

namespace VapSRClient.Client;

class SocketWrapper {
  public TcpSharpSocketClient client;
  public int port { 
    get => client.Port;
    set {
      client.Port = value;
    }
  }
  public string host {
    get => client.Host;
    set {
      client.Host = value;
    }
  }
  public SocketWrapper() {
    client = new()
    {
      KeepAlive = true,
      KeepAliveTime = 10,
      KeepAliveInterval = 1,
      KeepAliveRetryCount = 5
    };
  }

  public void SendRequest(C2STypes request) {
    SendRequest(request, null);
  }

  public void SendRequest(C2STypes request, object? data) {
    Plugin.Log.LogDebug($"Sending C2S packet of type {request}");
    client.SendBytes(new ClientRequest() { type = request, data = data }.Bytes());
  }

  public void Reconnect() {
    Task.Run(async () =>
    {
      Disconnect();
      await Task.Delay(500);
      client.Connect();
    });
  }

  public void Disconnect() {
    if (!client.Connected) return;
    SendRequest(C2STypes.Disconnect);
    client.Disconnect();
  }
}