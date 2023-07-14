using System;
using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    private Animator _mcAnimator;

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
        throw new NotImplementedException();
    }
}
