using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuRandomMove : MonoBehaviour
{
    // Define the boundaries for movement
    public float maxX = 25.5f;
    public float minX = -25.5f;
    public float minY = -16f;
    public float maxY = 16f;

    // Speed of movement and rotation
    public float moveSpeed = 2f;
    public float rotationSpeed = 100f;

    // Speed ranges
    public float minMoveSpeed = 20f;
    public float maxMoveSpeed = 25f;
    public float minRotationSpeed = 5f;
    public float maxRotationSpeed = 10f;

    // Current direction of movement
    private Vector2 direction;

    void Start()
    {
        // Randomize speeds for each instance
        moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
        rotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);
        
        // Randomly choose rotation direction
        rotationSpeed *= (Random.value > 0.5f) ? 1 : -1;
        
        // Randomly assign an initial direction
        AssignRandomDirection();
    }

    // Update is called once per frame
    void Update()
    {
        // Move the player in the current direction
        transform.Translate(direction * moveSpeed * Time.deltaTime);

        // Rotate the player in a circle
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Check for collisions with the screen edges and bounce with random angle
        if (transform.position.x <= minX || transform.position.x >= maxX)
        {
            direction.x = -direction.x;
            // Add random angle deviation for more interesting bounces
            float randomDeviation = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
            direction = Rotate(direction, randomDeviation);
            direction = direction.normalized;
            
            float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
            transform.position = new Vector2(clampedX, transform.position.y);
        }
        if (transform.position.y <= minY || transform.position.y >= maxY)
        {
            direction.y = -direction.y;
            // Add random angle deviation for more interesting bounces
            float randomDeviation = Random.Range(-30f, 30f) * Mathf.Deg2Rad;
            direction = Rotate(direction, randomDeviation);
            direction = direction.normalized;
            
            float clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
            transform.position = new Vector2(transform.position.x, clampedY);
        }
    }

    // Assign a random direction
    void AssignRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
    }

    // Helper method to rotate a vector by an angle
    private Vector2 Rotate(Vector2 v, float angle)
    {
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
}