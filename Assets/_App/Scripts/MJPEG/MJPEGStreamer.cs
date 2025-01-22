using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;


// Stream view of Unity camera:
//	attach this script to an empty game object
//	it should automatically find the main camera in the scene (or you can overrule it)
//	it should create a render texture, capture the camera view into it and stream it out

// Stream from a webcam:
//	attach this script to an empty game object
//	attach the webcam-tester script to the same object
//	it should automatically connect things, fetch the webcam texture and stream it out

// Point a webbrowser to http://localhost:8080


public class MJPEGStreamer : MonoBehaviour
{
	[Header("Stream Settings")]
	[SerializeField] private int port		= 8080;
	[SerializeField] private int quality	= 75;
	[SerializeField] private int frameRate	= 30;
	
	[Header("Source")]

	[SerializeField] private EnterpriseCameraAccessManager _enterpriseCameraAccessManager;



	
	private HttpListener										httpListener;
	private ConcurrentDictionary<string, HttpListenerContext>	activeClients;
	private CancellationTokenSource								cancellationTokenSource;
	private bool												isStreaming = false;
	private float												frameInterval;
	private float												lastFrameTime;



	private void Start()
	{
		frameInterval = 1f / frameRate;
		activeClients = new ConcurrentDictionary<string, HttpListenerContext>();
		StartServer();

		//_enterpriseCameraAccessManager.frameUpdate.AddListener(() => SendFrame());
	}


	private void StartServer()
	{
		try
		{
			httpListener = new HttpListener();
			httpListener.Prefixes.Add($"http://*:{port}/");
			httpListener.Start();
			
			Debug.Log($"MJPEG server started on port {port}");
			Debug.Log($"Access the stream at: http://localhost:{port}/");
			
			cancellationTokenSource = new CancellationTokenSource();
			ListenForClientsAsync();
			isStreaming = true;
		}
		catch(System.Exception e)
		{
			Debug.LogError($"Failed to start MJPEG server: {e.Message}");
		}
	}


	private async void ListenForClientsAsync()
	{
		while(!cancellationTokenSource.Token.IsCancellationRequested)
		{
			try
			{
				var context		= await httpListener.GetContextAsync();
				var clientId	= context.Request.RemoteEndPoint.ToString();
				
				activeClients.TryAdd(clientId, context);
				
				// Send HTTP headers
				var response			= context.Response;
				response.ContentType	= "multipart/x-mixed-replace; boundary=frame";
				response.Headers.Add("Access-Control-Allow-Origin", "*");
				response.Headers.Add("Connection", "keep-alive");
				
				Debug.Log($"New client connected: {clientId}");
			}
			catch(System.Exception e)
			{
				if(!cancellationTokenSource.Token.IsCancellationRequested)
					Debug.LogError($"Error accepting client: {e.Message}");
			}
		}
	}

	private void Update()
	{
		if(!isStreaming || Time.time - lastFrameTime < frameInterval)
			return;

		lastFrameTime = Time.time;
		SendFrame();
	}


	private void SendFrame()
	{
		if(activeClients.Count == 0)
			return;

		byte[] jpegBytes = null;

		if (_enterpriseCameraAccessManager != null)
		{
			jpegBytes = _enterpriseCameraAccessManager.GetMainCameraTexture2D().EncodeToJPG(quality);
		}

		if(jpegBytes != null)
		{
			foreach(var client in activeClients)
			{
				try
				{
					var response = client.Value.Response;
					var headers = $"\r\n--frame\r\nContent-Type: image/jpeg\r\nContent-Length: {jpegBytes.Length}\r\n\r\n";
					var headerBytes = System.Text.Encoding.ASCII.GetBytes(headers);
					
					response.OutputStream.Write(headerBytes, 0, headerBytes.Length);
					response.OutputStream.Write(jpegBytes, 0, jpegBytes.Length);
					response.OutputStream.Flush();
				}
				catch
				{
					activeClients.TryRemove(client.Key, out _);
					try { client.Value.Response.Close(); } catch { }
					Debug.Log($"Client disconnected: {client.Key}");
				}
			}
		}
	}

	private void OnDisable()
	{
		StopServer();
	}

	private void StopServer()
	{
		isStreaming = false;
		
		if (cancellationTokenSource != null)
		{
			cancellationTokenSource.Cancel();
			cancellationTokenSource.Dispose();
			cancellationTokenSource = null;
		}

		foreach (var client in activeClients)
		{
			try { client.Value.Response.Close(); } catch { }
		}
		activeClients.Clear();

		if (httpListener != null)
		{
			httpListener.Stop();
			httpListener.Close();
			httpListener = null;
		}
	}
}
