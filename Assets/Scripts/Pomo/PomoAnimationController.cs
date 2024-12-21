using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using LeTai.TrueShadow;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PomoAnimationController : MonoBehaviour
{
    public Animator animator;
    public Camera mainCamera;
    public RectTransform canvasRect;
    public RectTransform canvasOnHold;

    public Image pomo;
    public Material pomoOnHeld;
    public ShadowMaterial pomoShadowOnHeld;
    
    private Vector3 originCanvasPos;
    private Vector2 originCanvasSizeDelta;

    public Ease inflate;
    public Ease deflate;
    
    private bool isActionPlaying = false;           // 현재 특정 행동이 재생 중인지 나타내는 플래그
    private float actionTimer = 0f;                 // 현재 실행 중인 액션의 경과 시간
    private float actionDuration = 0f;              // 액션의 지속 시간
    
    public readonly int MoveX = Animator.StringToHash("MoveX");
    public readonly int MoveY = Animator.StringToHash("MoveY");
    public readonly int IsMoving = Animator.StringToHash("IsMoving");
    public static readonly int Greet = Animator.StringToHash("Greet");
    public static readonly int Water = Animator.StringToHash("Water");

    private void Start()
    {
        originCanvasPos = canvasRect.position;
        originCanvasSizeDelta = canvasRect.sizeDelta;
    }

    private void LateUpdate()
    {
        // 뽀모 Sprite를 렌더링하는 캔버스 방향 정렬
        canvasRect.rotation = mainCamera.transform.rotation;
    }

    public void PlayGreeting()
    {
        animator.SetTrigger(Greet);
    }

    public void PlayWatering()
    {
        animator.SetTrigger(Water);
    }

    public void Inflate()
    {
        canvasRect.DOSizeDelta(canvasOnHold.sizeDelta, 0.2f).SetEase(inflate);
        canvasRect.DOMoveY(canvasOnHold.position.y, 0.2f).SetEase(inflate);

        pomo.material = pomoOnHeld;
        pomoShadowOnHeld.enabled = true;
    }

    public void Deflate()
    {
        canvasRect.DOSizeDelta(originCanvasSizeDelta, 0.2f).SetEase(deflate);
        canvasRect.DOMoveY(originCanvasPos.y, 0.2f).SetEase(deflate);
        
        pomo.material = null;
        pomoShadowOnHeld.enabled = false;
    }
}
