using UnityEngine;
using UnityEngine.UI;
using MainMenuSettings.Extensions;
using System;
using System.Linq;
using System.Collections;
using UnityStandardAssets.ImageEffects;
using EZCameraShake;
using UmbraEvolution;
using VolumetricClouds3;
using BepInEx;
using UnityEngine.SceneManagement;

namespace VapSRClient;

public struct Placement 
{
	public float time;
	public string name;
}

public class PlacementScreen : MonoBehaviour
{
	internal PlacementUpdater updater;
	internal GameObject Placements;
	internal bool CanReturnToMenu = false;
	internal bool active = false;
	
	private void disableString(string disable) 
	{
		string[] Path = disable.Split('/');
		GameObject current = GameObject.Find(Path[0]);
		for (int i = 1; i < Path.Length; i++) 
			current = current.Find(Path[i]);
		current.SetActive(false);
	}
	
	private void disableComponents() 
	{
		GameObject UI = GameObject.Find("UI");
		GameObject cam = GameObject.Find("TPC").Find("Cam");
		UI.GetComponent<Omni>().enabled = false;
		UI.GetComponent<Cycles>().enabled = false;
		UI.GetComponent<AestheticUI>().enabled = false;
		UI.GetComponent<WorldState>().enabled = false;
		UI.GetComponent<CanvasScaler>().enabled = false;
		cam.GetComponent<Bloom>().enabled = false;
		cam.GetComponent<AmplifyColorEffect>().enabled = false;
		cam.GetComponent<CameraShaker>().enabled = false;
		cam.GetComponent<GlitchEffect>().enabled = false;
		cam.GetComponent<PerLayerCulling>().enabled = false;
		cam.GetComponent<Animator>().enabled = false;
		cam.GetComponent<RaymarchedClouds>().enabled = false;
	}
	
	public IEnumerator RunFinished(float localTime) 
	{
		CreateUI();
		yield return new WaitForSeconds(5);
		Plugin.Log.LogDebug("Disabling unnecessary objects");
		string[] disable = [
			"Saves", "Director", "World", "V-06", "S-105.1", "LockOn", "UI/LogDialogue", "UI/ui", "UI/Fader", "UI/Ripple",
			"UI/Image", "UI/Wheel Standard Dialogue UI", "Timer", "targetLookAt", "CameraMan", "TPC/Cam/CloudLayer0", "TPC/Cam/CloudLayer1", "TPC/Cam/CloudLayer2"
		];
		foreach (string str in disable)
			disableString(str);
		Plugin.Log.LogDebug("Disabling unnecessary components");
		disableComponents();
		Plugin.Log.LogDebug("Enabling Placement UI");
		Placements.SetActive(true);
		Camera camera = GameObject.Find("TPC").Find("Cam").GetComponent<Camera>();
		camera.backgroundColor = new(0, 0, 0, 1);
		camera.clearFlags = CameraClearFlags.SolidColor;
		Placement placement = new()
		{
			name = Plugin.comms.username.Value,
			time = localTime
		};
		updater.PlayerFinished(placement);
		active = true;
	}
	
	private void CreateUI() 
	{
		GameObject UIElement = new("Placements");
		Placements = UIElement;
		RectTransform transform = UIElement.AddComponent<RectTransform>();
		transform.anchoredPosition = new(0, 0);
		transform.anchorMax = new(0.5f, 0.5f);
		transform.anchorMin = new(0.5f, 0.5f);
		transform.sizeDelta = new(1150, 600);
		transform.localScale = new(1, 1, 1);
		UIElement.SetActive(false);
		Canvas canvas = UIElement.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.worldCamera = Camera.current;
		canvas.planeDistance = 1;
		UIElement.AddComponent<GraphicRaycaster>();
		Image image = UIElement.AddComponent<Image>();
		image.color = new(0, 0, 0, 1);
		GameObject PlacementContainer = UIElement.AddObject("PlacementContainer");
		RectTransform PlacementCTr = PlacementContainer.AddComponent<RectTransform>();
		PlacementCTr.sizeDelta = new(1000, 500);
		PlacementCTr.anchorMin = new(0.5f, 0.5f);
		PlacementCTr.anchorMax = new(0.5f, 0.5f);
		PlacementCTr.anchoredPosition = new(0, 20);
		ScrollRect scr = PlacementContainer.AddComponent<ScrollRect>();
		GameObject Viewport = PlacementContainer.AddObject("Viewport");
		RectTransform rect = Viewport.AddComponent<RectTransform>();
		rect.sizeDelta = new(1000, 500);
		rect.anchorMin = new(0.5f, 0.5f);
		rect.anchorMax = new(0.5f, 0.5f);
		scr.viewport = rect;
		Image img = Viewport.AddComponent<Image>();
		img.sprite = Find<Sprite>((v) => v.name == "Transparent");
		Viewport.AddComponent<RectMask2D>();
		GameObject Content = Viewport.AddObject("Content");
		VerticalLayoutGroup group = Content.AddComponent<VerticalLayoutGroup>();
		group.childAlignment = TextAnchor.UpperCenter;
		group.childForceExpandHeight = false;
		group.childForceExpandWidth = false;
		group.childControlHeight = true;
		group.childControlWidth = true;
		ContentSizeFitter fitter = Content.AddComponent<ContentSizeFitter>();
		fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
		scr.content = Content.GetComponent<RectTransform>();
		scr.horizontal = false;
		scr.scrollSensitivity = 50;
		updater = Viewport.AddComponent<PlacementUpdater>();
		Material wordsArtificer = Find<Material>((v) => v.name == "wordsArtificer");
		GameObject Return = UIElement.AddObject("Return");
		RectTransform RTTransform = Return.AddComponent<RectTransform>();
		RTTransform.sizeDelta = new(600, 100);
		RTTransform.anchoredPosition = new(0, -397.6f);
		Text RTText = Return.AddComponent<Text>();
		RTText.font = Find<Font>((v) => v.name == "Arial");
		RTText.material = wordsArtificer;
		RTText.alignment = TextAnchor.MiddleCenter;
		RTText.fontSize = 40;
		RTText.text = "Waiting for other players to finish.";
	}
	
