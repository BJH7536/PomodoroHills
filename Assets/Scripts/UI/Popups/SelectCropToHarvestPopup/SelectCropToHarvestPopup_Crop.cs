using System;
using PomodoroHills;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectCropToHarvestPopup_Crop : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Button button;
    
    public void SetUp(int id, UnityAction action)
    {
        CropData cropData = DataBaseManager.Instance.CropDatabase.GetCropById(id);
        ItemTableElement itemData = DataBaseManager.Instance.ItemTable.GetItemInformById(id);

        image.sprite = itemData.image;
        nameText.text = $"{cropData.Name}";
        TimeSpan time = TimeSpan.FromSeconds(cropData.HarvestSeconds);
        timeText.text = $"{time.Minutes:D2}:{time.Seconds:D2}";
        
        button.onClick.RemoveAllAndAddListener(()=>
        {
            PopupManager.Instance.HidePopup();
            action.Invoke();
        });
    }
    
    
}
