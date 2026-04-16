using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    //public Scrollbar volumeSlider;
    public TMPro.TMP_Dropdown turnDropdown;

    [SerializeField] private Slider walkSpeedSlider;
    [SerializeField] private TextMeshProUGUI walkSpeedText;

    private void Start()
    {
        //volumeSlider.onValueChanged.AddListener(SetGlobalVolume);
        turnDropdown.onValueChanged.AddListener(SetTurnPlayerPref);

        walkSpeedSlider.onValueChanged.AddListener(SetWalkSpeed);

        if (PlayerPrefs.HasKey("turn"))
            turnDropdown.SetValueWithoutNotify(PlayerPrefs.GetInt("turn"));

        if (PlayerPrefs.HasKey("walkSpeed"))
        {
            walkSpeedSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("walkSpeed"));
            walkSpeedText.text = PlayerPrefs.GetFloat("walkSpeed").ToString("0.00");
        }
    }
    
    public void SetGlobalVolume(float value)
    {
        AudioListener.volume = value;
    }

    public void SetTurnPlayerPref(int value)
    {
        GameEventsManager.instance.movementEvents.SetRotationType(value);
    }

    public void SetWalkSpeed(float value)
    {
        GameEventsManager.instance.movementEvents.SetWalkSpeed(value);
        walkSpeedText.text = value.ToString("0.00");
    }
}
