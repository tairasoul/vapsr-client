namespace VapSRClient.Client;

public static class ClientsideRoomData 
{
	public static string RoomCode;
	public static string Host;
	public static bool isHost = false;
	public static string[] OtherPlayers;
	
	public static void UpdatePlayers() 
	{
		RoomScreen.RemoveAllPlayers();
		Player host = new() 
		{
			name = Host,
			isHost = true
		};
		Player[] players = [host];
		foreach (string player in OtherPlayers ?? []) 
		{
			Player plr = new() 
			{
				name = player,
				isHost = false
			};
			players = [ .. players, plr ];
		}
		RoomScreen.UpdatePlayers(players);
	}
}