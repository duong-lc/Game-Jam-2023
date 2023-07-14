using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Core.Logging;
using Data_Classes;
using Sirenix.OdinInspector;
using UnityEngine;


[Serializable]
public class LaneControllerData
{
    [SerializeField] private KeyCode input;
    [SerializeField] private Vector3 hitPoint;
    [ReadOnly] [SerializeField] public LaneCollider collider;
    
    public KeyCode Input => input;
    public Vector3 HitPoint => hitPoint;
}

[CreateAssetMenu(fileName = "GameModeData", menuName = "ScriptableObjects/GameModeData", order = 0)]
public class GameModeData : SerializedScriptableObject
{
    [TitleGroup("Settings")]
    [Range(0,1)] public float volume;
    [Range(0, 1)] public float hitVolume;
    
    [TitleGroup("Scene Data")]
    public string mainMenuSceneName;
    public string levelSelectionSceneName;
    public string gamePlaySceneName;
    public string optionSceneName;

    [TitleGroup("Gameplay Data")] 
    [SerializeField] private int inputDelayInMS; //it's the issue with the keyboard and we need to have input delay 
    [SerializeField] private float songDelayInSeconds;
    [SerializeField] private float noteTime; //how much time the note is going to be on the screen
    [SerializeField] private float noteSpawnX; //the Z position for the note to be spawned at
    [SerializeField] private float noteTapX; //the Z position where the player should press the note
    public float NoteDespawnX => noteTapX - (noteSpawnX - noteTapX); //De-spawn position for notes
    public LayerMask noteLayerMask;
    [TitleGroup("Margin Of Error")]
    [SerializeField] private float _sliderHoldStickyTime;
    [SerializeField] private Dictionary<HitCondition, MarginOfError> noteHitCondDict = new();
    public float SliderHoldStickyTime => _sliderHoldStickyTime;
    
    [TitleGroup("Lane Controller Data")] 
    [SerializeField] private Dictionary<NoteData.LaneOrientation, LaneControllerData> laneControllerData = new();

    [TitleGroup("Hit Condition Data")]
    [SerializeField] private Dictionary<HitCondition, ScoreData> hitCondToScoreData = new();

    [TitleGroup("Note Data")]
    [SerializeField] private Dictionary<NoteType, string> typeToTag = new();

    public float InputDelayInMS => inputDelayInMS;
    public float SongDelayInSeconds => songDelayInSeconds;
    public float NoteTime => noteTime;
    public float NoteSpawnX => noteSpawnX;
    public float NoteTapX => noteTapX;

    #region Getters

    public ReadOnlyDictionary<HitCondition, MarginOfError> NoteHitCondDict => new(noteHitCondDict);

    public ReadOnlyDictionary<NoteData.LaneOrientation, LaneControllerData> LaneControllerData => new(laneControllerData);
    public ReadOnlyDictionary<HitCondition, ScoreData> HitCondToScoreData => new(hitCondToScoreData);

    public ReadOnlyDictionary<NoteType, string> TypeToTag => new (typeToTag);


    #endregion

    public ScoreData GetScoreData(HitCondition cond)
    {
        if (hitCondToScoreData.TryGetValue(cond, out ScoreData scoreData)) return scoreData;
#if UNITY_EDITOR
        NCLogger.Log($"Hit Condition: {cond} not found");
#endif
        return null;
    }

    public MarginOfError GetMOE (HitCondition cond) {
        if (noteHitCondDict.TryGetValue(cond, out MarginOfError MOE)) return MOE;
#if UNITY_EDITOR
        NCLogger.Log($"Hit Condition: {cond} not found");
#endif
        return null;
    }

    public Vector3 GetHitPoint(NoteData.LaneOrientation orientation) {
        if (laneControllerData.TryGetValue(orientation, out LaneControllerData data)) return data.HitPoint;
#if UNITY_EDITOR
        NCLogger.Log($"Orientation: {orientation} not found", LogLevel.ERROR);
#endif
        return Vector3.zero;
    }
    
    public KeyCode GetKeyCode(NoteData.LaneOrientation orientation) {
        if (laneControllerData.TryGetValue(orientation, out LaneControllerData data)) return data.Input;
#if UNITY_EDITOR
        NCLogger.Log($"Orientation: {orientation} not found", LogLevel.ERROR);
#endif
        return KeyCode.None;
    }

    public GameObject GetHitCondPrefab(HitCondition hitCond)
    {
        if (HitCondToScoreData.TryGetValue(hitCond, out ScoreData data)) return data.Prefab;
#if UNITY_EDITOR
        NCLogger.Log($"HitCondition: {hitCond} not found", LogLevel.ERROR);
#endif
        return new GameObject();
    }
    
    public LaneControllerData GetControlData (NoteData.LaneOrientation orientation) {
        if (laneControllerData.TryGetValue(orientation, out LaneControllerData data)) return data;
#if UNITY_EDITOR
        NCLogger.Log($"Controller Data: {data} not found", LogLevel.ERROR);
#endif
        return null;
    }
    
    public void GetLerpPoints (Data_Classes.NoteData.LaneOrientation noteOrientation, ref Vector3 startPos, ref Vector3 endPos)
    {
        if(!laneControllerData.TryGetValue(noteOrientation, out var data)) {
#if UNITY_EDITOR
            NCLogger.Log($"Orientation: {noteOrientation} is invalid, cannot Get Lerp Points", LogLevel.ERROR);
#endif
        }
       
        //Vector3 startPos = hitPoint + (dir * noteSpawnZ); 
        // startPos = new Vector3(data.HitPoint.x, data.HitPoint.y, noteSpawnZ);
        startPos = new Vector3(noteSpawnX, data.HitPoint.y, data.HitPoint.z);
        //Vector3 endPos = hitPoint + (dir * NoteDespawnZ);
        // endPos = new Vector3(data.HitPoint.x, data.HitPoint.y, NoteDespawnZ);
        endPos = new Vector3(NoteDespawnX, data.HitPoint.y, data.HitPoint.z);
    }
}
