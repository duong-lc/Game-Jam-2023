using System;
using System.Collections;
using System.Threading.Tasks;
using Core.Events;
using Core.Logging;
using DG.Tweening;
using Managers;
using Unity.VisualScripting;
using UnityEngine;
using DataClasses = Data_Classes;
using EventType = Core.Events.EventType;
using Random = UnityEngine.Random;

namespace NoteClasses
{
    public class NoteSlider : NoteBase
    {
        [Header("Slider Note Attributes")] 
        private DataClasses.NoteData.SliderData _sliderData;

        [SerializeField] private GameObject startNote;
        [SerializeField] private GameObject endNote;

        private double _startNoteSpawnTime;
        private double _endNoteSpawnTime;

        [SerializeField] private Transform[] lineRendererPoints;
        private LineRendererController[] _lineControllers;

        [SerializeField] private Animator animatorStart;
        [SerializeField] private Animator animatorStartShadow;
        [SerializeField] private Animator animatorEnd;
        
        [Header("Animation Clips Slider")] 
        [SerializeField] protected string sliderRun;
        [SerializeField] protected string sliderBlock;
        [SerializeField] protected string sliderHit1;
        [SerializeField] protected string sliderHit2;
        
        //Booleans to check note hit reg status
        private bool _isStartNoteHitCorrect = false;//is pressed down on start note correctly?
        private bool _isHolding = false;            //is slider being held on correctly?
        private bool _isEndNoteHitCorrect = false;  //is released on end note correctly?
        private bool _canMoveEndNote = true;
        private bool _canMoveStartNote = true;
        
        
        //Start and End Position to lerp start note and end note of slider
        private Vector3 _startPosStartNote;
        private Vector3 _endPosStartNote;
        private Vector3 _startPosEndNote;
        private Vector3 _endPosEndNote;

        private Vector3 _sliderLockPoint;    //position to lock slider start note when holding down 
        
        //caching to lighten garbage collectors
        private double TimeSinceStartNoteSpawned => CurrentSongTimeRaw - _startNoteSpawnTime;
        private double TimeSinceEndNoteSpawned => CurrentSongTimeRaw - _endNoteSpawnTime;
        private float AlphaStart => (float)(TimeSinceStartNoteSpawned / (NoteTime * 2));
        private float  AlphaEnd => (float)(TimeSinceEndNoteSpawned / (NoteTime * 2));
        private bool _runOnce = true;
        private bool _runOnce1 = true;
        private KeyCode _holdKey;
        private bool denyInput = false;

        private DataClasses.NoteData.LaneOrientation _mcLane;
        protected void Awake()
        {
            base.Awake();
            this.AddListener(EventType.ReceiveMCLane, param => _mcLane = (DataClasses.NoteData.LaneOrientation) param);
            _lineControllers = GetComponentsInChildren<LineRendererController>();
        }
        
        protected override void Start()
        {
            base.Start();
        }

        public override void Init(PooledObjectCallbackData data, Action<PooledObjectBase> killAction)
        {
            //Re-arm values for slider
            denyInput = false;
            _isStartNoteHitCorrect = false;
            _canMoveStartNote = true;
            _canMoveEndNote = true;
            
            var noteData = (NoteInitData)data;
            octaveNum = noteData.octave;
            noteOrientation = noteData.orientation;
            _sliderData = noteData.SliderData;

            animatorStart.Play(sliderRun);
            animatorStartShadow.Play(sliderRun);
            
            transform.GetChild(0).GameObject().SetActive(true);
            transform.GetChild(1).GameObject().SetActive(true);
            SetUpVariables();
            ToggleLineRenderers(false);
            SetUpLineControllers();
            SetLookDir(_startPosStartNote, _endPosStartNote);

            KillAction = killAction;
            canRelease = false;
            StartCoroutine(RunRoutine());
            // Debug.Break();
        }
        
        private void Update()
        {
            //UpdateStartNoteHoldStatus();
            if (GameModeManager.Instance.CurrentGameState != GameState.PlayMode) {
#if UNITY_EDITOR
                NCLogger.Log($"GameState should be PlayMode when it's {GameModeManager.Instance.CurrentGameState}", LogLevel.ERROR);
#endif
                return;
            }
            if (_canMoveStartNote ) {
                InterpolateStartNotePos();
            }
            if (_canMoveEndNote ) {
                InterpolateEndNotePos();
            }

            UpdateStartNoteFail();
            UpdateEndNoteHoldStatus();
        }

        private void InterpolateStartNotePos()
        {
            if (AlphaStart > 1) 
            {
                //Destroy(startNote);
                // canRelease = true;
                KillSlider();
                // Knockback();
            }
            else if (startNote)
            {
                StartCoroutine(ActivateStartNote());
                startNote.transform.position = Vector3.Lerp(_startPosStartNote, _endPosStartNote, AlphaStart);
                animatorStartShadow.transform.position = startNote.transform.position;
            }
        }

