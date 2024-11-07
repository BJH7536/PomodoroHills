using Cinemachine;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class TileController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] public CinemachineVirtualCamera camera;
    [SerializeField] private CameraManager CameraManager;
    [SerializeField] private MMF_Player MmfPlayer;

    public static void HideAllTile()
    {
        TileController[] otherTiles = FindObjectsOfType<TileController>();
        foreach (TileController otherTile in otherTiles)
        {
            otherTile.InvisibleChild();
        }
    }
    
    public static void HideAllTile(TileController except)
    {
        TileController[] otherTiles = FindObjectsOfType<TileController>();
        foreach (TileController otherTile in otherTiles)
        {
            if (otherTile != except)
            {
                otherTile.InvisibleChild();
            }
        }
    }
    
    public static void UnHideAllTile()
    {
        TileController[] otherTiles = FindObjectsOfType<TileController>();
        foreach (TileController otherTile in otherTiles)
        {
            otherTile.VisibleChild();
        }
    }
    
    public static void UnHideAllTile(TileController except)
    {
        TileController[] otherTiles = FindObjectsOfType<TileController>();
        foreach (TileController otherTile in otherTiles)
        {
            if (otherTile != except)
            {
                otherTile.VisibleChild();
            }
        }
    }
    
    private void Awake()
    {
        MmfPlayer = GetComponent<MMF_Player>();
    }

    [ContextMenu("VisibleChild")]
    public void VisibleChild()
    {
        GetComponent<Collider>().enabled = true;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
    }
    
    [ContextMenu("InvisibleChild")]
    public void InvisibleChild()
    {
        GetComponent<Collider>().enabled = false;
        
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    [ContextMenu("DisableCamera")]
    public void DisableCamera()
    {
        camera.gameObject.SetActive(false);
    }
    
    [ContextMenu("FindMyCamera")]
    public void FindMyCamera()
    {
        camera = GameObject.Find($"{gameObject.name}_Camera").GetComponent<CinemachineVirtualCamera>();
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        MmfPlayer.PlayFeedbacks();
        
        //CameraManager.CameraTransition(camera);

        // Sequence seq = DOTween.Sequence();
        // seq.Append(transform.DOScale(originScale * 1.2f, 0.25f).SetEase(Ease.InOutBounce));
        // seq.Append(transform.DOScale(originScale, 0.25f).SetEase(Ease.InOutBounce));
        
        TileController.HideAllTile(this);
        
        CameraManager.currentCameraMode = CameraManager.CameraMode.ZoomIn;
    }
}
