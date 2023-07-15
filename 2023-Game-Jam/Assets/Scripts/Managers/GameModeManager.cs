using System;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using Core.Logging;
using Core.Patterns;
using SO_Scripts;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using EventType = Core.Events.EventType;

public enum GameState
{
    Undefined,
    MainMenu,
    LevelSelection,
    PlayMode,
    PauseMode,
}

public class GameModeManager : Singleton<GameModeManager>
{
    
    //Main Menu 
    [SerializeField] private GameModeData gameModeData;
    [SerializeField] private MidiData currentMidiData;
    // [SerializeField] private PoolData poolData;

    public PostProcessProfile noColorGradingProfile;
    public PostProcessProfile colorGradingProfile;
    public PostProcessProfile colorGradingProfilelvl;
    
    // public PoolData PoolData => poolData;
    public MidiData CurrentMidiData
    {
        get => currentMidiData;
        set => currentMidiData = value;
    }

    public GameModeData GameModeData => gameModeData;
    
    private GameState _gameState;
    public GameState CurrentGameState {
        get
        {
            if (_gameState == GameState.Undefined) {
#if UNITY_EDITOR
                NCLogger.Log($"game state is {_gameState}", LogLevel.ERROR);
#endif
            }
            return _gameState;
        }
        
        set => Instance._gameState = value;
    }
    
    private void Awake() {
        base.Awake();
        if(Instance != null && Instance != this) Destroy(gameObject);
#if UNITY_EDITOR
        if(!CurrentMidiData) NCLogger.Log($"midiData is {CurrentMidiData}", LogLevel.ERROR);
        if(!GameModeData) NCLogger.Log($"midiData is {GameModeData}", LogLevel.ERROR);
#endif
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        //Screen.SetResolution(800, 450, true);
    }
}
