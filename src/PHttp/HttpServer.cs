using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PHttp
{
    public class HttpStateChangedEventArgs : EventArgs
    {
        public HttpStateChangedEventArgs(HttpServerState previousState, HttpServerState currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }

        public HttpServerState CurrentState { get; private set; }
        public HttpServerState PreviousState { get; private set; }
    }

    public class HttpServer : IDisposable
    {
        private const int WriteReadBufferSize = 4096;
        private const int ShutdownTimeout = 30;
        private const int WriteReadTimeOut = 90;

        private TcpListener _tcpListener;
        private AutoResetEvent _clientsChangedEvent = new AutoResetEvent(false);
        private readonly Dictionary<HttpClient, bool> _clients = new Dictionary<HttpClient, bool>();
        private bool _disposed;
        private HttpServerState _state = HttpServerState.Stopped;
        private readonly object _syncLock = new object();


        #region Properties

        public EventHandler StateChanged { get; set; }
        public HttpRequestEventHandler RequestReceived { get; set; }
        public EventHandler UnhandledException { get; set; }

        public HttpServerState State
        {
            get { return _state; }
            private set
            {
                if (value != _state)
                {
                    var e = new HttpStateChangedEventArgs(_state, value);
                    _state = value;
                    OnStateChanged(e);
                }
            }
        }

        public IPEndPoint EndPoint { get; private set; }
        public int Port { get; private set; }

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

        public HttpServer()
        {
            Port = 0;
            EndPoint = new IPEndPoint(IPAddress.Loopback, Port);
            ReadBufferSize = WriteReadBufferSize;
            WriteBufferSize = WriteReadBufferSize;
            ShutdowTimeout = TimeSpan.FromSeconds(ShutdownTimeout);
            ReadTimeout = TimeSpan.FromMinutes(WriteReadTimeOut);
            WriteTimeout = TimeSpan.FromMinutes(WriteReadTimeOut);
            ServerBanner = string.Format("PHttp/{0}", GetType().Assembly.GetName().Version);
        }

        public HttpServer(int port) : this()
        {
            Port = port;
            EndPoint.Port = port;
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            VerifyState(HttpServerState.Stopped);

            State = HttpServerState.Starting;
            Console.WriteLine("Starting server at: " + EndPoint);
            TimeoutManager = new HttpTimeoutManager(this);
            var listener = new TcpListener(EndPoint);

            try
            {
                listener.Start();
                EndPoint = (IPEndPoint)listener.LocalEndpoint;
                _tcpListener = listener;
                ServerUtility = new HttpServerUtility();
                Console.WriteLine("Server is running at: " + EndPoint);
            }
            catch (Exception e)
            {
                State = HttpServerState.Stopped;
                throw new PHttpException("Server failed to start", e);
            }

            State = HttpServerState.Started;
            BeginAcceptTcpClient();
        }

        public void Stop()
        {
            VerifyState(HttpServerState.Started);
            State = HttpServerState.Stopping;

            try
            {
                _tcpListener.Stop();
                StopClients();
            }
            catch (Exception e)
            {
                State = HttpServerState.Started;
                throw new PHttpException("Failed to stop HTTP server", e);
            }
            finally
            {
                _tcpListener = null;
                State = HttpServerState.Stopped;
                Console.WriteLine("Stopped HTTP server");
            }
        }

        #endregion

        #region Private/Internal Methods

        private void VerifyState(HttpServerState state)
        {
            if (_disposed)
                throw new ObjectDisposedException(this.GetType().Name);

            if (_state != state)
            {
                throw new InvalidOperationException("Expected server to be in the state");
            }
        }

        private void StopClients()
        {
            var shutdownStarted = DateTime.Now;
            bool forceShutdown = false;
            // Clients that are waiting for new requests are closed.

            List<HttpClient> clients;
            lock (_syncLock)
            {
                clients = new List<HttpClient>(_clients.Keys);
            }

            foreach (var client in clients)
            {
                client.RequestClose();
            }

            // First give all clients a chance to complete their running requests.
            while (true)
            {
                lock (_syncLock)
                {
                    if (_clients.Count == 0)
                        break;
                }

                var shutdownRunning = DateTime.Now - shutdownStarted;

                if (shutdownRunning.TotalSeconds >= ShutdownTimeout)
                {
                    forceShutdown = true;
                    break;
                }
                _clientsChangedEvent.WaitOne(ShutdownTimeout - Convert.ToInt32(shutdownRunning.TotalSeconds));
            }

            if (!forceShutdown)
                return;

            // If there are still clients running after the timeout, their
            // connections will be forcibly closed.
            lock (_syncLock)
            {
                clients = new List<HttpClient>(_clients.Keys);
            }

            foreach (var client in clients)
            {
                client.ForceClose();
            }

            // Wait for the registered clients to be cleared.
            while (true)
            {
                lock (_syncLock)
                {
                    if (_clients.Count == 0)
                        break;
                }
                _clientsChangedEvent.WaitOne();
            }
        }

        private void BeginAcceptTcpClient()
        {
            var listener = _tcpListener;
            if (listener != null)
                listener.BeginAcceptTcpClient(AcceptTcpClientCallback, null);
        }

        private void AcceptTcpClientCallback(IAsyncResult asyncResult)
        {
            var listener = _tcpListener;
            if (listener == null) return;

            try
            {
                var tcpClient = listener.EndAcceptTcpClient(asyncResult);

                if (listener.Server == null || _state != HttpServerState.Started)
                {
                    tcpClient.Close();
                    return;
                }

                var client = new HttpClient(this, tcpClient);
                RegisterClient(client);

                client.BeginRequest();

                BeginAcceptTcpClient();

            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception e)
            {
                Console.WriteLine("Error accepting Tcp Client: {0}", e.Message);
            }

        }

        internal void RaiseRequest(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            OnRequestReceived(new HttpRequestEventArgs(context));
        }

        protected virtual void OnRequestReceived(HttpRequestEventArgs httpRequestEventArgs)
        {
            if (RequestReceived != null)
            {
                RequestReceived.Invoke(this, httpRequestEventArgs);
            }
        }

        internal bool RaiseUnhandledException(HttpContext context, Exception exception)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            var e = new HttpExceptionEventArgs(context, exception);
            OnUnhandledException(e);
            return e.Handled;
        }

        protected virtual void OnUnhandledException(HttpExceptionEventArgs httpExceptionEventArgs)
        {
            if (UnhandledException != null)
            {
                UnhandledException.Invoke(this, httpExceptionEventArgs);
            }
        }

        private void RegisterClient(HttpClient client)
        {
            if (client == null) throw new ArgumentNullException("client");
            lock (_syncLock)
            {
                _clients.Add(client, true);
                _clientsChangedEvent.Set();
            }

        }

        internal void UnregisterClient(HttpClient client)
        {
            if (client == null) throw new ArgumentNullException("client");

            lock (_syncLock)
            {
                if (_clients.ContainsKey(client))
                {
                    _clients.Remove(client);
                    _clientsChangedEvent.Set();
                }

            }
        }

        protected virtual void OnStateChanged(HttpStateChangedEventArgs args)
        {
            if (StateChanged != null)
            {
                StateChanged.Invoke(this, args);
            }
        }

        #endregion

        #region Implemented Methods

        public void Dispose()
        {
            if (_disposed)
                return;

            if (_state == HttpServerState.Started)
                Stop();

            if (_clientsChangedEvent != null)
            {
                ((IDisposable)_clientsChangedEvent).Dispose();
                _clientsChangedEvent = null;
            }

            if (TimeoutManager != null)
            {
                TimeoutManager.Dispose();
                TimeoutManager = null;
            }

            _disposed = true;
        }


        #endregion
    }
}
