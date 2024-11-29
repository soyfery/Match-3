using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public void EasyLevel()
    {
        SceneManager.LoadScene(4);
    }
    public void SredLevel()
    {
        SceneManager.LoadScene(2);
    }
    public void HardLevel()
    {
        SceneManager.LoadScene(3);
    }
    public void Menu()
    {
        SceneManager.LoadScene(0);
    }
}