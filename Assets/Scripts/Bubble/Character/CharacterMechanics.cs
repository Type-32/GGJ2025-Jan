using System;
using Bubble.Audio;
using Bubble.Character.Interface;
using FirstGearGames.SmoothCameraShaker;
using ProtoLib.Library.Mono.Scripting;
using UnityEngine;

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
        private CharacterMovement _movement;
        private CharacterInteractions _interactions;
        private CharacterManager manager;
        
        [NaughtyAttributes.Button("Add Shield")]
        public void AddShield()
        {
            shields++;
            AudioManager.Instance.PlayAudio("GainShield");
        }

        protected void Start()
        {
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
                // TODO: add game over code
                manager.CharAPI.Get<Action>("onDeath").Invoke();
                _interactions.DisableControls();
                Destroy(this.gameObject, 2f);
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                Collider2D collider = GetComponent<Collider2D>();
                collider.enabled = false;
                rb.bodyType = RigidbodyType2D.Static;
                AudioManager.Instance.PlayAudio("BubbleDeath");
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
            manager.CharAPI.Get<Action<int>>("attributeShields").Invoke(shields);
        }
    }
}