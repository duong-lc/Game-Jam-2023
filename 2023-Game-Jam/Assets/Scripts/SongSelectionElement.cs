using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class SongSelectionElement : MonoBehaviour
{
    public Sprite albumCover;
    public Sprite discCover;
    public MidiData midiData;

    public GameObject coverGameObject;
    public GameObject discGameObject;

    [ReadOnly] public Image coverImage;
    [ReadOnly] public Image discImage;

    private Image CoverImage
    {
        get 
        {
            if (coverImage != null) return coverImage;

            if (!coverGameObject) return null;
            var cache = coverGameObject.GetComponent<Image>();
            if (cache != null) {
                coverImage = cache;
            }
            return coverImage;
        }
    }
    
    private Image DiscImage
    {
        get 
        {
            if (discImage != null) return discImage;

            if (!discGameObject) return null;
            var cache = discGameObject.GetComponent<Image>();
            if (cache != null) {
                discImage = cache;
            }
            return discImage;
        }
    }
    
    private void Start()
    {
        if(CoverImage != null && albumCover != null)
            CoverImage.sprite = albumCover;
        if(DiscImage != null && discCover != null)
            DiscImage.sprite = discCover;
    }

    public void UpdateCurrentSong()
    {
        GameModeManager.Instance.CurrentMidiData = midiData;
    }
}
