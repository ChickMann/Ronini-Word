using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip swordSound;
    [SerializeField] private AudioClip footStep;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    public void PlaySwordSound()
    {
        audioSource.PlayOneShot(swordSound);
     
    }
    public void PlayFootStep(bool isPlay)
    {
       
        if (isPlay)
        {
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
        }
    }
}
