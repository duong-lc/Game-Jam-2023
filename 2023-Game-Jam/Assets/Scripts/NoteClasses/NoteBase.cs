using UnityEngine;
using SO_Scripts;
using System;
using Core.Events;
using Core.Logging;
using Managers;
using EventType = Core.Events.EventType;

public enum NoteType
{
    Undefined,
    NormalNote,
    SliderNote,
}


namespace NoteClasses
{
    [RequireComponent(typeof(Collider))]
    public abstract class NoteBase : PooledObjectBase
    {
        [Header("Base Note Attributes")] 
        protected bool CanMove = true;

        [SerializeField] protected NoteType type;
        
        protected static double CurrentSongTimeAdjusted => SongManager.Instance.GetAudioSourceTimeAdjusted();
        protected static double CurrentSongTimeRaw => SongManager.Instance.GetAudioSourceTimeRaw();
        
        // [SerializeField] protected MidiData midiData;
        [SerializeField] protected SO_Scripts.NoteData noteData;
        public int octaveNum;
        public Data_Classes.NoteData.LaneOrientation noteOrientation;
        public Lane ownerLane;
        public NoteColorEnum colorEnum;
        
        [Header("Knock Back Attributes")] 
        [SerializeField] protected Vector3 knockVector;
        [SerializeField] protected Vector3 knockRotate;
        [SerializeField] protected float knockTime;
        [Space]
        [SerializeField] protected float stallTime;
        [Space]
        [SerializeField] protected Vector3 fallVector;
        [SerializeField] protected Vector3 fallRotate;
        [SerializeField] protected float fallTime;

        [Header("Animation Clips")] 
        [SerializeField] protected string groundRun;
        [SerializeField] protected string groundHit1;
        [SerializeField] protected string groundHit2;
        [Space]
        [SerializeField] protected string airRun;
        [SerializeField] protected string airHit1;
        [SerializeField] protected string airHit2;
        
        private Collider _collider;
        protected EventType noteHitEvent = EventType.NoteHitNoneEvent;
        protected GameModeData _gameModeData;
        protected static float NoteTime => GameModeManager.Instance.GameModeData.NoteTime;
        public Collider Collider {
            get {
                if (!_collider) _collider = GetComponent<Collider>();
                return _collider;
            }
        }
        

        public NoteType Type => type;
        
        protected void Awake()
        {
        }

        protected virtual void ToggleChildren(bool state)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(state);
            }
        }
        
        protected virtual void SetLookDir(Vector3 startPos, Vector3 endPos)
        {
            transform.forward = (endPos - startPos).normalized;
        }

        protected virtual void Start()
        {
            base.Start();
            _gameModeData = GameModeManager.Instance.GameModeData;
        }
        
        public HitCondition GetHitCondition(double songTime, double assignedTime, ref EventType eventType)
        {
            //hitOffset = CurrentSongTimeAdjusted - AssignedTime
            var hitOffset = songTime - assignedTime;
            foreach (var kvp in _gameModeData.NoteHitCondDict) {
                var margin = kvp.Value;
                var offset = Mathf.Abs((float)hitOffset);
                // NCLogger.Log($"hitOffset at Time {Time.time} is {hitOffset}");
                switch (hitOffset) {
                    case < 0:
                        switch (kvp.Key) {
                            case HitCondition.None:
                                if (offset > Mathf.Abs(_gameModeData.GetMOE(HitCondition.Early).EndMOE)) {
                                    // NCLogger.Log($"current {CurrentSongTimeAdjusted} assigned {assignedTime} result {CurrentSongTimeAdjusted - assignedTime} hit cond {kvp.Key}");
                                    eventType = EventType.NoteHitNoneEvent;
                                    return HitCondition.None;
                                }
                                break;
                            case HitCondition.EarlyPerfect:
                                if (offset >= Mathf.Abs(margin.BeginMOE) && offset < Mathf.Abs(margin.EndMOE)) {
                                    // NCLogger.Log($"current {CurrentSongTimeAdjusted} assigned {assignedTime} result {CurrentSongTimeAdjusted - assignedTime} hit cond {kvp.Key}");
                                    eventType = EventType.NoteHitPerfectEvent;
                                    return HitCondition.EarlyPerfect;
                                }
                                break;
                            case HitCondition.Early:
                                if (offset >= Mathf.Abs(margin.BeginMOE) && offset < Mathf.Abs(margin.EndMOE)) {
                                    // NCLogger.Log($"current {CurrentSongTimeAdjusted} assigned {assignedTime} result {CurrentSongTimeAdjusted - assignedTime} hit cond {kvp.Key}");
                                    eventType = EventType.NoteHitEarlyEvent;
                                    return HitCondition.Early;
                                }
                                break;
                            default:
                                break;
                                //throw new ArgumentOutOfRangeException();
                        }
                        break;
                    case > 0:
                        switch (kvp.Key) {
                            case HitCondition.LatePerfect:
                                if (offset >= Mathf.Abs(margin.BeginMOE) && offset < Mathf.Abs(margin.EndMOE)) {
                                    // NCLogger.Log($"current {CurrentSongTimeAdjusted} assigned {assignedTime} result {CurrentSongTimeAdjusted - assignedTime} hit cond {kvp.Key}");
                                    eventType = EventType.NoteHitPerfectEvent;
                                    return HitCondition.LatePerfect;
                                }
                                break;
                            case HitCondition.Late:
                                if (offset >= Mathf.Abs(margin.BeginMOE) && offset < Mathf.Abs(margin.EndMOE)) {
                                    // NCLogger.Log($"current {CurrentSongTimeAdjusted} assigned {assignedTime} result {CurrentSongTimeAdjusted - assignedTime} hit cond {kvp.Key}");
                                    eventType = EventType.NoteHitLateEvent;
                                    return HitCondition.Late;
                                }
                                break;
                            case HitCondition.Miss:
                                if (offset > Mathf.Abs(_gameModeData.GetMOE(HitCondition.Late).EndMOE)) {
                                    // NCLogger.Log($"offset {hitOffset} > {Mathf.Abs(_gameModeData.GetMOE(HitCondition.Late).EndMOE)} is {offset > Mathf.Abs(_gameModeData.GetMOE(HitCondition.Late).EndMOE)}");
                                    eventType = EventType.NoteMissEvent;
                                    return HitCondition.Miss;
                                }
                                break;
                            default:
                                break;
                                // throw new ArgumentOutOfRangeException();
                        }
                        break;
                }
            }
            // NCLogger.Log($"current {CurrentSongTimeAdjusted} assigned {assignedTime} result {CurrentSongTimeAdjusted - assignedTime} hit cond {HitCondition.None}");
            eventType = EventType.NoteHitNoneEvent;
            return HitCondition.None;
        }

        protected virtual void Knockback()
        {
            
        }
    }
}