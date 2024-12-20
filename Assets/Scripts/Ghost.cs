using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public bool TestMode = false;
    public Movement movement { get; private set; }

    public GhostHome home { get; private set; }
    public GhostScatter scatter { get; private set; }
    public GhostFrightened frightened { get; private set; }
    public IGhostChase chase { get; private set; }
    public GhostBehavior initialBehavior;

    public Transform pacman;
    public TestManager testManager;

    public int points = 200;

    private void Awake()
    {
        this.movement = GetComponent<Movement>();
        this.home = GetComponent<GhostHome>();
        this.scatter = GetComponent<GhostScatter>();
        this.chase = GetComponent<IGhostChase>();
        this.frightened = GetComponent<GhostFrightened>();
    }

    private void Start()
    {
        ResetState();
    }

    public void ResetState()
    {
        this.gameObject.SetActive(true);
        this.movement.ResetState();

        this.frightened.Disable();
        this.chase.Disable();
        this.scatter.Enable();

        if (this.home != this.initialBehavior) {
            this.home.Disable();
        }

        if (this.initialBehavior != null) {
            this.initialBehavior.Enable();
        }
    }

private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Player"))
    {
        if (this.frightened.enabled)
        {
            // Check for GameManager or TestManager and call the appropriate method
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.GhostEaten(this);
                return;
            }
        }
        else
        {
            // Check for GameManager or TestManager and call the appropriate method
            var gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.PacmanEaten();
                return;
            }

            if (testManager != null)
            {
                testManager.IncrementPacmanCaught();
            }
        }
    }
}


    public void SetPosition(Vector3 position)
    {
        // Keep the z-position the same since it determines draw depth
        position.z = transform.position.z;
        transform.position = position;
    }
}
