using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Bubble.SceneControls.Transition
{
    [System.Serializable]
    public class CrossFade : SceneTransition
    {
        public CanvasGroup crossFade;
        
        public override IEnumerator AnimateTransitionIn()
        {
            crossFade.blocksRaycasts = true;
            var tweener = crossFade.DOFade(1f, 1f);
            yield return tweener.WaitForCompletion();
        }
 
        public override IEnumerator AnimateTransitionOut()
        {
            var tweener = crossFade.DOFade(0f, 1f);
            yield return tweener.WaitForCompletion();
            crossFade.blocksRaycasts = false;
        }

        public override void InitializeTransition()
        {
            crossFade.blocksRaycasts = false;
            crossFade.alpha = 0f;
        }
    }
}