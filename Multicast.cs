using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MultiCast;

public class Multicast : IDisposable
{
    private string _ip;
    private  UdpClient _receiver;
    private  UdpClient _sender;
    private  IPAddress _address;
    private  int _port;
    public Multicast(string multicastIp, int port)
    {
        _ip = multicastIp;
        _port = port;
        _sender = new UdpClient();            
    }
    public bool Connect()
    {
        _address = IPAddress.Parse(_ip);
        if (!IsMulticastAddress(_address)) { return false;}
        _receiver = new UdpClient(_port);
        _receiver.JoinMulticastGroup(_address);
        _receiver.MulticastLoopback = true;
        return true;
    }
    private bool IsMulticastAddress(IPAddress ip)
    {
        if (ip.AddressFamily != AddressFamily.InterNetwork) return false;
        var bytes = ip.GetAddressBytes();
        return bytes[0] >= 224 && bytes[0] <= 239;
    }
    public async Task<string> ReceivingAsync()
    {
        var result = await _receiver.ReceiveAsync();
        var message = Encoding.UTF8.GetString(result.Buffer);
        return message;
    }

    public async Task SendMessageAsync(string message)
    {
        var data = Encoding.UTF8.GetBytes(message);
        var endpoint = new IPEndPoint(_address, _port);
        await _sender.SendAsync(data, data.Length, endpoint);
    }

    public void Dispose()
    {
        _receiver.Dispose();
        _sender.Dispose();
    }
}