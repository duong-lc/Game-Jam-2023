using System;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using GabrielBigardi.SpriteAnimator;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using EventType = Core.Events.EventType;

public class LevelSelectionController : MonoBehaviour
{
    private TransitionState state;
    private bool _consumeInput = false;

    //[SerializeField] private Image image;
    [SerializeField] private PostProcessVolume volume;
    private void Awake()
    {
        this.AddListener(EventType.GlobalTransitionCompleteEvent, param => state = (TransitionState) param);
    }
    
    private IEnumerator Start()
    {
        Time.timeScale = 1;
        volume.profile = GameModeManager.Instance.colorGradingProfilelvl;
        state = TransitionState.In;
        _consumeInput = true;
        GameModeManager.Instance.CurrentGameState = GameState.LevelSelection;
        
        this.FireEvent(EventType.GlobalTransitionEvent, TransitionState.Out);
        while (state == TransitionState.In) {
            yield return null;
        }
        _consumeInput = false;
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene(GameModeManager.Instance.GameModeData.gamePlaySceneName);
        }
    }

    public void LoadMainMenu()
    {
        if (_consumeInput) return;
        _consumeInput = true;
        StartCoroutine(
            LoadSceneRoutine(
                () =>   SceneManager.LoadScene(GameModeManager.Instance.GameModeData.mainMenuSceneName)));
      
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
    
    public void EnterGameplay()
    {
        if (_consumeInput) return;
            _consumeInput = true;
            StartCoroutine(
                LoadSceneRoutine(
                    () =>   SceneManager.LoadScene(GameModeManager.Instance.GameModeData.gamePlaySceneName)));
    
    }
}
