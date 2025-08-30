using System.Linq;
using System.Threading.Tasks;
using BepInEx;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VapSRClient.Extensions;

namespace VapSRClient;

public class EndScreen : MonoBehaviour
{
	GameObject UIElement;
	Text uiText;
	internal static bool runFinished = false;
	internal static float localTime = 0f;
	private bool uiCreated = false;
	private void CreateUI(float time) 
	{
		UIElement = new("EndScreenElement");
		RectTransform transform = UIElement.AddComponent<RectTransform>();
		transform.anchoredPosition = new(0, 0);
		transform.anchorMax = new(0.5f, 0.5f);
		transform.anchorMin = new(0.5f, 0.5f);
		transform.sizeDelta = new(1150, 600);
		transform.localScale = new(1, 1, 1);
		Canvas canvas = UIElement.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.worldCamera = Camera.current;
		Image image = UIElement.AddComponent<Image>();
		image.color = new(0, 0, 0, 1);
		GameObject textElem = new("Text");
		RectTransform textT = textElem.AddComponent<RectTransform>();
		textT.transform.parent = UIElement.transform;
		textT.anchoredPosition = new(0, 0);
		textT.anchorMin = new(0.5f, 0.5f);
		textT.anchorMax = new(0.5f, 0.5f);
		textT.sizeDelta = new(500, 200);
		Text text = textElem.AddComponent<Text>();
		text.fontSize = 29;
		text.alignment = TextAnchor.MiddleCenter;
		Font font = GetFont("orbitron-medium");
		text.font = font;
		Plugin.Log.LogInfo($"match found {Client.Client.MatchFound}");
		if (Client.Client.MatchFound)
			text.text = $"Waiting for opponent to finish.\nYou finished with a time of {FormatTime(time)}";
		else
			text.text = $"You finished with a time of {FormatTime(time)}\n\nPress G to return to main menu.";
		uiText = text;
		uiCreated = true;
	}
	
	public void Awake() 
	{
		localTime = 0f;
		runFinished = false;
	}
	
	public async void OpponentFinished(string opponent, float opponentTime, bool won) 
	{
		
		if (!uiCreated) 
		{
			while (true) 
			{
				await Task.Delay(50);
				if (uiCreated)
					break;
			}
		}
		if (won) 
		{
			uiText.text = $"You won against {opponent} with a time of {FormatTime(localTime)}.\n{opponent}'s time: {FormatTime(opponentTime)}\n\nPress G to return to main menu.\n\nDebug: opponent-{opponentTime} you-{localTime}";
		}
		else 
		{
			uiText.text = $"You lost against {opponent}, their time was {FormatTime(opponentTime)}.\nYour time: {FormatTime(localTime)}\n\nPress G to return to main menu.\n\nDebug: opponent-{opponentTime} you-{localTime}";
		}
		Task.Run(RunFinishedDelay);
	}

	private string FormatTime(float time)
	{
		int minutes = Mathf.FloorToInt(time / 60000);
		int seconds = Mathf.FloorToInt(time % 60000 / 1000);
		int milliseconds = Mathf.FloorToInt(time % 1000);
		return $"{minutes:00}:{seconds:00}.{milliseconds:000}";
	}
	
	public void RunFinished(float time) 
	{
		localTime = time;
		Task.Run(UICreationDelay);
	}
	
	private async Task UICreationDelay() 
	{
		await Task.Delay(5000);
		CreateUI(localTime);
		if (!Client.Client.MatchFound)
			runFinished = true;
		Time.timeScale = 0;
		vShooterMeleeInput input = GameObject.Find("S-105.1").GetComponent<vShooterMeleeInput>();
		input.enabled = false;
	}
	
	private async Task RunFinishedDelay() 
	{
		runFinished = true;
		await Task.Delay(2000);
		SpeedrunningUtils.OBS.ObsWebsocket.StopRecording();
	}
	
	private void Update() 
	{
		if (UnityInput.Current.GetKeyDown(KeyCode.G) && runFinished) 
		{
			KeyPressed();
		}
	}
	
	private void KeyPressed() 
	{
		SceneManager.LoadScene("Menu");
	}
	
	internal static Font GetFont(string name)
	{
		Object[] fonts = Resources.FindObjectsOfTypeAll(typeof(Font));
		foreach (Font font in fonts.Cast<Font>())
		{
			if (font.name == name) return font;
		}
		return null;
	}
}