using System;
using GooglePlayGames;
using UnityEngine;
using UnityEngine.AI;

public class PomoAnimationController : MonoBehaviour
{
    public Animator animator;
    public Camera mainCamera;
    public RectTransform canvas;

    private NavMeshAgent agent;
    private Vector3 lastPosition;
    
    public event Action OnActionStart;
    public event Action OnActionEnd;

    private bool isActionPlaying = false;
    private float actionTimer = 0f;
    private float actionDuration = 0f;

    private float wateringTimer = 0f;
    private float nextWateringTime = 0f;
    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        lastPosition = transform.position;
        
        ScheduleNextWatering();
    }

    private void Update()
    {
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
            Vector3 movement = transform.position - lastPosition;
            bool isMoving = movement.magnitude > 0.01f;
            animator.SetBool(IsMoving, isMoving);

            if (isMoving)
            {
                Vector3 cameraForward = mainCamera.transform.forward;
                Vector3 cameraRight = mainCamera.transform.right;

                float moveX = Vector3.Dot(movement.normalized, cameraRight.normalized);
                float moveY = Vector3.Dot(movement.normalized, cameraForward.normalized);

                animator.SetFloat(MoveX, moveX);
                animator.SetFloat(MoveY, moveY);
            }

            lastPosition = transform.position;
            
            wateringTimer += Time.deltaTime;
            if (wateringTimer >= nextWateringTime)
            {
                PlayWatering();
                ScheduleNextWatering();
            }
        }

        canvas.rotation = mainCamera.transform.rotation;
    }

    private void OnMouseDown()
    {
        PlayGreeting();
        
        PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_2);
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
        nextWateringTime = UnityEngine.Random.Range(10f, 30f);
    }
}
