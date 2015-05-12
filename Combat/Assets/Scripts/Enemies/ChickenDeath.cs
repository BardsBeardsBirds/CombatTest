using UnityEngine;
using System.Collections;


public class ChickenDeath : Death
{
    public GameObject ParticleEffect;
    public string AnimationTrigger = "Death";

    private Animator _animator;
    public void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public override void Die()
    {
        base.Die();

        _animator.SetTrigger(AnimationTrigger);

        StartCoroutine(CheckAnimationDone());
    }

    IEnumerator CheckAnimationDone()
    {

        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Death"))
        {
            //Debug.Log(_animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));
            yield return null;
        }

        while (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1.0f)
        {
            //Debug.Log(_animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));
            yield return null;
        }

        Instantiate(ParticleEffect, transform.position, transform.rotation);

        Destroy(gameObject);
    }
}

