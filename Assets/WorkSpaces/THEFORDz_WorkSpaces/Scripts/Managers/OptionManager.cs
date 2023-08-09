using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionManager : MonoBehaviour
{
    public Toggle fullScreenToggle;

    public ResolutionItem[] reolutions;

    public int selectedResolution;

    public TMP_Text resolutionText;

    public AudioMixer _mixer;

    public Slider masterSlider, musicSlider, sfxSlider;
    public TMP_Text masterText, musicText, sfxText;

    //public AudioSource sfxLoop;

    private void Start()
    {
        fullScreenToggle.isOn = Screen.fullScreen;

        bool foundRes = false;
        for (int i = 0; i < reolutions.Length; i++)
        {
            if (Screen.width == reolutions[i].horizontal && Screen.width == reolutions[i].vertical)
            {
                foundRes = true;

                selectedResolution = i;
                
                UpdateResolutionText();
            }
        }
        if (!foundRes)
        {
            resolutionText.text = Screen.width.ToString() + " x " + Screen.height.ToString();
        }

        if (PlayerPrefs.HasKey("MasterVol"))
        {
            _mixer.SetFloat("MasterVol", PlayerPrefs.GetFloat("MasterVol"));
            masterSlider.value = PlayerPrefs.GetFloat("MasterVol");
            
        }
        if (PlayerPrefs.HasKey("MusicVol"))
        {
            _mixer.SetFloat("MusicVol", PlayerPrefs.GetFloat("MusicVol"));
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol");
            
        }
        
        if (PlayerPrefs.HasKey("SFXVol"))
        {
            _mixer.SetFloat("SFXVol", PlayerPrefs.GetFloat("SFXVol"));
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol");
            
        }
        masterText.text = (masterSlider.value + 90).ToString();
        musicText.text = (musicSlider.value + 90).ToString();
        sfxText.text = (sfxSlider.value + 90).ToString();
    }

    public void ResPrev()
    {
        selectedResolution--;
        if (selectedResolution < 0)
        {
            selectedResolution = 0;
        }

        UpdateResolutionText();
    }

    public void ResNext()
    {
        selectedResolution++;
        if (selectedResolution > reolutions.Length -1)
        {
            selectedResolution = reolutions.Length - 1;
        }

        UpdateResolutionText();
    }

    public void UpdateResolutionText()
    {
        resolutionText.text = reolutions[selectedResolution].horizontal.ToString() + " x " +
                              reolutions[selectedResolution].vertical.ToString();
    }
    
    public void Apply()
    {

        Screen.SetResolution(reolutions[selectedResolution].horizontal, reolutions[selectedResolution].vertical, fullScreenToggle.isOn);
    }

    public void SetMasterVol()
    {
        masterText.text = (masterSlider.value + 90).ToString();
        _mixer.SetFloat("MasterVol", masterSlider.value);
        PlayerPrefs.SetFloat("MasterVol", masterSlider.value);
    }

    public void SetMusicVol()
    {
        musicText.text = (musicSlider.value + 90).ToString();
        _mixer.SetFloat("MusicVol", musicSlider.value);
        PlayerPrefs.SetFloat("MusicVol", musicSlider.value);
    }
    
    public void SetSFXVol()
    {
        sfxText.text = (sfxSlider.value + 90).ToString();
        _mixer.SetFloat("SFXVol", sfxSlider.value);
        PlayerPrefs.SetFloat("SFXVol", sfxSlider.value);
    }

    /*public void PlaySFXLoop()
    {
        sfxLoop.Play();
    }
    
    public void StopSFXLoop()
    {
        sfxLoop.Stop();
    }*/
}

[System.Serializable]
public class ResolutionItem
{
    public int horizontal, vertical;
}
