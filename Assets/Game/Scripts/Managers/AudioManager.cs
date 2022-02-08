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
    public AudioSource menuMusic;
    public AudioSource levelMusic;

    [Header("Cubey Audio")]
    public AudioSource[] cubeyJump;
    public AudioSource[] cubeyLand;
    public AudioSource[] cubeyPowerUp;
    public AudioSource[] cubey;
    public AudioSource[] itemPickup;
    public AudioSource[] snowThrowing;
    public AudioSource[] cubeyLandingSnow;
    public AudioSource[] cubeyExitOpen;
    public AudioSource[] cubeyCelebtration;

    public Camera cam;
    public AudioListener audioListener;

    public bool allowSounds;
    public bool allowMusic;

    public Toggle musicMute, audioToggle;

    private void Start()
    {
        if (PlayerPrefs.HasKey("music"))
        {
            var m = PlayerPrefs.GetInt("music");
            if (m == 1)
            {
                //ToggleMusic(false);
                musicMute.isOn = false;
                allowMusic = true;

                if (menuMusic != null)
                    PlayMusic(menuMusic);

                if (levelMusic != null)
                    PlayMusic(levelMusic);
            }
            else
            {
                //ToggleMusic(true);
                musicMute.isOn = true;
                allowMusic = false;

                if (menuMusic != null)
                    StopMusic(menuMusic);

                if (levelMusic != null)
                    StopMusic(levelMusic);
            }
        }

        if (PlayerPrefs.HasKey("sounds"))
        {
            var m = PlayerPrefs.GetInt("sounds");
            if (m == 1)
            {
                //ToggleSounds(true);
                audioToggle.isOn = false;
                allowSounds = true;
            }
            else
            {
                //ToggleSounds(false);
                audioToggle.isOn = true;
                allowSounds = false;
            }
        }

        if (cam == null)
        {
            cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            audioListener = cam.GetComponent<AudioListener>();
        }

        if (allowSounds)
            PlayButtonAudio(cubeyCubey);

    }

    public void ToggleSounds(bool on)
    {
        allowSounds = !on;

        if (santaSleighBells != null)
            santaSleighBells.enabled = !on;
        if (cubeySnowthrow != null)
            cubeySnowthrow.enabled = !on;

        if (on)
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
        var n = ChooseRandomClip(audio.Length);
        if (audio[n] == null)
        {
            Debug.Log("No audio clips found");
            return;
        }
        else if (!audio[n].isPlaying && allowSounds)
        {
            audio[n].Play();
        }
        else
        {
            // Debug.LogError("Audio issue! - " + audio[n].name);
        }


    }

    public void StopAudio(AudioSource[] audio)
    {
        for (int i = 0; i < audio.Length; i++)
        {
            if (audio[i].isPlaying)
                audio[i].Stop();
        }
    }

    public void MuteAudio(AudioSource audio, bool on)
    {
        audio.mute = on;
    }

    public void PlayMusic(AudioSource music)
    {
        if (!music.isPlaying && allowMusic)
            music.Play();
        //else
        //    music.Stop();
    }

    public void StopMusic(AudioSource music)
    {
        if (music.isPlaying)
            music.Stop();
    }

    public void ToggleMusic(bool on)
    {
        allowMusic = !on;

        if (!on)
        {
            if (menuMusic != null)
                PlayMusic(menuMusic);

            if (levelMusic != null)
                PlayMusic(levelMusic);

            PlayerPrefs.SetInt("music", 1);
        }
        else
        {
            if (menuMusic != null)
                StopMusic(menuMusic);

            if (levelMusic != null)
                StopMusic(levelMusic);

            PlayerPrefs.SetInt("music", 0);
        }

    }


}
