using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Data_Classes;
using DG.Tweening;
using Melanchall.DryWetMidi.MusicTheory;
using UnityEngine;
using EventType = Core.Events.EventType;
using Random = System.Random;

public class CharacterController : MonoBehaviour
{
    [SerializeField] private Vector3 groundedPos;
    [Header("Jump Attributes")]
    [SerializeField] private Vector3 jumpStopPos;
    [SerializeField] private float jumpTime;
    [SerializeField] private float stallTime;
    [SerializeField] private float fallTime;
    [Header("Dash Down Attributes")]
    [SerializeField] private Vector3 dashDownPos;
    [SerializeField] private float dashDownTime;
    [SerializeField] private float stallDashTime;
    [SerializeField] private float rePosTime;

    [Space] 
    public float speedIdle;
    public float yDisplacement;
    public float speedIdleX;
    public float xDisplacement;
    // private bool _isGrounded;
    private Sequence _sequence;

    [Header("Animator Parameter Attributes")] 
    [SerializeField] private List<string> attackList;
    [SerializeField] private List<string> hurtList;
    [SerializeField] private List<string> jumpList;
    [SerializeField] private List<string> landList;
    [SerializeField] private string switchLaneUp;
    [SerializeField] private string switchLaneDown;
    
    [SerializeField] private string groundBlock;
    [SerializeField] private string airBlock;
    [SerializeField] private string idle;

    private Animator _mcAnimator;
    public NoteData.LaneOrientation Orientation;
    public bool holdBusy;
    public NoteColorEnum playerColorEnum;
    private int _index;
    
    public int playerAtkPoint;
    public float sliderValue;

    public Sequence seq;
    public Sequence seq1;
    public Animator _MCAnimator {
        get {
            if (_mcAnimator == null) {
                _mcAnimator = GetComponent<Animator>();
            }
            return _mcAnimator;
        }
    }
    
    private void Awake()
    {
        this.AddListener(EventType.PlayerAttack, param => Attack((NoteData.LaneOrientation) param));
        this.AddListener(EventType.PlayerBlock, param => Block((NoteData.LaneOrientation) param));
        this.AddListener(EventType.PlayerEndBlock, param => Attack((NoteData.LaneOrientation) param));
    }

    private void Start()
    {
        StartIdle();
    }

    public void KillIdle()
    {
        seq.Kill();
        seq1.Kill();
    }
    
    public void StartIdle()
    {
        seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(transform.position.y - yDisplacement, speedIdle));
        // seq.Append(transform.DOMoveY(transform.position.y + .5f, 2));
        seq.SetLoops(-1, LoopType.Yoyo);
        seq.Play();
        
        seq1 = DOTween.Sequence();
        seq1.Append(transform.DOMoveX(transform.position.x - xDisplacement, speedIdleX));
        seq1.SetLoops(-1, LoopType.Yoyo);
        seq1.Play();
    }
    
    private void Attack(NoteData.LaneOrientation laneOrientation)
    {
        if (laneOrientation != Orientation) return;

        holdBusy = false;
        _sequence.Kill();
        _index++;
        _index = _index == attackList.Count ? 0 : _index;

        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => _MCAnimator.Play(attackList[_index]))
            .InsertCallback(stallDashTime, () => _MCAnimator.Play(idle));
    }

    private void Block(NoteData.LaneOrientation laneOrientation)
    {
        if (laneOrientation != Orientation) return;

        holdBusy = true;
        _sequence.Kill();
        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => _MCAnimator.Play(groundBlock));
    }
    
    
    public void PlayIdle()
    {
        _MCAnimator.Play(idle);
    }
}
