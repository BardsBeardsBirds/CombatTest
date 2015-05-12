using UnityEngine;
using System.Collections;

public class MoveInDirection : MonoBehaviour
{
    public Vector3 speed = Vector3.one;

    void Update()
    {
        transform.position += speed * Time.deltaTime;
    }
}
