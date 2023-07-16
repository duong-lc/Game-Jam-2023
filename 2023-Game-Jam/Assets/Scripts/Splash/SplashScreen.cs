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
    [SerializeField] private string videoFileName;
    
    public VideoPlayer VideoPlayer
    {
        get
        {
            if (!_playerComponent) _playerComponent = GetComponent<VideoPlayer>();
            return _playerComponent;
        }
    }

    private void Start()
    {
        PlayVideo();
    }

    public double VideoProgress => VideoPlayer.time / VideoPlayer.length;

    private void LateUpdate()
    {
        if (Time.time >= 5f)
        {
            SceneManager.LoadScene(1);
        }
    }

    public void PlayVideo()
    {
        if (VideoPlayer)
        {
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoFileName);
            VideoPlayer.url = videoPath;
            VideoPlayer.Play();
        }
    }
}