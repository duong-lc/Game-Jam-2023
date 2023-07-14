using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Core.Logging;
using Core.Patterns;
using DG.Tweening;
using UnityEngine;
using SO_Scripts;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Sirenix.Utilities;
using DataClass = Data_Classes;
using EventType = Core.Events.EventType;


namespace Managers
{
    public class LaneManager : Singleton<LaneManager>
    {
        private MidiData _midiData;
        private GameModeData _gameModeData;
        private Note[] _rawNoteArray;
        private readonly List<int> _ignoreIndexList = new ();
        
        public Lane[] LaneArray = { }; 
        
        private void Awake() {
            Core.Events.EventDispatcher.Instance.AddListener(EventType.CompileDataFromMidiEvent,
                param => CompileDataFromMidi((MidiFile) param));
            this.AddListener(EventType.LaneFinishSpawningEvent, param => StartCoroutine(CheckSongEnd()));
        }

        private void Start() {
            _midiData = GameModeManager.Instance.CurrentMidiData;
            _gameModeData = GameModeManager.Instance.GameModeData;
#if UNITY_EDITOR
            if(!_midiData) NCLogger.Log($"midiData is {_midiData}", LogLevel.ERROR);
            if(!_gameModeData) NCLogger.Log($"midiData is {_gameModeData}", LogLevel.ERROR);
#endif

            AssignColliderData();

            StartCoroutine(LaneValidationRoutine());
        }

        private IEnumerator LaneValidationRoutine()
        {
            for (var i = 0; i < LaneArray.Length; i++)
            {
                var laneEmpty = LaneArray[i].allNotesList.Count == 0;
                if (laneEmpty) {
                    i = i == 0 ? 0 : i - 1;
                    yield return null;
                }
            }

            //Validation complete
            foreach (var lane in LaneArray) {
                lane.canSpawn = true;
            }
            this.FireEvent(EventType.StartSongEvent);
        }

        private IEnumerator CheckSongEnd()
        {
            var canEnd = true;
            foreach (var lane in LaneArray) {
                if (lane.canSpawn) canEnd = false;
            }

            if (canEnd)
            {
                yield return new WaitForSeconds(3f + _gameModeData.NoteTime);
                var tween = SongManager.Instance.audioSource.DOFade(0, 3f);
                tween.OnComplete(() => this.FireEvent(EventType.GameEndedEvent));
            }
        }
        
        private void CompileDataFromMidi(MidiFile midiFile) {
            ICollection<Note> notes = midiFile.GetNotes();
            GameModeManager.Instance.CurrentMidiData.TotalRawNoteCount = notes.Count;
            
            _rawNoteArray = new Note[notes.Count];
            notes.CopyTo(_rawNoteArray, 0);

            SetTimeStampsAllLanes();

            DistributeNoteToLanes();
        }

        private void SetTimeStampsAllLanes()
        {
            if(_midiData == null)  _midiData = GameModeManager.Instance.CurrentMidiData;
            foreach (var laneMidiData in _midiData.laneMidiData.Values) {
                if (!laneMidiData.allNoteOnLaneList.IsNullOrEmpty())
                {
                    laneMidiData.allNoteOnLaneList.Clear();
#if UNITY_EDITOR
                    NCLogger.Log($"something is NOT empty");
#endif
                    //return;
                }
            }


            for (int index = 0; index < _rawNoteArray.Length; index++) //for every note in the note array
            {
                if (_ignoreIndexList.Contains(index)) continue;

                foreach (var kvp in _midiData.laneMidiData) {
                    if (_rawNoteArray[index].Octave == kvp.Value.LaneOctave) {
                        AddNoteToLane(ref index, kvp.Value.allNoteOnLaneList, kvp.Key, kvp.Value.LaneOctave);
                    }
                }
            }
        }


        #region Adding Notes To List
        private void AddNoteToLane(ref int index, List<DataClass.BaseNoteType> laneToAdd,
            DataClass.NoteData.LaneOrientation orientation, int octaveIndex)
        {
            if (_rawNoteArray[index].NoteName == _midiData.noteRestrictionNormalNote)
            {
                AddNormalNoteToList(ref index, laneToAdd, orientation, octaveIndex);
            }
            else if (_rawNoteArray[index].NoteName == _midiData.noteRestrictionSliderNote)
            {
                AddSliderNoteToList(ref index, laneToAdd, orientation, octaveIndex);
            }
        }
        
