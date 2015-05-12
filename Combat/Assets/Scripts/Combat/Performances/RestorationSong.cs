using System;
using UnityEngine;

public class RestorationSong : Performance
{
    public override void Cast(Transform target)
    {
        CombatManager.Instance.ThisPerformanceAudio.IsPlayingPerformance = true;
    }

    public override void SetCurrentPerformance()
    {
        CombatManager.Instance.CurrentPerformance = PerformanceEnum.RestorationSong;
    }

    public override void Damage(Transform target)
    {

    }
}