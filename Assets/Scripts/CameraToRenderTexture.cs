using UnityEngine;
using UnityEngine.UI;

public class CameraToRenderTexture : MonoBehaviour
{
    [SerializeField] public Camera mainCamera; // 이미지를 촬영하는 카메라
    [SerializeField] public RawImage rawImage; // Render Texture를 표시할 UI 요소

    void Start()
    {
        // 새로운 Render Texture 생성
        RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);

        // 카메라에 Render Texture 할당
        mainCamera.targetTexture = renderTexture;

        // RawImage에 Render Texture 할당하여 표시
        rawImage.texture = renderTexture;
    }
}