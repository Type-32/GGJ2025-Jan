using System;
using Bubble.Audio;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bubble.NPC
{
    public enum BirdAnimationRotation
    {
        Left,
        Right
    }
    public class Birdie : MonoBehaviour
    {
        [NaughtyAttributes.MinMaxSlider(0.01f, 10f)]
        public Vector2 speedRange; // Speed of the NPC
        public float spawnOffset = 2f; // Offset to spawn outside the viewport
        public bool StopMoving = false;
        public BirdAnimationRotation rotation = BirdAnimationRotation.Left;

        private Vector2 targetPosition; // Target position to fly towards
        private bool movingRight; // Direction of movement
        private Rigidbody2D rb; // Reference to the Rigidbody2D

        private float speed = 0f;
        private SpriteRenderer renderer;

        void Start()
        {
            // Get the Rigidbody2D component
            rb = GetComponent<Rigidbody2D>();
            renderer = GetComponent<SpriteRenderer>();

            // Randomly choose to move left or right
            movingRight = Random.Range(0, 2) == 0;

            // Set the initial position outside the viewport
            SetStartPosition();

            // Set the target position on the opposite side of the screen
            SetTargetPosition();
            
            speed = Random.Range(speedRange.x, speedRange.y);

            if ((movingRight && rotation == BirdAnimationRotation.Left) || (!movingRight && rotation == BirdAnimationRotation.Right))
            {
                renderer.flipX = true;
            }
        }

        void FixedUpdate()
        {
            // Move the NPC towards the target position using Rigidbody2D
            if(!StopMoving)
                rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime));

            // Destroy the NPC if it reaches the target position
            if (rb.position == targetPosition || transform.position.y < -30)
            {
                Destroy(gameObject);
            }
        }

        void SetStartPosition()
        {
            // Get the camera's viewport bounds
            Vector2 viewportMin = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
            Vector2 viewportMax = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

            // Randomly choose a Y position within the viewport height
            float randomY = Random.Range(viewportMin.y, viewportMax.y);

            // Set the starting position outside the viewport
            if (movingRight)
            {
                rb.position = new Vector2(viewportMin.x - spawnOffset, randomY);
            }
            else
            {
                rb.position = new Vector2(viewportMax.x + spawnOffset, randomY);
            }
        }

        void SetTargetPosition()
        {
            // Get the camera's viewport bounds
            Vector2 viewportMin = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
            Vector2 viewportMax = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

            // Set the target position on the opposite side of the screen
            if (movingRight)
            {
                targetPosition = new Vector2(viewportMax.x + spawnOffset, rb.position.y);
            }
            else
            {
                targetPosition = new Vector2(viewportMin.x - spawnOffset, rb.position.y);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Game/Hook"))
            {
                AudioManager.Instance.PlayAudio("HitBird");
            }
            if(other.gameObject.layer == LayerMask.NameToLayer("Game/Hook") || other.gameObject.layer == LayerMask.NameToLayer("Game/Character"))
            {
                StopMoving = true;
                Collider2D c = this.GetComponent<Collider2D>();
                c.enabled = false;
                rb.gravityScale = 1;
                // add random angular rotation
                rb.angularVelocity = Random.Range(-360f, 360f);
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}