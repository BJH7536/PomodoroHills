using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("About Camera")]
    [SerializeField] public CinemachineBlendListCamera cinemachineBlendListCamera;
    [SerializeField] private CinemachineVirtualCamera ZoomOutCamera;
    [SerializeField] List<CinemachineVirtualCameraBase> cameras;
    float _cameraTransitionDelay = 0.5f;
    
    private CinemachineBrain cinemachineBrain;
    private int targetIndex = -1;

    [SerializeField] public static CameraMode currentCameraMode = CameraMode.ZoomOut;
    
    public enum CameraMode
    {
        ZoomIn,
        ZoomOut,
    }
    
    private void Start()
    {
        Application.targetFrameRate = 120;
        
        // CinemachineBrain 컴포넌트 가져오기
        cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
    }
    
    public void CameraTransition(CinemachineVirtualCamera toCam)
    {
        float time1 = Time.realtimeSinceStartup;

        CinemachineBlendListCamera.Instruction prevInstruction = cinemachineBlendListCamera.m_Instructions[1];
        
        cinemachineBlendListCamera.m_Instructions = new CinemachineBlendListCamera.Instruction[2];

        cinemachineBlendListCamera.m_Instructions[0] = prevInstruction;
        cinemachineBlendListCamera.m_Instructions[0].m_VirtualCamera.gameObject.SetActive(false);
        //cinemachineBlendListCamera.m_Instructions[0].m_VirtualCamera.GetComponent<OriginParentMapper>().GoBackToOriginParent();
        
        toCam.transform.SetParent(cinemachineBlendListCamera.transform);
        cinemachineBlendListCamera.m_Instructions[1].m_VirtualCamera = toCam;
        cinemachineBlendListCamera.m_Instructions[1].m_Blend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
        cinemachineBlendListCamera.m_Instructions[1].m_Blend.m_Time = _cameraTransitionDelay;
        cinemachineBlendListCamera.m_Instructions[1].m_Hold = 1.0f;
        
        float time2 = Time.realtimeSinceStartup;
        Debug.Log($"Camera Blended into {toCam.name} | duration {time2 - time1}");
    }

    public void ZoomOut()
    {
        if (currentCameraMode == CameraMode.ZoomOut) return;
        
        CameraTransition(ZoomOutCamera);
        
        TileController.UnHideAllTile();

        currentCameraMode = CameraMode.ZoomOut;
    }
}
