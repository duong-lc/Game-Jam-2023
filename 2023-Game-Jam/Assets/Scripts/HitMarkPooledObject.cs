using System;
using System.Collections;
using System.Collections.Generic;
using Core.Logging;
using Data_Classes;
using DG.Tweening;
using TMPro;
using UnityEngine;


public class HitMarkPooledObject : PooledObjectBase
{
    public NoteData.LaneOrientation orientation { get; private set; }
    private Tween _tween;
    private float _time = 2f;
    
    public override void Init(PooledObjectCallbackData data, Action<PooledObjectBase> killAction)
    {
        var hitMarkData = (HitMarkInitData)data;
        orientation = hitMarkData.orientation;
        
        transform.position= GameModeManager.Instance.GameModeData.GetHitPoint(orientation);
        if (Camera.main != null) transform.rotation = Camera.main.transform.rotation;

        KillAction = killAction;
        canRelease = false;
        TweenHitText(gameObject);
        
        StartCoroutine(RunRoutine());
    }

    public override IEnumerator RunRoutine()
    {
        yield return new WaitForSeconds(_time);
        
       // NCLogger.Log($"kill {gameObject.name}");
        KillAction(this);
        yield return null;
    }

    private void TweenHitText(GameObject obj)
    {
        var t = obj.transform;
        var dest = t.position + t.up * 1.5f;
        var dest2 = dest + t.up * .5f;
        var tmp = t.GetComponent<TMP_Text>();
        tmp.alpha = 1;

        //Sequence hitSequence = DOTween.Sequence();
        // hitSequence.Append(t.DOMove(dest, 2f));
        // hitSequence.Append(DOTween.ToAlpha(()=> tmp.color, x=> tmp.color = x, 0, 1));

        t.DOMove(dest, _time);
        DOTween.ToAlpha(() => tmp.color, x => tmp.color = x, 0, 1.5f);
    }
}
