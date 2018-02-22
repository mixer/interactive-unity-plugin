using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;

namespace Microsoft
{
    class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string message)
        {
            this.Message = message;
        }

        public readonly string Message;
    }

    class ErrorEventArgs : EventArgs
    {
        public ErrorEventArgs(string errorMessage) : this(1, errorMessage)
        {
        }

        public ErrorEventArgs(int code, string errorMessage)
        {
            this.Code = code;
            this.Message = errorMessage;
        }

        public readonly string Message;
        public readonly int Code;
    }

    class CloseEventArgs : EventArgs
    {
        public CloseEventArgs(int code, string reason)
        {
            this.Code = code;
            this.Reason = reason;
        }

        public readonly string Reason;
        public readonly int Code;
    }

    class WebsocketException : Exception
    {
        public WebsocketException() { }

        public WebsocketException(string message) : base(message) { }

        public WebsocketException(string message, Exception inner) : base(message, inner) { }
    }

    class Reason
    {
        public Reason(ushort code, string reason, bool isClose)
        {
            this.code = code;
            this.reason = reason;
            this.isClose = true;
        }

        public ushort code;
        public string reason;
        public bool isClose;
    }

    class Websocket : MonoBehaviour, IDisposable
    {
        #region Public members

        public event EventHandler OnOpen;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<ErrorEventArgs> OnError;
        public event EventHandler<CloseEventArgs> OnClose;

        void Update()
        {
            // Process any events.
            if (websocketEvents.Count > 0)
            {
                lock (websocketEvents)
                {
                    processingEvents = websocketEvents;
                    websocketEvents = new Queue<object>();
                }

                while (processingEvents.Count > 0)
                {
                    object currEvent = processingEvents.Dequeue();
                    if (currEvent is string)
                    {
                        if (!this.open)
                        {
                            this.open = true;
                            RaiseConnect(currEvent as string);
                        }
                        else
                        {
                            RaiseMessage(currEvent as string);
                        }
                    }
                    else if (currEvent is Reason)
                    {
                        Reason reason = currEvent as Reason;
                        if (reason.isClose)
                        {
                            RaiseClose(reason.code, reason.reason);
                        }
                        else
                        {
                            RaiseError(reason.code, reason.reason);
                        }
                    }
                }
            }
        }

        void OnDestroy()
        {
            this.Close();
        }

        public void Open(System.Uri uri)
        {
            this.Open(uri, null);
        }

        public void Open(System.Uri uri, Dictionary<string, string> headers)
        {
            this.uri = uri;
            int ret = create_websocket(ref this.websocketHandle);
            if (0 != ret)
            {
                throw new WebsocketException("Failed to create websocket.");
            }

            socketsByHandle.Add(this.websocketHandle, this);

            if (null != headers)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    ret = add_header(this.websocketHandle, header.Key, header.Value);
                    if (0 != ret)
                    {
                        throw new WebsocketException(string.Format("Failed to add header [{0}]: {1}", header.Key, header.Value));
                    }
                }
            }

#if !UNITY_EDITOR && UNITY_WSA
        this.openAction = Windows.System.Threading.ThreadPool.RunAsync((op) =>
        {   
            ret = open_websocket(this.websocketHandle, this.uri.AbsoluteUri, OnConnectHandler, OnMessageHandler, OnErrorHandler, OnCloseHandler);
            if (0 != ret)
            {
                throw new WebsocketException("Failed to open websocket.");
            }
        });
#else
            this.websocketThread = new Thread(() =>
            {
                try
                {
                    ret = open_websocket(this.websocketHandle, this.uri.AbsoluteUri, Websocket.OnConnectHandler, Websocket.OnMessageHandler, Websocket.OnErrorHandler, Websocket.OnCloseHandler);
                    if (0 != ret)
                    {
                        throw new WebsocketException("Failed to open websocket.");
                    }
                }
                catch (Exception ex)
                {
                    if (ex is WebsocketException) throw;
                    Debug.LogException(ex);
                }
            });
            this.websocketThread.Start();
#endif
        }

        public void Send(string message)
        {
            lock (this.socketLock)
            {
                if (this.websocketHandle == IntPtr.Zero)
                {
                    throw new WebsocketException("Socket closed.");
                }

                int result = write_websocket(this.websocketHandle, message);
                if (0 != result)
                {
                    throw new WebsocketException("Send failed: " + result);
                }
            }
        }

        public void Close()
        {
            this.Dispose();
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // free unmanaged resources (unmanaged objects)            
                if (this.websocketHandle != IntPtr.Zero)
                {   
                    int result = close_websocket(this.websocketHandle);
                    if (0 != result)
                    {
#if !UNITY_EDITOR && UNITY_WSA
                        this.openAction.Cancel();
#else
                        // Force terminate the thread
                        this.websocketThread.Abort();
#endif
                    }

                    if (disposing)
                    {
                        socketsByHandle.Remove(this.websocketHandle);
                    }

                    this.websocketHandle = IntPtr.Zero;
#if !UNITY_WSA
                    this.websocketThread.Join();
#endif
                }

                disposedValue = true;
            }
        }

        ~Websocket()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        #region Private members
