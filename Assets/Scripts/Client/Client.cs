using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
    public GameObject _chatContainer;
    public GameObject _messagePrefab;

    public string _clientName;

    private bool _socketReady;
    private TcpClient _socket;
    private NetworkStream _stream;
    private StreamWriter _writer;
    private StreamReader _reader;

    private string _host = "127.0.0.1";
    private int _port = 6321;

    public void ConnectedToServer()
    {
        if (_socketReady)
            return;

        string inputHost;
        int inputPort;
        inputHost = GameObject.Find("HostInput").GetComponent<InputField>().text;
        if (inputHost != "")
            _host = inputHost;
        int.TryParse(GameObject.Find("PortInput").GetComponent<InputField>().text, out inputPort);
        if (inputPort != 0)
            _port = inputPort;

        try
        {
            _socket = new TcpClient(_host, _port);
            _stream = _socket.GetStream();
            _writer = new StreamWriter(_stream);
            _reader = new StreamReader(_stream);
            _socketReady = true;
        }
        catch(Exception e)
        {
            Debug.Log("Socket error : " + e.Message);
        }
    }

    private void Update()
    {
        if (_socketReady)
        {
            if (_stream.DataAvailable)
            {
                string data = _reader.ReadLine();
                if (data != null)
                    OnIncomingData(data);
            }
        }
    }

    private void OnIncomingData(string data)
    {
        if (data == Server.ASK_NAME_COMMAND)
        {
            Send(Server.RESPOND_NAME_COMMAND + Server.SPLIT_CHAR + _clientName);
            return;
        }

        GameObject message = Instantiate(_messagePrefab, _chatContainer.transform) as GameObject;
        message.GetComponentInChildren<Text>().text = data;
        Debug.Log("Server : " + data);
    }

    private void Send(string data)
    {
        if (!_socketReady)
            return;

        _writer.WriteLine(data);
        _writer.Flush();
    }

    public void OnSendButton()
    {
        string message = GameObject.Find("SendInput").GetComponent<InputField>().text;
        Send(message);
    }

    private void CloseSocket()
    {
        if (!_socketReady)
            return;

        _writer.Close();
        _reader.Close();
        _socket.Close();
        _socketReady = false;
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }
}
