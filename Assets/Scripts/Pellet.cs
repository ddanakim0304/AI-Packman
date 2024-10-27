using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour
{
    public int points = 10;

    protected virtual void Eat()
    {
        FindObjectOfType<GameManager>().PelletEaten(this);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Eat();
        }
    }
}