	static T Find<T>(Func<T, bool> predicate)
	{
		foreach (T find in Resources.FindObjectsOfTypeAll(typeof(T)).Cast<T>())
		{
			if (predicate(find)) return find;
		}
		return default;
	}
	
	internal static GameObject CreatePlacementObject(string name, float time) 
	{
		Material wordsArtificer = Find<Material>((v) => v.name == "wordsArtificer");
		GameObject Placement = new("Placement");
		Placement.AddComponent<RectTransform>();
		LayoutElement elem = Placement.AddComponent<LayoutElement>();
		elem.preferredHeight = 50;
		elem.preferredWidth = 1000;
		GameObject Place = Placement.AddObject("Place");
		RectTransform placeTrans = Place.AddComponent<RectTransform>();
		placeTrans.anchoredPosition = new(-400, 0);
		placeTrans.sizeDelta = new(200, 50);
		Text placeTxt = Place.AddComponent<Text>();
		placeTxt.resizeTextForBestFit = true;
		placeTxt.alignment = TextAnchor.MiddleCenter;
		Font ArialFont = Find<Font>((v) => v.name == "Arial");
		placeTxt.font = ArialFont;
		placeTxt.material = wordsArtificer;
		GameObject b1 = CreateBorder();
		b1.SetParent(Placement, false);
		b1.GetComponent<RectTransform>().anchoredPosition = new(-297.5f, 0);
		GameObject Name = Placement.AddObject("Name");
		RectTransform nameTrans = Name.AddComponent<RectTransform>();
		nameTrans.sizeDelta = new(500, 50);
		nameTrans.anchoredPosition = new(-40, 0);
		Text nameText = Name.AddComponent<Text>();
		nameText.font = ArialFont;
		nameText.resizeTextForBestFit = true;
		nameText.text = name;
		nameText.alignment = TextAnchor.MiddleCenter;
		nameText.material = wordsArtificer;
		GameObject b2 = CreateBorder();
		b2.SetParent(Placement, false);
		b2.GetComponent<RectTransform>().anchoredPosition = new(217.5f, 0);
		GameObject Time = Placement.AddObject("Time");
		RectTransform timeTrans = Time.AddComponent<RectTransform>();
		timeTrans.sizeDelta = new(280, 50);
		timeTrans.anchoredPosition = new(360, 0);
		Text timeText = Time.AddComponent<Text>();
		timeText.font = ArialFont;
		timeText.resizeTextForBestFit = true;
		timeText.text = FormatTime(time);
		timeText.alignment = TextAnchor.MiddleCenter;
		timeText.material = wordsArtificer;
		return Placement;
	}
	
	void Update() 
	{
		if (active) 
		{
			Cursor.lockState = CursorLockMode.None;
			CustomCursor.visible = true;
		}
		if (CanReturnToMenu) 
		{
			Placements.Find("Return").GetComponent<Text>().text = "Press G to return to menu.";
			if (UnityInput.Current.GetKeyDown(KeyCode.G)) 
			{
				SceneManager.LoadScene("Menu");
			}
		}
	}
	
	internal static GameObject CreateBorder() 
	{
		Material wordsArtificer = Find<Material>((v) => v.name == "wordsArtificer");
		GameObject Border = new("Border");
		RectTransform bRect = Border.AddComponent<RectTransform>();
		bRect.anchoredPosition = new(-295, 0);
		bRect.sizeDelta = new(5, 50);
		Border.AddComponent<Image>().material = wordsArtificer;
		return Border;
	}

	internal static string FormatTime(float time)
	{
		int minutes = Mathf.RoundToInt(time / 60000);
		int seconds = Mathf.RoundToInt(time % 60000 / 1000);
		int milliseconds = Mathf.RoundToInt(time % 1000);
		return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
	}
	
	internal static string GetOrdinalSuffix(int number)
	{
		if (number % 100 >= 11 && number % 100 <= 13)
		{
			return number + "th";
		}

		return (number % 10) switch
		{
			1 => number + "st",
			2 => number + "nd",
			3 => number + "rd",
			_ => number + "th",
		};
	}
}
