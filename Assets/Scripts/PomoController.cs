using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PomoController : MonoBehaviour
{
    public NavMeshAgent agent;
    public float commandInterval = 2f;
    public float moveRadius = 5f;

    private Queue<ICommand> commandQueue = new Queue<ICommand>();

    private float timer;
    private bool isActionPlaying = false;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, moveRadius);
    }
    
    private void Start()
    {
        // PomoAnimationController에서 이벤트 구독
        PomoAnimationController animationController = GetComponent<PomoAnimationController>();
        animationController.OnActionStart += OnActionStart;
        animationController.OnActionEnd += OnActionEnd;
    }

    private void Update()
    {
        if (isActionPlaying)
        {
            agent.isStopped = true;
            return;
        }
        else
        {
            agent.isStopped = false;
        }

        timer += Time.deltaTime;
        if (timer >= commandInterval)
        {
            Vector3 finalPosition = GetRandomPoint(transform.position, moveRadius);

            // 경로 유효성 검사
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(finalPosition, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                MoveCommand moveCommand = new MoveCommand(agent, finalPosition);
                AddCommand(moveCommand);
                timer = 0f;
            }
            // 유효한 경로가 아닐 경우 타이머를 초기화하지 않음
        }

        // Command 꺼내어 실행시키기
        if (commandQueue.Count > 0 && !isActionPlaying)
        {
            ICommand command = commandQueue.Dequeue();
            command.Execute();
        }
    }

    private Vector3 GetRandomPoint(Vector3 center, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
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