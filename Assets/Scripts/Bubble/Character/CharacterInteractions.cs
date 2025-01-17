using System;
using Bubble.Character.Interface;
using ProtoLib.Library.Facet;
using ProtoLib.Library.Mono.Scripting;
using UnityEngine.InputSystem;

namespace Bubble.Character
{
    public class CharacterInteractions : ScriptComponent<ICharacterComponent>, ICharacterComponent
    {
        private CharacterControls _controls;
        public FacetAPI API { get; } = FacetAPI.Create()
            .Callback<Action>("onShootGrapple")
            .Callback<Action>("onReleaseGrapple")
            .Callback<Action>("onDash")
            .Build();

        public void InitControls()
        {
            if (_controls == null)
                _controls = new CharacterControls();
        }

        protected override void Awake()
        {
            base.Awake();
            InitControls();
        }

        protected void OnEnable()
        {
            InitControls();
            EnableControls();
        }
        
        protected void OnDisable()
        {
            DisableControls();
        }

        public void EnableControls()
        {
            _controls.Enable();
            _controls.Player.Grapple.performed += OnShootGrapple;
            _controls.Player.Grapple.canceled += OnReleaseGrapple;
            _controls.Player.Dash.performed += OnDash;
        }

        public void DisableControls()
        {
            _controls.Disable();
            _controls.Player.Grapple.performed -= OnShootGrapple;
            _controls.Player.Grapple.canceled -= OnReleaseGrapple;
            _controls.Player.Dash.performed -= OnDash;
        }

        private void OnShootGrapple(InputAction.CallbackContext ctx)
        {
            API.Get<Action>("onShootGrapple").Invoke();
        }
        private void OnReleaseGrapple(InputAction.CallbackContext ctx)
        {
            API.Get<Action>("onReleaseGrapple").Invoke();
        }
        
        private void OnDash(InputAction.CallbackContext ctx)
        {
            API.Get<Action>("onDash").Invoke();
        }
    }
}