using UnityEngine;

namespace Bubble.Parallax
{
 
    [ExecuteInEditMode]
    public class ParallaxCamera : MonoBehaviour
    {
        public delegate void ParallaxCameraDelegate(float deltaMovement);
        public ParallaxCameraDelegate OnCameraTranslateX;
        public ParallaxCameraDelegate OnCameraTranslateY;
 
        private float _oldPosX;
        private float _oldPosY;
 
        void Start()
        {
            _oldPosX = transform.position.x;
            _oldPosY = transform.position.y;
        }
 
        void Update()
        {
            if (!Mathf.Approximately(transform.position.x, _oldPosX))
            {
                if (OnCameraTranslateX != null)
                {
                    float delta = _oldPosX - transform.position.x;
                    OnCameraTranslateX(delta);
                }
 
                _oldPosX = transform.position.x;
            }
            
            if (!Mathf.Approximately(transform.position.y, _oldPosY))
            {
                if (OnCameraTranslateY != null)
                {
                    float delta = _oldPosY - transform.position.y;
                    OnCameraTranslateY(delta);
                }
 
                _oldPosY = transform.position.y;
            }
        }
    }
}