using System;
using UnityEngine;

public class ToneWaterfall : Performance
{
    public GameObject Projectile;
    public float Cooldown = 0.2f;

    private bool _canShoot = true;

    public void Awake()
    {
        Projectile = Resources.Load("Prefabs/Projectile") as GameObject;
    }

    public void ResetShot()
    {
        _canShoot = true;
    }

    public override void Cast(Transform target)
    {
        if (_canShoot)
        {
            GameObject player = CombatManager.Instance.Player;

            DamageOnTriggerEnter bullet = Instantiate(Projectile.GetComponent<DamageOnTriggerEnter>(), player.transform.position + player.transform.forward + Vector3.up * .14f, player.transform.rotation) as DamageOnTriggerEnter;
        //    Weapon wep = GetComponentInChildren<Weapon>();
      //      Debug.Log(wep.WeaponDamage());
            //bullet.DamageAmount = wep.WeaponDamage() * DamagePercentage;
            //bullet.Target = target;
            bullet.gameObject.GetComponent<MoveForward>().Target = target;

            _canShoot = false;
            Invoke("ResetShot", Cooldown);
        }
    }

    public override void SetCurrentPerformance()
    {
        CombatManager.Instance.CurrentPerformance = PerformanceEnum.ToneWaterfall;
    }

    public override void Damage(Transform target)
    {

    }
}

