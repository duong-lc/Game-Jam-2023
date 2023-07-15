using System;
using UnityEngine;
using SO_Scripts;
using Data_Classes;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Logging;
using Managers;
using DataClass = Data_Classes;
using NoteClasses;
using Sirenix.Utilities;
using EventType = Core.Events.EventType;

public class NoteInitData : PooledObjectCallbackData
{
    public int octave { get; private set; }
    public DataClass.NoteData.LaneOrientation orientation { get; private set; }
    public double timeStamp { get; private set; }
    public DataClass.NoteData.SliderData SliderData { get; private set; }

    public Lane lane { get; private set; }

    public NoteInitData(Lane lane, Vector3 position, Transform parent, int octave, 
        DataClass.NoteData.LaneOrientation orientation, double timeStamp) {
        // this.eventType = eventType;
        this.position = position;
        this.parent = parent;
        this.octave = octave;
        this.orientation = orientation;
        this.timeStamp = timeStamp;
        this.lane = lane;
        // this.spawnIndex = spawnIndex;
    }
    
    public NoteInitData(Lane lane, Vector3 position, Transform parent, int octave, 
        DataClass.NoteData.LaneOrientation orientation, DataClass.NoteData.SliderData data) {
        // this.eventType = eventType;
        this.position = position;
        this.parent = parent;
        this.octave = octave;
        this.orientation = orientation;
        this.SliderData = data;
        this.lane = lane;
        // this.spawnIndex = spawnIndex;
    }
}

public enum NoteColorEnum
{
    Red,
    Blue
}

public class Lane : MonoBehaviour
{
    // private SpriteRenderer _highlightSprite;
    private MidiData _midiData;
    private GameModeData _gameModeData;

    public List<BaseNoteType> allNotesList = new List<BaseNoteType>();

    public bool canSpawn = false;
    
    [Header("GameJamShiz")]
    public NoteColorEnum currentColorEnum;

    private int _spawnIndex = 0;//index spawn to loop through the timestamp array to spawn notes based on timestamp
    private int _inputIndex;//input index to loop through the timestamp array to form note input queue 
    [SerializeField] private DataClass.NoteData.LaneOrientation orientation;

    public DataClass.NoteData.LaneOrientation LaneOrientation => orientation;
    //Variables for caching

    private GameObject _normalNotePrefab;
    private GameObject _sliderNotePrefab;

    // private Vector3 _spawnLocation => new Vector3(_gameModeData.GetHitPoint(LaneOrientation).x, 0, _gameModeData.NoteSpawnZ);
    private Vector3 _spawnLocation => new Vector3(_gameModeData.NoteSpawnX, 0, _gameModeData.GetHitPoint(LaneOrientation).z);
    private double AudioTimeRaw => SongManager.Instance.GetAudioSourceTimeRaw();
    
    private LaneCollider _laneCol;
    public LaneCollider LaneCollider {
        get {
            if (!_laneCol) _laneCol = GetComponent<LaneCollider>();
            return _laneCol;
        }
    }

    private ObjectPool[] _notePoolArray;
    public ObjectPool[] NotePools {
        get {
            if (_notePoolArray.IsNullOrEmpty()) _notePoolArray = GetComponentsInChildren<ObjectPool>();
            return _notePoolArray;
        }
    }

    private void Awake()
    {
       // _spawnLocation = new Vector3(_gameModeData.GetHitPoint(LaneOrientation).x, 0, _gameModeData.NoteSpawnZ);
    }

    private void Start() {
        _midiData = GameModeManager.Instance.CurrentMidiData;
        _gameModeData = GameModeManager.Instance.GameModeData;
#if UNITY_EDITOR
        if(!_midiData) NCLogger.Log($"midiData is {_midiData}", LogLevel.ERROR);
        if(!_gameModeData) NCLogger.Log($"gameModeData is {_gameModeData}", LogLevel.ERROR);
#endif

        canSpawn = false;
        
        _normalNotePrefab = _midiData.noteNormalPrefab;
        _sliderNotePrefab = _midiData.noteSliderPrefab;
        // HighlightSprite.enabled = false;

        allNotesList = _midiData.laneMidiData[LaneOrientation].allNoteOnLaneList;
    }

