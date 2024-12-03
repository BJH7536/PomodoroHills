using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PomoAnimationController : MonoBehaviour
{
    public Animator animator;
    public Camera mainCamera;
    public RectTransform canvas;

    private bool isActionPlaying = false;           // ���� Ư�� �ൿ�� ��� ������ ��Ÿ���� �÷���
    private float actionTimer = 0f;                 // ���� ���� ���� �׼��� ��� �ð�
    private float actionDuration = 0f;              // �׼��� ���� �ð�
    
    public readonly int MoveX = Animator.StringToHash("MoveX");
    public readonly int MoveY = Animator.StringToHash("MoveY");
    public readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void LateUpdate()
    {
        // �Ǹ� Sprite�� �������ϴ� ĵ���� ���� ����
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
