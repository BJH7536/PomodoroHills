using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class TileController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TMP_Text DebugText;
    [SerializeField] public CinemachineVirtualCamera camera;
    [SerializeField] private CameraManager CameraManager;
    [SerializeField] private MMF_Player MmfPlayer;

    private void Awake()
    {
        MmfPlayer = GetComponent<MMF_Player>();
    }

    [ContextMenu("VisibleChild")]
    public void VisibleChild()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(true);
    }
    
    [ContextMenu("InvisibleChild")]
    public void InvisibleChild()
    {
        gameObject.transform.GetChild(0).gameObject.SetActive(false);
    }

    [ContextMenu("DisableCamera")]
    public void DisableCamera()
    {
        camera.gameObject.SetActive(false);
    }
    
    [ContextMenu("FindMyCamera")]
    public void FindMyCamera()
    {
        camera = GameObject.Find($"{gameObject.name}_Camera").GetComponent<CinemachineVirtualCamera>();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        DebugText.text = gameObject.name;

        CameraManager.CameraTransition(camera);

        // Sequence seq = DOTween.Sequence();
        // seq.Append(transform.DOScale(originScale * 1.2f, 0.25f).SetEase(Ease.InOutBounce));
        // seq.Append(transform.DOScale(originScale, 0.25f).SetEase(Ease.InOutBounce));
        
        MmfPlayer.PlayFeedbacks();
    }
}
