using System;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using Core.Logging;
using Core.Patterns;
using DG.Tweening;
using Managers;
using NoteClasses;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SO_Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using EventType = Core.Events.EventType;
using NoteData = Data_Classes.NoteData;
[Serializable]
[Flags]
public enum HitCondition {
    None,
    Early,
    EarlyPerfect,
    LatePerfect,
    Late,
    Miss,
}

[Serializable]
public class ScoreData
{
    [SerializeField] private int score;
    [SerializeField] private bool countTowardsCombo;
    [SerializeField] private bool resetCombo;
    [SerializeField] private GameObject hitMarkPrefab;

    public int Score => score;
    public bool CountTowardsCombo => countTowardsCombo;
    public GameObject Prefab => hitMarkPrefab;
}

[Serializable]
public class MarginOfError
{
    [HorizontalGroup]
    [SerializeField] private float beginMOE;
    
    [HorizontalGroup]
    [SerializeField] private float endMOE;

    public float BeginMOE => beginMOE;
    public float EndMOE => endMOE;
}

public class HitMarkInitData : PooledObjectCallbackData
{
    public NoteData.LaneOrientation orientation { get; private set; }
    public HitCondition cond { get; private set; }
    public NoteBase noteRef { get; private set; }
    
    public HitMarkInitData (NoteBase noteRef, HitCondition cond, NoteData.LaneOrientation orientation) {
        this.cond = cond;
        this.orientation = orientation;
        this.noteRef = noteRef;
    }
}

public class ScoreManager : Singleton<ScoreManager>
{
    [SerializeField] private AudioSource hitAudioSource;
    [SerializeField] private AudioSource missAudioSource;
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text accuracyText;
    [SerializeField] private TMP_Text accuracyShadowText;
    [SerializeField] private Slider slider;
    [Header("Hit Display")]
    [SerializeField] private TMP_Text airLaneDisplay;
    [SerializeField] private TMP_Text airLaneDisplay1;
    [SerializeField] private TMP_Text groundLaneDisplay;
    [SerializeField] private TMP_Text groundLaneDisplay1;
    [SerializeField] private float fadeInRate;
    [SerializeField] private float stallRate;
    [SerializeField] private float fadeOutRate;
    [SerializeField] private float scaleStart;
    [SerializeField] private float scaleEnd;
    [Space] 
    [SerializeField] private EndGameTransition endGameScreen;
    [SerializeField] private Transform tweenTransform;

    [Header("GameJam Mechanics")] 
    

    // public int playerRedAtk;
    // public int playerBlueAtk;
    
    [SerializeField] private TMP_Text bossHPText;
   
    // [SerializeField] private int playerAtkPoint;
    [SerializeField] private int bossHpPoint;
    // private float _currentAtkPoint;
    
    private MidiData _midiData;
    private GameModeData _gameModeData;

    public float AccuracyFloat =>(_currentScore / _currentPerfectScore) * 100;
    private int _currentScore;

    private float _totalCurrentScore = 0;
    private float _totalPerfectScore => _midiData.TotalRawNoteCount *
                                        _gameModeData.HitCondToScoreData[HitCondition.EarlyPerfect].Score;
    
    private float _currentPerfectScore => _noteCount * _gameModeData.HitCondToScoreData[HitCondition.EarlyPerfect].Score;
    private float _totalPerfectScoreCache;
    private int _currentCombo;

    private int _noteCount;
    private int _maxCombo;
    private int _missCount;
    private Camera _mainCam;
    private Transform _comboTCache;
    private Tweener _scaleComboTweener;
    private Ratings _ratings;
    private Sequence _sequenceAir;
    private Sequence _sequenceAir1;
    private Sequence _sequenceGround;
    private Sequence _sequenceGround1;
    private Vector3 _ogComboScale;
    // private Dictionary<Transform, Sequence> _seqToTransformDict;

    public int perfectHits;
    public int earlyHits;
    public int lateHits;
    public int missHits;

