using System.Net;
using System.Net.Sockets;

if (args.Length != 4 ||
    !int.TryParse(args[0], out int listenPort) || listenPort is < 1024 or > 65535 ||
    !int.TryParse(args[1], out int serverPort) || serverPort is < 1024 or > 65535 ||
    !int.TryParse(args[2], out int delayMs) || delayMs is < 0 or > 1000 ||
    !int.TryParse(args[3], out int dropEvery) || dropEvery is < 0 or > 1000)
{
    Console.Error.WriteLine("Usage: NetworkConditions <listen-port> <server-port> <one-way-delay-ms> <drop-every-nth-packet; 0 disables loss>");
    return 1;
}

using UdpClient fromClient = new(new IPEndPoint(IPAddress.Loopback, listenPort));
using UdpClient fromServer = new(new IPEndPoint(IPAddress.Loopback, 0));
IPEndPoint server = new(IPAddress.Loopback, serverPort);
IPEndPoint? client = null;
int clientPackets = 0;
int serverPackets = 0;
Console.WriteLine($"Network conditioner ready on UDP {listenPort}: {delayMs} ms each way, drop every {dropEvery} packets.");

async Task ForwardAsync(UdpClient socket, byte[] payload, IPEndPoint destination)
{
    try
    {
        await Task.Delay(delayMs);
        await socket.SendAsync(payload, payload.Length, destination);
    }
    catch (ObjectDisposedException)
    {
        // The test runner closed the proxy.
    }
    catch (SocketException error)
    {
        Console.Error.WriteLine($"UDP forwarding failed: {error.SocketErrorCode}");
    }
}

async Task ReceiveClientAsync()
{
    while (true)
    {
        UdpReceiveResult packet = await fromClient.ReceiveAsync();
        if (client is null)
        {
            client = packet.RemoteEndPoint;
        }
        if (!client.Equals(packet.RemoteEndPoint))
        {
            continue; // One test client per proxy, so another sender cannot steal its route.
        }
        if (dropEvery > 0 && ++clientPackets % dropEvery == 0)
        {
            Console.WriteLine($"Dropped client packet {clientPackets}.");
            continue;
        }
        _ = ForwardAsync(fromServer, packet.Buffer, server);
    }
}

async Task ReceiveServerAsync()
{
    while (true)
    {
        UdpReceiveResult packet = await fromServer.ReceiveAsync();
        if (client is null || !server.Equals(packet.RemoteEndPoint))
        {
            continue;
        }
        if (dropEvery > 0 && ++serverPackets % dropEvery == 0)
        {
            Console.WriteLine($"Dropped server packet {serverPackets}.");
            continue;
        }
        _ = ForwardAsync(fromClient, packet.Buffer, client);
    }
}

await Task.WhenAll(ReceiveClientAsync(), ReceiveServerAsync());
return 0;
