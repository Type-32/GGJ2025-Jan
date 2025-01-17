using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bubble
{
    [RequireComponent(typeof(Collider2D))]
    public class SceneControllerAccessor : MonoBehaviour
    {
        public string SceneName;
        public Collider2D Collider;
        public void LoadScene()
        {
            SceneController.Instance.LoadScene(SceneName);
        }

        public void LoadScene(string scene)
        {
            SceneController.Instance.LoadScene(scene);
        }
        
        public void NextScene()
        {
            SceneController.Instance.NextScene();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(SceneName != "") LoadScene();
            else NextScene();
        }
    }
}