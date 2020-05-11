using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class Server : MonoBehaviour
{
    public const string ASK_NAME_COMMAND = "%NAME";
    public const string RESPOND_NAME_COMMAND = "&NAME";
    public const char SPLIT_CHAR = '|';

    private List<ServerClient> _clients;
    private List<ServerClient> _disconnectList;

    public int _port = 6321;
    private TcpListener _server;
    private bool _serverStarted;

    private void Start()
    {
        _clients = new List<ServerClient>();
        _disconnectList = new List<ServerClient>();

        try
        {
            _server = new TcpListener(IPAddress.Any, _port);
            _server.Start();

            StartListening();
            _serverStarted = true;
            Debug.Log("Server has been started on port " + _port.ToString());
        }
        catch (Exception e)
        {
            Debug.Log("Socket error : " + e.Message);
        }
    }

    private void Update()
    {
        if (!_serverStarted)
            return;

        foreach (ServerClient client in _clients)
        {
            if (!IsConnected(client._tcp))
            {
                client._tcp.Close();
                _disconnectList.Add(client);
                continue;
            }
            else
            {
                NetworkStream s = client._tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();

                    if (data != null)
                        OnIncomingData(client, data);
                }
            }
        }

        for (int i = 0; i < _disconnectList.Count - 1; i++)
        {
            Broadcast(_disconnectList[i]._clientName + " has disconnected.", _clients);

            _clients.Remove(_disconnectList[i]);
            _disconnectList.RemoveAt(i);
        }
    }

    private void StartListening()
    {
        _server.BeginAcceptTcpClient(AcceptTcpClient, _server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        // Retrieve the delegate, _server
        TcpListener listener = (TcpListener)ar.AsyncState;

        _clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();
        
        Broadcast(ASK_NAME_COMMAND, new List<ServerClient>() { _clients[_clients.Count - 1] });
    }

    private bool IsConnected(TcpClient client)
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        catch
        {
            return false;
        }
    }

    private void OnIncomingData(ServerClient client, string data)
    {
        if (data.Contains(RESPOND_NAME_COMMAND))
        {
            client._clientName = data.Split(SPLIT_CHAR)[1];
            Broadcast(client._clientName + " has connected!", _clients);
            return;
        }

        Broadcast(client._clientName +  " : " + data, _clients);
        Debug.Log(client._clientName + " has sent the following message : " + data);
    }

    private void Broadcast(string data, List<ServerClient> clients)
    {
        foreach (ServerClient client in clients)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client._tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch(Exception e)
            {
                Debug.Log("Write error : " + e.Message + " to client " + client._clientName);
            }
        }
    }

    public void Broadcast(string data)
    {
        foreach (ServerClient client in _clients)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client._tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Write error : " + e.Message + " to client " + client._clientName);
            }
        }
    }
}
