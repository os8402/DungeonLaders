using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform _target;
    void Start()
    {
        
    }

    void Update()
    {
    
        Vector3 targetPos = _target.position.normalized;

        float dot = Vector3.Dot(targetPos, transform.up);

        Debug.Log($"{gameObject.name}  :  {dot}");
    }
}
