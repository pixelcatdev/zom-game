using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public AudioClip ambBiomeCountryside;
    public AudioClip ambBiomeUrban;
    public AudioClip ambBiomeMountain;

    private AudioSource track01, track02;
    private bool isPlayingTrack01;

    public static AudioController instance;

    // Singleton Initialization
    void Awake()
    {
        if (!AudioController.instance)
        {
            AudioController.instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        track01 = gameObject.AddComponent<AudioSource>();
        track02 = gameObject.AddComponent<AudioSource>();
        isPlayingTrack01 = true;

        //SwapTrack(defaultAmbience);
    }

    public void SwitchBiomeAmbience(string biome)
    {
        if(biome == "Urban")
        {
            track02.clip = ambBiomeUrban;
        }
        else
        {
            track02.clip = ambBiomeCountryside;
        }
    }

    public void SwapTrack(AudioClip newClip)
    {
        if(isPlayingTrack01 == true)
        {
            track02.clip = newClip;
            track02.Play();
            track01.Stop();
        }
        else
        {
            track01.clip = newClip;
            track01.Play();
            track02.Stop();
        }
    }
}
