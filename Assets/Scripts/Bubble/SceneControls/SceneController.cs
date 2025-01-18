using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProtoLib.Library.Mono.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bubble.SceneControls
{
    public class SceneController : Singleton<SceneController>
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private GameObject transitionContainer;
        
        private List<SceneTransition> _sceneTransitions = new();
        public int CurrentSceneIndex => SceneManager.GetActiveScene().buildIndex;

        private void Start()
        {
            _sceneTransitions = transitionContainer.GetComponentsInChildren<SceneTransition>().ToList();
            _sceneTransitions.ForEach(transition =>
            {
                transition.InitializeTransition();
                transition.gameObject.SetActive(false);
            });
        }

        public void LoadScene(string sceneName, string transitionName = "CrossFade")
        {
            StartCoroutine(LoadSceneAsync(sceneName, transitionName));
        }

        public void NextScene()
        {
            if(CurrentSceneIndex + 1 >= SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(0);
            }
            else
            {
                SceneManager.LoadScene(CurrentSceneIndex + 1);
            }
        }

        private IEnumerator LoadSceneAsync(string sceneName, string transitionName)
        {
            SceneTransition transition = _sceneTransitions.Find(t => t.name == transitionName);
            transition.gameObject.SetActive(true);
            AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
            if (scene != null)
            {
                scene.allowSceneActivation = false;
                yield return transition.AnimateTransitionIn();
                progressBar.gameObject.SetActive(true);

                do
                {
                    progressBar.value = scene.progress;
                    yield return null;
                } while (scene.progress < 0.9f);

                scene.allowSceneActivation = true;
                progressBar.gameObject.SetActive(false);
                yield return transition.AnimateTransitionOut();
                transition.gameObject.SetActive(false);
            }
        }
    }
}