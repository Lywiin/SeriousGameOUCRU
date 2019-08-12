using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    /*** PUBLIC VARIABLES ***/

    public Sound[] sounds;


    /*** PRIVATE VARIABLES ***/



    /*** INSTANCE ***/

    private static AudioManager _instance;
    public static AudioManager Instance { get { return _instance; } }


    /***** MONOBEHAVIOUR FUNCTIONS *****/

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }

        DontDestroyOnLoad(gameObject);

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();

            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.playOnAwake = false;
        }
    }


    /***** SOUNDS FUNCTIONS *****/

    public Sound FindSound(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        return s;
    }

    public Sound Play(string name)
    {
        Sound s = FindSound(name);
        if (s == null)
            return null;

        s.source.Play();
        return s;
    }

    public Sound SmoothPlay(string name, float duration)
    {
        Sound s = FindSound(name);
        if (s == null)
            return null;

        s.source.Play();
        StartCoroutine(SoundFadeIn(s, duration));
        return s;
    }

    public void SmoothStop(string name, float duration)
    {
        Sound s = FindSound(name);
        StartCoroutine(SoundFadeOut(s, duration));
    }

    public IEnumerator SoundFadeIn(Sound s, float duration)
    {
        float targetVolume = s.volume;
        s.source.volume = 0f;

        float step = targetVolume / (duration * 10f);

        while (s.source.volume < targetVolume)
        {
            s.source.volume += step;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public IEnumerator SoundFadeOut(Sound s, float duration)
    {        
        float targetVolume = 0f;
        s.source.volume = s.volume;

        float step = s.source.volume / (duration * 10f);

        while (s.source.volume > targetVolume)
        {
            s.source.volume -= step;
            yield return new WaitForSeconds(0.1f);
        }

        s.source.Stop();
    }

    // public void PlayGameTheme()
    // {
    //     Sound s = FindSound("GameTheme");

    //     if (!s.source.isPlaying)
    //     {
    //         SmoothPlay("GameTheme", 2f);
    //     }
    // }
}
