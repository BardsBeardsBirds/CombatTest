using System;
using UnityEngine;

public abstract class Performance : MonoBehaviour
{
    public float DamagePercentage = 1f;
    public float CoolDownTimer = 0f;

    public abstract void Cast(Transform target);
    public abstract void SetCurrentPerformance();
//    public abstract void Damage();
    public abstract void Damage(Transform target);
}