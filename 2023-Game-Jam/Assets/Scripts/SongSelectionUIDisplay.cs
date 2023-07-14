using System;
using System.Collections;
using System.Collections.Generic;
using AirFishLab.ScrollingList;
using Core.Events;
using Core.Patterns;
using TMPro;
using UnityEngine;
using EventType = Core.Events.EventType;

public class SongSelectionUIDisplay : Singleton<SongSelectionUIDisplay>
{
    public CircularScrollingList list;
    
    public TMP_Text TMP_Score;
    public TMP_Text TMP_Perfect;
    public TMP_Text TMP_Early;
    public TMP_Text TMP_Late;
    public TMP_Text TMP_Miss;
    public TMP_Text TMP_Ratings;

    public TMP_Text songName;
    public TMP_Text artistName;

    private void Awake()
    {
        this.AddListener(EventType.UpdateStatsLevelSelectionEvent, param => UpdateCurrentSongUI());
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        UpdateCurrentSongUI();
    }

    public void UpdateCurrentSongUI()
    {
        var songData = list.GetCenteredBox().GetComponent<SongSelectionElement>();
        if (songData.midiData == null) return;
        
        TMP_Score.text = songData.midiData.score.ToString();
        TMP_Perfect.text = songData.midiData.perfectHits.ToString();
        TMP_Early.text = songData.midiData.earlyHits.ToString();
        TMP_Late.text = songData.midiData.lateHits.ToString();
        TMP_Miss.text = songData.midiData.missHits.ToString();
        TMP_Ratings.text = songData.midiData.ratings.ToString();

        songName.text = songData.midiData.songTitle;
        artistName.text = songData.midiData.artist;
        
        songData.UpdateCurrentSong();
    }
}
