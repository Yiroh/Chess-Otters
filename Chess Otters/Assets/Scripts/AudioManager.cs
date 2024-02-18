using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Static Instances
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get 
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    instance = new GameObject("Spawned AudioManager", typeof(AudioManager)).GetComponent<AudioManager>();
                }
            }
            
            return instance;
        }
        private set
        {
            instance = value;
        }
    }
    #endregion

    #region Fields
    private AudioSource musicSource;
    private AudioSource musicSource2; // For transitions
    private AudioSource sfxSource;

    private float musicVolume = 1.0f; // TODO add UI to change this.
    private float sfxVolume = 1.0f;

    private bool firstMusicSourcePlaying;
    #endregion

    private void Awake ()
    {
        // Make sure to not destroy this instance!
        DontDestroyOnLoad(this.gameObject);

        // Create audio sources
        musicSource = this.gameObject.AddComponent<AudioSource>();
        musicSource2 = this.gameObject.AddComponent<AudioSource>();
        sfxSource = this.gameObject.AddComponent<AudioSource>();

        // Set Volume
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);

        // Loop music trakcs
        musicSource.loop = true;
        musicSource2.loop = true;
    }

    public void PlayMusic (AudioClip musicClip)
    {
        // Which source is playing?
        AudioSource activeSource = (firstMusicSourcePlaying) ? musicSource : musicSource2;

        activeSource.clip = musicClip;
        activeSource.volume = 1;
        activeSource.Play();
    }
    public void PlayMusicWithFade(AudioClip newClip, float transitionTime = 1.0f)
    {
        // Which source is playing?
        AudioSource activeSource = (firstMusicSourcePlaying) ? musicSource : musicSource2;

        StartCoroutine(UpdateMusicWithFade(activeSource, newClip, transitionTime));
    }
    public void PlayMusicWithCrossFade(AudioClip musicClip, float transitionTime = 1.0f)
    {
        AudioSource activeSource = (firstMusicSourcePlaying) ? musicSource : musicSource2;
        AudioSource newSource = (firstMusicSourcePlaying) ? musicSource2 : musicSource;

        // Swap source
        firstMusicSourcePlaying = !firstMusicSourcePlaying;

        // Set fields
        newSource.clip = musicClip;
        newSource.Play();
        StartCoroutine(UpdateMusicWithCrossFade(activeSource, newSource, transitionTime));
    }
    private IEnumerator UpdateMusicWithFade(AudioSource activeSource, AudioClip newClip, float transitionTime)
    {
        // Make sure source is active and playing
        if(!activeSource.isPlaying)
        {
            activeSource.Play();
        }

        float t = 0.0f;

        // Fade Out
        for (t = 0; t <= transitionTime; t += Time.deltaTime)
        {
            activeSource.volume = (musicVolume - ((t/ transitionTime) * musicVolume));
            yield return null;
        }

        activeSource.Stop();
        activeSource.clip = newClip;
        activeSource.Play();

        // Fade In
        for (t = 0; t < transitionTime; t += Time.deltaTime)
        {
            activeSource.volume = (t / transitionTime) * musicVolume;
            yield return null;
        }
    }
    private IEnumerator UpdateMusicWithCrossFade(AudioSource original, AudioSource newSource, float transitionTime)
    {
        float t = 0.0f;

        for(t = 0.0f; t <= transitionTime; t += Time.deltaTime)
        {
            original.volume = (musicVolume - ((t/ transitionTime) * musicVolume));
            newSource.volume = (t / transitionTime) * musicVolume;
            yield return null;
        }

        original.Stop();
    }


    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
    public void PlaySFX(AudioClip clip, float volume)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        musicSource2.volume = volume;
    }
    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
