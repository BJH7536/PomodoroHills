using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PomoController : MonoBehaviour
{
    public NavMeshAgent agent;
    public float commandInterval = 2f;
    public float moveRadius = 5f;
    public float minDistance = 2f; // 최소 거리 추가

    private Queue<ICommand> commandQueue = new Queue<ICommand>();

    private float timer;
    private bool isActionPlaying = false;

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
        // PomoAnimationController에서 이벤트 구독
        PomoAnimationController animationController = GetComponent<PomoAnimationController>();
        animationController.OnActionStart += OnActionStart;
        animationController.OnActionEnd += OnActionEnd;
        
        MakeRandomMoveCommand();
    }

    private void Update()
    {
        if (isActionPlaying)
        {
            agent.isStopped = true;
            agent.ResetPath();
            return;
        }
        else
        {
            agent.isStopped = false;
        }

        timer += Time.deltaTime;
        if (timer >= commandInterval)
        {
            MakeRandomMoveCommand();
            // 유효한 경로가 아닐 경우 타이머를 초기화하지 않음
        }

        // Command 꺼내어 실행시키기
        if (commandQueue.Count > 0 && !isActionPlaying)
        {
            ICommand command = commandQueue.Dequeue();
            command.Execute();
        }
    }

    private void MakeRandomMoveCommand()
    {
        Vector3 finalPosition = GetRandomPoint(transform.position, moveRadius, minDistance);

        // 경로 유효성 검사
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(finalPosition, path);
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            MoveCommand moveCommand = new MoveCommand(agent, finalPosition);
            AddCommand(moveCommand);
            timer = 0f;
        }
    }

    private Vector3 GetRandomPoint(Vector3 center, float maxRadius, float minRadius)
    {
        // 무작위 방향을 구함
        Vector3 randomDirection = Random.insideUnitSphere.normalized;

        // 최소 거리와 최대 거리 사이의 무작위 거리 구하기
        float distance = Random.Range(minRadius, maxRadius);

        // 방향과 거리를 사용하여 위치 계산
        Vector3 randomPoint = center + randomDirection * distance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, maxRadius, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            return center;
        }
    }

    public void AddCommand(ICommand command)
    {
        commandQueue.Enqueue(command);
    }

    private void OnActionStart()
    {
        isActionPlaying = true;
    }

    private void OnActionEnd()
    {
        isActionPlaying = false;
    }
}