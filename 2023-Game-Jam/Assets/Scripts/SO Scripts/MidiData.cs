using System;
using UnityEngine;
using Data_Classes;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Melanchall.DryWetMidi.MusicTheory;
using Sirenix.OdinInspector;
using UnityEditor;

public enum Ratings
{
    S,
    A,
    B,
    C,
    D,
    F
}


[Serializable]
public class LaneMidiData
{
    [SerializeField] [Range(0, 8)] private int laneOctave;
    
    public List<BaseNoteType> allNoteOnLaneList = new();

    public int LaneOctave => laneOctave;
}


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Midi Data", order = 1)]
public class MidiData : SerializedScriptableObject
{
   [Header("MIDI Related & Time Offset Data")]
   public AudioClip songClip;
   
   public float songDelayInSeconds; //delay the song after a certain amount of time

   public string fileLocation; //file location for the MIDI file

   public int TotalRawNoteCount;
   
   [Header("Note Prefabs")]
   public GameObject noteNormalPrefab;
   public GameObject noteSliderPrefab;

   
   [Header("Lane Data")]
   public NoteName noteRestrictionNormalNote;
   public NoteName noteRestrictionSliderNote;

   public Dictionary<NoteData.LaneOrientation, LaneMidiData> laneMidiData = new ();

   [Header("Song High Score")] 
   public Ratings ratings;
   public float accuracy;
   public int maxCombo;
   public int score;
   public int perfectHits;
   public int earlyHits;
   public int lateHits;
   public int missHits;

   [Header("Song Misc Info")] 
   public string artist;
   public string songTitle;
   public Sprite albumCover;
}

