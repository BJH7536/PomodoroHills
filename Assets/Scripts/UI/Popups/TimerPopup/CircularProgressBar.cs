using UnityEngine;
using UnityEngine.UI;
using VInspector;

public class CircularProgressBar : MonoBehaviour
{
	#region Fields

	[Range(0,100)]
	public float Percentage = 0;

	[SerializeField]
	private Image progressImage;
	[SerializeField]
	private RectTransform progressEndContainer;
	[SerializeField]
	private RectTransform progressEndImage;

	[SerializeField] private Image circle_end;
	[SerializeField] private Image circle_start;
	
	[SerializeField] private Color runningFocus;
	[SerializeField] private Color runningRelax;
	[SerializeField] private Color paused;
	
	private int totalTimeInSeconds; // 전체 시간을 초 단위로 저장

	#endregion
	
	[Button]
	public void ChangeColorFocus()
	{
		progressImage.color = runningFocus;
		circle_start.color = runningFocus;
		circle_end.color = runningFocus;
	}
	
	[Button]
	public void ChangeColorRelax()
	{
		progressImage.color = runningRelax;
		circle_start.color = runningRelax;
		circle_end.color = runningRelax;
	}
	
	[Button]
	public void ChangeColorPaused()
	{
		progressImage.color = paused;
		circle_start.color = paused;
		circle_end.color = paused;
	}
	
	public void SetTotalTime(int totalTimeInMinutes)
	{
		totalTimeInSeconds = totalTimeInMinutes * 60;

		DebugEx.Log($"전체 시간이 {totalTimeInSeconds}로 설정됨");
		
		Canvas.ForceUpdateCanvases();
	}

	/// <summary>
	/// 남은 시간을 기준으로 진행 바를 업데이트하는 함수
	/// </summary>
	/// <param name="remainingTimeInSeconds"></param>
	public void UpdateByRemainingTime(int remainingTimeInSeconds)
	{
		if (totalTimeInSeconds <= 0) return; // 전체 시간이 설정되지 않은 경우 처리하지 않음

		float percentage = ((float)remainingTimeInSeconds / totalTimeInSeconds);
		progressImage.fillAmount = percentage;
		float angle = percentage * 360.0f;
		progressEndContainer.localEulerAngles = new Vector3(0, 0, -angle);
		progressEndImage.localEulerAngles = new Vector3(0, 0, angle);
		
		//DebugEx.Log($"Circular percentage is  {percentage}");
	}

	private void UpdateProgress(float value)
	{
		float fillAmount = (value / 100.0f);
		progressImage.fillAmount = fillAmount;
		float angle = fillAmount * 360.0f;
		progressEndContainer.localEulerAngles = new Vector3(0, 0, -angle);
		progressEndImage.localEulerAngles = new Vector3(0, 0, angle);
	}

	public float GetFillAmount()
	{
		return progressImage.fillAmount;
	}

	public void Fill()
	{
		UpdateProgress(100);
	}
}
