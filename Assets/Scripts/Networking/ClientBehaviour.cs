using UnityEngine;
using System.Runtime.Serialization;
using Unity.Networking.Transport;


public class ClientBehaviour : MonoBehaviour
{
    public string ServerIP= "172.16.0.103";
    public ushort port = 8888;

    NetworkDriver m_Driver;
    NetworkConnection m_Connection;

    // Start is called before the first frame update
    void Start()
    {
        m_Driver = NetworkDriver.Create(new UDPNetworkInterface());

        var endpoint = NetworkEndpoint.Parse(ServerIP, port);
        m_Connection = m_Driver.Connect(endpoint);

        Debug.Log("attemptnig to connect to server on " + endpoint.Address + " " + endpoint.Port);
    }

    private void OnDestroy()
    {
        m_Driver.Dispose();
    }

    // Update is called once per frame
    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            Debug.Log("Failed to connect");
            return;
        }

        Unity.Collections.DataStreamReader stream;
        NetworkEvent.Type cmd;
        m_Connection.PopEvent(m_Driver, out stream);

        Debug.Log(stream.ReadByte());
        //while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        //{
        //    Debug.Log("Here");
        //    if (cmd == NetworkEvent.Type.Connect)
        //    {
        //        Debug.Log("We are now connected to the server.");

        //        uint value = 1;
        //        m_Driver.BeginSend(m_Connection, out var writer);
        //        writer.WriteUInt(value);
        //        m_Driver.EndSend(writer);
        //    }
        //    else if (cmd == NetworkEvent.Type.Data)
        //    {
        //        Debug.Log("Recievimg data from the server");
        //        uint value = stream.ReadUInt();
        //        Debug.Log($"Got the value {value} back from the server.");

        //        m_Connection.Disconnect(m_Driver);
        //        m_Connection = default;
        //    }
        //    else if (cmd == NetworkEvent.Type.Disconnect)
        //    {
        //        Debug.Log("Client got disconnected from server.");
        //        m_Connection = default;
        //    }
        //}
    }
}
