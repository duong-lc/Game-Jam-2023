using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;
using UnityEngine;
using SO_Scripts;
using System.IO;
using Core.Events;
using Core.Logging;
using Core.Patterns;
using Sirenix.OdinInspector;
using UnityEngine.Networking;
using EventType = Core.Events.EventType;

namespace Managers
{
    public class SongManager : Singleton<SongManager>
    {
        [SerializeField] private MidiData _midiData;
        private GameModeData _gameModeData;
        public AudioSource audioSource; //audio source to play the song
        public static MidiFile MidiFile;//static ref to midi file, this is where it will load on run

        

        private void Awake() {
            this.AddListener(EventType.StartSongEvent, param => Invoke(nameof(StartSong), _midiData.songDelayInSeconds));
        }

        private void Start()
        {
            _midiData = GameModeManager.Instance.CurrentMidiData;
            _gameModeData = GameModeManager.Instance.GameModeData;
#if UNITY_EDITOR
            if(!_midiData) NCLogger.Log($"midiData is {_midiData}", LogLevel.ERROR);
            if(!_gameModeData) NCLogger.Log($"midiData is {_gameModeData}", LogLevel.ERROR);
#endif
            
            audioSource.volume = _gameModeData.volume;
            audioSource.clip = _midiData.songClip; 
            /*
            checking if the "StreamingAssets" path is a website or not, depending on the platform that loads the midi file
                    for example, windows, mac, linux = file location where as webgl = website
            if not look in local folder
            */
            // if (Application.streamingAssetsPath.StartsWith("http://") || Application.streamingAssetsPath.StartsWith("https://")){
            //     StartCoroutine(ReadFromWebsite());//start coroutine to wait to load
            // }else{
            //     ReadFromFile();
            // }
            //StartCoroutine(GetRequest("https://www.dropbox.com/s/ibrp3ki2z69sjbm/HSOTD.mid?dl=1"));
#if UNITY_EDITOR
            ReadFromFile();
#endif
        }

        private IEnumerator GetRequest(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = url.Split('/');
                int page = pages.Length - 1;

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                        var results = webRequest.downloadHandler.data;
                        using (var stream = new MemoryStream(results))
                        {
                            MidiFile = MidiFile.Read(stream);
                            Core.Events.EventDispatcher.Instance.FireEvent(EventType.CompileDataFromMidiEvent,MidiFile);
                            Invoke(nameof(StartSong), _midiData.songDelayInSeconds);
                        }
                        break;
                }
            }
        }
        
        /// <summary>
        /// Read Midi file from website, auto request through coroutine.
        /// </summary>
        private IEnumerator ReadFromWebsite()
        {
            //requesting unity web request the midi file
            // using (UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + _midiData.fileLocation)){
            using (UnityWebRequest www = UnityWebRequest.Get(Application.streamingAssetsPath + "/" + _midiData.fileLocation)){
                yield return www.SendWebRequest();
                
                //checking to see if there's any network errors
    #pragma warning disable CS0618
                if(www.isNetworkError || www.isHttpError)
    #pragma warning restore CS0618
                {
                    Debug.LogError(www.error);
                }
                //if no error, load result from data
                //then send these results to memory stream
                //then load the stream onto midi file
                else
                {
                    byte[] results = www.downloadHandler.data;
                    using (var stream = new MemoryStream(results))
                    {
                        MidiFile = MidiFile.Read(stream);
                        //LaneManager.Instance.CompileDataFromMidi(MidiFile);
                        Core.Events.EventDispatcher.Instance.FireEvent(EventType.CompileDataFromMidiEvent,MidiFile);
                    }
                }
            }
        }
        
        /// <summary>
        /// Read Midi file from local file dir.
        /// </summary>
        
        [Button("Compile Current Midi Data")]
        private void ReadFromFile()
        {
             MidiFile = MidiFile.Read(Application.streamingAssetsPath +"/"+ _midiData.fileLocation);
             // var obj = Resources.Load(Application.streamingAssetsPath + "/" + _midiData.fileLocation);
             
           //convert midi data to necessary data for rhythm game, append them all to list
            //LaneManager.Instance.CompileDataFromMidi(MidiFile);
            Core.Events.EventDispatcher.Instance.FireEvent(EventType.CompileDataFromMidiEvent,MidiFile);
        }
            
        
        public void StartSong()
        {
            audioSource.Play();
        }
    
        ///<summary>
        ///This is an utility function to return the audio source time
        ///Instead of using "AudioSource.time" we returning a double of playback pos in PCM sample divided by freq (Hz) for the accuracy 
        ///</summary>
        public double GetAudioSourceTimeRaw()
        {
            var sourceTimeRaw = (double) audioSource.timeSamples / audioSource.clip.frequency;
            return sourceTimeRaw;
        }

        /// <summary>
        /// Get Current AudioTime with inputDelay compensated, mainly used for detecting hits for notes
        /// </summary>
        /// <returns></returns>
        public double GetAudioSourceTimeAdjusted()
        {
            return (double) (GetAudioSourceTimeRaw() - (_gameModeData.InputDelayInMS / 1000.0));
        }

        public static void PauseSong()
        {
            Instance.audioSource.Pause();
        }
            
        public static void PlaySong()
        {
            Instance.audioSource.Play();
        }
    }
}
