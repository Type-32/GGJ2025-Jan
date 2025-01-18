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
        private CharacterMovement _movement;

        protected void Start()
        {
            _movement = this.ScriptManager.GetScriptComponent<CharacterMovement>();
            _movement.RelayAPI.Get<Action>("onHookAttached").Subscribe(TryGrappleShake);
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
                Destroy(this.gameObject);
            }
            else
            {
                shields -= 1;
                if (wasteShieldShakeData != null)
                    CameraShakerHandler.Shake(wasteShieldShakeData);
            }
        }

        public void TryGrappleShake()
        {
            if (grappleSuccessShakeData != null)
                CameraShakerHandler.Shake(grappleSuccessShakeData);
            AudioManager.Instance.PlayAudio("HookHit");
        }
    }
}