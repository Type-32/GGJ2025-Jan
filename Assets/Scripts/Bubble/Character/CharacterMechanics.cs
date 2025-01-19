using System;
using Bubble.Audio;
using Bubble.Character.Interface;
using FirstGearGames.SmoothCameraShaker;
using ProtoLib.Library.Mono.Scripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Bubble.Character
{
    public class CharacterMechanics : ScriptComponent<ICharacterComponent>, ICharacterComponent
    {
        public int shields = 0;

        [Header("Camera Shake Controls"), SerializeField]
        private ShakeData wasteShieldShakeData;
        [SerializeField] private ShakeData grappleSuccessShakeData;
        [SerializeField] private ShakeData dashShakeData;
        
        [SerializeField] private ParticleSystem dashEffect;
        [SerializeField] private GameObject shield;
        [SerializeField] private Light2D shieldLight;
        private CharacterMovement _movement;
        private CharacterInteractions _interactions;
        private CharacterManager manager;
        private new Collider2D collider;
        
        [NaughtyAttributes.Button("Add Shield")]
        public void AddShield()
        {
            shields++;
            if (shieldLight != null)
            {
                shieldLight.intensity = 10 + shields;
            }
            AudioManager.Instance.PlayAudio("GainShield");
        }

        protected void Start()
        {
            collider = GetComponent<Collider2D>();
            manager = GetComponent<CharacterManager>();
            _movement = this.ScriptManager.GetScriptComponent<CharacterMovement>();
            _interactions = this.ScriptManager.GetScriptComponent<CharacterInteractions>();
            _movement.RelayAPI.Get<Action>("onHookAttached").Subscribe(() =>
            {
                if (grappleSuccessShakeData != null)
                    CameraShakerHandler.Shake(grappleSuccessShakeData);
                AudioManager.Instance.PlayAudio("HookHit");
            });
            _movement.RelayAPI.Get<Action>("onPlayerDashed").Subscribe(() =>
            {
                if (dashEffect != null)
                    dashEffect.Play();
                if (dashShakeData != null)
                    CameraShakerHandler.Shake(dashShakeData);
                AudioManager.Instance.PlayAudio("Dash");
            });
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("CharacterReaction/OnTouchTryDestroy"))
            {
                TryDestroy();
            }
        }

        public void TryDestroy()
        {
            if (shields <= 0)
            {
                AudioManager.Instance.PlayAudio("BubbleDeath");
                // TODO: add game over code
                manager.CharAPI.Get<Action>("onDeath").Invoke();
                _interactions.DisableControls();
                Destroy(this.gameObject, 1);
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                collider.enabled = false;
                rb.bodyType = RigidbodyType2D.Static;
            }
            else
            {
                AudioManager.Instance.PlayAudio("DecreaseShield");
                shields -= 1;
                if (wasteShieldShakeData != null)
                    CameraShakerHandler.Shake(wasteShieldShakeData);
            }
        }

        protected void FixedUpdate()
        {
            shield.gameObject.SetActive(shields > 0);
            if (shieldLight)
            {
                shieldLight.intensity = 10 + shields;
            }
            shield.transform.rotation = Quaternion.Euler(0, 0, AngleRotate(Time.time * 50));
            manager.CharAPI.Get<Action<int>>("attributeShields").Invoke(shields);

            if (transform.position.y < -20 && collider.enabled)
            {
                TryDestroy();
            }
        }

        protected float AngleRotate(float number)
        {
            return number % 360;
        }
    }
}