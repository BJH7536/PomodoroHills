using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using LeTai.TrueShadow;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public interface ICommand
{
    // ======================================== IMPORTANT ========================================
    // PomoController.ExecuteNextCommand()에서 이 ExecuteAsync 함수를 실행하는데,
    // 이 ExecuteAsync 함수를 실행하는 도중에 예외가 발생하면 PomoController.ExecuteNextCommand() 에서 try-catch로 예외를 잡아 처리한다.
    // 그러니 ExecuteAsync 함수를 구현할 때는 꼭 필요한 경우가 아니라면 이 안에 try-catch 문을 쓰지는 말 것.
    
    // PomoController.ExecuteNextCommand()에서 try-catch로 예외를 잡아야
    // [현재 명령 취소] -> [다음 명령 수행] 으로 자연스럽게 이어질 수 있다.
    // ============================================================================================
    
    UniTask ExecuteAsync(CancellationTokenSource cancellationTokenSource);
    void Cancel();          // 명령이 취소될 때만 Call. Finish보다 먼저 Call.
    void Finish();          // 명령이 다 끝나면 무조건 Call. 취소될 때도 Call.
}

public class MoveCommand : ICommand
{
    private PomoController pomoController;
    private PomoAnimationController animationController;
    private Camera camera;
    private NavMeshAgent agent;
    public Vector3 destination;
    private Vector3 lastPosition;
    
    private bool isMovementFinished = false;

    public MoveCommand(PomoController pomoController, PomoAnimationController animationController, NavMeshAgent agent, Vector3 destination)
    {
        this.pomoController = pomoController;
        this.animationController = animationController;
        this.agent = agent;
        this.destination = destination;

        camera = Camera.main;
        
        lastPosition = pomoController.transform.position;
    }

    public async UniTask ExecuteAsync(CancellationTokenSource cancellationTokenSource)
    {
        agent.SetDestination(destination);

        await UniTask.WhenAll(
            AnimationLoop(cancellationTokenSource),
            CheckFinished(cancellationTokenSource)
        );
    }

