using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacman : MonoBehaviour
{
    public Movement movement { get; private set; }
    private void Awake()
    {
        movement = GetComponent<Movement>();
    }

    private void Update()
    {
        // Up Movement
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            this.movement.SetDirection(Vector2.up);
        }
        // Down Movement
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            this.movement.SetDirection(Vector2.down);
        }
        // Left Movement
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            this.movement.SetDirection(Vector2.left);
        }
        // Right Movement
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            this.movement.SetDirection(Vector2.right);
        }
    }

}
