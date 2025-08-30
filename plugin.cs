using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Drawing;
using System.Drawing.Imaging;
using VapSRClient.Client;

namespace VapSRClient;

// initial test done by vainstar, im rewriting and further working on/polishing the code and server
// so, mix names. vainstar + tairasoul
[BepInPlugin("vainsoul.vaproxy.vapsrclient", "VAP-SR-Client", "0.2.0")]
public class Plugin : BaseUnityPlugin 
{
	internal static ManualLogSource Log;
	internal static ConfigFile cfg;
	internal static Client.Client comms;
	internal static Harmony harmony = new("vainsoul.vaproxy.vapsrclient");
	internal static Timer timer;
	internal static Texture2D CursorImage;
	internal static CustomCursor cursorInstance;
	internal static int SpeedrunSlot = 10000;
	
	private void ReadCursor() 
	{
		using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("vapsr-client.resources.cursor.png");
		using Bitmap bitmap = new(stream);
		byte[] pixelData = new byte[bitmap.Width * bitmap.Height * 4];
		Rectangle rect = new(0, 0, bitmap.Width, bitmap.Height);
		BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
		System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, pixelData, 0, pixelData.Length);
		bitmap.UnlockBits(bmpData);
		CursorImage = new(bitmap.Width, bitmap.Height, TextureFormat.RGBA32, false);
		CursorImage.LoadRawTextureData(pixelData);
		CursorImage.Apply();
	}
	
	void Awake() 
	{
		cfg = Config;
		Log = Logger;
		Log.LogInfo("VAP-SR-Client awake.");
		Log.LogWarning("KEEP IN MIND VAPSR IS STILL EXTREMELY UNSTABLE. EXPECT CRASHES OR FEATURES NOT WORKING CORRECTLY");
		harmony.PatchAll();
	}
	
	void Start() 
	{
		ReadCursor();
		Settings.Register();
		GameObject cursorObj = new("CustomCursor");
		DontDestroyOnLoad(cursorObj);
		cursorInstance = cursorObj.AddComponent<CustomCursor>();
		SceneManager.activeSceneChanged += SceneLoaded;
		comms = new(cfg);
		timer = new();
		GameObject TimerObj = new("SpeedrunTimer");
		DontDestroyOnLoad(TimerObj);
		timer = TimerObj.AddComponent<Timer>();
		Application.wantsToQuit += () => 
		{
			comms.Disconnect();
			return true;
		};
		AppDomain.CurrentDomain.ProcessExit += (sender, e) => 
		{
			comms.Disconnect();
		};
		try 
		{
			comms.Connect();
		}
		catch {};
	}
	
	private bool inMenu = false;
	
	void Update() 
	{
		if (inMenu) 
		{
			CustomCursor.visible = true;
		}
		else 
		{
			CustomCursor.visible = Cursor.visible;
		}
	}
	
	void SceneLoaded(Scene old, Scene scene) 
	{
		if (scene.name == "Menu") 
		{
			Settings.ClearSlotData(SpeedrunSlot);
			inMenu = true;
			GameObject MainMenuLoadscreenObj = new("VapSRLoadscreen");
			MainMenuLoadscreenObj.AddComponent<MainMenuLoadscreen>();
			MainMenuLoadscreenObj.SetActive(true);
			if (Client.Client.MatchLoaded && !EndScreen.runFinished && !Client.Client.InPrivateRoom)
				comms.Forfeit();
			Client.Client.MatchLoaded = false;
			Client.Client.MatchFound = false;
			timer.StopTimer();
			timer.ResetTime();
			if (Client.Client.InPrivateRoom) 
			{
				RoomScreen.CreateBaseCanvas(ClientsideRoomData.RoomCode, comms.StartPrivateRoom, comms.LeavePrivateRoom, ClientsideRoomData.isHost);
				ClientsideRoomData.UpdatePlayers();
			}
		}
		else {
			inMenu = false;
		}
	}
}