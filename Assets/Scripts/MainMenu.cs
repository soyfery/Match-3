using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public void Exit()
    {
        Debug.Log("Игра закрылась");
        Application.Quit();
    }
}
