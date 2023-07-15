using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class SplashScreen : MonoBehaviour
{
    private VideoPlayer _playerComponent;

    public VideoPlayer VideoPlayer
    {
        get
        {
            if (!_playerComponent) _playerComponent = GetComponent<VideoPlayer>();
            return _playerComponent;
        }
    }

    public double VideoProgress => VideoPlayer.time / VideoPlayer.length;

    private void LateUpdate()
    {
        if (VideoProgress >= .99)
        {
            SceneManager.LoadScene(1);
        }
    }
}