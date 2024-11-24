using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using MainMenuSettings.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace VapSRClient;

public struct Player 
{
	public string name;
	public bool isHost;
}

public class RoomScreen 
{
	private static GameObject techroom;
	public static void CreateBaseCanvas(string code, Action start, Action leave, bool host) 
	{
		GameObject obj = new("RoomScreen");
		RectTransform rect = obj.AddComponent<RectTransform>();
		obj.SetActive(false);
		rect.localScale = new(0.0338f, 0.0338f, 0.0338f);
		rect.Rotate(0, 270, 0);
		rect.position = new(0, 2.3f, 0);
		rect.anchorMax = new(0, 0);
		rect.anchorMin = new(0, 0);
		obj.AddComponent<GraphicRaycaster>();
		GameObject MainSquare = obj.AddObject("MainSquare");
		RectTransform msrect = MainSquare.AddComponent<RectTransform>();
		msrect.anchoredPosition = new(-7, 62);
		msrect.sizeDelta = new(300, 300);
		GameObject CodeSquare = MainSquare.AddObject("Code");
		RectTransform coderect = CodeSquare.AddComponent<RectTransform>();
		coderect.anchoredPosition = new(0, 140);
		coderect.sizeDelta = new(100, 20);
		Button csButton = CodeSquare.AddComponent<Button>();
		csButton.onClick.AddListener(() => 
		{
			GUIUtility.systemCopyBuffer = code;
		});
		Image codeimage = CodeSquare.AddComponent<Image>();
		codeimage.color = new(1, 1, 1, 0.14f);
		GameObject codeText = CodeSquare.AddObject("Text");
		RectTransform ctRect = codeText.AddComponent<RectTransform>();
		ctRect.sizeDelta = new(100, 20);
		Text ctText = codeText.AddComponent<Text>();
		ctText.verticalOverflow = VerticalWrapMode.Overflow;
		ctText.text = code;
		ctText.font = ReadySync.GetFont("Arial");
		ctText.fontSize = 20;
		ctText.alignment = TextAnchor.MiddleCenter;
		ctText.resizeTextForBestFit = true;
		GameObject playersObj = MainSquare.AddObject("Players");
		RectTransform playersRect = playersObj.AddComponent<RectTransform>();
		GameObject playerlist = playersObj.AddObject("PlayerList");
		ScrollRect scrollR = playerlist.AddComponent<ScrollRect>();
		scrollR.horizontal = false;
		GameObject viewport = playerlist.AddObject("Viewport");
		RectTransform vRect = viewport.AddComponent<RectTransform>();
		vRect.sizeDelta = new(300, 250);
		Image vImage = viewport.AddComponent<Image>();
		vImage.color = new(1, 1, 1, 0.1f);
		GameObject content = viewport.AddObject("Content");
		RectTransform cRect = content.AddComponent<RectTransform>();
		cRect.sizeDelta = new(300, 250);
		content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		scrollR.content = cRect;
		GridLayoutGroup gridLayout = content.AddComponent<GridLayoutGroup>();
		gridLayout.cellSize = new(100, 20);
		gridLayout.childAlignment = TextAnchor.UpperLeft;
		playersRect.sizeDelta = new(300, 250);
		playersRect.anchoredPosition = new(0, -2);
		GameObject leaveButton = buttonObj("leave", leave);
		leaveButton.SetParent(MainSquare, false);
		RectTransform lRect = leaveButton.GetComponent<RectTransform>();
		if (host) {
			GameObject startButton = buttonObj("start", start);
			startButton.SetParent(MainSquare, false);
			RectTransform sRect = startButton.GetComponent<RectTransform>();
			Plugin.cursorInstance.StartCoroutine(PositionSet(sRect, lRect));
		}
		else 
			Plugin.cursorInstance.StartCoroutine(PositionSet(lRect));
		if (CodeScreen.techroom != null)
			techroom = CodeScreen.techroom;
		else
			techroom = GameObject.Find("3Dstuff").Find("Tech_room_1");
		DisableObjects();
		obj.SetActive(true);
	}
	
	static IEnumerator PositionSet(RectTransform leave) 
	{
		yield return new WaitForSeconds(0.2f);
		leave.position = new(0, -0.404f, 3.9923f);
	}
	
	static IEnumerator PositionSet(RectTransform start, RectTransform leave) 
	{
		yield return new WaitForSeconds(0.2f);
		start.position = new(0, -0.4044f, -4.4616f);
		leave.position = new(0, -0.404f, 3.9923f);
	}
	
	static T Find<T>(Func<T, bool> predicate)
	{
		foreach (T find in Resources.FindObjectsOfTypeAll(typeof(T)).Cast<T>())
		{
			if (predicate(find)) return find;
		}
		return default;
	}
	
