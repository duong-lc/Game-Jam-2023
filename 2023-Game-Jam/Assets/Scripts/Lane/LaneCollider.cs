using System;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using Core.Logging;
using Data_Classes;
using NoteClasses;
using StaticClass;
using UnityEngine;
using EventType = Core.Events.EventType;

[RequireComponent(typeof(Lane))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class LaneCollider : MonoBehaviour
{
    private Rigidbody _rb;
    private Collider _col;
    private List<NoteBase> noteList = new ();
    private LayerMask _noteLayerMask;
    private Lane _lane;

    public Lane Lane
    {
        get
        {
            if (!_lane) _lane = GetComponent<Lane>();
            return _lane;
        }
    }
    
    public NoteData.LaneOrientation LaneOrientation => Lane.LaneOrientation;

    public Rigidbody Rigidbody {
        get {
            if (!_rb) {
                _rb = GetComponent<Rigidbody>();
            }
            return _rb;
        }
    }
    
    public Collider Collider {
        get {
            if (!_col) {
                _col = GetComponent<Collider>();
            }
            return _col;
        }
    }

    public LayerMask NoteLayerMask {
        get {
            _noteLayerMask = GameModeManager.Instance.GameModeData.noteLayerMask;
            return _noteLayerMask;
        }
    }

    private void Awake() {
        this.AddListener(EventType.NoteHitEarlyEvent,param => RemoveNote(((HitMarkInitData)param).noteRef));
        this.AddListener(EventType.NoteHitPerfectEvent, param => RemoveNote(((HitMarkInitData)param).noteRef));
        this.AddListener(EventType.NoteHitLateEvent, param => RemoveNote(((HitMarkInitData)param).noteRef));
        this.AddListener(EventType.NoteMissEvent, param => RemoveNote(((HitMarkInitData)param).noteRef));
        
        
        Collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider col) {
        if (!col.gameObject.activeSelf) return;
        var note = GetNote(col);
        if (note) noteList.Add(note);
    }

    private void OnTriggerExit(Collider col) {
       // if (!col.gameObject.activeSelf) return;
        var note = GetNote(col);
        if (note && noteList.Contains(note)) noteList.Remove(note);
    }

    private NoteBase GetNote(Collider col) {
        if (!CheckLayerMask.IsInLayerMask(col.gameObject, NoteLayerMask)) {
            // NCLogger.Log($"Not a Note");
            return null;
        }

        NoteBase note = null;
        foreach (var kvp in GameModeManager.Instance.GameModeData.TypeToTag)
        {
            if (col.CompareTag(kvp.Value))
            {
                switch (kvp.Key)
                {
                    case NoteType.NormalNote:
                        note = col.GetComponent<NoteNormal>();
                        break;
                    case NoteType.SliderNote:
                        note = col.GetComponent<NoteSlider>();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        return !note ? null : note;
    }

    public NoteBase GetApproachingNote() {
        return noteList.Count == 0 ? null : noteList[0];
    }

    public void RemoveNote(NoteBase note) {
        //if (note.noteOrientation == LaneOrientation) {
        if (note && noteList.Contains(note)) noteList.Remove(note);
        // note.transform.position = new Vector3(0, -999, 0);
        //}
    }
}
