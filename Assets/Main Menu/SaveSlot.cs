using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlot : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private string profileId = "";

    [Header("Content")]
    [SerializeField] private GameObject noDataContent;
    [SerializeField] private GameObject hasDataContent;
    [SerializeField] private TextMeshProUGUI percentageCompleteText;
    [SerializeField] private TextMeshProUGUI deathCountText;

    [Header("Clear Data button")]
    [SerializeField] private Button clearButton;

    public bool hasData { get; private set; } = false;

    private Button saveSlotButton;


    private void Awake()
    {
        saveSlotButton = this.GetComponent<Button>();
        if (profileId.Equals(""))
        {
            Debug.LogWarning("SaveSlot Doesn't have a profileID " + profileId);
        }
    }

    public void SetData(GameData data)
    {
        // There's no data for this profileID
        if(data == null)
        {
            //Debug.Log(profileId + " " + "No data");
            hasData = false;
            noDataContent.SetActive(true); 
            hasDataContent.SetActive(false);
            clearButton.gameObject.SetActive(false);
        }
        // There's data for this profileID
        else
        {
            //Debug.Log(profileId + " " + "Has data");
            hasData = true;
            noDataContent.SetActive(false);
            hasDataContent.SetActive(true);
            clearButton.gameObject.SetActive(true);
            deathCountText.text = "This is a test don't worry about it";
        }
    } 

    public string GetProfileId()
    {
        return profileId;
    }

    public void SetInteractable(bool interactable)
    {
        saveSlotButton.interactable = interactable;
        clearButton.interactable = interactable;
    }
}
