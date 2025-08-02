using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;


public class UIManager : MonoBehaviour
{
    public Button botonHost;
    public Button botonCliente;
    
    public GameObject discoveryServer;
    public GameObject discoveryClient;

    public TMP_Text estadoTexto;
    public GameObject estadoPanel;

    public GameObject[] disabledObjects;

  

    void Start()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No se encontró el NetworkManager.");
            return;
        }

        // Suscribirse al evento de conexión de cliente (solo si eres host)
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;


        botonHost.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            discoveryServer.SetActive(true); // activa broadcast del host
        });

        botonCliente.onClick.AddListener(() =>
        {
            discoveryClient.SetActive(true); // activa escucha del cliente
        });

    }

    void Update()
    {
        if (!NetworkManager.Singleton.IsListening)
        {
            estadoTexto.text = "No conectado";
            estadoPanel.GetComponent<Image>().color = Color.red;
        }
        else if (NetworkManager.Singleton.IsHost)
        {
            int totalClientes = NetworkManager.Singleton.ConnectedClients.Count - 1; // -1 porque el host también cuenta como cliente

            if (totalClientes > 0)
            {
                estadoTexto.text = $"Host (Clientes: {totalClientes})";
                estadoPanel.GetComponent<Image>().color = Color.green;
            }
            else
            {
                estadoTexto.text = "Host esperando clientes...";
                estadoPanel.GetComponent<Image>().color = Color.green;
            }
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            estadoTexto.text = "Cliente conectado";
            estadoPanel.GetComponent<Image>().color = Color.cyan;
            DeactivateAll();
        }


    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost && clientId != NetworkManager.Singleton.LocalClientId)
        {
            Debug.Log($"Cliente conectado con ID: {clientId}");

            // Cambiar el texto del panel y su color
            estadoTexto.text = $"Cliente conectado (ID: {clientId})";
            estadoPanel.GetComponent<Image>().color = Color.yellow;

            // Opcional: podrías iniciar alguna lógica adicional aquí
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
    }


    public void DeactivateAll()
    {
        foreach (var obj in disabledObjects)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }
}
