using System.Net.Sockets;

public class ServerClient
{
    private const string DEFAULT_CLIENT_NAME = "Guest";

    public TcpClient _tcp;
    public string _clientName;

    public ServerClient(TcpClient clientSocket)
    {
        _clientName = DEFAULT_CLIENT_NAME;
        _tcp = clientSocket;
    }
}
