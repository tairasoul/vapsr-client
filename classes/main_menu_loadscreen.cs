using BepInEx;
using MainMenuSettings.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace VapSRClient;

public class MainMenuLoadscreen : MonoBehaviour
{
	private GameObject LoadscreenObject;
	private Text LoadscreenText;
	public bool canCancel = false;
	void Awake() 
	{
		LoadscreenObject = GameObject.Find("Canvas").Find("LoadingScreen");
		Text text = LoadscreenObject.Find("Text (Legacy)").GetComponent<Text>();
		RectTransform transform = text.GetComponent<RectTransform>();
		transform.sizeDelta = new(400, 120);
		LoadscreenText = text;
	}
	
	public void Update() 
	{
		if (canCancel) 
		{
			if (UnityInput.Current.GetKeyDown(KeyCode.Escape)) 
			{
        MatchmakingCancelled();
      }
		}
	}

	private void MatchmakingCancelled() {
		Plugin.comms.CancelMatchmaking();
		canCancel = false;
    LoadscreenObject.SetActive(false);
    Settings.Page.SetActive(true);
  }
	
	public void FindingMatch() 
	{
		canCancel = true;
		LoadscreenObject.SetActive(true);
		LoadscreenText.text = "Trying to find match.\nPress escape to cancel.";
	}
	
	public void SetText(string text) {
		LoadscreenObject.SetActive(true);
		LoadscreenText.text = text;
	}
}