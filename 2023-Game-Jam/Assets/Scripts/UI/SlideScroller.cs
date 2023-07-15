using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SlideScroller : MonoBehaviour
{
    private int curSlide = 0;
    public List<GameObject> slides = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool FindSlide(int change)
    {
        int slideFind = curSlide + change;
        GameObject temp = null;

        try
        {
            temp = slides[slideFind].gameObject;
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
        if (FindSlide(-1) == true)
        {
            curSlide--;
            foreach(GameObject slide in slides)
            {
                slide.SetActive(false);
            }
            slides[curSlide].SetActive(true);
        }
    }

    public void ScrollRight()
    {
        if (FindSlide(1) == true)
        {
            curSlide++;
            foreach (GameObject slide in slides)
            {
                slide.SetActive(false);
            }
            slides[curSlide].SetActive(true);
        }
    }

    public void ExitTutorial()
    {
        curSlide = 0;
        foreach (GameObject slide in slides)
        {
            slide.SetActive(false);
        }
        slides[curSlide].SetActive(true);
    }
}
