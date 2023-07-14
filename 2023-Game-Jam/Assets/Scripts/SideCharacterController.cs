using System;
using System.Collections;
using System.Collections.Generic;
using Core.Events;
using DG.Tweening;
using UnityEngine;
using EventType = Core.Events.EventType;

public class SideCharacterController : MonoBehaviour
{
    [Header("Jump Attribute")]
    [SerializeField] private Vector3 jumpStopPos;
    [SerializeField] private float jumpTime;
    [SerializeField] private float aStallTime;
    [SerializeField] private float fallTime;
    [SerializeField] private float aFadeInRate;
    [SerializeField] private float aFadeOutRate;
    
    [Header("Ground Attribute")]
    [SerializeField] private Vector3 dashDownPos;
    [SerializeField] private float gStallTime;
    [SerializeField] private float gFadeInRate;
    [SerializeField] private float gFadeOutRate;
    
    [Header("Header Attribute")]
    [SerializeField] private List<string> attackList;
    [SerializeField] private string blockLoop;
    private int _index;
    
    private Sequence _sequence;

    private Animator _scAnimator;
    public Animator SCAnimator {
        get {
            if (_scAnimator == null) {
                _scAnimator = GetComponent<Animator>();
            }
            return _scAnimator;
        }
    }

    private SpriteRenderer _spriteRenderer;
    public SpriteRenderer SpriteRenderer {
        get {
            if (_spriteRenderer == null) {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            return _spriteRenderer;
        }
    }

    private Color alpha1 => new Color(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b,
        1);
    private Color alpha0 => new Color(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b,
        0);
    private float alphaRate => SpriteRenderer.color.a / 1;

    private float jumpRate => Vector3.Distance(transform.position, jumpStopPos) /
                              Vector3.Distance(dashDownPos, jumpStopPos);
    
    private void Awake()
    {
        this.AddListener(EventType.SideCharacterBlockGround, _ => BlockGround());
        this.AddListener(EventType.SideCharacterBlockAir, _ => BlockAir());
        this.AddListener(EventType.SideCharacterAttackGround, _ => AttackGround());
        this.AddListener(EventType.SideCharacterAttackAir, _ => AttackAir());
    }

    private void Start()
    {
        transform.position = dashDownPos;
        SpriteRenderer.color = alpha0;
    }

    private void AttackGround()
    {
        _sequence.Kill();
        IncrementAtkIndex();

        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => transform.position = dashDownPos)
            .Append(SpriteRenderer.DOFade(1, gFadeInRate * alphaRate))
            .InsertCallback((gFadeInRate * alphaRate)/2,() => SCAnimator.Play(attackList[_index]))
            .Append(SpriteRenderer.DOFade(1, gStallTime))
            .Append(SpriteRenderer.DOFade(0, gFadeOutRate));
    }

    private void AttackAir()
    {
        _sequence.Kill();
        IncrementAtkIndex();
        
        _sequence = DOTween.Sequence();
        _sequence
            // .InsertCallback(0, () => transform.position = dashDownPos)
            .Insert(0, SpriteRenderer.DOFade(1, aFadeInRate * alphaRate))
            .Append(transform.DOMove(jumpStopPos, jumpTime * jumpRate).SetEase(Ease.OutCubic))
            .InsertCallback((jumpTime * jumpRate) / 2, () => SCAnimator.Play(attackList[_index]))
            .Append(transform.DOMove(jumpStopPos + Vector3.up * .1f, aStallTime))
            // .Append(aStallTime + jumpTime * jumpRate, transform.DOMove(dashDownPos, fallTime).SetEase(Ease.InSine));
            .Append(transform.DOMove(dashDownPos, fallTime).SetEase(Ease.InSine))
            .Insert(aStallTime + jumpTime * jumpRate, SpriteRenderer.DOFade(0, aFadeOutRate));
    }
    
    private void BlockGround()
    {
        _sequence.Kill();

        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => transform.position = dashDownPos)
            .AppendCallback(() => SCAnimator.Play(blockLoop))
            .Append(SpriteRenderer.DOFade(1, gFadeInRate * alphaRate));
    }

    private void BlockAir()
    {
        _sequence.Kill();

        _sequence = DOTween.Sequence();
        _sequence
            .AppendCallback(() => transform.position = jumpStopPos)
            .AppendCallback(() => SCAnimator.Play(blockLoop))
            .Append(SpriteRenderer.DOFade(1, gFadeInRate * alphaRate));
    }

    private void IncrementAtkIndex()
    {
        _index++;
        _index = _index == attackList.Count ? 0 : _index;
    }
}
