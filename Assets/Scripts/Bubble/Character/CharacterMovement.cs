using System;
using Bubble.Character.Interface;
using JetBrains.Annotations;
using ProtoLib.Library.Mono.Helper;
using ProtoLib.Library.Mono.Scripting;
using UnityEngine;

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
        public float initialPullForce = 10f; // Adjustable initial pull force
        public float maxPullSpeed = 15f; // Maximum speed for the pull
        private Vector2 grappleTargetPoint;
        
        private CharacterInteractions _inter;
        private void Start()
        {
            _inter = this.ScriptManager.GetScriptComponent<CharacterInteractions>();
            _inter.API.Get<Action>("onShootGrapple").Subscribe(Grapple);
            _inter.API.Get<Action>("onReleaseGrapple").Subscribe(ReleaseGrapple);
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

            isGrappling = true;
        }

        public void ReleaseGrapple()
        {
            // Destroy the SpringJoint2D component
            if (springJoint != null)
            {
                Destroy(springJoint);
            }

            isGrappling = false;
        }

        // private void Update()
        // {
        //     if (isGrappling)
        //     {
        //         ApplyPullForce();
        //     }
        // }

        private void ApplyPullForce()
        {
            // Calculate the direction to the grapple target
            Vector2 direction = (grappleTargetPoint - rigidbody.position).normalized;

            // Calculate the distance to the target
            float distance = Vector2.Distance(rigidbody.position, grappleTargetPoint);

            // Apply a force proportional to the distance
            float forceMagnitude = Mathf.Clamp(distance * initialPullForce, 0, maxPullSpeed);
            rigidbody.AddForce(direction * forceMagnitude, ForceMode2D.Force);
        }
    }
}