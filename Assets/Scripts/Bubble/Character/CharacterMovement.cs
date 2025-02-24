using System;
using Bubble.Audio;
using Bubble.Character.Interface;
using Bubble.Misc;
using JetBrains.Annotations;
using ProtoLib.Library.Facet;
using ProtoLib.Library.Mono.Helper;
using ProtoLib.Library.Mono.Scripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bubble.Character
{
    public class CharacterMovement : ScriptComponent<ICharacterComponent>, ICharacterComponent
    {
        public FacetAPI RelayAPI { get; } = FacetAPI.Create()
            .Callback<Action>("onHookAttached")
            .Callback<Action>("onPlayerDashed")
            .Realtime<Action<bool>>("isGrappling")
            .Build();
        public new Rigidbody2D rigidbody;
        private bool isGrappling = false;
        private bool isShootingHook = false;
        private SpringJoint2D springJoint;
        [SerializeField] private float dashCooldown = 3f;
        [SerializeField] private int hookMaxReach = 30;

        [Header("Grapple Settings")]
        public float springFrequency = 1f; // Controls the speed of the pull
        public float dampingRatio = 1f;   // Controls the oscillation of the pull
        public float grapplePullDistance = 0.1f; // How close the player gets to the anchor point
        public float dashForce = 10f; // Adjustable initial pull force
        public float maxPullSpeed = 15f; // Maximum speed for the pull
        private Vector2 grappleTargetPoint;
        public LayerMask grappleableLayer; // Layer for grappleable objects

        [Header("Hook Settings")]
        public GameObject hookPrefab; // Prefab for the hook object
        public float hookSpeed = 20f; // Speed at which the hook travels
        private HookBehaviour currentHook; // Reference to the current hook instance
        private Rigidbody2D hookRB;
        private Vector2 hookInitialDir;

        
        [Header("Line Renderer Settings")]
        public LineRenderer lineRenderer; // Reference to the LineRenderer component
        public Texture[] grappleTextures; // Array of replaceable textures for the line
        public float textureScrollSpeed = 1f; // Speed at which the texture scrolls along the line
        
        private CharacterInteractions _inter;
        private float cooling = 0f;

        private Quaternion _targetRotation = Quaternion.identity;
        private bool _stopInterpolating = false;
        private bool _stopCooling = false;
        private CharacterManager manager;
        
        private void Start()
        {
            manager = GetComponent<CharacterManager>();
            _inter = this.ScriptManager.GetScriptComponent<CharacterInteractions>();
            _inter.API.Get<Action>("onShootGrapple").Subscribe(TryShootHook);
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
        
        private void TryShootHook()
        {
            if (isGrappling || isShootingHook) return;

            // 1. Get Mouse Position
            grappleTargetPoint = GetMousePosition();

            // 2. Check if the target point is on a "Grappleable" layer
            RaycastHit2D hit = Physics2D.Raycast(rigidbody.position, (grappleTargetPoint - rigidbody.position).normalized, Mathf.Infinity, grappleableLayer);
            // if (hit.collider == null)
            // {
            //     Debug.Log("Grapple failed: No grappleable object found.");
            //     return;
            // }

            // 3. Spawn the hook object
            currentHook = Instantiate(hookPrefab, rigidbody.position, Quaternion.identity).GetComponent<HookBehaviour>();
            hookInitialDir = (grappleTargetPoint - (Vector2)currentHook.transform.position).normalized;
            currentHook.API.Get<Action>("onHookHit").Subscribe(AttachGrapple);
            currentHook.API.Get<Action>("onHookNullify").Subscribe(OnHookNullify);
            currentHook.transform.rotation = FacingTowardsRotation(currentHook.transform.position, GetMousePosition(), -90);
            isShootingHook = true;
        }

        
        private void MoveHook()
        {
            // Move the hook towards the target point
            if (!currentHook) return;
            
            if (!hookRB)
                hookRB = currentHook.GetComponent<Rigidbody2D>();
            hookRB.linearVelocity = hookInitialDir * hookSpeed;
        }

        public void AttachGrapple()
        {
            isShootingHook = false;
            // 1. Get Mouse Position
            Vector2 targetPoint = ConversionHelper.Vec3ToVec2(currentHook.transform.position);
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

            RelayAPI.Get<Action>("onHookAttached").Invoke();
            isGrappling = true;
        }

        public void ReleaseGrapple()
        {
            lineRenderer.enabled = false;
            // Destroy the SpringJoint2D component
            if (springJoint != null)
            {
                Destroy(springJoint);
            }
            
            // Disable the LineRenderer
            if (lineRenderer != null)
            {
                lineRenderer.SetPosition(0, Vector3.zero); // Start at the player's position
                lineRenderer.SetPosition(1, Vector3.zero); // End at the hook's position
                lineRenderer.enabled = false;
            }
            
            // Destroy the hook object
            if (currentHook != null)
            {
                Destroy(currentHook.gameObject);
                currentHook = null;
            }

            isGrappling = false;
            isShootingHook = false;
            
            _targetRotation = Quaternion.identity;
        }

        public void OnDash()
        {
            if (cooling / dashCooldown < 1f) return;
            
            _stopCooling = true;
            cooling = 0f;
            _stopCooling = false;
            
            Debug.Log("On Dash");
            
            rigidbody.linearVelocity = Vector2.zero;
            Vector2 direction = (GetMousePosition() - rigidbody.position).normalized;

            // Calculate the distance to the target
            float distance = Vector2.Distance(rigidbody.position, grappleTargetPoint);

            // Apply a force proportional to the distance
            float forceMagnitude = Mathf.Clamp(distance * dashForce, 0, maxPullSpeed);
            rigidbody.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
            
            SetInterpolatingRotation(FacingTowardsRotation(transform.position, GetMousePosition()));
            RelayAPI.Get<Action>("onPlayerDashed").Invoke();
        }
        
        private void UpdateLineRenderer()
        {
            if (lineRenderer && currentHook && rigidbody)
            {
                // Update the positions of the LineRenderer
                lineRenderer.SetPosition(0, rigidbody.position); // Start at the player's position
                lineRenderer.SetPosition(1, currentHook.transform.position); // End at the hook's position

                // Scroll the texture along the line
                float offset = Time.time * textureScrollSpeed;
                lineRenderer.material.mainTextureOffset = new Vector2(offset, 0);
            }

            // lineRenderer.enabled = !!currentHook;
            // Debug.Log(!!currentHook);
        }

        private void Update()
        {
            RelayAPI.Get<Action<bool>>("isGrappling").Invoke(isGrappling);
            UpdateLineRenderer();

            if (currentHook is not null)
            {
                lineRenderer.enabled = true;
                

                if (Vector2.Distance(ConversionHelper.Vec3ToVec2(currentHook.transform.position),
                        ConversionHelper.Vec3ToVec2(transform.position)) > hookMaxReach)
                {
                    // Destroy(currentHook.gameObject);
                    OnHookNullify();
                    AudioManager.Instance.PlayAudio("OutOfRange");
                }
                
                if(isShootingHook)
                    MoveHook();
                if(isGrappling)
                    _targetRotation = FacingTowardsRotation(transform.position, currentHook.transform.position);
            }
            
            transform.rotation = Quaternion.Lerp(transform.rotation, _targetRotation, Time.deltaTime * 5);
        }

        private void FixedUpdate()
        {
            if(!_stopCooling)
                cooling = Math.Clamp(cooling + Time.fixedDeltaTime, 0, dashCooldown);

            
            manager.CharAPI.Get<Action<float>>("attributeDashCooldownProgress").Invoke(cooling / dashCooldown);
            // Debug.Log(cooling);
        }

        private void OnHookNullify()
        {
            currentHook = null;
            hookRB = null;
            ReleaseGrapple();
        }

        private void SetInterpolatingRotation(Quaternion rot)
        {
            _stopInterpolating = true;
            transform.rotation = rot;
            _stopInterpolating = false;
        }

        private Quaternion FacingTowardsRotation(Vector2 origin, Vector2 target, float angleOffset = 0)
        {
            Vector2 directionToHook = (target - origin).normalized;

            // Calculate the target rotation to face the hook
            float angle = Mathf.Atan2(directionToHook.y, directionToHook.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0, 0, angle + angleOffset); // Adjust for sprite orientation
        }
    }
}