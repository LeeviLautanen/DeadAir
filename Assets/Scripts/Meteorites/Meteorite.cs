using System.Collections.Generic;
using UnityEngine;

public class Meteorite : MonoBehaviour
{
    public int Damage = 5;
    public float Speed = 10f;

    void Start()
    {
    }

    void FixedUpdate()
    {
        transform.position += Speed * Time.deltaTime * transform.forward;
    }

    void OnDestroy()
    {
    }
}
