using System;
using UnityEngine;
using UnityEngine.AI;

public class PomoAnimationController : MonoBehaviour
{
    public Animator animator;
    public Camera mainCamera;
    public RectTransform canvas;

    private NavMeshAgent agent;
    private Vector3 lastPosition;

    // 이벤트 델리게이트 정의
    public event Action OnActionStart;
    public event Action OnActionEnd;

    private bool isActionPlaying = false;
    private float actionTimer = 0f;
    private float actionDuration = 0f;

    private float wateringTimer = 0f;
    private float nextWateringTime = 0f;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;

        // 무작위 Watering 시간 설정
        ScheduleNextWatering();
    }

    private void Update()
    {
        // 특수 애니메이션 재생 중인지 확인
        if (isActionPlaying)
        {
            actionTimer += Time.deltaTime;
            if (actionTimer >= actionDuration)
            {
                isActionPlaying = false;
                OnActionEnd?.Invoke();
            }
        }
        else
        {
            // 이동 방향 및 애니메이션 파라미터 설정
            Vector3 movement = transform.position - lastPosition;
            bool isMoving = movement.magnitude > 0.01f;
            animator.SetBool("IsMoving", isMoving);

            if (isMoving)
            {
                Vector3 cameraForward = mainCamera.transform.forward;
                Vector3 cameraRight = mainCamera.transform.right;

                float moveX = Vector3.Dot(movement.normalized, cameraRight.normalized);
                float moveY = Vector3.Dot(movement.normalized, cameraForward.normalized);

                animator.SetFloat("MoveX", moveX);
                animator.SetFloat("MoveY", moveY);
            }

            lastPosition = transform.position;

            // Watering 애니메이션 재생 체크
            wateringTimer += Time.deltaTime;
            if (wateringTimer >= nextWateringTime)
            {
                PlayWatering();
                ScheduleNextWatering();
            }
        }

        // 캔버스가 카메라를 향하도록 회전
        canvas.LookAt(mainCamera.transform);
    }

    private void OnMouseDown()
    {
        // Greeting 애니메이션 재생
        PlayGreeting();
    }

    public void PlayGreeting()
    {
        if (isActionPlaying) return;

        animator.SetTrigger("Greet");
        StartAction(animator.GetCurrentAnimatorStateInfo(0).length);
    }

    public void PlayWatering()
    {
        if (isActionPlaying) return;

        animator.SetTrigger("Water");
        StartAction(animator.GetCurrentAnimatorStateInfo(0).length);
    }

    private void StartAction(float duration)
    {
        isActionPlaying = true;
        actionTimer = 0f;
        actionDuration = duration;
        OnActionStart?.Invoke();
    }

    private void ScheduleNextWatering()
    {
        wateringTimer = 0f;
        nextWateringTime = UnityEngine.Random.Range(10f, 30f); // 최소 10초, 최대 30초
    }
}