        private void InterpolateEndNotePos()
        {
            //when the end note is spawnable/movable
            if(CurrentSongTimeRaw >= _endNoteSpawnTime)
            {
                if (!endNote) return; 
                //otherwise, the position of note will be lerped between the spawn position and despawn position based on the alpha
                if (Math.Abs(endNote.transform.position.x - _sliderLockPoint.x) > 0 && AlphaEnd < 0.5f)
                {
                    endNote.transform.position = Vector3.Lerp(_startPosEndNote, _endPosEndNote, AlphaEnd);
                    // StartCoroutine(ActivateEndNote());
                }
                else
                {
                    _canMoveEndNote = false;
                    endNote.transform.position = startNote.transform.position;
                }
            }
            else
            {
                endNote.transform.position = _startPosEndNote;
            }
        }

        /// <summary>
        /// Called when pressing hit button or smt
        /// </summary>
        public bool OnNoteHitStartNote()
        {
            if (denyInput) return false;
            if (_isStartNoteHitCorrect && _isHolding) return false;
            
            var cond = GetHitCondition(CurrentSongTimeAdjusted , _sliderData.timeStampKeyDown, ref noteHitEvent);
            if (cond != HitCondition.None && cond != HitCondition.Miss) {
                //Hit
                this.FireEvent(noteHitEvent,  new HitMarkInitData(this, cond, noteOrientation));
                this.FireEvent(EventType.SliderNoteHoldingEvent, noteOrientation);
                
                //Setting condition for endNote evaluation
                _isStartNoteHitCorrect = true;
                _isHolding = true;
                
                animatorStart.Play(sliderBlock);
                animatorStartShadow.Play(sliderBlock);
                
                _canMoveStartNote = false;
                startNote.transform.position = _sliderLockPoint;
                return true;
            }

            return false;
        }

        private void UpdateStartNoteFail()//put in update
        {
            if (canRelease) return;
            if (_isHolding && _isStartNoteHitCorrect) return;
            //Doesn't press, let start note passes
            var cond = GetHitCondition(CurrentSongTimeAdjusted , _sliderData.timeStampKeyDown, ref noteHitEvent);
            if (cond == HitCondition.Miss && !_isStartNoteHitCorrect) {
                EventDispatcher.Instance.FireEvent(noteHitEvent, new HitMarkInitData(this, cond, noteOrientation));
                // Destroy(gameObject);
                // canRelease = true;
                KillSlider();
                // Knockback();
            }
        }

        private void UpdateStartNoteHoldStatus()//put in update
        {
            if (_isStartNoteHitCorrect && Input.GetKeyUp(_holdKey))
            {
                _isHolding = false;
#if UNITY_EDITOR
                NCLogger.Log($"up!!");
#endif
            }
        }

        public bool OnNoteHitEndNote()//when release key
        {
            if (denyInput) return false;
            bool isDestroy = false;
            if (!Input.GetKey(_holdKey) && _isStartNoteHitCorrect && _isHolding)
            {
                _isHolding = false;
                var cond = GetHitCondition(CurrentSongTimeAdjusted , _sliderData.timeStampKeyUp, ref noteHitEvent);
                if (cond != HitCondition.None && cond != HitCondition.Miss)
                {
                    // NCLogger.Log($"release the slider NOT miss");
                    //Hit
                    _isEndNoteHitCorrect = true;

                    isDestroy = true;
                    EventDispatcher.Instance.FireEvent(EventType.RemoveSliderFromHoldListEvent, this);
                    EventDispatcher.Instance.FireEvent(noteHitEvent,  new HitMarkInitData(this, cond, noteOrientation));
                    denyInput = true;
                    // canRelease = true;
                    Knockback();
                }
                else if (cond == HitCondition.Miss || cond == HitCondition.None)
                {
                    // NCLogger.Log($"release the slider MISS");
                    isDestroy = true;
                    //release too early
                    //miss
                    EventDispatcher.Instance.FireEvent(EventType.RemoveSliderFromHoldListEvent, this);
                    EventDispatcher.Instance.FireEvent(EventType.NoteMissEvent, new HitMarkInitData(this, HitCondition.Miss, noteOrientation));
                    denyInput = true;
                    // canRelease = true;
                    KillSlider();
                    // Knockback();
                }
            }

            return isDestroy;
        }

