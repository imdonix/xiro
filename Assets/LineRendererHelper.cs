using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRendererHelper : MonoBehaviour
{

    LineRenderer line;
    public float alpha;
    public float speed = 5;

    void Awake()
    {
        line = gameObject.GetComponent<LineRenderer>();
        alpha = 0.5f;
    }


    float die = 0.25f;
    private void Update()
    {
        die -= Time.deltaTime;
        if(die < 0)
        {
            Destroy(gameObject);
        }
    }

    internal void SetPositions(Vector3[] vector3s)
    {
        line.SetPositions(vector3s);
    }
}
