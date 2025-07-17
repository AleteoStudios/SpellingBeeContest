using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public Button botonHost;
    public Button botonCliente;
    public Button botonDesconectar;
    public GameObject discoveryServer;
    public GameObject discoveryClient;

    public TMP_Text estadoTexto;
    public GameObject estadoPanel;

    public string sceneName; 

    void Start()
    {
        sceneName = SceneManager.GetActiveScene().name;
        botonHost.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            discoveryServer.SetActive(true); // activa broadcast del host
        });

        botonCliente.onClick.AddListener(() =>
        {
            discoveryClient.SetActive(true); // activa escucha del cliente
        });

        botonDesconectar.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.Shutdown(); // detiene host + cliente
                Debug.Log("🔴 Host desconectado");
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkManager.Singleton.Shutdown(); // detiene cliente
                Debug.Log("🔴 Cliente desconectado");
            }

            // Opcional: recargar escena para reiniciar estado
            // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        });
    }

    void Update()
    {
        if (!NetworkManager.Singleton.IsListening)
        {
            estadoTexto.text = "🔴 No conectado";
            estadoPanel.GetComponent<Image>().color = Color.red;
        }
        else if (NetworkManager.Singleton.IsHost)
        {
            estadoTexto.text = "🟢 Host";
            estadoPanel.GetComponent<Image>().color = Color.green;
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            estadoTexto.text = "🔵 Cliente conectado";
            estadoPanel.GetComponent<Image>().color = Color.cyan;
        }
        
        //NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
