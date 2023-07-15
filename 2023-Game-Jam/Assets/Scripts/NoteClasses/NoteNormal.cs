using System;
using System.Collections;
using Core.Events;
using Core.Logging;
using Managers;
using UnityEngine;
using Data_Classes;
using DG.Tweening;
using Melanchall.DryWetMidi.MusicTheory;
using EventType = Core.Events.EventType;
using Random = UnityEngine.Random;

namespace NoteClasses
{
    public class NoteNormal : NoteBase
    {
        [Header("Default Note Attributes")]
        private double _timeInstantiated; //time to instantiate the note
        public double assignedTime;//the time the note needs to be tapped by the player

        private Vector3 _startPos;
        private Vector3 _endPos;

        private double TimeSinceInstantiated => CurrentSongTimeRaw - _timeInstantiated;
        private float Alpha => ((float)(TimeSinceInstantiated / (NoteTime * 2)));
        private HitCondition hitCond;
        
        [SerializeField] private Animator animatorMain;
        [SerializeField] private Animator animatorShadow;
        
        private bool _doOnce;
        protected void Awake()
        {
            
        }
        
        protected override void Start()
        {
            base.Start();
            // CanMove = true;
            // Collider.enabled = true;
        }

       

        public override void Init(PooledObjectCallbackData data, Action<PooledObjectBase> killAction)
        {
            var noteData = (NoteInitData)data;
            octaveNum = noteData.octave;
            noteOrientation = noteData.orientation;
            assignedTime = noteData.timeStamp;
            ownerLane = noteData.lane;
            switch (ownerLane.currentColorEnum)
            {
                case NoteColorEnum.Blue:
                    animatorMain.gameObject.GetComponent<SpriteRenderer>().color = LaneManager.Instance.colorBlue;
                    this.colorEnum = NoteColorEnum.Blue;
                    break;
                case NoteColorEnum.Red:
                    animatorMain.gameObject.GetComponent<SpriteRenderer>().color = LaneManager.Instance.colorRed;
                    this.colorEnum = NoteColorEnum.Red;
                    break;
            }
            
            SetUpVariables();
            SetLookDir(_startPos, _endPos);

            
            
            // switch (noteData.orientation)
            // {
            //     case NoteData.LaneOrientation.One:
            //         animatorMain.Play(airRun);
            //         animatorShadow.Play(airRun);
            //         break;
            //     case NoteData.LaneOrientation.Two:
            //         animatorMain.Play(groundRun);
            //         animatorShadow.Play(groundRun);
            //         break;
            // }
            // NCLogger.Log($"orientation: {noteOrientation}");
            
            KillAction = killAction;
            canRelease = false;
            CanMove = true;
            Collider.enabled = true;
            _doOnce = true;

            ToggleChildren(true);
            StartCoroutine(RunRoutine());
        }
        
        public override IEnumerator RunRoutine()
        {
            while (!canRelease) {
                if (NoteTime - TimeSinceInstantiated <= 1/* && _doOnce*/)
                {
                    ;
                }
                
                yield return null;
            }
            
            KillAction(this);
            yield return null;
        }
        
        
        private void Update()
        {
            // NCLogger.Log($"current note hit cond {hitCond} ");
            
            if (GameModeManager.Instance.CurrentGameState != GameState.PlayMode) {
                // NCLogger.Log($"GameState should be PlayMode when it's {GameModeManager.Instance.CurrentGameState}", LogLevel.ERROR);
                return;
            }
            OnNoteMissNormalNote();
            
            if (!CanMove) return;
            InterpolateNotePos();
        }

        private void InterpolateNotePos()
        {
            if(Alpha <= 1f)//otherwise, the position of note will be lerped between the spawn position and despawn position based on the alpha
            {
                transform.position = Vector3.Lerp(_startPos, _endPos, Alpha);
            }
            else
            {
#if UNITY_EDITOR
                NCLogger.Log($"go pass earth bound");
#endif
                //Destroy(gameObject);
                canRelease = true;
            }
        }


        public bool OnNoteHitNormalNote() {
            hitCond = GetHitCondition(CurrentSongTimeAdjusted , assignedTime, ref noteHitEvent);
            if (hitCond != HitCondition.None && hitCond != HitCondition.Miss) {
                this.FireEvent(noteHitEvent, new HitMarkInitData(this, hitCond, noteOrientation));
                // NCLogger.Log($"hit the mf wall");
                // Destroy(gameObject);
                Knockback();
                // canRelease = true;
                return true;
            }
            return false;
        }

        public void OnNoteMissNormalNote() {
            if (canRelease) return;
            if (!CanMove) return;
            
            hitCond = GetHitCondition(CurrentSongTimeAdjusted , assignedTime, ref noteHitEvent);
            if (hitCond == HitCondition.Miss) {
                EventDispatcher.Instance.FireEvent(noteHitEvent,  new HitMarkInitData(this, hitCond, noteOrientation));
                canRelease = true;
            }
        }
        
        
        
        public void InitializeDataOnSpawn(ref int octave, ref NoteData.LaneOrientation laneOrientation, ref double timeStamp)
        {
            octaveNum = octave;
            noteOrientation = laneOrientation;//pass the orientation property
            assignedTime = timeStamp;//get the time the note should be tapped by player and add to the array
        }

        private void SetUpVariables()
        {
            _timeInstantiated = SongManager.Instance.GetAudioSourceTimeRaw();
            GameModeManager.Instance.GameModeData.GetLerpPoints(noteOrientation, ref _startPos, ref _endPos);
            CanMove = true;
        }

        protected override void Knockback()
        {
            Collider.enabled = false;
            CanMove = false;
            var knock1 = noteOrientation == NoteData.LaneOrientation.One ? airHit1 : groundHit1;
            var knock2 = noteOrientation == NoteData.LaneOrientation.One ? airHit2 : groundHit2;
            var num = Random.Range(1, 10);
            string clip = num % 2 == 0 ? knock1 : knock2;

            animatorMain.Play(clip);
            animatorShadow.Play(clip);
            
            Sequence sequence = DOTween.Sequence();
            sequence
                .Insert(0, transform.DOMove(transform.position + knockVector, knockTime))
                .Insert(0, transform.DORotate(knockRotate, knockTime))
                .Insert(knockTime, transform.DOMove(transform.position + knockVector, stallTime))
                .Insert(knockTime + stallTime, transform.DOMove(transform.position + fallVector, fallTime).SetEase(Ease.InSine))
                .Insert(knockTime + stallTime, transform.DORotate(fallRotate, fallTime))
                .InsertCallback(knockTime + stallTime + fallTime, () => canRelease = true);
        }
    }
}