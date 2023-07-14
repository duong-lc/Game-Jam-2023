using System;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using DG.Tweening;
using UnityEngine;
using Core.Events;
using EventType = Core.Events.EventType;

public class PauseTransition : MonoBehaviour
{
    public enum RibbonState {
        Playing,
        Pause,
        Return
    }
    
    [SerializeField] private GameObject[] ribbons;
    [SerializeField] private GameObject[] disc;

    [SerializeField] private float PlayLocalX;
    [SerializeField] private float PauseLocalX;
    [SerializeField] private float ReturnLocalX;
    
    [SerializeField] private float DiscPlayLocalX;
    [SerializeField] private float DiscPauseLocalX;
    [SerializeField] private float DiscReturnLocalX;

    private void Awake()
    {
        this.AddListener(EventType.PauseTransitionEvent, param => RibbonFlowTransition((RibbonState) param));
    }

    private void Start()
    {
        #region Preset Positions
        foreach (var ribbon in ribbons) {
            ribbon.transform.localPosition = new Vector3(PlayLocalX,
                ribbon.transform.localPosition.y,
                ribbon.transform.localPosition.z);
        }
        foreach (var item in disc)
        {
            item.transform.localPosition = new Vector3(DiscPlayLocalX,
                item.transform.localPosition.y,
                item.transform.localPosition.z);
            item.transform.localEulerAngles = new Vector3(item.transform.localRotation.eulerAngles.x,
                item.transform.localRotation.eulerAngles.y, -90);
        }
        #endregion

        
    }
    
    private void RibbonFlowTransition(RibbonState state)
    {
        float destX = PlayLocalX;
        float destDiscX = DiscPlayLocalX;
        switch (state) {
            case RibbonState.Playing:
                Sequence playingSequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
                
                float t1 = 0;
                foreach (var item in disc) {
                    playingSequence.Insert(.1f, item.transform.DOLocalMoveX(DiscPlayLocalX, 0.7f));
                    playingSequence.Insert(.1f, item.transform.DOLocalRotate(
                        new Vector3(item.transform.localRotation.eulerAngles.x, item.transform.localRotation.eulerAngles.y, -90), 
                        0.4f));
                }
                foreach (var ribbon in ribbons) {
                    playingSequence.Insert(t1, ribbon.transform.DOLocalMoveX(PlayLocalX, 0.3f));
                    t1 += .1f;
                }
                
                
                playingSequence.Play();
                break;
            case RibbonState.Pause:
                Sequence pauseSequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
                
                float t2 = 0;
                foreach (var ribbon in ribbons) {
                    pauseSequence.Insert(t2, ribbon.transform.DOLocalMoveX(PauseLocalX, 0.3f));
                    t2 += .1f;
                }
                foreach (var item in disc) {
                    pauseSequence.Insert(.8f, item.transform.DOLocalMoveX(DiscPauseLocalX, 0.4f));
                    pauseSequence.Insert(.8f, item.transform.DOLocalRotate(
                        new Vector3(item.transform.localRotation.eulerAngles.x, item.transform.localRotation.eulerAngles.y, 0), 
                        0.4f));
                }
                
                pauseSequence.Play();
                break;
            case RibbonState.Return:
                destX = ReturnLocalX;
                destDiscX = DiscReturnLocalX;
                break;
        }
        
        // foreach (var ribbon in ribbons)
        // {
        //     ribbon.transform.DOLocalMoveX(destX, 0.3f);
        //     yield return new WaitForSeconds(.1f);
        // }
        //
        //
        // foreach (var item in disc)
        // {
        //     item.transform.DOLocalMoveX(destDiscX, 0.4f);
        // }

        
        
    }
}
