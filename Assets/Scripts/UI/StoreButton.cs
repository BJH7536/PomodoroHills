using System;
using UnityEngine;
using UnityEngine.UI;

public class StoreButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Awake()
    {
        button.onClick.RemoveAllAndAddListener(ShowStorePopup);
    }

    public void ShowStorePopup()
    {
        PopupManager.Instance.ShowPopup<StorePopup>();
    }
}
