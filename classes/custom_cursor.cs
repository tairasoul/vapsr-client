using System;
using System.Linq;
using BepInEx;
using MainMenuSettings.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VapSRClient;

public class CustomCursor : MonoBehaviour
{
	GameObject cursorObj;
	public static bool visible;
	
	static T Find<T>(Func<T, bool> predicate)
	{
		foreach (T find in Resources.FindObjectsOfTypeAll(typeof(T)).Cast<T>())
		{
			if (predicate(find)) return find;
		}
		return default;
	}
	
	public void Start() 
	{
		Material wordsArtificer = Find<Material>((v) => v.name == "wordsArtificer");
		GameObject thisObj = gameObject;
		Canvas canvas = thisObj.AddComponent<Canvas>();
		canvas.sortingOrder = 999999;
		canvas.renderMode = RenderMode.ScreenSpaceCamera;
		canvas.planeDistance = 1;
		RectTransform rt = thisObj.GetComponent<RectTransform>() ?? thisObj.AddComponent<RectTransform>();
		rt.anchorMax = new(0.5f, 0.5f);
		rt.anchorMin = new(0.5f, 0.5f);
		rt.anchoredPosition = new(0, 0);
		cursorObj = thisObj.AddObject("Cursor");
		GameObject.DontDestroyOnLoad(cursorObj);
		Image img = cursorObj.AddComponent<Image>();
		img.sprite = CreateSpriteFromTexture(Plugin.CursorImage);
		img.material = wordsArtificer;
	}
	private Sprite CreateSpriteFromTexture(Texture2D texture)
	{
		Rect rect = new(0, 0, texture.width, texture.height);
		Vector2 pivot = new(0.5f, 0.5f);
		return Sprite.Create(texture, rect, pivot, 100.0f);
	}
	
	void Update() 
	{
		Material wordsArtificer = Find<Material>((v) => v.name == "wordsArtificer");
		cursorObj.GetComponent<Image>().material = wordsArtificer;
		cursorObj.SetActive(visible);
		if (SceneManager.GetActiveScene().buildIndex == 2) 
		{
			GameObject UICamera = GameObject.Find("MAINMENU").Find("UICamera");
			visible = UICamera.activeSelf;
		}
		if (visible)
			Cursor.visible = false;
			
		cursorObj.SetActive(visible);
	}
	
	void LateUpdate() 
	{
		RectTransform rect = cursorObj.GetComponent<RectTransform>();
		rect.localRotation = new(0, 0, 0.866f, -0.5f);
		rect.sizeDelta = new(45, 45);

		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			(RectTransform)transform,
			UnityInput.Current.mousePosition,
			null,
			out Vector2 mousePos
		);
		Vector2 offset = new(11, -24);

		rect.anchoredPosition = mousePos + offset;
	}
}