    // public int PlayerAtkPoint => playerAtkPoint;
    public int BossHpPoint => bossHpPoint;
    public int MissCount => _missCount;
    public int CurrentCombo => _currentCombo;
    public int MaxCombo => _maxCombo;
    public int CurrentScore => _currentScore;
    
    
    private ObjectPool[] _notePoolArray;
    public ObjectPool[] NotePools {
        get {
            if (_notePoolArray.IsNullOrEmpty()) _notePoolArray = GetComponentsInChildren<ObjectPool>();
            return _notePoolArray;
        }
    }
    
    private void Awake() {
        EventDispatcher.Instance.AddListener(EventType.NoteHitEarlyEvent, param => OnHit((HitMarkInitData) param));
        EventDispatcher.Instance.AddListener(EventType.NoteHitPerfectEvent, param => OnHit((HitMarkInitData) param));
        EventDispatcher.Instance.AddListener(EventType.NoteHitLateEvent, param => OnHit((HitMarkInitData) param));
        EventDispatcher.Instance.AddListener(EventType.NoteMissEvent, param => OnHit((HitMarkInitData) param));
        
        this.AddListener(EventType.GameEndedEvent, param => CacheHighScore());
    }

    // Start is called before the first frame update
    private void Start() {
        _midiData = GameModeManager.Instance.CurrentMidiData;
        _gameModeData = GameModeManager.Instance.GameModeData;
        _mainCam = Camera.main;

        _comboTCache = tweenTransform.transform;

        hitAudioSource.volume = _gameModeData.hitVolume;
        missAudioSource.volume = _gameModeData.hitVolume;
        
        perfectHits = 0;
        earlyHits = 0;
        lateHits = 0;
        missHits = 0;

        _ogComboScale = tweenTransform.localScale;
        bossHPText.text = "x" + bossHpPoint.ToString();

    }

