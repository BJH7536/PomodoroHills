using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PomoAnimationController : MonoBehaviour
{
    public Animator animator;
    public Camera mainCamera;
    public RectTransform canvas;

    private bool isActionPlaying = false;           // 현재 특정 행동이 재생 중인지 나타내는 플래그
    private float actionTimer = 0f;                 // 현재 실행 중인 액션의 경과 시간
    private float actionDuration = 0f;              // 액션의 지속 시간
    
    public readonly int MoveX = Animator.StringToHash("MoveX");
    public readonly int MoveY = Animator.StringToHash("MoveY");
    public readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void LateUpdate()
    {
        // 뽀모 Sprite를 렌더링하는 캔버스 방향 정렬
        canvas.rotation = mainCamera.transform.rotation;
    }

    public void PlayGreeting()
    {

        animator.SetTrigger("Greet");
    }

    public void PlayWatering()
    {

        animator.SetTrigger("Water");
    }
}
