using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSoundClick : MonoBehaviour
{
    [SerializeField] 
    private AudioClip click;
    
    public void PlayClickSound()
    {
        AudioManager.Instance.PlaySoundEffect(click);
    }
}
