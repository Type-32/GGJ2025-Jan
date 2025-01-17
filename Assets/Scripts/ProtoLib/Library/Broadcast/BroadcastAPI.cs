using System;
using ProtoLib.Library.Facet;
using UnityEngine;

namespace ProtoLib.Library.Broadcast
{
    public class BroadcastAPI
    {
        private static BroadcastAPI _instance;
        public static BroadcastAPI Instance
        {
            get
            {
                _instance ??= new BroadcastAPI();
                return _instance;
            }
        }

        protected readonly FacetAPI _api;

        private BroadcastAPI()
        {
            _api = FacetAPI
                .Create()
                .Callback<Action<GameObject, string>>("Global") // Global message broadcasting
                .Callback<Action<string>>("GlobalString") // String-only messages
                .Callback<Action<GameObject>>("GlobalObject") // GameObject-only messages
                .Build();
        }

        /// <summary>
        /// Broadcast a message with a GameObject reference
        /// </summary>
        /// <param name="sender">The GameObject sending the message</param>
        /// <param name="message">The message to broadcast</param>
        public void Broadcast(GameObject sender, string message)
        {
            _api.Get<Action<GameObject, string>>("Global").Invoke(sender, message);
        }

        /// <summary>
        /// Broadcast a message without a GameObject reference
        /// </summary>
        /// <param name="message">The message to broadcast</param>
        public void Broadcast(string message)
        {
            _api.Get<Action<string>>("GlobalString").Invoke(message);
        }

        /// <summary>
        /// Broadcast a GameObject without a message
        /// </summary>
        /// <param name="sender">The GameObject to broadcast</param>
        public void Broadcast(GameObject sender)
        {
            _api.Get<Action<GameObject>>("GlobalObject").Invoke(sender);
        }

        /// <summary>
        /// Subscribe to GameObject and string messages
        /// </summary>
        /// <param name="handler">The handler for the broadcast</param>
        public void Subscribe(Action<GameObject, string> handler)
        {
            _api.Get<Action<GameObject, string>>("Global").Subscribe(handler);
        }

        /// <summary>
        /// Subscribe to string-only messages
        /// </summary>
        /// <param name="handler">The handler for the broadcast</param>
        public void Subscribe(Action<string> handler)
        {
            _api.Get<Action<string>>("GlobalString").Subscribe(handler);
        }

        /// <summary>
        /// Subscribe to GameObject-only messages
        /// </summary>
        /// <param name="handler">The handler for the broadcast</param>
        public void Subscribe(Action<GameObject> handler)
        {
            _api.Get<Action<GameObject>>("GlobalObject").Subscribe(handler);
        }

        /// <summary>
        /// Unsubscribe from GameObject and string messages
        /// </summary>
        /// <param name="handler">The handler to unsubscribe</param>
        public void Unsubscribe(Action<GameObject, string> handler)
        {
            _api.Get<Action<GameObject, string>>("Global").Unsubscribe(handler);
        }

        /// <summary>
        /// Unsubscribe from string-only messages
        /// </summary>
        /// <param name="handler">The handler to unsubscribe</param>
        public void Unsubscribe(Action<string> handler)
        {
            _api.Get<Action<string>>("GlobalString").Unsubscribe(handler);
        }

        /// <summary>
        /// Unsubscribe from GameObject-only messages
        /// </summary>
        /// <param name="handler">The handler to unsubscribe</param>
        public void Unsubscribe(Action<GameObject> handler)
        {
            _api.Get<Action<GameObject>>("GlobalObject").Unsubscribe(handler);
        }

        /// <summary>
        /// Unsubscribe all handlers from all channels
        /// </summary>
        public void UnsubscribeAll()
        {
            _api.Get<Action<GameObject, string>>("Global").UnsubscribeAll();
            _api.Get<Action<string>>("GlobalString").UnsubscribeAll();
            _api.Get<Action<GameObject>>("GlobalObject").UnsubscribeAll();
        }
    }
}
