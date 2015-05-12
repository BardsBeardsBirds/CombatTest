using System;
using UnityEngine;
using UnityEngine.UI;

public class PerformanceData
{
    public void LoadPerformanceData()
    {
        CombatManager.Instance.PerformanceDuration.Add(PerformanceEnum.EmptyPerformance, 0f);
        CombatManager.Instance.PerformanceDuration.Add(PerformanceEnum.AwkwardDance, 3.2f);
        CombatManager.Instance.PerformanceDuration.Add(PerformanceEnum.RestorationSong, 6.8f);
        CombatManager.Instance.PerformanceDuration.Add(PerformanceEnum.StridentStringCut, 3f);
        CombatManager.Instance.PerformanceDuration.Add(PerformanceEnum.ToneWaterfall, .2f);

        CombatManager.Instance.PerformanceCooldownDuration.Add(PerformanceEnum.EmptyPerformance, 0f);
        CombatManager.Instance.PerformanceCooldownDuration.Add(PerformanceEnum.AwkwardDance, 8f);
        CombatManager.Instance.PerformanceCooldownDuration.Add(PerformanceEnum.RestorationSong, 12f);
        CombatManager.Instance.PerformanceCooldownDuration.Add(PerformanceEnum.StridentStringCut, 3f);
        CombatManager.Instance.PerformanceCooldownDuration.Add(PerformanceEnum.ToneWaterfall, 2f);

        CombatManager.Instance.PerformanceRange.Add(PerformanceEnum.EmptyPerformance, 0f);
        CombatManager.Instance.PerformanceRange.Add(PerformanceEnum.AwkwardDance, 4f);
        CombatManager.Instance.PerformanceRange.Add(PerformanceEnum.RestorationSong, 2f);
        CombatManager.Instance.PerformanceRange.Add(PerformanceEnum.StridentStringCut, 4f);
        CombatManager.Instance.PerformanceRange.Add(PerformanceEnum.ToneWaterfall, 10f);

        CombatManager.Instance.PerformanceDamage.Add(PerformanceEnum.EmptyPerformance, 0f);
        CombatManager.Instance.PerformanceDamage.Add(PerformanceEnum.AwkwardDance, 5f);
        CombatManager.Instance.PerformanceDamage.Add(PerformanceEnum.RestorationSong, 0f);
        CombatManager.Instance.PerformanceDamage.Add(PerformanceEnum.StridentStringCut, 12f);
        CombatManager.Instance.PerformanceDamage.Add(PerformanceEnum.ToneWaterfall, 10f);

        CombatManager.Instance.PerformanceIcons.Add(PerformanceEnum.EmptyPerformance, CombatManager.Instance.EmptySlotIcon);
        CombatManager.Instance.PerformanceIcons.Add(PerformanceEnum.AwkwardDance, CombatManager.Instance.AwkwardDanceIcon);
        CombatManager.Instance.PerformanceIcons.Add(PerformanceEnum.RestorationSong, CombatManager.Instance.RestorationSongIcon);
        CombatManager.Instance.PerformanceIcons.Add(PerformanceEnum.StridentStringCut, CombatManager.Instance.StridentStringCutIcon);
        CombatManager.Instance.PerformanceIcons.Add(PerformanceEnum.ToneWaterfall, CombatManager.Instance.ToneWaterfallIcon);
    }
}