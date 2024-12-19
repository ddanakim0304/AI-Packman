using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;
    public Pacman pacman;
    public Transform pellets;
    public GameObject gameCompletedCanvas;
    public GameObject gameOverCanvas;

    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; }
    public int lives { get; private set; } = 3;

    private void Start()
    {
        gameCompletedCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);
        Time.timeScale = 1;
        // Start the first round when the scene starts
        NewRound();
    }

    private void Update()
    {
        // Check if lives are depleted and pause the game
        if (this.lives <= 0)
        {
            GameOver();
        }
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
        gameCompletedCanvas.SetActive(false);
        gameOverCanvas.SetActive(false);

        ResetGhostMultiplier();
        
        // Activate all ghosts
        foreach (Ghost ghost in this.ghosts)
        {
            ghost.ResetState();
        }

        // Activate Pacman
        pacman.ResetState();
    }

    private void GameOver()
    {
        gameOverCanvas.SetActive(true);
        // Deactivate all ghosts
        foreach (Ghost ghost in this.ghosts)
        {
            ghost.gameObject.SetActive(false);
        }

        // Deactivate Pacman
        this.pacman.gameObject.SetActive(false);

        // Stop the game
        Time.timeScale = 0;
    }

    private void SetScore(int score)
    {
        // Update the score
        this.score = score;
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * this.ghostMultiplier;
        // Increase score when a ghost is eaten
        SetScore(this.score + points);
        this.ghostMultiplier++;
    }

    public void PacmanEaten()
    {
        // Deactivate Pacman and reduce lives by one
        pacman.gameObject.SetActive(false);
        SetLives(lives - 1);

        if (lives > 0)
        {
            // Reset game state after 3 seconds if lives remain
            Invoke(nameof(ResetState), 2.0f);
        }
        else
        {
            // End the game if no lives remain
            GameOver();
        }
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        SetScore(this.score + pellet.points);

        if (!HasRemainingPellets())
        {
            GameCompleted();
        }
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        foreach (Ghost ghost in this.ghosts)
        {
            ghost.frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        CancelInvoke(nameof(ResetGhostMultiplier));
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        // Check if any pellets are still active
        foreach (Transform pellet in this.pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    private void GameCompleted()
    {
        // Show the Game Completed Canvas
        if (gameCompletedCanvas != null)
        {
            gameCompletedCanvas.SetActive(true);
        }

        // Stop the game
        Time.timeScale = 0;
    }

    private void ResetGhostMultiplier()
    {
        this.ghostMultiplier = 1;
    }

    private void SetLives(int lives)
    {
        // Update the number of lives
        this.lives = lives;
    }
}
