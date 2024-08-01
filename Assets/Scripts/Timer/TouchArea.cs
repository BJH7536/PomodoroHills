using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchArea : MonoBehaviour, IPointerDownHandler
{
    public GameObject inputPanel;

    public void OnPointerDown(PointerEventData eventData)
    {
        inputPanel.SetActive(true);     //입력패널 활성화
    }


}
