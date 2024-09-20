using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 팝업을 생성하는 팩토리 클래스입니다.
/// 지정된 타입의 팝업 프리팹을 로드하고 인스턴스를 생성합니다.
/// </summary>
public static class PopupFactory
{
    /// <summary>
    /// 팝업 타입별로 로드된 프리팹을 캐싱하는 딕셔너리입니다.
    /// </summary>
    private static readonly Dictionary<Type, GameObject> prefabCache = new Dictionary<Type, GameObject>();

    /// <summary>
    /// 특정 타입의 팝업을 생성하는 메서드입니다.
    /// 프리팹을 로드하고 인스턴스를 생성하여 반환합니다.
    /// </summary>
    /// <typeparam name="T">생성할 팝업의 타입</typeparam>
    /// <returns>생성된 팝업 인스턴스</returns>
    public static T CreatePopup<T>() where T : Popup
    {
        Type popupType = typeof(T);

        // 캐시에 프리팹이 있는지 확인
        if (!prefabCache.TryGetValue(popupType, out GameObject prefab))
        {
            // 프리팹 이름을 팝업 클래스 이름과 동일하게 가정
            string prefabName = popupType.Name;
            string prefabPath = $"Prefabs/UI/Popups/{prefabName}";

            // Resources 폴더에서 프리팹 로드
            prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"팝업 프리팹을 찾을 수 없습니다: {prefabPath}");
                return null;
            }

            // 캐시에 프리팹 저장
            prefabCache[popupType] = prefab;
        }

        // 팝업 인스턴스 생성
        GameObject popupInstance = GameObject.Instantiate(prefab);
        return popupInstance.GetComponent<T>();
    }
}