using UnityEngine;
using System.Collections;

public class MoveForward : MonoBehaviour
{
    public float speed = 1.0f;
    public Transform Target;
    private Vector3 _lastPositionTarget;

    public void Awake()
    {
    }

    void Update()
    {
        MoveTowardsTarget();
    }

    public void MoveTowardsTarget()
    {

        if (Target == null)
        {
            this.transform.position = Vector3.MoveTowards(transform.position, _lastPositionTarget, 10 * Time.deltaTime);
            if (this.transform.position == _lastPositionTarget)
            {
                Destroy(gameObject);
            }
            return;
        }
        this.transform.position = Vector3.MoveTowards(transform.position, Target.position, 10 * Time.deltaTime);
        _lastPositionTarget = Target.position;
    }
}