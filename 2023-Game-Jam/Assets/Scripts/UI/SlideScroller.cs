using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SlideScroller : MonoBehaviour
{
    private float startTime;
    [SerializeField] private bool _moving = false;
    [SerializeField] private int curSlide = 0;

    private Vector3 startPos;
    private Vector3 curPos;
    private Vector3 destPos;
    // Start is called before the first frame update
    void Start()
    {
        startPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        if (_moving)
        {
            float movingTime = (Time.time - startTime) * 10f;
            //Debug.Log(movingTime);
            this.transform.position = Vector3.Lerp(curPos, destPos, movingTime);
            if (movingTime >= 1)
            {
                _moving = false;
            }
        }
    }
    public bool FindSlide(int change)
    {
        int slideFind = curSlide + change;
        GameObject temp = null;

        try
        {
            temp = this.transform.Find("slide (" + slideFind + ")").gameObject;
        }
        catch (Exception e)
        {
            Console.WriteLine("Something went wrong.");
        }
        if (temp != null)
        {
            return true;
        }
        else return false;
    }
    public void ScrollLeft()
    {
        if (!_moving && FindSlide(-1) == true)
        {
            curSlide--;
            curPos = this.transform.position;
            destPos = new Vector3(this.transform.position.x + 500, this.transform.position.y, this.transform.position.z);
           // Debug.Log(destPos);
            _moving = true;
            startTime = Time.time;
        }
    }

    public void ScrollRight()
    {
        if (!_moving && FindSlide(1) == true)
        {
            curSlide++;
            curPos = this.transform.position;
            destPos = new Vector3(this.transform.position.x - 500, this.transform.position.y, this.transform.position.z);
            //Debug.Log(destPos);
            _moving = true;
            startTime = Time.time;
        }
    }

    public void ExitTutorial()
    {
        this.transform.position = startPos;
        curSlide = 0;
    }
}
