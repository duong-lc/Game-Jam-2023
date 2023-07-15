using System.Collections;
using System.Collections.Generic;
using Core.Logging;
using DG.Tweening;
using UnityEngine;

public class StarRotate : MonoBehaviour
{
    public float rotateTime;
    
    // Start is called before the first frame update
    void Start()
    {
        var rotation = transform.localEulerAngles;

        transform.DOLocalRotate( new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + 180), rotateTime)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Yoyo);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
