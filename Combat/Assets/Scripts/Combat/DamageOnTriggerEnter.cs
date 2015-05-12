using System.Collections;
using UnityEngine;

public class DamageOnTriggerEnter : MonoBehaviour
{
    public float DamageAmount = 1f;
    public Transform Target;

    void Start()
    {
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag != "Player")
        {

            float baseDamage = CombatManager.Instance.PerformanceDamage[PerformanceEnum.ToneWaterfall];
            float calculatedDamage = CombatManager.Instance.DamageCalculations.CalculateDamage(baseDamage);


            col.gameObject.GetComponent<HP>().Damage(calculatedDamage);
            col.gameObject.GetComponent<HP>().ShowDamage(calculatedDamage);

            Destroy(gameObject);
        }
    }
}