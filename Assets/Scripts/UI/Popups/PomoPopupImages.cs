using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PomoPopupImages", menuName = "PomodoroHills/PomoPopupImages")]
public class PomoPopupImages : ScriptableObject
{
    [Serializable]
    public class SpriteParts
    {
        public string name;
        public Sprite sprite;
    }
    
    [SerializeField]
    public List<SpriteParts> faceSprites;
    [SerializeField]
    public List<SpriteParts> bodySprites;
    private Dictionary<string, Sprite> faceSpriteDictionary;
    private Dictionary<string, Sprite> bodySpriteDictionary;

    private void OnEnable()
    {
        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        // �� ��������Ʈ ��ųʸ� �ʱ�ȭ
        faceSpriteDictionary = new Dictionary<string, Sprite>();
        foreach (var facePart in faceSprites)
        {
            if (!faceSpriteDictionary.ContainsKey(facePart.name))
            {
                faceSpriteDictionary.Add(facePart.name, facePart.sprite);
            }
        }

        // ���� ��������Ʈ ��ųʸ� �ʱ�ȭ
        bodySpriteDictionary = new Dictionary<string, Sprite>();
        foreach (var bodyPart in bodySprites)
        {
            if (!bodySpriteDictionary.ContainsKey(bodyPart.name))
            {
                bodySpriteDictionary.Add(bodyPart.name, bodyPart.sprite);
            }
        }
    }

    /// <summary>
    /// �̸����� �� ��������Ʈ�� ã�� �Լ�
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Sprite GetFaceSpriteByName(string name)
    {
        if (faceSpriteDictionary.TryGetValue(name, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"Face sprite with name '{name}' not found.");
            return null;
        }
    }

    /// <summary>
    /// �̸����� ���� ��������Ʈ�� �޴� �Լ�
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public Sprite GetBodySpriteByName(string name)
    {
        if (bodySpriteDictionary.TryGetValue(name, out Sprite sprite))
        {
            return sprite;
        }
        else
        {
            Debug.LogWarning($"Body sprite with name '{name}' not found.");
            return null;
        }
    }
}