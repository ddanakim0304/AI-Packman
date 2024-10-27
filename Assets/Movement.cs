using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{

    public float speed = 8.0f;
    public float speedMultiplier = 1.0f;
    public Vector2 initialDirection;
    public LayerMask obsticleLayer;
    public Rigidbody2D rb { get; private set; }

    private void Awake()
    {
        this.rb = GetComponent<Rigidbody2D>();
    }


}
