using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; set; }

    [Header("Menu Audio")]
    public AudioSource menuButtonForward, menuButtonBack;
    public AudioSource menuStartLevel;
    public AudioSource santaSleighBells;
    public AudioSource cubeySnowthrow;
    public AudioSource cubeyCubey;

    [Header("Music")]
    public AudioSource gameMusic;
    // public AudioSource levelMusic;

    [Header("Cubey Audio")]
    public AudioSource[] cubeyJump;
    public AudioSource[] cubeyLand;
    public AudioSource[] cubeyPowerUp;
    public AudioSource[] cubey;
    public AudioSource[] itemPickup;
    public AudioSource[] snowThrowing;
    public AudioSource[] cubeyLandingSnow;
    public AudioSource[] cubeyExitOpen;
    public AudioSource[] cubeyCelebration;

    [SerializeField] private Camera cam;
    [SerializeField] private AudioListener audioListener;

    [SerializeField] private bool allowSounds;
    public bool allowMusic;

    [SerializeField] private GameObject audioButtons;
    [SerializeField] private Toggle musicMute; 
    [SerializeField] private Toggle audioToggle;

    public GameObject AudioButtons
    {
        get => audioButtons;
        set => audioButtons = value;
    }
    
    private void Start()
    {
        var m = PlayerPrefs.GetInt("music", 1);
        
        if (m == 1) // on
        {
            musicMute.isOn = false;
            // musicMutePause.isOn = false;
            allowMusic = true;

            if (gameMusic != null)
                PlayMusic(gameMusic);

            // if (levelMusic != null)
            //     PlayMusic(levelMusic);
        }
        else
        {
            musicMute.isOn = true;
            // musicMutePause.isOn = true;
            allowMusic = false;

            if (gameMusic != null)
                StopMusic(gameMusic);

            // if (levelMusic != null)
            //     StopMusic(levelMusic);
        }
        

        var s = PlayerPrefs.GetInt("sounds", 1);
        if (s == 1)
        {
            audioToggle.isOn = false;
            // audioTogglePause.isOn = false;
            allowSounds = true;
        }
        else
        {
            audioToggle.isOn = true;
            // audioTogglePause.isOn = true;
            allowSounds = false;
        }

        if (cam == null)
        {
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            audioListener = cam.GetComponent<AudioListener>();
        }

        if (allowSounds)
        {
            PlayButtonAudio(cubeyCubey);
        }

    }

    public void ToggleSounds(bool state)
    {
        allowSounds = !state;

        if (santaSleighBells != null)
            santaSleighBells.enabled = !state;
        if (cubeySnowthrow != null)
            cubeySnowthrow.enabled = !state;

        if (state)
            PlayerPrefs.SetInt("sounds", 0);
        else
            PlayerPrefs.SetInt("sounds", 1);
    }

    public int ChooseRandomClip(int max)
    {
        var n = Random.Range(0, max);

        return n;
    }

    public void PlayButtonAudio(AudioSource audio)
    {
        if (!audio.isPlaying && allowSounds)
        {
            audio.Play();
        }
    }

    public void PlayAudio(AudioSource[] audio)
    {
        // Debug.Log("audio name: " + audio[0].name);

        var n = ChooseRandomClip(audio.Length);
        if (audio[n] == null)
        {
            Debug.Log("No audio clips found");
            return;
        }

        StopAudio(audio);

        if (!audio[n].isPlaying && allowSounds)
        {
            audio[n].Play();
        }
        /*else
        {
            // could be playing
            Debug.LogError("Audio issue! - " + audio[n].name);
        }*/
    }

    public void StopAudio(AudioSource[] audio)
    {
        for (int i = 0; i < audio.Length; i++)
        {
            if (audio[i] != null && audio[i].isPlaying)
                audio[i].Stop();
        }
    }

    /// <summary>
    /// Mute = true
    /// Unmute = false
    /// </summary>
    /// <param name="audio"></param>
    /// <param name="state"></param>
    public void MuteAudio(AudioSource audio, bool state)
    {
        if (audio != null && audio.isPlaying)
        {
            // Debug.Log("Mute audio: " + state);
            audio.mute = state;    
        }
        /*else
        {
            Debug.Log("Mute audio: " + audio + ", audio.isPlaying: " + audio.isPlaying);
        }*/
    }

    public void PlayMusic(AudioSource music)
    {
        if (!music.isPlaying && allowMusic)
        {
            music.Play();
        }
    }

    public void StopMusic(AudioSource music)
    {
        if (music.isPlaying)
        {
            music.Stop();
        }
    }

    public void ToggleMusic(bool on)
    {
        allowMusic = !on;

        if (!on) // off
        {
            if (gameMusic != null)
                PlayMusic(gameMusic);

            // if (levelMusic != null)
            //     PlayMusic(levelMusic);

            PlayerPrefs.SetInt("music", 1); // on
        }
        else
        {
            if (gameMusic != null)
                StopMusic(gameMusic);

            // if (levelMusic != null)
            //     StopMusic(levelMusic);

            PlayerPrefs.SetInt("music", 0); // off
        }
    }
}
