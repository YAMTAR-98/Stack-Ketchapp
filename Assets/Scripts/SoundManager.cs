using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    public List<AudioClip> audioClips = new List<AudioClip>();
    public AudioClip cutSound;
    public AudioSource audioSource;
    private int index = 0;
    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public int PlaySound()
    {
        //Debug.Log(index);
        index++;
        if (index > audioClips.Count)
        {
            index = audioClips.Count - 1;
        }
        audioSource.clip = audioClips[index - 1];
        audioSource.Play();
        return index - 1;
    }
    public void PlayCutSound()
    {
        audioSource.clip = cutSound;
        audioSource.Play();
    }
    public void ResetCounter()
    {
        index = 0;
    }
}
