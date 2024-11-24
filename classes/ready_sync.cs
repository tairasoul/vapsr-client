using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Invector.vCharacterController;
using UnityEngine;
using UnityEngine.UI;
using VapSRClient.Reflection;

namespace VapSRClient;

public class ReadySync : MonoBehaviour
{
	GameObject UIElement;
	internal static bool StartBypass = false;
	private IEnumerator CreateUI() 
	{
		while (true) 
		{
			if (GameObject.Find("UI") != null)
				break;
			yield return new WaitForEndOfFrame();
		}
		EventLog.CreateContainer();
		UIElement = new("ReadySyncElement");
		RectTransform transform = UIElement.AddComponent<RectTransform>();
		UIElement.transform.parent = GameObject.Find("UI").transform;
		transform.anchoredPosition = new(0, 0);
		transform.anchorMax = new(0.5f, 0.5f);
		transform.anchorMin = new(0.5f, 0.5f);
		transform.sizeDelta = new(1150, 600);
		transform.localScale = new(1, 1, 1);
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
		text.text = "Waiting for your opponent to load.";
	}
	
	private void Start() 
	{
		Plugin.timer.ResetTime();
		Time.timeScale = 0;
		StartCoroutine(CreateUI());
		vShooterMeleeInput input = GameObject.Find("S-105.1").GetComponent<vShooterMeleeInput>();
		input.enabled = false;
		Task.Run(SendReady);
	}
	
	private async void SendReady() 
	{
		await Task.Delay(5000);
		Plugin.comms.Loaded();
		StartBypass = true;
	}
	
	public void RunStarted() 
	{
		ReflectionHelper helper = new(GameObject.FindFirstObjectByType<Omni>());
		StartCoroutine(helper.CallMethod<IEnumerator>("Start", null));
		Time.timeScale = 1;
		vShooterMeleeInput input = GameObject.Find("S-105.1").GetComponent<vShooterMeleeInput>();
		input.enabled = true;
		Destroy(UIElement);
		Destroy(gameObject);
		Destroy(this);
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