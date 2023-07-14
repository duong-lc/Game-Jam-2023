using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Events;
using Data_Classes;
using DG.Tweening;
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
    private bool _isGrounded;
    private Sequence _sequence;

    [Header("Animator Parameter Attributes")] 
    [SerializeField] private List<string> attackList;
    [SerializeField] private List<string> hurtList;
    [SerializeField] private List<string> jumpList;
    [SerializeField] private List<string> landList;

    [SerializeField] private string groundBlock;
    [SerializeField] private string airBlock;
    [SerializeField] private string idle;

    private Animator _mcAnimator;
    private NoteData.LaneOrientation _orientation;
    private int _index;
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
        this.AddListener(EventType.PlayerAttackGround, _ => AttackGround());
        this.AddListener(EventType.PlayerAttackAir, _ => AttackAir());
        this.AddListener(EventType.PlayerJump, _ => Jump());
        this.AddListener(EventType.PlayerBlockGround, _ => BlockGround());
        this.AddListener(EventType.PlayerBlockAir, _ => BlockAir());
        this.AddListener(EventType.RequestIsGrounded, _ => this.FireEvent(EventType.ReceiveIsGrounded, _isGrounded));
        this.AddListener(EventType.RequestMCLane, _ =>  this.FireEvent(EventType.ReceiveMCLane, _orientation));
    }

    private void Start() {
        _isGrounded = true;
        _orientation = NoteData.LaneOrientation.Two;
    }

    private void AttackGround()
    {

        _isGrounded = true;
        _orientation = NoteData.LaneOrientation.Undefined;
        
        _sequence.Kill();
        var rate = Vector3.Distance(transform.position, dashDownPos) / Vector3.Distance(dashDownPos, jumpStopPos);
        _index++;
        _index = _index == attackList.Count ? 0 : _index;

        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => _orientation = NoteData.LaneOrientation.Two)
            .AppendCallback(() => _MCAnimator.Play(attackList[_index]))
            .Append(transform.DOMove(dashDownPos, dashDownTime * rate).SetEase(Ease.Linear))
            .Append(transform.DOMove(dashDownPos, stallDashTime))
            .Append(transform.DOMove(groundedPos, rePosTime).SetEase(Ease.InSine))
            .AppendCallback(() => _MCAnimator.Play(idle))
            .AppendCallback(() => _orientation = NoteData.LaneOrientation.Undefined);
    }

    private void AttackAir()
    {

        _isGrounded = false;
        _orientation = NoteData.LaneOrientation.Undefined;
        
        _sequence.Kill();
        var rate = Vector3.Distance(transform.position, jumpStopPos) / Vector3.Distance(groundedPos, jumpStopPos);
        //var index = (int) UnityEngine.Random.Range(0, attackList.Count);
        _index++;
        _index = _index == attackList.Count ? 0 : _index;
        
        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => _orientation = NoteData.LaneOrientation.One)
            .AppendCallback(() => _MCAnimator.Play(attackList[_index]))
            .Append(transform.DOMove(jumpStopPos, jumpTime * rate).SetEase(Ease.OutCubic))
            .Append(transform.DOMove(jumpStopPos + Vector3.up * .1f, stallTime))
            .Append(transform.DOMove(groundedPos, fallTime).SetEase(Ease.InSine))
            .AppendCallback(() => _isGrounded = true)
            .AppendCallback(() => _MCAnimator.Play(landList[0]))
            .AppendCallback(() => _orientation = NoteData.LaneOrientation.Undefined);
    }

    private void Jump()
    {
        _isGrounded = false;
        _sequence.Kill();
        var rate = Vector3.Distance(transform.position, jumpStopPos) / Vector3.Distance(groundedPos, jumpStopPos);
        var i = (int) UnityEngine.Random.Range(0, jumpList.Count);
        var jumpStr = jumpList[i];
        var landStr = landList[i];
        
        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => _MCAnimator.Play(jumpStr))
            .Append(transform.DOMove(jumpStopPos, jumpTime * rate).SetEase(Ease.OutCubic))
            .Append(transform.DOMove(jumpStopPos + Vector3.up * .1f, stallTime))
            .Append(transform.DOMove(groundedPos, fallTime).SetEase(Ease.InSine))
            .AppendCallback(() => _MCAnimator.Play(landStr))
            .AppendCallback(() => _isGrounded = true);
    }

    private void BlockGround()
    {
        _isGrounded = true;
        _orientation = NoteData.LaneOrientation.Two;
        
        _sequence.Kill();
        var rate = Vector3.Distance(transform.position, dashDownPos) / Vector3.Distance(dashDownPos, jumpStopPos);
        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => _MCAnimator.Play(groundBlock))
            .Append(transform.DOMove(dashDownPos, dashDownTime * rate).SetEase(Ease.Linear));
    }

    private void BlockAir()
    {
        _isGrounded = false;
        _orientation = NoteData.LaneOrientation.One;
        
        _sequence.Kill();
        var rate = Vector3.Distance(transform.position, jumpStopPos) / Vector3.Distance(groundedPos, jumpStopPos);
        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => _MCAnimator.Play(airBlock))
            .Append(transform.DOMove(jumpStopPos, jumpTime * rate).SetEase(Ease.OutCubic));
    }

    public void PlayIdle()
    {
        _MCAnimator.Play(idle);
    }
}


// public static class EnumerableExtension
// {
//     public static T PickRandom<T>(this IEnumerable<T> source)
//     {
//         return source.PickRandom(1).Single();
//     }
//
//     public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
//     {
//         return source.Shuffle().Take(count);
//     }
//
//     public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
//     {
//         return source.OrderBy(x => Guid.NewGuid());
//     }
// }