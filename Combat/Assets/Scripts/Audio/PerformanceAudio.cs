using UnityEngine;
using UnityEngine.Audio;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public class PerformanceAudio : MonoBehaviour
{
    private AudioSource _performanceAudioSource;

   // public AudioMixerSnapshot StridentStringCutSnapshot;
    public bool IsPlayingPerformance;

    public Dictionary<PerformanceEnum, AudioClip> PerformanceSoundClips = new Dictionary<PerformanceEnum, AudioClip>() { };

    public void Awake()
    {
        _performanceAudioSource = GameObject.Find("PerformanceAudioSource").GetComponent<AudioSource>();

        PerformanceSoundClips.Add(PerformanceEnum.AwkwardDance, Resources.Load("Audio/Performances/Awkward Dance") as AudioClip);
        PerformanceSoundClips.Add(PerformanceEnum.RestorationSong, Resources.Load("Audio/Performances/Restoration Song") as AudioClip);
        PerformanceSoundClips.Add(PerformanceEnum.StridentStringCut, Resources.Load("Audio/Performances/Strident String Cut") as AudioClip);
    }
    
    public void Update()
    {
        if (IsPlayingPerformance)
            PlayPerformance();
    }

    public void PlayPerformance()
    {
        _performanceAudioSource.clip = PerformanceSoundClips[CombatManager.Instance.CurrentPerformance] as AudioClip;
        _performanceAudioSource.Play();
        IsPlayingPerformance = false;
    }
}