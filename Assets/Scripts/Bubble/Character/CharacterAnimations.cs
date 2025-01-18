using System;
using Bubble.Character.Interface;
using ProtoLib.Library.Mono.Scripting;
using UnityEngine;

namespace Bubble.Character
{
    public class CharacterAnimations : ScriptComponent<ICharacterComponent>, ICharacterComponent
    {
        private static readonly int Dash = Animator.StringToHash("dash");
        private static readonly int Grapple = Animator.StringToHash("isGrappling");
        private static readonly int Death = Animator.StringToHash("death");
        public Animator animator;
        private CharacterMovement _movement;
        private CharacterManager manager;

        protected void Start()
        {
            manager = GetComponent<CharacterManager>();
            _movement = this.ScriptManager.GetScriptComponent<CharacterMovement>();
            _movement.RelayAPI.Get<Action<bool>>("isGrappling").Subscribe(grappling =>
            {
                animator.SetBool(Grapple, grappling);
            });
            
            manager.CharAPI.Get<Action>("onDeath").Subscribe(() =>
            {
                animator.SetBool(Death, true);
            });
        }

        private void DashAnim()
        {
            animator.SetTrigger(Dash);
        }
    }
}