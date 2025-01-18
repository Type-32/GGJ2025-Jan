using UnityEngine;

namespace Bubble.Parallax
{
    [ExecuteInEditMode]
    public class ParallaxLayer : MonoBehaviour
    {
        public float parallaxFactor;
        public bool lockXAxis = false;
        public bool lockYAxis = false;
 
        public void MoveX(float delta)
        {
            if (lockXAxis) return;
            Vector3 newPos = transform.localPosition;
            newPos.x -= delta * parallaxFactor;
            transform.localPosition = newPos;
        }
        public void MoveY(float delta)
        {
            if(lockYAxis) return;
            Vector3 newPos = transform.localPosition;
            newPos.y -= delta * parallaxFactor;
            transform.localPosition = newPos;
        }
    }
}