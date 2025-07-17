using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Unity.Netcode;

public class NetworkDiscoveryClient : MonoBehaviour
{
    UdpClient udpClient;
    Thread listenThread;

    public int listenPort = 47777;
    public int serverPort = 7777; // El puerto real del servidor Unity

    void Start()
    {
        udpClient = new UdpClient(listenPort);
        listenThread = new Thread(ListenForBroadcast);
        listenThread.IsBackground = true;
        listenThread.Start();
    }

    void ListenForBroadcast()
    {
        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, listenPort);

        while (true)
        {
            try
            {
                byte[] data = udpClient.Receive(ref anyIP);
                string message = Encoding.UTF8.GetString(data);

                if (message == "MY_HOST")
                {
                    Debug.Log("Host encontrado en: " + anyIP.Address);

                    // Detener escucha para evitar múltiples conexiones
                    udpClient.Close();

                    ConnectToServer(anyIP.Address.ToString());
                    break;
                }
            }
            catch
            {
                break;
            }
        }
    }

    void ConnectToServer(string ip)
    {
        // Ejecutar esto en el hilo principal
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.SetConnectionData(ip, (ushort)serverPort);
            NetworkManager.Singleton.StartClient();
        });
    }

    void OnApplicationQuit()
    {
        udpClient?.Close();
        listenThread?.Abort();
    }
}