    async UniTask AnimationLoop(CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            while (!cancellationTokenSource.IsCancellationRequested && !isMovementFinished && pomoController)
            {
                // 매 프레임마다 직전 프레임과의 비교를 통해 움직이는 애니메이션 연출
                Vector3 movement = pomoController.transform.position - lastPosition;
                bool isMoving = movement.magnitude > 0.01f;
                animationController.animator.SetBool(animationController.IsMoving, isMoving);

                if (isMoving)
                {
                    Vector3 cameraForward = camera.transform.forward;
                    Vector3 cameraRight = camera.transform.right;

                    float moveX = Vector3.Dot(movement.normalized, cameraRight.normalized);
                    float moveY = Vector3.Dot(movement.normalized, cameraForward.normalized);

                    animationController.animator.SetFloat(animationController.MoveX, moveX);
                    animationController.animator.SetFloat(animationController.MoveY, moveY);
                }

                lastPosition = pomoController.transform.position;
            
                await UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    async UniTask CheckFinished(CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            while (true)
            {
                // cancellationTokenSource.Token.ThrowIfCancellationRequested();
            
                // 에이전트의 존재 여부 및 NavMesh 여부 체크
                if (agent == null || !agent.isOnNavMesh || !agent.isActiveAndEnabled)
                {
                    isMovementFinished = true;
                    return;
                }
                
                // 이동이 끝났는지 확인
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        isMovementFinished = true;
                        return;
                    }
                }
    
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    public void Cancel()
    {
        agent.ResetPath();
        isMovementFinished = true;
        animationController.animator.SetBool(animationController.IsMoving, false);
    }
    
    public void Finish()
    {
        agent.ResetPath();
        isMovementFinished = true;
        animationController.animator.SetBool(animationController.IsMoving, false);
    }
}

public class DialogueCommand : ICommand
{
    private PomoController pomoController;
    private string dialogueText;
    private float characterDelay;
    private float displayDuration;

    private GameObject dialogueBubble;
    private TMP_Text dialogueTMP;

    public DialogueCommand(PomoController pomoController, string dialogueText, float characterDelay, float displayDuration)
    {
        this.pomoController = pomoController;
        this.dialogueText = dialogueText;
        this.characterDelay = characterDelay;
        this.displayDuration = displayDuration;

        dialogueBubble = pomoController.DialogueBubble;
        dialogueTMP = pomoController.DialogueText;
    }

    public async UniTask ExecuteAsync(CancellationTokenSource cancellationTokenSource)
    {
        dialogueTMP.text = "";
        
        // 말풍선에 대사 띄우기
        await UniTask.WhenAll(ShowDialogue(cancellationTokenSource), 
                OpenAndCloseDialogueBubble(cancellationTokenSource));
    }
    
    private async UniTask ShowDialogue(CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            int i = 0;
            while (i < dialogueText.Length)
            {
                //cancellationTokenSource.Token.ThrowIfCancellationRequested();
            
                if (i < dialogueText.Length - 1 && dialogueText[i] == '\\' && dialogueText[i + 1] == 'n')
                {
                    dialogueTMP.text += '\n';
                    i += 2;
                }
                else
                {
                    dialogueTMP.text += dialogueText[i];
                    i++;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(characterDelay), cancellationToken: cancellationTokenSource.Token);
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }
    
    private async UniTask OpenAndCloseDialogueBubble(CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            // cancellationTokenSource.Token.ThrowIfCancellationRequested();
        
            dialogueBubble.SetActive(true);

            await UniTask.Delay(TimeSpan.FromSeconds(displayDuration), cancellationToken: cancellationTokenSource.Token);

            dialogueBubble.SetActive(false);
        }
        catch (OperationCanceledException)
        {
            
        }
    }
    
    public void Cancel()
    {
        dialogueTMP.text = "";
        dialogueBubble.SetActive(false);
    }

    public void Finish()
    {
        dialogueTMP.text = "";
        dialogueBubble.SetActive(false);
    }
}

public class WateringCommand : ICommand
{
    private PomoController pomoController;
    private PomoAnimationController animationController;

    private float checkRadius = 3.0f;

    private Collider[] colliders = new Collider[100];
    
    public WateringCommand(PomoController pomoController, PomoAnimationController animationController)
    {
        this.pomoController = pomoController;
        this.animationController = animationController;
    }

    public async UniTask ExecuteAsync(CancellationTokenSource cancellationTokenSource)
    {
        // 물주기 애니메이션 재생
        animationController.PlayWatering();

        // 가장 가까운, 작물이 심어진 농업용 건물을 찾아
        // 해당 건물에 심어져있는 작물의 성장을 더한다.
        FarmBuilding farmBuilding = FindClosestObject(pomoController.transform.position, checkRadius, pomoController.BuildingLayerMask);
        if (farmBuilding != null)
        {
            farmBuilding.ReduceGrowthTimeByPercentage(pomoController.ReducePercentage);
        }
        
        await UniTask.Delay(2000, cancellationToken: cancellationTokenSource.Token);
        
        // 행동 당 기본적으로 할당되는 시간을 일찍 끝내기
        cancellationTokenSource.Cancel();
    }

    public FarmBuilding FindClosestObject(Vector3 center, float searchRadius, LayerMask layerMask)
    {
        // 가장 가까운 농업용 건물을 찾는다.
        DrawRangeCircle(center, searchRadius);
        
        var size = Physics.OverlapSphereNonAlloc(center, searchRadius, colliders, layerMask);

        FarmBuilding closestPlaceable = null;
        float minDistance = Mathf.Infinity;

        for (int i = 0; i < size; i++)
        {
            // 범위 안의 FarmBuilding 중에 
            if (colliders[i].transform.parent.TryGetComponent(out FarmBuilding building))
            {
                // 농업용 건물 중에 
                // 작물이 심어져 있는 건물 중에 
                if (DataBaseManager.Instance.BuildingDatabase.GetTagsByBuildingId(building.id).Any(item => item == "농업용") &&
                    building.IsCropPlanted)
                {
                    // 작물이 심어져 있는 건물 중에
                    
                    // 콜라이더의 가장 가까운 점을 구함
                    Vector3 closestPoint = colliders[i].ClosestPoint(center);
                    float distance = Vector3.Distance(center, closestPoint);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestPlaceable = building;
                    }
                }
            }
        }

        return closestPlaceable;
    }
    
    void DrawRangeCircle(Vector3 center, float rangeRadius = 5f, int segments = 36)
    {
        Color lineColor = Color.magenta;
        
        float angle = 0f;
        float angleStep = 360f / segments;

        Vector3 startPoint = Vector3.zero;
        Vector3 endPoint = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float rad = Mathf.Deg2Rad * angle;
            float x = Mathf.Sin(rad) * rangeRadius;
            float z = Mathf.Cos(rad) * rangeRadius;

            endPoint = center + new Vector3(x, 0f, z);

            if (i > 0)
            {
                // 이전 점과 현재 점 사이에 라인 그리기
                Debug.DrawLine(startPoint, endPoint, lineColor, 0.2f);
            }

            startPoint = endPoint;
            angle += angleStep;
        }
    }
    
