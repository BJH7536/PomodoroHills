using UnityEngine;
using UnityEngine.AI;

public interface ICommand
{
    void Execute();
}

public class MoveCommand : ICommand
{
    private NavMeshAgent agent;
    private Vector3 destination;

    public MoveCommand(NavMeshAgent agent, Vector3 destination)
    {
        this.agent = agent;
        this.destination = destination;
    }

    public void Execute()
    {
        agent.SetDestination(destination);
    }
}
