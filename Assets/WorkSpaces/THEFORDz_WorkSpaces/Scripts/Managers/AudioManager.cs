using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : SingletonPersistent<AudioManager>
{
    [Header("Mixer")]
    public AudioMixer _mixer;
    
    [Header("Music")]
    [SerializeField]
    private AudioSource m_introSource;
    [SerializeField]
    private AudioSource m_gameplaySource;
    
    [Header("SFX")]
    [SerializeField]
    private AudioSource m_sfxSource;
    
    private readonly float k_volumeSteps = 0.01f;

    private float m_maxMusicVolume = .5f;

    
    public enum MusicName
    {
        intro,
        gameplay
    };
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("MasterVol"))
        {
            _mixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));

        }
        if (PlayerPrefs.HasKey("MusicVol"))
        {
            _mixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
        }
        if (PlayerPrefs.HasKey("SFXVol"))
        {
            _mixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
        }
        PlayMusic(MusicName.intro);
    }
    
    public void PlaySoundEffect(AudioClip clip)
    {
        m_sfxSource.PlayOneShot(clip);
    }

    public void StopSoundEffect(AudioClip clip)
    {
        m_sfxSource.Stop();
    }
    
    public void PlayMusic(MusicName musicToPlay)
    {
        if (musicToPlay == MusicName.intro)
        {
            m_introSource.enabled = true;
            m_introSource.Play();

            m_gameplaySource.Stop();
            m_gameplaySource.enabled = false;
        }
        else
        {
            m_gameplaySource.enabled = true;
            m_gameplaySource.Play();

            m_introSource.Stop();
            m_introSource.enabled = false;
        }
    }
    
    public void SwitchToGameplayMusic()
    {        
        m_gameplaySource.volume = 0f;
        m_gameplaySource.enabled = true;
        m_gameplaySource.Play();

        StartCoroutine(SwitchMusicToPlay(MusicName.gameplay));
    }

    public void SwitchToIntroMusic()
    {
        m_introSource.enabled = true;
        m_introSource.Play();
        
        StartCoroutine(SwitchMusicToPlay(MusicName.intro));
    }
    
    private IEnumerator SwitchMusicToPlay(MusicName musicToPlay)
    {
        yield return FadeInMusicToPlayFadeOutCurrentMusic(musicToPlay);

        StopAudioOfCurrentMusic(musicToPlay);
    }
    
    private IEnumerator FadeInMusicToPlayFadeOutCurrentMusic(MusicName musicToPlay)
    {
        float volume = 0f;
        // Repeat until the volume go up to the max
        while (volume <= m_maxMusicVolume)
        {
            if (musicToPlay == MusicName.intro)
            {
                m_introSource.volume += k_volumeSteps;
                m_gameplaySource.volume -= k_volumeSteps;
            }
            else
            {
                m_introSource.volume -= k_volumeSteps;
                m_gameplaySource.volume += k_volumeSteps;
            }
            yield return new WaitForEndOfFrame();
        }
    }
    
    private void StopAudioOfCurrentMusic(MusicName musicToPlay)
    {
        if (musicToPlay == MusicName.intro)
        {
            m_gameplaySource.Stop();
            m_gameplaySource.enabled = false;
        }
        else
        {
            m_introSource.Stop();
            m_introSource.enabled = false;
        }
    }

    public void StopMusic()
    {
        m_gameplaySource.Stop();
    }
}