    public void Cancel()
    {
    }

    public void Finish()
    {
        animationController.animator.Play("Idle");
    }
}

public class HeldCommand : ICommand
{
    private PomoController pomoController;
    private PomoAnimationController pomoAnimationController;
    private CancellationTokenSource cancellationTokenSource;
    private Camera mainCamera;
    private LayerMask _layerMask;
    
    private Vector3 groundOffSet = new Vector3(0,0.58f,0);
    private float renderingOffSet = -3.0f;
    
    public HeldCommand(PomoController pomoController, PomoAnimationController pomoAnimationController)
    {
        this.pomoController = pomoController;
        this.pomoAnimationController = pomoAnimationController;
        mainCamera = Camera.main;

        // Ground는 포함하되 Building은 제외하는 LayerMask
        _layerMask = this.pomoController.GroundLayerMask & ~this.pomoController.BuildingLayerMask;
    }

    public async UniTask ExecuteAsync(CancellationTokenSource _cancellationTokenSource)
    {
        // 건물의 UI들만 터치 비활성화
        // TODO : 최적화 방안 탐색
        foreach (var gameObject in GameObject.FindGameObjectsWithTag("Placeable"))
        {
            if (gameObject.TryGetComponent(out GraphicRaycaster gr))
            {
                gr.enabled = false;
            }
        }

        this.cancellationTokenSource = _cancellationTokenSource;

        pomoController.EnableTrigger();
        pomoAnimationController.Inflate();
        
        // TouchManager 이벤트 구독
        TouchManager.Instance.OnDrag += OnDrag;

        // 카메라 움직임 비활성화 
        CameraManager.Instance.DisableCameraPanning();
        
        // 취소될 때까지 무한 대기
        await UniTask.WaitUntilCanceled(cancellationTokenSource.Token);
    }
    
    private void OnDrag(Vector2 screenPosition)
    {
        // 스크린 좌표를 월드 좌표로 변환
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue, 0.2f);
        if (Physics.Raycast(ray, out hit, 100f, _layerMask))
        {
            pomoController.transform.position = hit.point + groundOffSet;
        }
    }

    public void Cancel()
    {
    }

    public void Finish()
    {
        foreach (var gameObject in GameObject.FindGameObjectsWithTag("Placeable"))
        {
            if (gameObject.TryGetComponent(out GraphicRaycaster gr))
            {
                gr.enabled = true;
            }
        }
        
        pomoController.DisableTrigger();
        pomoAnimationController.Deflate();
        
        TouchManager.Instance.OnDrag -= OnDrag;
        
        // 카메라 움직임 활성화 
        CameraManager.Instance.EnableCameraPanning();
    }
}