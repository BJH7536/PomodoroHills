using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using static Cinemachine.CinemachineVirtualCameraBase;

public class GameManger : MonoBehaviour
{
    [Header("About Cameras")]
    [SerializeField] CinemachineBlendListCamera cinemachineBlendListCamera;
    [SerializeField] List<CinemachineVirtualCameraBase> cameras;
    float _cameraTransitionDelay = 1.5f;

    [Header("About Popup")]
    [SerializeField] Popup MainPopup;

    private CinemachineBrain cinemachineBrain;
    private int targetIndex = -1;

    private void Start()
    {
        // CinemachineBrain 컴포넌트 가져오기
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();

        if (cinemachineBrain != null)
        {
            cinemachineBrain.m_CameraActivatedEvent.AddListener(OnCameraTransitionComplete);
        }
    }

    /// <summary>
    /// 카메라 전환이 정확히 시작될 때 수행되는 함수
    /// </summary>
    /// <param name="fromCam"></param>
    /// <param name="toCam"></param>
    private void OnCameraTransitionComplete(ICinemachineCamera toCam, ICinemachineCamera fromCam)
    {
        if (fromCam == null || toCam == null) return;

        if (((CinemachineVirtualCameraBase) toCam) == cameras[1])       // 카메라가 메인 화면을 비출 때
        {
            Debug.LogWarning($"Camera transition Starts. from {((CinemachineVirtualCameraBase)fromCam).gameObject.name} to {((CinemachineVirtualCameraBase)toCam).gameObject.name}");

            // 메인 화면의 팝업을 뿅하고 시각화
            StartCoroutine(showPopupWithDelay(_cameraTransitionDelay, MainPopup));
        } 
        else
        {
            if (MainPopup.gameObject.activeSelf)
                StartCoroutine(closePopupWithDelay(0, MainPopup));
        }
    }

    // 메인 화면
    public void lookIsland()
    {
        lookSometingByIndex(0);
    }

    // 게임 플레이 화면
    public void lookFireRange()
    {
        lookSometingByIndex(1);
    }

    // 설정 화면 ?
    public void lookAbandonedPlane()
    {
        lookSometingByIndex(2);
    }
    public void lookSometingByIndex(int index)
    {
        float time1 = Time.realtimeSinceStartup;

        for (int i = 0; i < cameras.Count; i++)
        {
            //cameras[i].transform.SetParent(cinemachineBlendListCamera.transform);
            cinemachineBlendListCamera.m_Instructions[i].m_VirtualCamera = cameras[i];

            cinemachineBlendListCamera.m_Instructions[i].m_Blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
            cinemachineBlendListCamera.m_Instructions[i].m_Blend.m_Time = _cameraTransitionDelay;
            cinemachineBlendListCamera.m_Instructions[i].m_Hold = 1.0f;
        }

        cinemachineBlendListCamera.m_Instructions[cameras.Count - 1].m_VirtualCamera = cameras[index];

        targetIndex = index;

        float time2 = Time.realtimeSinceStartup;
        Debug.Log($"Camera Blended into {index}th Camera | duration {time2 - time1}");
    }

    public IEnumerator showPopupWithDelay(float delay, Popup popup)
    {
        yield return new WaitForSeconds(delay);

        if (targetIndex == 1)
            popup.gameObject.SetActive(true);
    }

    public IEnumerator closePopupWithDelay(float delay, Popup popup)
    {
        yield return new WaitForSeconds(delay);

        popup.gameObject.SetActive(false);
    }

}
