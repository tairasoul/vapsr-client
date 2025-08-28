using System.Collections.Generic;
using System.Linq;
using MainMenuSettings.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace VapSRClient;

public class PlacementUpdater : MonoBehaviour
{
	internal Placement[] placements = [];
	internal GameObject PlacementContent;
	internal PlacementChildUpdater[] updaters = [];
	internal static PlacementUpdater instance;
	internal Queue<Placement> queue = new();
	
	void Awake() 
	{
		instance = this;
		PlacementContent = gameObject.Find("Content");
	}
	
	public void PlayerFinished(Placement placement) 
	{
		queue.Enqueue(placement);
	}
	
	void Update() 
	{
		if (queue.Count != 0) 
		{
			Placement placement = queue.Dequeue();
			Plugin.Log.LogDebug($"Player {placement.name} finished with time {PlacementScreen.FormatTime(placement.time)}");
			placements = [ .. placements, placement ];
			Plugin.Log.LogDebug("Creating placement object for new placement");
			GameObject PlacementObject = PlacementScreen.CreatePlacementObject(placement.name, placement.time);
			PlacementObject.SetParent(PlacementContent, false);
			Plugin.Log.LogDebug("Creating placement updater");
			PlacementChildUpdater updater = PlacementObject.AddComponent<PlacementChildUpdater>();
			updater.Updater = this;
			Plugin.Log.LogDebug("Adding new updater");
			updaters = [ .. updaters, updater ];
			placements = [.. placements.OrderBy((v) => v.time)];
		}
	}
}

public class PlacementChildUpdater : MonoBehaviour 
{
	internal PlacementUpdater Updater;
	
	void Update() 
	{
		GameObject Name = gameObject.Find("Name");
		GameObject Time = gameObject.Find("Time");
		GameObject Place = gameObject.Find("Place");
		Text PlayerNameText = Name.GetComponent<Text>();
		Text TimeText = Time.GetComponent<Text>();
		Text PlacementText = Place.GetComponent<Text>();
		int siblingIndex = gameObject.transform.GetSiblingIndex();
		Placement ourPlacement = Updater.placements[siblingIndex];
		PlayerNameText.text = ourPlacement.name;
		TimeText.text = PlacementScreen.FormatTime(ourPlacement.time);
		PlacementText.text = PlacementScreen.GetOrdinalSuffix(siblingIndex + 1);
	}
}