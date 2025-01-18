using System.Collections.Generic;
using UnityEngine;

namespace Bubble.Parallax
{
    [ExecuteInEditMode]
    public class ParallaxBackground : MonoBehaviour
    {
        public ParallaxCamera parallaxCamera;
        List<ParallaxLayer> parallaxLayers = new List<ParallaxLayer>();
 
        void Start()
        {
            if (parallaxCamera == null)
                parallaxCamera = Camera.main.GetComponent<ParallaxCamera>();
 
            if (parallaxCamera != null)
            {
                parallaxCamera.OnCameraTranslateX += MoveX;
                parallaxCamera.OnCameraTranslateY += MoveY;
            }
 
            SetLayers();
        }
 
        void SetLayers()
        {
            parallaxLayers.Clear();
 
            for (int i = 0; i < transform.childCount; i++)
            {
                ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();
 
                if (layer != null)
                {
                    layer.name = "Layer-" + i;
                    parallaxLayers.Add(layer);
                }
            }
        }
 
        void MoveX(float delta)
        {
            foreach (ParallaxLayer layer in parallaxLayers)
            {
                layer.MoveX(delta);
            }
        }
        
        void MoveY(float delta)
        {
            foreach (ParallaxLayer layer in parallaxLayers)
            {
                layer.MoveY(delta);
            }
        }
    }

}