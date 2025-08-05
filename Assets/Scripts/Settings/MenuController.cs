using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

    public void ChangeScene(int sceneID)
    {
        SceneManager.LoadScene(sceneID);
    }

    public void ReiniciarAplicacion()
    {

        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        Process.Start(exePath);
        Application.Quit();

    }

    public void ExitApp()

    { 
        Application.Quit();
    }

}
