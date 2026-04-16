using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private List<string> pickedUpItems = new List<string>();
    private List<string> pickedUpItemsKeys = new List<string>();

    public bool isDev = false;

    private void Awake()
    {
        GameEventsManager.instance.itemEvents.onItemGrabbed += OnItemGrabbed;

        // Load the picked up items
        if (!isDev)
            LoadPickedUpItems();
    }

    private void OnDestroy()
    {
        GameEventsManager.instance.itemEvents.onItemGrabbed -= OnItemGrabbed;
    }

    private void OnItemGrabbed(ObjectInfo item)
    {
        // get the items id 
        string itemId = item.Data.name;

        // check if the item has already been picked up
        if (!pickedUpItems.Contains(itemId))
        {
            // Trigger the panel and audio
            GameEventsManager.instance.itemEvents.ItemGrabbedPressed(item);
            GameEventsManager.instance.audioEvents.Play(item.Data.AudioClip);

            // Add the item to the list
            pickedUpItems.Add(itemId);

            // Save to player prefs
            SavePickedUpItems(itemId);
        }
    }

    private void SavePickedUpItems(string itemId)
    {
        string pickedUpItemString = "PickedUpItem-" + itemId;

        if (PlayerPrefs.HasKey("PickedUpItems"))
        {
            // Get the current list of picked up items
            string pickedUpItemsString = PlayerPrefs.GetString("PickedUpItems", "");

            // Add the new item to the list
            pickedUpItemsString += itemId + ",";

            // Save the new list to player prefs
            PlayerPrefs.SetString("PickedUpItems", pickedUpItemsString);
        }
        else
        {
            // Save the new item to player prefs
            PlayerPrefs.SetString("PickedUpItems", itemId + ",");
        }
        PlayerPrefs.Save();

        //Also add the path for api when its implemented to Items
        if (DataManager._instance != null)
            DataManager._instance.SaveData(pickedUpItemString, pickedUpItemString, "item");

    }

    private void LoadPickedUpItems()
    {
        if (PlayerPrefs.HasKey("PickedUpItems"))
        {
            // Get the current list of picked up items
            string pickedUpItemsString = PlayerPrefs.GetString("PickedUpItems", "");

            // Add the items to the list
            pickedUpItems = new List<string>(pickedUpItemsString.Split(','));
        }
    }
}
