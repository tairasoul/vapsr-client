using UnityEngine;
using MainMenuSettings;

namespace VapSRClient;

public class Settings : MonoBehaviour 
{
	internal static GameObject Page;
	public static void Register() 
	{
		ButtonOption queue = new() 
		{
			Text = "Queue for match",
			Clicked = () => 
			{
				Plugin.comms.StartMatchmaking();
				MainMenuLoadscreen loadscreen = GameObject.FindFirstObjectByType<MainMenuLoadscreen>();
				loadscreen.FindingMatch();
				Page.SetActive(false);
				PlayerPrefs.SetInt("Slot", Plugin.SpeedrunSlot);
			}
		};
		ButtonOption reconnect = new() 
		{
			Text = "Reconnect TCP client",
			Clicked = () => 
			{
				Plugin.comms.Reconnect();
			}
		};
		ButtonOption newPrivateRoom = new() 
		{
			Text = "Create new private room",
			Clicked = () => 
			{
				Plugin.comms.CreatePrivateRoom();
			}
		};
		ButtonOption joinPrivateRoom = new() 
		{
			Text = "Join private room",
			Clicked = () => 
			{
				CodeScreen.CreateBaseCanvas((string code) => 
				{
					Plugin.comms.JoinPrivateRoom(code.Trim());
				}, (string _) => 
				{
					CodeScreen.DeactivateScreen();
				});
			}
		};
		ModOptions options = new() 
		{
			buttons = [queue, reconnect, newPrivateRoom, joinPrivateRoom],
			CreationCallback = (GameObject obj) => 
			{
				Page = obj;
			}
		};
		
		MenuSettings.RegisterMod("VapSRClient", "vainsoul.vaproxy.vap-sr-client", options);
	}

	internal static void ClearSlotData(int ID)
	{
		PlayerPrefs.SetInt("fresh" + ID, 0);
	}
}
