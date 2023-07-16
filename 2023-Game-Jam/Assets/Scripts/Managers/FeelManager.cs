using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Core.Events;
using Data_Classes;
using DG.Tweening;
using UnityEngine;
using EventType = Core.Events.EventType;

public class FeelManager : MonoBehaviour
{
    public class DutchData
    {
        public float magnitude;
        public float duration;

        public DutchData(float magnitude, float duration) {
            this.magnitude = magnitude;
            this.duration = duration;
        }
    }
    
    private CinemachineVirtualCamera _camSettings;
    private float _fov;
    private float _dutch;
    private Tweener _dutchTweener;
    private Dictionary<NoteData.LaneOrientation, DutchData> _laneToDutch = new Dictionary<NoteData.LaneOrientation, DutchData>()
    {
        {NoteData.LaneOrientation.One, new DutchData(4, .25f)},
        {NoteData.LaneOrientation.Two,  new DutchData(-4, .35f)},
        {NoteData.LaneOrientation.Three,  new DutchData(4, .35f)},
        {NoteData.LaneOrientation.Four,  new DutchData(7, .25f)},
    };
    

    private void Awake()
    {
        this.AddListener(EventType.SliderNoteHoldingEvent, param => OnSliderHold((NoteData.LaneOrientation) param));
        this.AddListener(EventType.SliderNoteReleaseEvent, param => OnSliderRelease((NoteData.LaneOrientation) param));
    }

    private void Start()
    {
        _camSettings = FindObjectOfType<CinemachineVirtualCamera>();
        _fov = _camSettings.m_Lens.FieldOfView;
        _dutch = _camSettings.m_Lens.Dutch;
        _dutchTweener = null;
        // _laneToDutch = new Dictionary<NoteData.LaneOrientation, DutchData>()
        // {
        //     {NoteData.LaneOrientation.One, new DutchData(-7, .15f)},
        //     {NoteData.LaneOrientation.Two,  new DutchData(-4, .25f)},
        //     {NoteData.LaneOrientation.Three,  new DutchData(4, .25f)},
        //     {NoteData.LaneOrientation.Four,  new DutchData(7, .15f)},
        // };
    }


    private void OnSliderHold(NoteData.LaneOrientation orientation)
    {
        if (_dutchTweener != null) {
            _dutchTweener.Kill();
            _dutchTweener.OnKill(() => _dutchTweener = null);
        }
        
        _dutchTweener = DOVirtual.Float(_camSettings.m_Lens.Dutch, _camSettings.m_Lens.Dutch + _laneToDutch[orientation].magnitude, 
            _laneToDutch[orientation].duration, RotateCamera).OnComplete(()=>_dutchTweener = null);
    }

    private void OnSliderRelease(NoteData.LaneOrientation orientation)
    {
        if (_dutchTweener != null) {
            _dutchTweener.Kill();
            _dutchTweener.OnKill(() => _dutchTweener = null);
        }
        
        _dutchTweener = DOVirtual.Float(_camSettings.m_Lens.Dutch, _camSettings.m_Lens.Dutch - _laneToDutch[orientation].magnitude, 
            .5f, RotateCamera).OnComplete(ResetRotation);
    }
    
    private void RotateCamera(float currFloat)
    {
        _camSettings.m_Lens.Dutch = currFloat;
    }

    private void ResetRotation()
    {
        if (_dutchTweener != null) {
            _dutchTweener.Kill();
            _dutchTweener.OnKill(() => _dutchTweener = null);
        }
        
        _dutchTweener = DOVirtual.Float(_camSettings.m_Lens.Dutch, 0, .5f, RotateCamera);
    }
    
    private void ChangeFOV()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
