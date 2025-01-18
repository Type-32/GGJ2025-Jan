using System;
using System.Linq;
using ProtoLib.Library.Facet;
using UnityEngine;

namespace Bubble.Misc
{
    public class HookBehaviour : MonoBehaviour
    {
        public FacetAPI API { get; } = FacetAPI.Create()
            .Callback<Action>("onHookHit")
            .Callback<Action>("onHookNullify")
            .Build();

        [SerializeField] private Rigidbody2D _rigidbody2D;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(!other.gameObject.name.Contains("Bird"))
                HookHit();
        }

        private void HookHit()
        {
            _rigidbody2D.bodyType = RigidbodyType2D.Static;
            API.Get<Action>("onHookHit").Invoke();
        }

        private void HookNullify()
        {
            _rigidbody2D.bodyType = RigidbodyType2D.Dynamic;
            Collider2D c = this.GetComponent<Collider2D>();
            if (c)
                c.enabled = false;
            API.Get<Action>("onHookNullify").Invoke();
        }
    }
}