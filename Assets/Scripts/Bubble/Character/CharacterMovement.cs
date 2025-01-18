using System;
using Bubble.Character.Interface;
using JetBrains.Annotations;
using ProtoLib.Library.Mono.Helper;
using ProtoLib.Library.Mono.Scripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bubble.Character
{
    public class CharacterMovement : ScriptComponent<ICharacterComponent>
    {
        
        public new Rigidbody2D rigidbody;
        private bool isGrappling = false;
        private SpringJoint2D springJoint;

        [Header("Grapple Settings")]
        public float springFrequency = 1f; // Controls the speed of the pull
        public float dampingRatio = 1f;   // Controls the oscillation of the pull
        public float grapplePullDistance = 0.1f; // How close the player gets to the anchor point
        public float dashForce = 10f; // Adjustable initial pull force
        public float maxPullSpeed = 15f; // Maximum speed for the pull
        private Vector2 grappleTargetPoint;
        
        [Header("Line Renderer Settings")]
        public LineRenderer lineRenderer; // Reference to the LineRenderer component
        public Texture[] grappleTextures; // Array of replaceable textures for the line
        public float textureScrollSpeed = 1f; // Speed at which the texture scrolls along the line
        
        private CharacterInteractions _inter;
        private void Start()
        {
            _inter = this.ScriptManager.GetScriptComponent<CharacterInteractions>();
            _inter.API.Get<Action>("onShootGrapple").Subscribe(Grapple);
            _inter.API.Get<Action>("onReleaseGrapple").Subscribe(ReleaseGrapple);
            _inter.API.Get<Action>("onDash").Subscribe(OnDash);
            // Ensure the LineRenderer is disabled initially
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }
        }

        public Vector2 GetMousePosition()
        {
            if (Camera.main != null) return Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return Vector2.zero;
        }

        public void Grapple()
        {
            // 1. Get Mouse Position
            Vector2 targetPoint = GetMousePosition();
            grappleTargetPoint = targetPoint;

            // 2. Create and configure SpringJoint2D
            springJoint = gameObject.AddComponent<SpringJoint2D>();
            springJoint.autoConfigureDistance = false;
            springJoint.distance = grapplePullDistance; // Set a small distance to pull the player towards the anchor
            springJoint.frequency = springFrequency;    // Adjust the speed of the pull
            springJoint.dampingRatio = dampingRatio;    // Reduce oscillation

            // 3. Set anchor points
            springJoint.anchor = Vector2.zero; // Anchor at the player's center
            springJoint.connectedAnchor = targetPoint; // Anchor at the mouse position
            springJoint.enableCollision = true; // Enable collision between the player and the anchor
            
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2; // Two points: player and anchor
                lineRenderer.textureMode = LineTextureMode.Tile; // Tile the texture along the line

                // Set a random texture from the array (or use a specific one)
                if (grappleTextures.Length > 0)
                {
                    lineRenderer.material.mainTexture = grappleTextures[Random.Range(0, grappleTextures.Length)];
                }
            }

            isGrappling = true;
        }

        public void ReleaseGrapple()
        {
            // Destroy the SpringJoint2D component
            if (springJoint != null)
            {
                Destroy(springJoint);
            }
            
            // Disable the LineRenderer
            if (lineRenderer != null)
            {
                lineRenderer.enabled = false;
            }

            isGrappling = false;
        }

        public void OnDash()
        {
            rigidbody.linearVelocity = Vector2.zero;
            Vector2 direction = (GetMousePosition() - rigidbody.position).normalized;

            // Calculate the distance to the target
            float distance = Vector2.Distance(rigidbody.position, grappleTargetPoint);

            // Apply a force proportional to the distance
            float forceMagnitude = Mathf.Clamp(distance * dashForce, 0, maxPullSpeed);
            rigidbody.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
        }
        
        private void UpdateLineRenderer()
        {
            if (lineRenderer != null)
            {
                // Update the positions of the LineRenderer
                lineRenderer.SetPosition(0, rigidbody.position); // Start at the player's position
                lineRenderer.SetPosition(1, grappleTargetPoint); // End at the anchor point

                // Scroll the texture along the line
                float offset = Time.time * textureScrollSpeed;
                lineRenderer.material.mainTextureOffset = new Vector2(offset, 0);
            }
        }

        private void Update()
        {
            if (isGrappling)
            {
                UpdateLineRenderer();
            }
        }
    }
}