    public void SetLocalListOnLane(List<BaseNoteType> listToSet)
    {
        // allNotesList.Clear();
        //NCLogger.Log($"{listToSet.Count}");
        // allNotesList = listToSet;
    }

    private void Update()
    {
        SpawningNotesFromList();
    }
    
    private void SpawningNotesFromList()
    {
        if (!canSpawn) return;
        if (allNotesList.Count <= 0)
        {
#if UNITY_EDITOR
            NCLogger.Log($"NO NOTES TO SPAWN {allNotesList.Count}", LogLevel.ERROR);
#endif
            return;
        }
        else
        {
            //print($"list size {allNoteOnLaneList.Count}");
        }

        switch (LaneOrientation)
        {
            case DataClass.NoteData.LaneOrientation.One:
                currentColorEnum = LaneManager.Instance.CurrentTimeColorSwapInfo().upperLaneColor;
                break;
            case DataClass.NoteData.LaneOrientation.Two:
                currentColorEnum = LaneManager.Instance.CurrentTimeColorSwapInfo().lowerLaneColor;
                break;
            default:
                throw new ArgumentOutOfRangeException();
                break;
        }
        
        switch (allNotesList[_spawnIndex].noteID)
        {
            case DataClass.NoteData.NoteID.NormalNote:
                NoteNormalType noteNormalCast = (NoteNormalType) allNotesList[_spawnIndex];

                //if current song time reaches point to spawn a note
                if (AudioTimeRaw >= noteNormalCast.timeStamp - _gameModeData.NoteTime)
                {
                    // switch (noteNormalCast.laneOrientation)
                    // {
                    //     case DataClass.NoteData.LaneOrientation.One:
                    //         this.FireEvent(EventType.SpawnNoteNormalAir, new NoteInitData(this, _spawnLocation,
                    //             transform, noteNormalCast.octaveNum,noteNormalCast.laneOrientation,noteNormalCast.timeStamp));
                    //         break;
                    //     case DataClass.NoteData.LaneOrientation.Two:
                    //         this.FireEvent(EventType.SpawnNoteNormalGround, new NoteInitData(this, _spawnLocation,
                    //             transform, noteNormalCast.octaveNum,noteNormalCast.laneOrientation,noteNormalCast.timeStamp));
                    //         break;
                    // }
                    this.FireEvent(EventType.SpawnNoteNormalGround, new NoteInitData(this, _spawnLocation,
                        transform, noteNormalCast.octaveNum,noteNormalCast.laneOrientation,noteNormalCast.timeStamp));
                    
                    
                    IncrementSpawnIndex();
                }

                break;
            case DataClass.NoteData.NoteID.SliderNote:
                NoteSliderType noteSliderCast = (NoteSliderType) allNotesList[_spawnIndex];

                if (AudioTimeRaw >= noteSliderCast.sliderData.timeStampKeyDown - _gameModeData.NoteTime)
                {
                    switch (currentColorEnum)
                    {
                        case NoteColorEnum.Red:
                            this.FireEvent(EventType.SpawnNoteSliderRed, new NoteInitData( this, _spawnLocation, 
                                transform, noteSliderCast.octaveNum,noteSliderCast.laneOrientation,noteSliderCast.sliderData));
                            break;
                        case NoteColorEnum.Blue:
                            this.FireEvent(EventType.SpawnNoteSliderBlue, new NoteInitData( this, _spawnLocation, 
                                transform, noteSliderCast.octaveNum,noteSliderCast.laneOrientation,noteSliderCast.sliderData));
                            break;
                    }
                    
                   IncrementSpawnIndex();
                }

                break;
        }
    }

    private void IncrementSpawnIndex()
    {
        //increment the index
        if(_spawnIndex + 1 <= allNotesList.Count - 1)
            _spawnIndex++;
        else
        {
            canSpawn = false;
            this.FireEvent(EventType.LaneFinishSpawningEvent);
        }
    }
    
}
