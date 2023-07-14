using System;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using Core.Logging;
using Core.Patterns;
using GabrielBigardi.SpriteAnimator;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EventType = Core.Events.EventType;

public class GameSceneController : Singleton<GameSceneController>
{
    [SerializeField] private GameObject endGameOverlay;
    [SerializeField] private GameObject pauseOverlay;
    [SerializeField] private GameObject[] inGameHUDs;

    [SerializeField] private TMP_Text[] artists;
    [SerializeField] private TMP_Text[] songTitles;
    [SerializeField] private Image cover;
    // [SerializeField] private GameObject visual;
    
    private MidiData _midiData;
    private GameModeData _gameModeData;
    
    private TransitionState state;
    private bool _consumeInput = false;

    [SerializeField] private PostProcessVolume volume;
    private void Awake()
    {
        this.AddListener(EventType.GlobalTransitionCompleteEvent, param => state = (TransitionState) param);
        
        _midiData = GameModeManager.Instance.CurrentMidiData;
        _gameModeData = GameModeManager.Instance.GameModeData;
#if UNITY_EDITOR
        if(!pauseOverlay) NCLogger.Log($"pauseOverlay not assigned", LogLevel.ERROR);
        if(inGameHUDs.Length == 0) NCLogger.Log($"inGameHUD not assigned", LogLevel.ERROR);
#endif

        //this.AddListener(EventType.GameEndedEvent, param => LoadEndScreenOverlay());
        
    }

    private IEnumerator Start()
    {
        Time.timeScale = 1;
        volume.profile = GameModeManager.Instance.colorGradingProfile;
        SongManager.PlaySong();
        state = TransitionState.In;
        _consumeInput = true;
        GameModeManager.Instance.CurrentGameState = GameState.PlayMode;
        
        this.FireEvent(EventType.GlobalTransitionEvent, TransitionState.Out);
        while (state == TransitionState.In) {
            yield return null;
        }
        _consumeInput = false;
        

        ToggleInGameHUD(true);
        pauseOverlay.SetActive(false);
        endGameOverlay.SetActive(false);
        // visual.SetActive(true);
        
        foreach (var artist in artists) {
            artist.text = _midiData.artist;
        }
        foreach (var songTitle in songTitles) {
            songTitle.text = _midiData.songTitle;
        }

        cover.sprite = _midiData.albumCover;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().buildIndex == 2)
            {
                LoadPauseOverlay();
            }
        }
    }

    public void LoadEndScreenOverlay()
    {
        GameModeManager.Instance.CurrentGameState = GameState.PauseMode;
        SongManager.PauseSong();
        Time.timeScale = 0;

        ToggleInGameHUD(false);
        endGameOverlay.SetActive(true);
        //visual.SetActive(false);
    }
    
    public void LoadPauseOverlay()
    {
        //NCLogger.Log($"lmao xd");
        // GameModeManager.SetGameState(GameModeManager.GameState.PauseMode);
        GameModeManager.Instance.CurrentGameState = GameState.PauseMode;
        SongManager.PauseSong();
        Time.timeScale = 0;

        ToggleInGameHUD(false);
        pauseOverlay.SetActive(true);    
        this.FireEvent(EventType.PauseTransitionEvent, PauseTransition.RibbonState.Pause);
    }

    public void LoadReturnToGame()
    {
        // GameModeManager.SetGameState(GameModeManager.GameState.PlayMode);
        GameModeManager.Instance.CurrentGameState = GameState.PlayMode;
        SongManager.PlaySong();
        Time.timeScale = 1;

        ToggleInGameHUD(true);
        //pauseOverlay.SetActive(false);
        
        this.FireEvent(EventType.PauseTransitionEvent, PauseTransition.RibbonState.Playing);
        EventDispatcher.Instance.FireEvent(EventType.UnPauseEvent);
    }

    public void LoadRestartLevel()
    {
        if (_consumeInput) return;
        Time.timeScale = 1;
        _consumeInput = true;
        StartCoroutine(
            LoadSceneRoutine(
                () =>  SceneManager.LoadScene(GameModeManager.Instance.GameModeData.gamePlaySceneName)));
    }

    public void LoadReturnToLevelSelectionScene()
    {
        if (_consumeInput) return;
        Time.timeScale = 1;
        _consumeInput = true;
        StartCoroutine(
            LoadSceneRoutine(
                () => SceneManager.LoadScene(GameModeManager.Instance.GameModeData.levelSelectionSceneName)));
    }
    
    private IEnumerator LoadSceneRoutine(Action callback)
    {
        this.FireEvent(EventType.GlobalTransitionEvent, TransitionState.In);
        
        while (state != TransitionState.In) {
            yield return null;
        }
        this.FireEvent(EventType.GlobalTransitionEvent, TransitionState.Out);
        callback.Invoke();
    }

    private void ToggleInGameHUD(bool state)
    {
        foreach (var hud in inGameHUDs)
        {
            hud.SetActive(state);
        }
    }
}
