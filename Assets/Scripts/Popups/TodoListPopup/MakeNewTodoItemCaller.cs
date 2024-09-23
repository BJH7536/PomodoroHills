using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

public class MakeNewTodoItemCaller : MonoBehaviour
{
    [SerializeField] private RectTransform myRectTransform;
    [SerializeField] private GameObject MakeNewTodoItem;

    private RectTransform targetRectT;
    private Vector2 oriAMin;
    private Vector2 oriAMax;
    private Vector2 oriAP;
    
    private void Awake()
    {
        targetRectT = MakeNewTodoItem.GetComponent<RectTransform>();
        
        oriAMin = targetRectT.anchorMin;
        oriAMax = targetRectT.anchorMax;
        oriAP = targetRectT.anchoredPosition;
    }

    public void OpenMakeNewTodoItem()
    {
        MakeNewTodoItem.SetActive(true);
        
        targetRectT.anchorMin = Vector2.right;
        targetRectT.anchorMax = Vector2.right;
        targetRectT.anchoredPosition = myRectTransform.anchoredPosition;

        targetRectT.GetComponent<CanvasGroup>().alpha = 0;
        targetRectT.GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetEase(Ease.InExpo).ToUniTask().Forget();
        
        targetRectT.DOAnchorMin(oriAMin, 0.5f).ToUniTask().Forget();
        targetRectT.DOAnchorMax(oriAMax, 0.5f).ToUniTask().Forget();
        targetRectT.DOAnchorPos(oriAP, 0.5f, true).ToUniTask().Forget();

    }
    
}
