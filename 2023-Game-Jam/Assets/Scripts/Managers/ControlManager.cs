using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Logging;
using NoteClasses;
using Sirenix.OdinInspector;
using UnityEngine;
using SO_Scripts;
using UnityEngine.InputSystem;
using EventType = Core.Events.EventType;
using NoteData = Data_Classes.NoteData;
using TouchPhase = UnityEngine.TouchPhase;

namespace Managers
{
    public class ControlManager : MonoBehaviour
    {
        private MidiData _midiData;
        private GameModeData _gameModeData;
        [Space]
        private List<NoteSlider> _currentHoldSliders = new List<NoteSlider>();

        private bool _isPlayerGrounded;
        private Camera _camera;
        private NoteData.LaneOrientation _mcLane;
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
            this.AddListener(EventType.ReceiveIsGrounded, param => _isPlayerGrounded = (bool) param);
            this.AddListener(EventType.ReceiveMCLane, param => _mcLane = (NoteData.LaneOrientation) param);
            
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
            
            // multiTouchInputActions.Add(playerInput.actions["Touch0"]);
            // multiTouchInputActions.Add(playerInput.actions["Touch1"]);
            // multiTouchInputActions.Add(playerInput.actions["Touch2"]);
            // multiTouchInputActions.Add(playerInput.actions["Touch3"]);
        }

        public Vector3 GetWorldPositionOnPlane(Vector3 screenPosition, float z) {
            Ray ray = Camera.main.ScreenPointToRay(screenPosition);
            Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
            float distance;
            xy.Raycast(ray, out distance);
            return ray.GetPoint(distance);
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
            
            // NCLogger.Log($"print out touching");
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                var ray = camera.ScreenPointToRay(touch.position);
                //Debug.DrawRay(camera.transform.position, ray.direction * 100, Color.green, 5f);
                //NCLogger.Log($"{touchPos}");
            }

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
                if (_currentHoldSliders.Count == 0) {
                    switch (laneCollider.LaneOrientation)
                    {
                        case NoteData.LaneOrientation.One:
                            this.FireEvent(EventType.RequestIsGrounded);
                            if(_isPlayerGrounded)
                                this.FireEvent(EventType.PlayerJump);
                            break;
                        case NoteData.LaneOrientation.Two:
                            this.FireEvent(EventType.PlayerAttackGround);
                            break;
                    }
                }
                else
                {
                    switch (laneCollider.LaneOrientation)
                    {
                        case NoteData.LaneOrientation.One:
                            this.FireEvent(EventType.SideCharacterAttackAir);
                            break;
                        case NoteData.LaneOrientation.Two:
                            this.FireEvent(EventType.SideCharacterAttackGround);
                            break;
                    }
                }
                
                return false;
            }

            switch (note.Type)
            {
                case NoteType.NormalNote:
                    var canRemove = ((note as NoteNormal)!).OnNoteHitNormalNote();
                    // if(canRemove) laneCollider.RemoveNote(note);
                    if (_currentHoldSliders.Count == 0)
                    {
                        switch (laneCollider.LaneOrientation)
                        {
                            case NoteData.LaneOrientation.One:
                                this.FireEvent(EventType.PlayerAttackAir);
                                break;
                            case NoteData.LaneOrientation.Two:
                                this.FireEvent(EventType.PlayerAttackGround);
                                break;
                        }
                    }
                    else
                    {
                        switch (laneCollider.LaneOrientation)
                        {
                            case NoteData.LaneOrientation.One:
                                this.FireEvent(EventType.SideCharacterAttackAir);
                                break;
                            case NoteData.LaneOrientation.Two:
                                this.FireEvent(EventType.SideCharacterAttackGround);
                                break;
                        }
                    }
                    break;
                case NoteType.SliderNote:
                    var slider = note as NoteSlider;
                    if (_currentHoldSliders.Contains(slider)) break;
                    if (!(slider!.OnNoteHitStartNote())) break;
                    
                    this.FireEvent(EventType.RequestMCLane);
                    if (_currentHoldSliders.Count == 0 || _mcLane == NoteData.LaneOrientation.Undefined)
                    {
                        switch (laneCollider.LaneOrientation)
                        {
                            case NoteData.LaneOrientation.One:
                                this.FireEvent(EventType.PlayerBlockAir);
                                break;
                            case NoteData.LaneOrientation.Two:
                                this.FireEvent(EventType.PlayerBlockGround);
                                break;
                        }
                    }
                    else
                    {
                        switch (laneCollider.LaneOrientation)
                        {
                            case NoteData.LaneOrientation.One:
                                this.FireEvent(EventType.SideCharacterBlockAir);
                                break;
                            case NoteData.LaneOrientation.Two:
                                this.FireEvent(EventType.SideCharacterBlockGround);
                                break;
                        }
                    }
                   
                    _currentHoldSliders.Add(slider);
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
            
            this.FireEvent(EventType.RequestMCLane);
            if (_mcLane == slider.noteOrientation)
            {
                switch (laneCollider.LaneOrientation)
                {
                    case NoteData.LaneOrientation.One:
                        this.FireEvent(EventType.PlayerAttackAir);
                        break;
                    case NoteData.LaneOrientation.Two:
                        this.FireEvent(EventType.PlayerAttackGround);
                        break;
                }
            }
            else
            {
                switch (laneCollider.LaneOrientation)
                {
                    case NoteData.LaneOrientation.One:
                        this.FireEvent(EventType.SideCharacterAttackAir);
                        break;
                    case NoteData.LaneOrientation.Two:
                        this.FireEvent(EventType.SideCharacterAttackGround);
                        break;
                }
            }
            
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
