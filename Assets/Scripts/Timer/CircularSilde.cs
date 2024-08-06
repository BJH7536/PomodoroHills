// Copyright (C) 2023 ricimi. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at https://unity.com/legal/as-terms.
// 다른 스크립트가 함수를 요청할수 있도록 수정
//TimerSlide 게임오브젝트에 현재 기존 스크립트가 꺼진상태로 살아있음

using UnityEngine;
using UnityEngine.UI;


public class CircularSilde : MonoBehaviour
{
	[Range(0,100)]
	public float Percentage = 0;

	[SerializeField]
	private Image progressImage;

	private void Update()
	{
	}

	public void CallUpdateProgress(float value, float total)		//프로그레스 바 업데이트 호출
    {
		if (total != 0)												//백분율 계산
		{
            value = Mathf.Round((value / total) * 1000f) / 1000f;	
        }
		else { value = 1; }
		UpdateProgress(value);
    }
    private void UpdateProgress(float value)
	{
		float fillAmount = (value);
		progressImage.fillAmount = fillAmount;
		float angle = fillAmount * 360.0f;
	}

	public float GetFillAmount()
	{
		return progressImage.fillAmount;
	}
}