        private void UpdateEndNoteHoldStatus()
        {
            if (canRelease) return;
            if (_isStartNoteHitCorrect && _isHolding)
            {
                //release too late <- will probably throw away as this game mode does not account for late releases.
                if (_sliderData.timeStampKeyUp + _gameModeData.SliderHoldStickyTime <= CurrentSongTimeAdjusted) {
                    //hit - since passes the end note, auto hit
                    this.FireEvent(EventType.NoteHitLateEvent,  new HitMarkInitData(this, HitCondition.Late, noteOrientation));
                    this.FireEvent(EventType.RemoveSliderFromHoldListEvent, this);
                    this.FireEvent(EventType.RequestMCLane);
                    if (_mcLane == noteOrientation)
                    {
                        switch (noteOrientation) {
                            case DataClasses.NoteData.LaneOrientation.One:
                                this.FireEvent(EventType.PlayerAttackAir);
                                break;
                            case DataClasses.NoteData.LaneOrientation.Two:
                                this.FireEvent(EventType.PlayerAttackGround);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    } else {
                        switch (noteOrientation) {
                            case DataClasses.NoteData.LaneOrientation.One:
                                this.FireEvent(EventType.SideCharacterAttackAir);
                                break;
                            case DataClasses.NoteData.LaneOrientation.Two:
                                this.FireEvent(EventType.SideCharacterAttackGround);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                   
                    //Destroy(gameObject);
                    KillSlider();
                    // canRelease = true;
                    // Knockback();
                }
            }
        }

        protected override void Knockback()
        {
            denyInput = true;
            _canMoveEndNote = false;
            _canMoveStartNote = false;
            Collider.enabled = false;
            // var seq = DOTween.Sequence();
            // seq.AppendCallback(() => transform.position = new Vector3(999, 999, 999));
            // seq.InsertCallback(.1f, () => canRelease = true);
            
            var num = Random.Range(1, 10);
            string clip = num % 2 == 0 ? sliderHit1 : sliderHit2;
            animatorStart.Play(clip);
            animatorStartShadow.Play(clip);
            
            var sequence = DOTween.Sequence();
            sequence
                .Insert(0, transform.DOMove(transform.position + knockVector, knockTime))
                .Insert(0, transform.DORotate(knockRotate, knockTime))
                .Insert(knockTime, transform.DOMove(transform.position + knockVector, stallTime))
                .Insert(knockTime + stallTime, transform.DOMove(transform.position + fallVector, fallTime).SetEase(Ease.InSine))
                .Insert(knockTime + stallTime, transform.DORotate(fallRotate, fallTime))
                .InsertCallback(knockTime + stallTime + fallTime, () => canRelease = true);
        }
        
        public void KillSlider()
        {
            // canRelease = true;
            denyInput = true;
            _canMoveEndNote = false;
            _canMoveStartNote = false;
            Collider.enabled = false;
            // NCLogger.Log($"KILL LSIDER");
            var seq = DOTween.Sequence();
            seq.AppendCallback(() => transform.position = new Vector3(999, 999, 999));
            seq.InsertCallback(.1f, () => canRelease = true);
        }

        #region Initializer Methods
        private IEnumerator ActivateStartNote()
        {
            if (!_runOnce) yield break;
            yield return null;
            startNote.SetActive(true);
            foreach (var line in _lineControllers)
                line.gameObject.SetActive(true);
            _runOnce = false;
        }

        private IEnumerator ActivateEndNote()
        {
            if (!_runOnce1)  yield break;
            yield return null;
            endNote.SetActive(false);
            _runOnce1 = false;
        }
        
        private void SetUpLineControllers()
        {
            foreach (var line in _lineControllers) {
                line.SetUpLine(lineRendererPoints);
            }
        }

        private void ToggleLineRenderers(bool status)
        {
            foreach (var line in _lineControllers)
            {
                line.gameObject.SetActive(status);
            }
        }

        private void SetUpVariables()
        {
            //setting up spawn time stamps
            _startNoteSpawnTime = _sliderData.timeStampKeyDown - NoteTime;
            _endNoteSpawnTime = _sliderData.timeStampKeyUp - NoteTime;

            GameModeManager.Instance.GameModeData.GetLerpPoints(noteOrientation, ref _startPosStartNote, ref _endPosStartNote);
            _sliderLockPoint = GameModeManager.Instance.GameModeData.GetHitPoint(noteOrientation);
            
            _startPosEndNote = _startPosStartNote;
            _endPosEndNote = _endPosStartNote;
            _runOnce = _runOnce1 = true;
            _holdKey = GameModeManager.Instance.GameModeData.GetKeyCode(noteOrientation);
        }

        public void InitializeDataOnSpawn(ref int octave, ref DataClasses.NoteData.LaneOrientation orientation, ref DataClasses.NoteData.SliderData sliderData)
        {
            octaveNum = octave;
            noteOrientation = orientation;
            _sliderData = sliderData;
        }
        
        #endregion
    }
}