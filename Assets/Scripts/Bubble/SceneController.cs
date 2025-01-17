using ProtoLib.Library.Mono.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bubble
{
    public class SceneController : Singleton<SceneController>
    {
        public int CurrentSceneIndex => SceneManager.GetActiveScene().buildIndex;
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
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
    }
}