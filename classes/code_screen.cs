using System;
using MainMenuSettings.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace VapSRClient;

public class CodeScreen 
{
	private static string Code = "";
	internal static GameObject techroom;
	public static void CreateBaseCanvas(Action<string> submit, Action<string> cancel) 
	{
		Code = "";
		GameObject obj = new("CodeScreen");
		obj.SetActive(false);
		RectTransform rect = obj.AddComponent<RectTransform>();
		rect.localScale = new(0.0338f, 0.0338f, 0.0338f);
		rect.Rotate(0, 270, 0);
		rect.position = new(0, 2.3f, 0);
		rect.anchorMax = new(0, 0);
		rect.anchorMin = new(0, 0);
		GameObject MainSquare = obj.AddObject("MainSquare");
		RectTransform msrect = MainSquare.AddComponent<RectTransform>();
		msrect.anchoredPosition = new(-7, 62);
		msrect.sizeDelta = new(300, 300);
		GameObject EnterCode = MainSquare.AddObject("Enter Code");
		RectTransform ecRect = EnterCode.AddComponent<RectTransform>();
		ecRect.sizeDelta = new(300, 50);
		Image ecBg = EnterCode.AddComponent<Image>();
		ecBg.color = new(1, 1, 1, 0.14f);
		GameObject textO = EnterCode.AddObject("Text");
		RectTransform textR = textO.AddComponent<RectTransform>();
		textR.sizeDelta = new(300, 50);
		Text textC = textO.AddComponent<Text>();
		textC.fontSize = 25;
		textC.font = ReadySync.GetFont("Arial");
		textC.alignment = TextAnchor.MiddleCenter;
		GameObject placeholder = EnterCode.AddObject("Placeholder");
		RectTransform placeholderR = placeholder.AddComponent<RectTransform>();
		placeholderR.sizeDelta = new(300, 50);
		Text placeholderC = placeholder.AddComponent<Text>();
		placeholderC.fontSize = 25;
		placeholderC.font = ReadySync.GetFont("Arial");
		placeholderC.text = "Enter code";
		placeholderC.alignment = TextAnchor.MiddleCenter;
		placeholderC.color = new(1, 1, 1, 0.3f);
		InputField field = EnterCode.AddComponent<InputField>();
		field.textComponent = textC;
		field.onEndEdit.AddListener((string code) => Code = code);
		field.onSubmit.AddListener((string code) => Code = code);
		field.onValueChanged.AddListener((string code) => Code = code);
		field.placeholder = placeholderC;
		GameObject join = buttonObj("join", submit);
		GameObject cancelO = buttonObj("cancel", cancel);
		join.SetParent(MainSquare, false);
		cancelO.SetParent(MainSquare, false);
		join.transform.localScale = new(1, 1, 1);
		join.transform.position = new(0, 2.9956f, -4.4616f);
		cancelO.transform.localScale = new(1, 1, 1);
		cancelO.transform.position = new(0, 2.996f, 3.9923f);
		techroom = GameObject.Find("Tech_room_1");
		DisableObjects();
		obj.AddComponent<GraphicRaycaster>();
		obj.SetActive(true);
	}
	
	public static async void DisableObjects() 
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
	
	private static GameObject buttonObj(string txt, Action<string> clicked) 
	{
		GameObject button = new("button");
		Button buttonComp = button.AddComponent<Button>();
		buttonComp.onClick.AddListener(() => clicked.Invoke(Code));
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
		GameObject Room = GameObject.Find("CodeScreen");
		Room.GetComponent<GraphicRaycaster>().enabled = false;
		Terminal.SetActive(true);
		screen.enabled = true;
		Canvas.transform.position = new(-8.79f, 4.38f, -0.25f);
		vainsoul.SetActive(true);
		techroom.SetActive(true);
		GameObject.Destroy(Room);
		Cursor.visible = false;
	}
}