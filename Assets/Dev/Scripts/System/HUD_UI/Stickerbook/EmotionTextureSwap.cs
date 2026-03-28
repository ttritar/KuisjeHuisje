using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EmotionMaterialPair
{
    public Emotion Emotion;
    public Material Material;
}


public class EmotionTextureSwap : MonoBehaviour
{
    [SerializeField] private List<EmotionMaterialPair> _emotionTextureList = new();
    private Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void SwapTexture(Emotion emotion)
    {
        foreach (var pair in _emotionTextureList)
        {
            if (pair.Emotion == emotion)
            {
                _renderer.material = pair.Material;
                return;
            }
        }
        Debug.LogWarning("[EmotionTextureSwap] No material found for emotion " + emotion);
    }
}
