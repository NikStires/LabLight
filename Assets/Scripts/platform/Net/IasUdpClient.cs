using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using UniRx;

#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
using Windows.Storage.Streams;
#endif

// Hide udp listener bugs when running in editor or hololens
// TODO move frame construction out
public class IasUdpClient : INetworkFrameProducer
{
    public string Hostname { set; get; }
    public IHttp http;

    public IasUdpClient(IHttp http)
    {
        this.http = http;
        Listen(9000);
    }

    [Serializable]
    class RequestHeartbeat
    {
        public string id;
#if UNITY_EDITOR
        public string ip;
#endif
    }

    List<NetStateFrame> frameList = new List<NetStateFrame>();

#if UNITY_EDITOR

    void _Listen(int port)
    {
        Task.Run(() =>
        {
            using (var udpClient = new UdpClient(port))
            {
                while (true)
                {
                    try
                    {
                        var remote = new IPEndPoint(IPAddress.Any, 0);
                        var buffer = udpClient.Receive(ref remote);
                        var jsonString = Encoding.ASCII.GetString(buffer);
                        var frame = Parsers.ParseNetStateFrame(jsonString);

                        lock (frameList)
                        {
                            frameList.Add(frame);
                        }
                    }
                    catch (Exception e)
                    {
                        //ServiceRegistry.Logger.LogError(e.ToString());
                    }
                }
            }
        });
    }

#else
    DatagramSocket socket;

    async void _Listen(int port)
    {
        // UdpClient doesn't receive packets when running on Hololens
        // Attempting to send a packet to get it working like same finding below, results in a few packets,
        // then stops. Code below works to keep UDP flowing

        var portStr = port.ToString();
        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;
        try
        {
            await socket.BindServiceNameAsync(portStr);

            // Unclear if this is needed
            await Task.Delay(1000);

            // Send data, otherwise receiving does not work (UDP stack bug)
            var outputStream = await socket.GetOutputStreamAsync(new HostName("255.255.255.255"), portStr);
            DataWriter writer = new DataWriter(outputStream);
            writer.WriteString("Online");
            writer.StoreAsync();
        }
        catch (Exception e)
        {
            ServiceRegistry.Logger.LogError("UDP Error: " + e.ToString());
            ServiceRegistry.Logger.LogError(Windows.Networking.Sockets.SocketError.GetStatus(e.HResult).ToString());
            return;
        }
    }

    // This function is called on background thread!
    private void Socket_MessageReceived(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
    {
        // Note: avoid use of args.GetDataStream().AsStreamForRead();
        // OutOfMemory issues within Unity: https://forum.unity.com/threads/hololens-udp-client-with-datagramsocket-and-memory-leak.583816/

        if (args == null) return;

        try
        {
            using (var reader = args.GetDataReader())
            {
                var buf = new byte[reader.UnconsumedBufferLength];
                reader.ReadBytes(buf);
                var jsonString = Encoding.UTF8.GetString(buf);
                var frame = Parsers.ParseNetStateFrame(jsonString);

                lock (frameList)
                {
                    frameList.Add(frame);
                }
            }
        }
        catch (Exception e)
        {
            // MainThreadDispatcher.Instance.Enqueue(() => ServiceRegistry.Logger.LogError("Message Received: " + e.ToString()));
        }
    }
#endif

    public void Listen(int port)
    {
        _Listen(port);

        // Start heartbeat
        // This will notify the server to begin sending us streaming state updates
        // If we timeout, server will hangup
        Heartbeat();
        Observable.Interval(TimeSpan.FromSeconds(15)).Subscribe(x => Heartbeat());
    }

    void Heartbeat()
    {
        var request = new RequestHeartbeat();
        request.id = SystemInfo.deviceName; // TODO refactor
#if UNITY_EDITOR
        // Not getting updates when using ipv6 addresses
        request.ip = "localhost";
#endif

        http.PostJson(Config.GetResourcePath("/client/register"), request).Subscribe();
        // ServiceRegistry.Logger.Log("Heartbeat");
    }

    public List<NetStateFrame> GetAndClearFrames()
    {
        lock (frameList)
        {
            var retList = new List<NetStateFrame>(frameList);
            frameList.Clear();
            return retList;
        }
    }
}
