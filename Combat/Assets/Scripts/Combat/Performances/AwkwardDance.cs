using System;
using UnityEngine;

public class AwkwardDance : Performance
{
    public override void Cast(Transform target)
    {
        CombatManager.Instance.ThisPerformanceAudio.IsPlayingPerformance = true;

    }

    public override void SetCurrentPerformance()
    {
        CombatManager.Instance.CurrentPerformance = PerformanceEnum.AwkwardDance;
    }

    public override void Damage(Transform target)
    {
        float baseDamage = CombatManager.Instance.PerformanceDamage[PerformanceEnum.AwkwardDance];
        float calculatedDamage = CombatManager.Instance.DamageCalculations.CalculateDamage(baseDamage);

        target.GetComponent<HP>().Damage(calculatedDamage);
        target.GetComponent<HP>().ShowDamage(calculatedDamage);
    }
}