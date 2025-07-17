using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetworkDiscoveryServer : MonoBehaviour
{
    UdpClient udpServer;
    IPEndPoint broadcastEndpoint;

    public int broadcastPort = 47777;
    public float broadcastInterval = 1.0f;

    void Start()
    {
        udpServer = new UdpClient();
        udpServer.EnableBroadcast = true;
        broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);

        InvokeRepeating(nameof(SendBroadcast), 1f, broadcastInterval);
    }

    void SendBroadcast()
    {
        string message = "MY_HOST"; // Puedes cambiar el mensaje por uno personalizado
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpServer.Send(data, data.Length, broadcastEndpoint);
    }

    void OnApplicationQuit()
    {
        udpServer?.Close();
    }
}