        private void AddNormalNoteToList(ref int index, List<DataClass.BaseNoteType> laneToAdd,
            DataClass.NoteData.LaneOrientation orientation, int octaveIndex)
        {
            //get the time stamp for that note. (note.Time does not return the format we want as it uses time stamp temp map in midi, so conversion is needed)
            var metricTimeSpan =
                TimeConverter.ConvertTo<MetricTimeSpan>(_rawNoteArray[index].Time, SongManager.MidiFile.GetTempoMap());
            DataClass.NoteNormalType noteNormalLocal = new DataClass.NoteNormalType
            {
                octaveNum = octaveIndex,
                noteID = DataClass.NoteData.NoteID.NormalNote,
                timeStamp = (double) metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds +
                            (double) metricTimeSpan.Milliseconds / 1000f,
                laneOrientation = orientation
            };
            //adding the time stamp (in seconds) to the array of time stamp
            laneToAdd.Add(noteNormalLocal);
        }

        private void AddSliderNoteToList(ref int index, List<DataClass.BaseNoteType> laneToAdd,
            DataClass.NoteData.LaneOrientation orientation, int octaveIndex)
        {
            //get the time stamp for that note. (note.Time does not return the format we want as it uses time stamp temp map in midi, so conversion is needed)
            var metricTimeSpan =
                TimeConverter.ConvertTo<MetricTimeSpan>(_rawNoteArray[index].Time, SongManager.MidiFile.GetTempoMap());
            /*Instead of relying on local val instantiated outside foreach loop
            create another loop inside when see current is slider note. Keep looping in the 
            note stream until you hit a note with the same octave and note restriction. That will
            be the end note
            
            Create a ignore note list so if the outer loop hit the same end note from the slider note
            it will ignore and continue
            */
            //For each 2 notes which is a slider, reset data for new slider note
            DataClass.NoteData.SliderData sliderNoteData = new DataClass.NoteData.SliderData
            {
                timeStampKeyDown = (double) metricTimeSpan.Minutes * 60f + metricTimeSpan.Seconds +
                                   (double) metricTimeSpan.Milliseconds / 1000f
            };
            for (int j = index + 1; j < _rawNoteArray.Length; j++)
            {
                //Check for next note on the same octave and on same line
                if (_rawNoteArray[j].Octave != octaveIndex ||
                    _rawNoteArray[j].NoteName != _midiData.noteRestrictionSliderNote) continue;
                var metricTimeSpan2 =
                    TimeConverter.ConvertTo<MetricTimeSpan>(_rawNoteArray[j].Time, SongManager.MidiFile.GetTempoMap());
                sliderNoteData.timeStampKeyUp = (double) metricTimeSpan2.Minutes * 60f + metricTimeSpan2.Seconds +
                                                (double) metricTimeSpan2.Milliseconds / 1000f;
                DataClass.NoteSliderType noteSliderLocal = new DataClass.NoteSliderType
                {
                    octaveNum = octaveIndex,
                    noteID = DataClass.NoteData.NoteID.SliderNote,
                    sliderData = sliderNoteData,
                    //Assigning note's lane orientation
                    laneOrientation = orientation
                };
                laneToAdd.Add(noteSliderLocal);
                _ignoreIndexList.Add(j);
                break;
            }
        }
        #endregion
        
        public void DistributeNoteToLanes()
        {
            foreach (Lane lane in LaneArray) {
                lane.SetLocalListOnLane(_midiData.laneMidiData[lane.LaneOrientation].allNoteOnLaneList);
            }
        }

        private void AssignColliderData() {
            foreach (var lane in LaneArray)
            {
                if (!_gameModeData.LaneControllerData.TryGetValue(lane.LaneOrientation, out var data)) {
#if UNITY_EDITOR
                    NCLogger.Log($"Lane Orientation {lane.LaneOrientation} not found in LaneControllerData", LogLevel.ERROR);
#endif
                    return;
                }
                data.collider = lane.LaneCollider;
            }
        }
    }
}
