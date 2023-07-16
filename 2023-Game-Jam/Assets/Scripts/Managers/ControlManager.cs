using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Logging;
using Core.Patterns;
using DG.Tweening;
using Lean.Common;
using NoteClasses;
using Sirenix.OdinInspector;
using UnityEngine;
using SO_Scripts;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using EventType = Core.Events.EventType;
using NoteData = Data_Classes.NoteData;
using TouchPhase = UnityEngine.TouchPhase;

namespace Managers
{
    public class ControlManager : Singleton<ControlManager>
    {
        private MidiData _midiData;
        private GameModeData _gameModeData;
        [Space]
        private List<NoteSlider> _currentHoldSliders = new List<NoteSlider>();

        private bool _isPlayerGrounded;
        private Camera _camera;
        private NoteData.LaneOrientation _mcLane;

        [Header("Game Jam")]
        public Transform upperLaneT;
        public Transform lowerLaneT;
        public Transform upperLane_UI_T;
        public Transform lowerLane_UI_T;
        public float swapTime;
        public List<CharacterController> players;

        [Space] 
        public PlayerUI redPlayerUI;
        public PlayerUI bluePlayerUI;
        [SerializeField] private float addPercentage = 0.05f;
        private Camera camera
        {
            get
            {
                if(_camera != null) return _camera;
                _camera = Camera.main;
                return _camera;
            }
        }

        private void Awake()
        {
            EventDispatcher.Instance.AddListener(EventType.UnPauseEvent, param => CheckHoldStatus());
            EventDispatcher.Instance.AddListener(EventType.RemoveSliderFromHoldListEvent, param => RemoveSliderFromList((NoteSlider) param));
            // this.AddListener(EventType.ReceiveIsGrounded, param => _isPlayerGrounded = (bool) param);
            // this.AddListener(EventType.ReceiveMCLane, param => _mcLane = (NoteData.LaneOrientation) param);
            
            // if(!playerInput) NCLogger.Log($"playerInput is not assigned", LogLevel.ERROR);
        }

        private void Start()
        {
            _midiData = GameModeManager.Instance.CurrentMidiData;
            _gameModeData = GameModeManager.Instance.GameModeData;
#if UNITY_EDITOR
            if(!_midiData) NCLogger.Log($"midiData is {_midiData}", LogLevel.ERROR);
            if(!_gameModeData) NCLogger.Log($"midiData is {_gameModeData}", LogLevel.ERROR);
#endif

            foreach (var player in players)
            {
                if (player.Orientation == NoteData.LaneOrientation.One)
                    player.transform.position = upperLaneT.position;
                else
                    player.transform.position = lowerLaneT.position;
            }
            UpdatePlayerUIColor();
            redPlayerUI.UpdateUIPosition(upperLane_UI_T.position);
            bluePlayerUI.UpdateUIPosition(lowerLane_UI_T.position);
        }

        public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z) {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
            float distance;
            xy.Raycast(ray, out distance);
            return ray.GetPoint(distance);
        }

        public void AddPlayerScore(NoteData.LaneOrientation laneOrientation)
        {
            foreach (var player in players)
            {
                if (player.Orientation == laneOrientation)
                {
                    // player.playerAtkPoint++;
                    player.sliderValue += addPercentage;
                    // attackSlider.DOValue(_currentAtkPoint, .2f);
                    this.FireEvent(EventType.PlayerUIUpdate, player);
                    return;
                }
            }
        }

        public void ResetPlayerScore(NoteData.LaneOrientation laneOrientation)
        {
            foreach (var player in players)
            {
                if (player.Orientation == laneOrientation)
                {
                    player.sliderValue = 0;
                    this.FireEvent(EventType.PlayerUIReset, player);
                    return;
                }
            }
        }
        
        private void UpdatePlayerUIColor()
        {
            foreach (var player in players)
            {
                if (player.Orientation == NoteData.LaneOrientation.One && player.playerColorEnum == NoteColorEnum.Red)
                {
                   redPlayerUI.UpdateUIPosition(upperLane_UI_T.position);
                   bluePlayerUI.UpdateUIPosition(lowerLane_UI_T.position);
                }
                else
                {
                    bluePlayerUI.UpdateUIPosition(upperLane_UI_T.position);
                    redPlayerUI.UpdateUIPosition(lowerLane_UI_T.position);
                }
            }
        }
        
        // Update is called once per frame
        private void Update()
        {
            if (GameModeManager.Instance.CurrentGameState != GameState.PlayMode) return;
            foreach (var kvp in _gameModeData.LaneControllerData)
            {
                if (Input.GetKeyDown(kvp.Value.Input)) {
                    
                    if(!NoteInteractInputDown(kvp.Value.collider)) continue;
                }
            
                if (Input.GetKeyUp(kvp.Value.Input)) {
                    
                    if(!NoteInteractInputUp(kvp.Value.collider)) continue;
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                CharacterSwap();
            }


            // // NCLogger.Log($"print out touching");
            // if (Input.touchCount > 0)
            // {
            //     Touch touch = Input.GetTouch(0);
            //     var ray = camera.ScreenPointToRay(touch.position);
            //     //Debug.DrawRay(camera.transform.position, ray.direction * 100, Color.green, 5f);
            //     //NCLogger.Log($"{touchPos}");
            // }

#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                foreach (Touch t in Input.touches)
                {
                    var ray = camera.ScreenPointToRay(Input.GetTouch(t.fingerId).position);
                    if (Input.GetTouch(t.fingerId).phase == TouchPhase.Began)
                    {
                        
                        NCLogger.Log($"print out touching");
                        var hits = Physics.RaycastAll(ray, Mathf.Infinity);
                        foreach (var hit in hits)
                        {
                            foreach (var data in _gameModeData.LaneControllerData.Values)
                            {
                                if (data.collider.Collider == hit.collider)
                                {
                                    NCLogger.Log($"print out touching");
                                    // data.collider.Lane.HighlightSprite.enabled = true;
                                    if(!NoteInteractInputDown(data.collider)) continue;
                                }
                            }
                        }
                    }
                    if (Input.GetTouch(t.fingerId).phase == TouchPhase.Ended)
                    {
                        var hits = Physics.RaycastAll(ray, Mathf.Infinity);
                        foreach (var hit in hits)
                        {
                            foreach (var data in _gameModeData.LaneControllerData.Values)
                            {
                                if (data.collider.Collider == hit.collider)
                                {
                                    // data.collider.Lane.HighlightSprite.enabled = false;
                                    if(!NoteInteractInputUp(data.collider)) continue;
                                }
                            }
                        }
                    }
                }
            }
#endif 
        }

