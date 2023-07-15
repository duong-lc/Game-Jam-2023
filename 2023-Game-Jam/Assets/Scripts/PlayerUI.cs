using System;
using Core.Events;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EventType = Core.Events.EventType;

public class PlayerUI : MonoBehaviour
{
    public SpriteRenderer swordIcon;
    public TMP_Text text;
    public Image slider;
    public Slider attackSlider;
    
    public Color blueColor;
    public Color redColor;
    
    public NoteColorEnum colorEnum;

    private void Awake()
    {
        this.AddListener(EventType.PlayerUIUpdate, param => UpdateBar((CharacterController)param));
        this.AddListener(EventType.PlayerUIReset, param => ResetBar((CharacterController)param));
    }

    private void Start()
    {
        if (colorEnum == NoteColorEnum.Blue) {
            swordIcon.color = blueColor;
            text.color = blueColor;
            slider.color = blueColor;
        }
        else
        {
            swordIcon.color = redColor;
            text.color = redColor;
            slider.color = redColor;
        }
    }

    public void UpdateUIPosition(Vector3 newPos)
    {
        transform.position = newPos;
    }

    public void UpdateBar(CharacterController player)
    {
        if (player.playerColorEnum == colorEnum)
        {
            attackSlider.DOValue(player.sliderValue, .2f);
            if ( player.sliderValue >= 1)
            {
                player.sliderValue = 0;
                attackSlider.DOValue(0, .3f);
                player.playerAtkPoint++;
                UpdateText(player);
            }
        }
    }
    
    public void UpdateText(CharacterController player)
    {
        if (player.playerColorEnum == colorEnum)
        {
            attackSlider.value = player.sliderValue;
            text.text = player.playerAtkPoint.ToString();
        }
    }

    public void ResetBar(CharacterController player)
    {
        if(player.playerColorEnum == colorEnum) 
            attackSlider.DOValue(0, .3f);
    }
}