	public static void DisableObjects() 
	{
		GameObject Canvas = GameObject.Find("Canvas");
		GameObject vainsoul = Canvas.Find("vainsoul.vaproxy.vap-sr-client");
		GameObject Terminal = GameObject.Find("3Dstuff").Find("WallTerminal (1)");
		HomeScreen screen = Canvas.GetComponent<HomeScreen>();
		techroom.SetActive(false);
		screen.enabled = false;
		Terminal.SetActive(false);
		Canvas.transform.position = new(-8.79f, 4.38f, 54);
		vainsoul.SetActive(false);
	}
	
	public static void RemoveAllPlayers() 
	{
		GameObject Players = GameObject.Find("RoomScreen").Find("MainSquare").Find("Players").Find("PlayerList").Find("Viewport").Find("Content");
		GameObject[] children = Players.GetChildren();
		if (children.Length != 0)
			foreach (GameObject child in Players.GetChildren())
				GameObject.Destroy(child);
	}
	
	public static void UpdatePlayers(Player[] players) 
	{
		foreach (Player player in players) 
			CreatePlayer(player.name, player.isHost);
		Task.Run(RefreshView);
	}
	
	private static async Task RefreshView() 
	{
		GameObject Players = GameObject.Find("RoomScreen").Find("MainSquare").Find("Players").Find("PlayerList").Find("Viewport").Find("Content");
		await Task.Delay(200);
		Players.SetActive(false);
		await Task.Delay(50);
		Players.SetActive(true);
	}
	
	private static void CreatePlayer(string name, bool isHost) 
	{
		GameObject Players = GameObject.Find("RoomScreen").Find("MainSquare").Find("Players").Find("PlayerList").Find("Viewport").Find("Content");
		GameObject player = new(name);
		RectTransform pRect = player.AddComponent<RectTransform>();
		pRect.sizeDelta = new(100, 30);
		Image pImage = player.AddComponent<Image>();
		if (isHost)
			pImage.color = new(0.25f, 0.5f, 0.6f, 0.6f);
		else
			pImage.color = new(1, 1, 1, 0.14f);
		GameObject pText = player.AddObject("Text");
		RectTransform textRect = pText.AddComponent<RectTransform>();
		textRect.sizeDelta = new(100, 20);
		Text text = pText.AddComponent<Text>();
		text.font = ReadySync.GetFont("Arial");
		text.text = name;
		text.resizeTextForBestFit = true;
		text.alignment = TextAnchor.MiddleCenter;
		player.SetParent(Players, false);
	}
	
	private static GameObject buttonObj(string txt, Action clicked) 
	{
		GameObject button = new("button");
		Button buttonComp = button.AddComponent<Button>();
		buttonComp.onClick.AddListener(() => clicked.Invoke());
		RectTransform bRect = button.AddComponent<RectTransform>();
		bRect.sizeDelta = new(50, 20);
		Image bImage = button.AddComponent<Image>();
		bImage.color = new(1, 1, 1, 0.14f);
		GameObject bText = button.AddObject("Text");
		RectTransform textRect = bText.AddComponent<RectTransform>();
		textRect.sizeDelta = new(50, 20);
		Text text = bText.AddComponent<Text>();
		text.fontSize = 14;
		text.font = ReadySync.GetFont("Arial");
		text.text = txt;
		text.alignment = TextAnchor.MiddleCenter;
		return button;
	}
	
	public static void DeactivateScreen() 
	{
		GameObject Canvas = GameObject.Find("Canvas");
		GameObject vainsoul = Canvas.Find("vainsoul.vaproxy.vap-sr-client");
		GameObject Terminal = GameObject.Find("3Dstuff").Find("WallTerminal (1)");
		HomeScreen screen = Canvas.GetComponent<HomeScreen>();
		GameObject Room = GameObject.Find("RoomScreen");
		Terminal.SetActive(true);
		screen.enabled = true;
		Canvas.transform.position = new(-8.79f, 4.38f, -0.25f);
		vainsoul.SetActive(true);
		techroom.SetActive(true);
		GameObject.Destroy(Room);
		Cursor.visible = false;
	}
	
	public static void DeactivateScreenAlternate() 
	{
		GameObject Canvas = GameObject.Find("Canvas");
		GameObject Terminal = GameObject.Find("3Dstuff").Find("WallTerminal (1)");
		HomeScreen screen = Canvas.GetComponent<HomeScreen>();
		GameObject Room = GameObject.Find("RoomScreen");
		Terminal.SetActive(true);
		screen.enabled = true;
		Canvas.transform.position = new(-8.79f, 4.38f, -0.25f);
		techroom.SetActive(true);
		Room.SetActive(false);
	}
}