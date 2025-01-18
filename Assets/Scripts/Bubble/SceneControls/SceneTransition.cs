using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bubble.SceneControls
{
    public abstract class SceneTransition : MonoBehaviour
    {
        public abstract IEnumerator AnimateTransitionIn();
        public abstract IEnumerator AnimateTransitionOut();
        public abstract void InitializeTransition();
    }
}