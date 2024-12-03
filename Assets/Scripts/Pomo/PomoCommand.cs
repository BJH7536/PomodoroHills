using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public interface ICommand
{
    UniTask ExecuteAsync(CancellationTokenSource cancellationTokenSource);
    void Cancel();
    void Finish();
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
    private PomoAnimationController animationController;

    public WateringCommand(PomoAnimationController animationController)
    {
        this.animationController = animationController;
    }

    public async UniTask ExecuteAsync(CancellationTokenSource cancellationTokenSource)
    {
        // 물주기 애니메이션 재생
        animationController.PlayWatering();

        UniTask.Delay(2000, cancellationToken: cancellationTokenSource.Token);
    }

    public void Cancel()
    {
    }

    public void Finish()
    {
    }
}
