using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;
    public Pacman pacman;
    public Transform pellets;

    public int score { get; private set; }
    public int lives { get; private set; }

    private void Start()
    {
        // Initialize a new game when the scene starts
        NewGame();
    }

    private void Update()
    {
        // Restart the game if lives are depleted and any key is pressed
        if (this.lives <= 0 && Input.anyKeyDown)
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        // Reset score and lives, then start a new round
        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        // Reactivate all pellets for the new round
        foreach (Transform pellet in this.pellets)
        {
            pellet.gameObject.SetActive(true);
        }

        // Reset positions and states of Pacman and ghosts
        ResetState();
    }

    private void ResetState()
    {
        // Activate all ghosts
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].gameObject.SetActive(true);
        }

        // Activate Pacman
        this.pacman.gameObject.SetActive(true);
    }

    private void GameOver()
    {
        // Deactivate all ghosts
        for (int i = 0; i < ghosts.Length; i++)
        {
            ghosts[i].gameObject.SetActive(false);
        }

        // Deactivate Pacman
        this.pacman.gameObject.SetActive(false);
    }

    private void SetScore(int score)
    {
        // Update the score
        this.score = score;
    }

    private void SetLives(int lives)
    {
        // Update the number of lives
        this.lives = lives;
    }

    public void GhostEaten(Ghost ghost)
    {
        // Increase score when a ghost is eaten
        SetScore(this.score + ghost.points);
    }

    public void PacmanEaten()
    {
        // Deactivate Pacman and reduce lives by one
        this.pacman.gameObject.SetActive(false);
        SetLives(this.lives - 1);

        if (this.lives > 0)
        {
            // Reset game state after 3 seconds if lives remain
            Invoke(nameof(ResetState), 3.0f);
        }
        else
        {
            // End the game if no lives remain
            GameOver();
        }
    }
}
