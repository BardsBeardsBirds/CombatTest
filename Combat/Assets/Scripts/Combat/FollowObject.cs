using UnityEngine;
using System.Collections;

public class FollowObject : MonoBehaviour {

    public string ObjectTag;
    public float Speed = 1.0f;

    private GameObject _objectReference;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	    if(_objectReference == null)
        {
            _objectReference = GameObject.FindGameObjectWithTag(ObjectTag);
        }
        else
            transform.position += (_objectReference.transform.position - transform.position).normalized * Speed * Time.deltaTime;
	}
}
