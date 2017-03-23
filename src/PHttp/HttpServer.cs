using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PHttp
{
	public class HttpServer : IDisposable
	{
		private const int WriteReadBufferSize = 4096;
		private const int ShutdownTimeout = 30;
		private const int WriteReadTimeOut = 90;

		private TcpListener _tcpListener;
		private AutoResetEvent _clientsChangedEvent = new AutoResetEvent (false);
		private bool _disposed;
		private HttpServerState _state = HttpServerState.Stopped;

		#region Properties

		public EventHandler StateChanged{ get; set; }

		public HttpServerState State { 
			get { return _state; } 
			set {
				if (value != _state) {
					_state = value;
					OnStateChanged (new EventArgs());
				}
			}
		}

		public IPEndPoint EndPoint { get; private set; }
		public int Port{ get; private set;}

		public int ReadBufferSize { get; set; }

		public int WriteBufferSize { get; set; }

		public string ServerBanner { get; set; }

		public TimeSpan ReadTimeout { get; set; }

		public TimeSpan WriteTimeout { get; set; }

		public TimeSpan ShutdowTimeout { get; set; }

		internal HttpServerUtility ServerUtility { get; private set; }

		internal HttpTimeoutManager TimeoutManager { get; private set; }

		#endregion

		#region Constructor

		public HttpServer ()
		{
			Port = 0;
			EndPoint = new IPEndPoint (IPAddress.Loopback, Port);
			ReadBufferSize = WriteReadBufferSize;
			WriteBufferSize = WriteReadBufferSize;
			ShutdowTimeout = TimeSpan.FromSeconds (ShutdownTimeout);
			ReadTimeout = TimeSpan.FromMinutes (WriteReadTimeOut);
			WriteTimeout = TimeSpan.FromMinutes (WriteReadTimeOut);
			ServerBanner = string.Format ("PHttp/{0}", GetType ().Assembly.GetName ().Version);
		}

		public HttpServer(int port) : this(){
			Port = port;
			EndPoint.Port = port;
		}

		#endregion

		#region Public Methods

		public void Start ()
		{
			if (_state != HttpServerState.Stopped)
				return;

			State = HttpServerState.Starting;
			Console.WriteLine ("Starting server at: " + EndPoint);
			TimeoutManager = new HttpTimeoutManager (this);
			var listener = new TcpListener (EndPoint);

			try{
				listener.Start();
				_tcpListener = listener;
				ServerUtility = new HttpServerUtility();
				Console.WriteLine("Server is running at: " + EndPoint);				
			}
			catch (Exception e){ 
				State = HttpServerState.Stopped;
				throw new PHttpException ("Server failed to start", e);
			}

			State = HttpServerState.Started;
			BeginAcceptTcpClient ();
		}

		public void Stop ()
		{

		}

		#endregion

		#region Private/Internal Methods

		private void VerifyState (HttpServerState state)
		{
			if (_disposed)
				throw new ObjectDisposedException (this.GetType ().Name);

			if (_state != state) {
				throw new InvalidOperationException ("Expected server to be in the state");
			}
		}

		private void StopClients ()
		{

		}

		private void BeginAcceptTcpClient ()
		{
			var listener = _tcpListener;
			if (listener == null)
				return;

			listener.BeginAcceptTcpClient (AcceptTcpClientCallback, null);
		}

		private void AcceptTcpClientCallback (IAsyncResult asyncResult)
		{

		}

		private void RegisterClient (HttpClient client)
		{

		}

		internal void UnregisterClient (HttpClient client)
		{

		}

		protected virtual void OnStateChanged (EventArgs args)
		{
			if (StateChanged != null) {
				StateChanged.Invoke (this, args);
			}
		}

		#endregion

		#region Implemented Methods

		public void Dispose ()
		{
			if (_disposed)
				return;
			
			if (_state == HttpServerState.Started)
				Stop ();
			
			if (_clientsChangedEvent != null) {
				_clientsChangedEvent.Dispose();
				_clientsChangedEvent = null;
			}

			_disposed = true;
		}

		#endregion
	}
}