        private CharacterController GetCharacterOnLane(NoteData.LaneOrientation laneOrientation)
        {
            foreach (var player in players)
            {
                if (player.Orientation == laneOrientation)
                    return player;
            }

            return null;
        }
        
        private void CharacterSwap()
        {
            if (players.Any(player => player.holdBusy)) {
                return;
            }
            
            foreach (var player in players)
            {
                player.KillIdle();
                if (player.Orientation == NoteData.LaneOrientation.One)
                {
                    var tween = player.transform.DOMove(lowerLaneT.position, swapTime);
                    tween.OnComplete(() => player.StartIdle());
                    player.Orientation = NoteData.LaneOrientation.Two;
                }
                else
                {
                    var tween1 = player.transform.DOMove(upperLaneT.position, swapTime);
                    tween1.OnComplete(() => player.StartIdle());
                    player.Orientation = NoteData.LaneOrientation.One;
                }
            }

            UpdatePlayerUIColor();
        }
        
        private bool NoteInteractInputDown(LaneCollider laneCollider)
        {
     
            //TODO: Instead of getting component, get the lane based on the keyEntry.
            //In said lane, there will be a List that store all Active Notes in scene  -List<NoteBase>
            //use linq to compare hit.object with notebase.object in list. If matches, return notebase with
            //game object tag, then use switch to cast to normal or slider. 
            //the purpose is to have less performance intensive code but haven't really measured - just a theory.
            var note = laneCollider.GetApproachingNote();
            if (!note)
            {
                this.FireEvent(EventType.PlayerAttack, laneCollider.LaneOrientation);
                return false;
            }

            switch (note.Type)
            {
                case NoteType.NormalNote:
                    var noteCasted = (note as NoteNormal);
                    if (!noteCasted) return false;

                    var characterOnLane = GetCharacterOnLane(laneCollider.LaneOrientation);
                    var isMatched = characterOnLane.playerColorEnum == noteCasted.colorEnum;

                    this.FireEvent(EventType.PlayerAttack, laneCollider.LaneOrientation);
                    if (isMatched) {
                        var canRemove = noteCasted.OnNoteHitNormalNote();
                    }
                    break;
                case NoteType.SliderNote:
                    var slider = note as NoteSlider;
                    if (!slider) return false;
                    
                    var characterOnLaneSlider = GetCharacterOnLane(laneCollider.LaneOrientation);
                    var isMatchedSlider = characterOnLaneSlider.playerColorEnum == slider.colorEnum;

                    if (_currentHoldSliders.Contains(slider)) break;
                    
                    if (isMatchedSlider)
                    {
                        if (!(slider.OnNoteHitStartNote())) break;
                        this.FireEvent(EventType.PlayerBlock, laneCollider.LaneOrientation);
                        _currentHoldSliders.Add(slider);
                    }
                    else
                    {
                        this.FireEvent(EventType.PlayerAttack, laneCollider.LaneOrientation);
                    }
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        private bool NoteInteractInputUp(LaneCollider laneCollider)
        {
            var slider = _currentHoldSliders.Find(x => x.noteOrientation == laneCollider.LaneOrientation);

            if (!slider) {
//                NCLogger.Log($"Slider not found", LogLevel.WARNING);
                _currentHoldSliders.Remove(slider);
                return false;
            }
            
            this.FireEvent(EventType.PlayerEndBlock, laneCollider.LaneOrientation);
            
            slider.OnNoteHitEndNote();
            _currentHoldSliders.Remove(slider);
            
            return true;
        }
        
        private void RemoveSliderFromList(NoteSlider slider)
        {
            this.FireEvent(EventType.SliderNoteReleaseEvent, slider.noteOrientation);
            StartCoroutine(DelayedRemoveSliderRoutine(slider));
        }

        /// <summary>
        /// Delay removing the slider note from holding list by 1 frame due to
        /// CheckHoldStatus() still iterating through the list.
        /// </summary>
        /// <param name="slider"></param>
        /// <returns></returns>
        private IEnumerator DelayedRemoveSliderRoutine(NoteSlider slider)
        {
            yield return null;
            _currentHoldSliders.Remove(slider);
            // Destroy((slider.gameObject));
            // slider.KillSlider();
        }

        private void CheckHoldStatus()
        {
            foreach (var slider in _currentHoldSliders) {
                slider.OnNoteHitEndNote();
            }
        }
    }
}
