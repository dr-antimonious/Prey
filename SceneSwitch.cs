using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : MonoBehaviour
{
    private void Start()
    {
        Cursor.visible = true;
    }

    public void NextScene()
    {
        int id = SceneManager.GetActiveScene().buildIndex == 2
            ? 0
            : SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(id);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
