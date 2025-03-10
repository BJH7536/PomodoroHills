using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GooglePlayGames;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class PomoController : MonoBehaviour
{
    private Collider collider;
    private PomoAnimationController animationController;
    [SerializeField] private LongPressDetector longPressDetector;
    
    private Queue<ICommand> commandQueue = new Queue<ICommand>();
    private ICommand currentCommand;
    private CancellationTokenSource cancellationTokenSource;
    [SerializeField] private bool isExecuting = false;      // 현재 특별한 행동을 진행 중인지 여부

    [SerializeField] private int queueMax = 10;
    [SerializeField] private float queueAutoFilSeconds = 1.0f;
    [SerializeField] private float timeForPerAction = 7f;
    
    [Header("Move")]
    [SerializeField] public NavMeshAgent agent;
    [SerializeField] public float moveRadius = 10f;
    [SerializeField] public float minDistance = 3f;
    [SerializeField] public ParticlePooler ParticlePooler;
    [SerializeField] private AudioClip pomoDust;
    
    [Header("Dialogue")]
    [SerializeField] public GameObject DialogueBubble;
    [SerializeField] public TMP_Text DialogueText;

    [Header("Watering")] 
    [SerializeField] public float ReducePercentage = 20f;       // 현재 작물의 남은 성장 시간에서 줄일 비율(%)
    
    [Header("Held")]
    [SerializeField] private AudioClip onHold;

    private Camera mainCamera;
    [NonSerialized] public LayerMask GroundLayerMask;
    [NonSerialized] public LayerMask BuildingLayerMask;
    
    private void OnDrawGizmosSelected()
    {
        // 최대 반경 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, moveRadius);

        // 최소 반경 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, minDistance);
    }
    
    private void Start()
    {
        mainCamera = Camera.main;

        GroundLayerMask = 1 << LayerMask.NameToLayer("Ground");
        BuildingLayerMask = 1 << LayerMask.NameToLayer("3D");
        
        collider = GetComponent<Collider>();
        
        // PomoAnimationController에서 이벤트 구독
        animationController = GetComponent<PomoAnimationController>();
        
        // TouchManager의 OnDoubleClick 이벤트 구독
        TouchManager.Instance.OnDoubleClick += OnDoubleClick;
        
        // 롱프레스 이벤트 구독
        longPressDetector.onLongPress.AddListener(OnHold);
        longPressDetector.onLongPressEnd.AddListener(OnRelease);
        
        // 말풍선 UI는 끄고 시작
        DialogueBubble.SetActive(false);
        
        // 다음 WateringCommand 스케줄링
        //WateringCommandLoop().Forget();
        
        // 이동 혹은 말걸기 Command 생성 시작
        RandomCommandCycle().Forget();

        // 첫번째 Command부터 시작 
        ExecuteNextCommand().Forget();
    }

    private void OnDisable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.OnDoubleClick -= OnDoubleClick;
        }
        
        longPressDetector.onLongPress.RemoveListener(OnHold);
        longPressDetector.onLongPressEnd.RemoveListener(OnRelease);
    }

    public void EnableTrigger()
    {
        collider.isTrigger = true;
    }
    
    public void DisableTrigger()
    {
        collider.isTrigger = false;
    }
    
    #region 사용자의 입력에 반응
    
    /// <summary>
    /// 뽀모에서 손을 놓을 때
    /// </summary>
    private void OnMouseUp()
    {
        // 뽀모를 꾸욱 눌러서 들었다가 놓을 때는 여기선 아무 기능 안한다.
        if (currentCommand is HeldCommand) return;
        
        // 애니메이션 출력
        animationController.PlayGreeting();
        
        // 처음 인사하기 업적
        PlayGamesPlatform.Instance.UnlockAchievement(GPGSIds.achievement_2);
        
        // 현재 대화 중이 아니면 DialogueCommand를 최우선으로 실행
        InsertCommandAtFront(CreateDialogueCommand());
    }
    
    private void OnDoubleClick(Vector2 screenPosition)
    {
        // 카메라에서 레이캐스트하여 클릭 위치의 월드 좌표 얻기
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 1f);
        
        if (Physics.Raycast(ray, out hit, 100, GroundLayerMask))
        {
            // NavMesh 위의 위치인지 확인
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(hit.point, out navHit, 1.0f, NavMesh.AllAreas))
            {
                // 최우선으로 이동 명령 수행
                InsertCommandAtFront(CreateMoveCommand(navHit.position));
                
                // 파티클 재생
                PlayParticleAtPosition(navHit.position);
                
                // 효과음 재생
                SoundManager.Instance.Play(pomoDust);
            }
        }
    }

    private void OnHold()
    {
        CancelCurrentCommand();
        
        // HeldCommand 생성 후 큐의 맨 앞에 추가
        HeldCommand heldCommand = new HeldCommand(this, animationController);
        InsertCommandAtFront(heldCommand);
                        
        // 효과음 재생
        SoundManager.Instance.Play(onHold);
    }

    void OnRelease()
    {
        InsertCommandAtFront(new WateringCommand(this, animationController));
    }
    
    #endregion
    
    #region Command Queue Management

    private void EnqueueCommand(ICommand command)
    {
        commandQueue.Enqueue(command);
    }

    private async UniTaskVoid ExecuteNextCommand()
    {
        while (true)
        {
            if (commandQueue.Count > 0)
            {
                isExecuting = true;
                currentCommand = commandQueue.Dequeue();
                cancellationTokenSource = new CancellationTokenSource();
                try
                {
                    // 명령 실행과 7초 카운트다운을 병렬로 시작
                    var commandTask = currentCommand.ExecuteAsync(cancellationTokenSource);
                    var countdownTask = UniTask.Delay((int)(timeForPerAction * 1000), cancellationToken: cancellationTokenSource.Token);

                    // 명령과 카운트다운이 모두 완료될 때까지 대기
                    await UniTask.WhenAll(commandTask, countdownTask);
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException || (ex is AggregateException aggEx && aggEx.InnerExceptions.All(e => e is OperationCanceledException)))
                    {
                        DebugEx.Log("OperationCanceledException 발생함");
                        currentCommand.Cancel();
                    }
                    else
                    {
                        DebugEx.LogError($"명령 실행 중 예외 발생: {ex}");
                    }
                }
                finally
                {
                    currentCommand.Finish();
                    cancellationTokenSource.Dispose();
                    cancellationTokenSource = null;
                }
                isExecuting = false;
            }
            else
            {
                await UniTask.Yield();
            }
        }
    }
    
    /// <summary>
    /// 현재 명령큐의 제일 앞에 명령을 삽입
    /// </summary>
    /// <param name="command"></param>
    private void InsertCommandAtFront(ICommand command)
    {
        // 현재 Command가 있다면 취소
        CancelCurrentCommand();
        
        var newQueue = new Queue<ICommand>();
        newQueue.Enqueue(command);
        foreach (var cmd in commandQueue)
        {
            newQueue.Enqueue(cmd);
        }
        commandQueue = newQueue;
    }
    
    /// <summary>
    /// 현재 Command를 취소시키는 기능
    /// </summary>
    private void CancelCurrentCommand()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
        }
    }
    
        /// <summary>
    /// 뽀모가 혼자 도는 사이클
    /// 돌아다니거나, 말을 걸거나
    /// </summary>
    private async UniTaskVoid RandomCommandCycle()
    {
        while (true)
        {
            if (commandQueue.Count < queueMax)
            {
                if (Random.Range(0.0f, 1.0f) < 0.8f)
                {
                    // 이동 명령 생성
                    EnqueueCommand(CreateRandomMoveCommand());
                }
                else
                {
                    // 대화 명령 생성
                    EnqueueCommand(CreateDialogueCommand());
                }
        
            }
            
            await UniTask.Delay((int)(queueAutoFilSeconds * 1000));
        }
    }
    
    private MoveCommand CreateRandomMoveCommand()
    {
        // 목적지 설정
        Vector3 finalPosition = GetRandomMovablePoint(transform.position, moveRadius, minDistance);

        MoveCommand command = null;

        while (true)
        {
            command = CreateMoveCommand(finalPosition);

            if (command != null) return command;
            
            finalPosition = GetRandomMovablePoint(transform.position, moveRadius, minDistance);
        }
    }
    
    private MoveCommand CreateMoveCommand(Vector3 position)
    {
        // 경로 유효성 검사
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(position, path);
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            
            return new MoveCommand(this, animationController, agent, position);
        }
        else
        {
            //DebugEx.LogWarning("유효하지 않은 위치입니다.");
            return null;
        }
    }
    
    private Vector3 GetRandomMovablePoint(Vector3 center, float maxRadius, float minRadius)
    {
        
        Vector2 vec = Random.insideUnitCircle.normalized;
        Vector3 randomDirection = new Vector3(vec.x, 0, vec.y);

        // 최소 거리와 최대 거리 사이의 무작위 거리 구하기
        float distance = Random.Range(minRadius, maxRadius);
        
        // 방향과 거리를 사용하여 위치 계산
        Vector3 randomPoint = center + randomDirection * distance;

        return randomPoint;
    }
    
    private DialogueCommand CreateDialogueCommand()
    {
        // 현재 시간에 적절한 대사 찾기
        int currentHour = System.DateTime.Now.Hour;
        PomoDialogue.Dialogue dialogue = DataBaseManager.Instance.PomoDialogue.GetDialogueForCurrentTime(currentHour);

        // DialogueCommand 생성 및 추가
        return new DialogueCommand(this, dialogue.Text, dialogue.characterDelay, dialogue.Delay);
    }
    
    private async UniTaskVoid WateringCommandLoop()
    {
        while (true)
        {
            await UniTask.Delay((int)(Random.Range(30f, 60f) * 1000));
            
            WateringCommand wateringCommand = new WateringCommand(this, animationController);
            commandQueue.Enqueue(wateringCommand);
        }
    }

    
    #endregion

    #region Particle

    private void PlayParticleAtPosition(Vector3 position)
    {
        GameObject particle = ParticlePooler.GetParticle();
        particle.transform.position = position;
        particle.SetActive(true);

        // 파티클 시스템이 끝나면 풀로 반환하도록
        ReturnParticleWhenFinished(particle).Forget();
    }

    private async UniTaskVoid ReturnParticleWhenFinished(GameObject particle)
    {
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            
            await UniTask.Delay((int)((ps.main.duration + ps.main.startLifetime.constantMax) * 1000)); // 클립 길이만큼 대기
            
            ParticlePooler.ReturnParticle(particle);
        }
        else
        {
            // 파티클 시스템이 없으면 바로 반환
            ParticlePooler.ReturnParticle(particle);
        }
    }

    #endregion
}