using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System;

public class Server : MonoBehaviour
{
    #region Singleton implementation
    public static Server Instance { set; get; }

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public NetworkDriver driver;
    private NativeList<NetworkConnection> connections;

    // So the connection doesn't drop
    private bool isActive = false;
    private const float keepAliveTickRate = 20.0f;
    private float lastKeepAlive;

    public Action connectionDropped;

    // Methods
    public void Init(ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4; // Connect to us
        endpoint.Port = port;

        if(driver.Bind(endpoint) != 0)
        {
            // Unable to connect.
            Debug.Log("Unable to bind on port " + endpoint.Port);
            return;
        }
        else
        {
            driver.Listen();
            Debug.Log("Currently listening on port " + endpoint.Port);
        }

        // 2 is max players
        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isActive = true;
    }
    public void Shutdown()
    {
        if (isActive)
        {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
        }
    }
    public void OnDestroy()
    {
        Shutdown();
    }

    public void Update()
    {
        if(!isActive)
        {
            return;
        }

        KeepAlive();

        driver.ScheduleUpdate().Complete();

        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }
    private void KeepAlive()
    {
        if (Time.time - lastKeepAlive > keepAliveTickRate)
        {
            lastKeepAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }

    private void CleanupConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }
    }
    private void AcceptNewConnections()
    {
        // Accept new connections
        NetworkConnection c;
        while ((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
        }
    }
    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(stream, connections[i], this);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected from server");
                    connections[i] = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    Shutdown();
                }
            }
        }
    }

    // Server Specific
    public void Broadcast(NetMessage msg)
    {
        // Sends message to all clients
        for(int i = 0; i < connections.Length; i++)
        {
            if(connections[i].IsCreated)
            {
                //Debug.Log($"Sending {msg.Code} to : {connections[i].InteralId}");
                SendToClient(connections[i], msg);
            }
        }
    }
    private void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        // Sends message to a single client
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }
}
