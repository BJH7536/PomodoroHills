using System;
using DG.Tweening;
using UnityEngine;

public class ui_popper : MonoBehaviour
{
    private void OnEnable()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
    }
}