    private void OnHit(HitMarkInitData param)
    {
        TMP_Text display = null;
        TMP_Text display1 = null;
        string text = "None";
        
        display = param.orientation == NoteData.LaneOrientation.One ? airLaneDisplay : groundLaneDisplay;
        display1 = param.orientation == NoteData.LaneOrientation.One ? airLaneDisplay1 : groundLaneDisplay1; 
        
        switch (param.cond)
        {
            case HitCondition.Early:
                text = "EARLY";
                earlyHits++;
                break;
            case HitCondition.EarlyPerfect:
                text = "PERFECT";
                perfectHits++;
                break;
            case HitCondition.LatePerfect:
                text = "PERFECT";
                perfectHits++;
                break;
            case HitCondition.Late:
                text = "LATE";
                lateHits++;
                break;
            case HitCondition.Miss:
                text = "MISS";
                missHits++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Sequence seq;
        if (param.orientation == NoteData.LaneOrientation.One)
        {
            _sequenceAir.Kill();
            _sequenceAir1.Kill();
            display.text = text;
            display1.text = text;
            display.transform.localScale = Vector3.one * scaleStart;
            display1.transform.localScale = Vector3.one * scaleStart;
            _sequenceAir = DOTween.Sequence();
            _sequenceAir
                .Append(display.transform.DOScale(scaleEnd, fadeInRate))
                .Insert(0, display.DOFade(1, fadeInRate))
                .Append(display.DOFade(1, stallRate))
                .Append(display.DOFade(0, fadeOutRate));
            _sequenceAir1 = DOTween.Sequence();
            _sequenceAir1
                .Append(display1.transform.DOScale(scaleEnd, fadeInRate))
                .Insert(0, display.DOFade(1, fadeInRate))
                .Append(display1.DOFade(.33f, stallRate))
                .Append(display1.DOFade(0, fadeOutRate));
        }
        else
        {
            _sequenceGround.Kill();
            _sequenceGround1.Kill();
            display.text = text;
            display1.text = text;
            display.transform.localScale = Vector3.one * scaleStart;
            display1.transform.localScale = Vector3.one * scaleStart;
            _sequenceGround = DOTween.Sequence();
            _sequenceGround
                .Append(display.transform.DOScale(scaleEnd, fadeInRate))
                .Insert(0, display.DOFade(1, fadeInRate))
                .Append(display.DOFade(1, stallRate))
                .Append(display.DOFade(0, fadeOutRate));
            _sequenceGround1 = DOTween.Sequence();
            _sequenceGround1
                .Append(display1.transform.DOScale(scaleEnd, fadeInRate))
                .Insert(0, display.DOFade(1, fadeInRate))
                .Append(display1.DOFade(.33f, stallRate))
                .Append(display1.DOFade(0, fadeOutRate));
        }

        var scoreData = _gameModeData.GetScoreData(param.cond);
#if UNITY_EDITOR
        if (scoreData == null) { NCLogger.Log($"null score data", LogLevel.ERROR); }
#endif
        
        _currentCombo += scoreData.CountTowardsCombo ? 1 : 0;
        _currentScore += scoreData.Score;
        if (_totalCurrentScore == 0) _totalCurrentScore = _totalPerfectScore;
        if (_totalPerfectScoreCache == 0) _totalPerfectScoreCache = _totalPerfectScore;
        _totalCurrentScore -= _gameModeData.HitCondToScoreData[HitCondition.EarlyPerfect].Score -
                              _gameModeData.HitCondToScoreData[param.cond].Score;
        _noteCount++;
        if (param.cond == HitCondition.Miss && param.cond != HitCondition.None)
        {
            if (_currentCombo > _maxCombo) _maxCombo = _currentCombo;
            missAudioSource.Play();
            _missCount++;
            _currentCombo = 0;
            // _currentAtkPoint = 0;
            // attackSlider.DOValue(0, .3f);
            ControlManager.Instance.ResetPlayerScore(param.orientation);
        }
        else if (param.cond != HitCondition.None && param.cond != HitCondition.Miss)
        {
            hitAudioSource.Play();
        }

        UpdateScoreText();
        UpdateComboText();
        UpdateAccuracyText();

        if (param.cond == HitCondition.EarlyPerfect || param.cond == HitCondition.LatePerfect)
        {
          

            ControlManager.Instance.AddPlayerScore(param.orientation);
        }
    }

    public Ratings GetRatings()
    {
        _ratings = AccuracyFloat switch
        {
            < 50 => Ratings.F,
            <= 60 => Ratings.D,
            <= 70 => Ratings.C,
            <= 80 => Ratings.B,
            <= 90 => Ratings.A,
            <= 100 => Ratings.S,
            _ => _ratings
        };
        return _ratings;
    }

    private void CacheHighScore()
    {
        if (_currentScore > _midiData.score)
        {
            _midiData.score = _currentScore;
            _midiData.ratings = GetRatings();
            _midiData.accuracy = AccuracyFloat;
            _midiData.maxCombo = _maxCombo;
            _midiData.perfectHits = perfectHits;
            _midiData.earlyHits = earlyHits;
            _midiData.lateHits = lateHits;
            _midiData.missHits = missHits;

            endGameScreen.isHighScore = true;
        }

        if (missHits == 0)
        {
            endGameScreen.isFullCombo = true;
        }
        
        this.FireEvent(EventType.EndGameTransitionEvent);
    }

    private void UpdateScoreText()
    {
        var score = _currentScore.ToString();
        var wSpace = 7 - score.Length;
        string newStr = "";
        for(var i = 0; i < wSpace; i++) {
            newStr += "0";
        }
        scoreText.text = newStr + score;
    }
    
    private void UpdateAccuracyText()
    {
        accuracyText.text = AccuracyFloat.ToString("F2") + "%";
    }
    
    private void UpdateComboText()
    {
        // ResetTransform();
        // if (_scaleComboTweener == null)
        // {
        //     _scaleComboTweener = tweenTransform.DOPunchScale(-tweenTransform.forward * .5f, .2f, 1, .5f).OnComplete(() => _scaleComboTweener = null);
        // } else {
        //     if (_scaleComboTweener.IsActive())
        //     {
        //         _scaleComboTweener.Kill();
        //         _scaleComboTweener.OnKill(ResetTransform);
        //         _scaleComboTweener = tweenTransform.DOPunchScale(-tweenTransform.forward * .5f, .2f, 1, .5f).OnComplete(() => _scaleComboTweener = null);
        //     }
        // }
           
        comboText.text = "x" + _currentCombo.ToString();
    }

    private void ResetTransform()
    {
        tweenTransform.position = _comboTCache.position;
        tweenTransform.localScale = _ogComboScale;
        // NCLogger.Log($" reset scale {comboText.transform.localScale}");
    }

    public void DisplayScoreScreen()
    {
        
    }
}