#if !UNITY_EDITOR && UNITY_WSA
    private Windows.Foundation.IAsyncAction openAction;
#endif
        private Uri uri;
        private IntPtr websocketHandle = IntPtr.Zero;
        private Thread websocketThread;
        private object socketLock = new object();
        private static Dictionary<IntPtr, Websocket> socketsByHandle = new Dictionary<IntPtr, Websocket>();

        private Queue<object> websocketEvents = new Queue<object>();
        private Queue<object> processingEvents = new Queue<object>();
        private bool open = false;
        
        [MonoPInvokeCallback(typeof(OnConnectDelegate))]
        private static void OnConnectHandler(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string connectMessage, uint connectMessageSize)
        {
            Websocket socket = null;
            socketsByHandle.TryGetValue(websocketHandle, out socket);
            if (null == socket)
            {
                Debug.LogError("Failed to find Websocket instance for this callback.");
                return;
            }

            lock (socket.websocketEvents)
            {
                socket.websocketEvents.Enqueue(connectMessage);
            }
        }

        [MonoPInvokeCallback(typeof(OnMessageDelegate))]
        private static void OnMessageHandler(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string message, uint messageSize)
        {
            Websocket socket = null;
            socketsByHandle.TryGetValue(websocketHandle, out socket);
            if (null == socket)
            {
                Debug.LogError("Failed to find Websocket instance for this callback.");
                return;
            }

            lock (socket.websocketEvents)
            {
                socket.websocketEvents.Enqueue(message);
            }
        }

        [MonoPInvokeCallback(typeof(OnErrorDelegate))]
        private static void OnErrorHandler(IntPtr websocketHandle, ushort code, [MarshalAs(UnmanagedType.LPStr)] string message, uint messageSize)
        {
            Websocket socket = null;
            socketsByHandle.TryGetValue(websocketHandle, out socket);
            if (null == socket)
            {
                Debug.LogError("Failed to find Websocket instance for this callback.");
                return;
            }

            lock (socket.websocketEvents)
            {
                socket.websocketEvents.Enqueue(new Reason(code, message, false));
            }
        }

        [MonoPInvokeCallback(typeof(OnCloseDelegate))]
        private static void OnCloseHandler(IntPtr websocketHandle, ushort code, [MarshalAs(UnmanagedType.LPStr)] string reason, uint reasonSize)
        {
            Websocket socket = null;
            socketsByHandle.TryGetValue(websocketHandle, out socket);
            if (null == socket)
            {
                Debug.LogError("Failed to find Websocket instance for this callback.");
                return;
            }

            lock (socket.websocketEvents)
            {
                socket.websocketEvents.Enqueue(new Reason(code, reason, false));
            }
        }

        private void RaiseConnect([MarshalAs(UnmanagedType.LPStr)] string connectMessage)
        {
            if (disposedValue)
            {
                return;
            }

            EventHandler handler = this.OnOpen;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        private void RaiseMessage([MarshalAs(UnmanagedType.LPStr)] string message)
        {
            if (disposedValue)
            {
                return;
            }

            EventHandler<MessageEventArgs> handler = this.OnMessage;
            if (handler != null)
            {
                handler(this, new MessageEventArgs(message));
            }
        }
        
        private void RaiseError(ushort errorCode, string errorMessage)
        {
            if (disposedValue)
            {
                return;
            }

            EventHandler<ErrorEventArgs> handler = this.OnError;
            if (handler != null)
            {
                handler(this, new ErrorEventArgs(errorCode, errorMessage));
            }
        }
        
        private void RaiseClose(ushort code, [MarshalAs(UnmanagedType.LPStr)] string reason)
        {
            if (disposedValue)
            {
                return;
            }

            EventHandler<CloseEventArgs> handler = this.OnClose;
            if (handler != null)
            {
                handler(this, new CloseEventArgs(code, reason));
            }
        }

        #endregion

        #region PInvoke
        
        private delegate void OnConnectDelegate(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string connectMessage, uint connectMessageSize);
        private delegate void OnMessageDelegate(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string message, uint messageSize);
        private delegate void OnErrorDelegate(IntPtr websocketHandle, ushort errorCode, [MarshalAs(UnmanagedType.LPStr)] string errorMessage, uint errorMessageSize);
        private delegate void OnCloseDelegate(IntPtr websocketHandle, ushort code, [MarshalAs(UnmanagedType.LPStr)] string reason, uint reasonSize);

        [DllImport("simplewebsocket")]
        private static extern int create_websocket(ref IntPtr websocketHandlePtr);

        [DllImport("simplewebsocket")]
        private static extern int add_header(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

        [DllImport("simplewebsocket")]
        private static extern int open_websocket(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string uri, OnConnectDelegate onConnect, OnMessageDelegate onMessage, OnErrorDelegate onError, OnCloseDelegate onClose);

        [DllImport("simplewebsocket")]
        private static extern int write_websocket(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string message);

        [DllImport("simplewebsocket")]
        private static extern int read_websocket(IntPtr websocketHandle, OnMessageDelegate onMessage);

        [DllImport("simplewebsocket")]
        private static extern int close_websocket(IntPtr websocketHandle);

        #endregion
    }
}