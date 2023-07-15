using System.Collections;
using System.Collections.Generic;
using Core.Logging;
using DG.Tweening;
using UnityEngine;

public class TwitchingSprite : MonoBehaviour
{
    public Vector3[] positionArray;
    public Vector3[] rotationArray;
    public Vector3[] scaleArray;
    public float[] timeArray;
    
    // Start is called before the first frame update
    void Start()
    {
        var rotation = transform.localEulerAngles;
        var scale = transform.localScale;
        var twitchSequence = DOTween.Sequence();
        if (rotationArray.Length != scaleArray.Length) {
#if UNITY_EDITOR
            NCLogger.Log($"Not Equal", LogLevel.ERROR);
#endif
            return;
        }

        for (int i = 0; i < rotationArray.Length; i++)
        {
            twitchSequence.Insert(timeArray[i], transform.DORotate(rotationArray[i], 1f))
                .Insert(timeArray[i], transform.DOScale(scaleArray[i], 1f))
                .Insert(timeArray[i], transform.DOMove(positionArray[i],1f))
                .AppendInterval(timeArray[i]);
#if UNITY_EDITOR
            NCLogger.Log($"asdsads");
#endif
        }
        twitchSequence.SetLoops(-1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
