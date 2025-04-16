using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameScene;

    public void StarGame()
    {
        SceneManager.LoadScene(gameScene);
    }
}
