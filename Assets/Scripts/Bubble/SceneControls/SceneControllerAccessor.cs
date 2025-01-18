using UnityEngine;

namespace Bubble.SceneControls
{
    public class SceneControllerAccessor : MonoBehaviour
    {
        public string SceneName;
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