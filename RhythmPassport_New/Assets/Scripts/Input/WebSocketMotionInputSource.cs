using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace RhythmPassport.Input
{
    public sealed class WebSocketMotionInputSource : MonoBehaviour, IMotionInputSource
    {
        [SerializeField] private string webSocketUrl = "ws://127.0.0.1:8765";
        [SerializeField] private bool connectOnEnable = false;
        [SerializeField] private float reconnectDelaySeconds = 2f;

        private readonly ConcurrentQueue<MotionInputFrame> pendingFrames = new ConcurrentQueue<MotionInputFrame>();
        private CancellationTokenSource cancellationTokenSource;
        private ClientWebSocket clientWebSocket;
        private bool shouldReconnect;

        public string SourceName => "WebSocket";

        public string StatusText { get; private set; } = "대기 중";

        public bool IsReady => clientWebSocket != null && clientWebSocket.State == WebSocketState.Open;

        public event Action<MotionInputFrame> MotionDetected;

        public void SetEndpoint(string url)
        {
            webSocketUrl = url;
        }

        public void Connect()
        {
            if (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
            {
                return;
            }

            shouldReconnect = true;
            cancellationTokenSource ??= new CancellationTokenSource();
            _ = ConnectAndReceiveLoopAsync(cancellationTokenSource.Token);
        }

        public void Disconnect()
        {
            shouldReconnect = false;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource = null;

            if (clientWebSocket != null)
            {
                try
                {
                    clientWebSocket.Dispose();
                }
                catch
                {
                }

                clientWebSocket = null;
            }

            StatusText = "연결 해제";
        }

        private void OnEnable()
        {
            if (connectOnEnable)
            {
                Connect();
            }
        }

        private void OnDisable()
        {
            Disconnect();
        }

        private void Update()
        {
            while (pendingFrames.TryDequeue(out var frame))
            {
                MotionDetected?.Invoke(frame);
            }
        }

        private async Task ConnectAndReceiveLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && shouldReconnect)
            {
                try
                {
                    StatusText = $"연결 시도 중: {webSocketUrl}";
                    clientWebSocket = new ClientWebSocket();
                    await clientWebSocket.ConnectAsync(new Uri(webSocketUrl), cancellationToken);
                    StatusText = "연결됨";

                    await ReceiveLoopAsync(clientWebSocket, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    StatusText = "연결 취소";
                    break;
                }
                catch (Exception ex)
                {
                    StatusText = $"연결 실패: {ex.Message}";
                }
                finally
                {
                    if (clientWebSocket != null)
                    {
                        clientWebSocket.Dispose();
                        clientWebSocket = null;
                    }
                }

                if (!shouldReconnect || cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                StatusText = "재연결 대기 중";
                await Task.Delay(TimeSpan.FromSeconds(reconnectDelaySeconds), cancellationToken);
            }
        }

        private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken cancellationToken)
        {
            var buffer = new byte[2048];

            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var builder = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        StatusText = "서버가 연결을 종료했습니다.";
                        return;
                    }

                    builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                }
                while (!result.EndOfMessage);

                HandleIncomingJson(builder.ToString());
            }
        }

        private void HandleIncomingJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var message = JsonUtility.FromJson<WebSocketMotionMessage>(json);
            if (message == null)
            {
                StatusText = "수신 실패: 잘못된 JSON";
                return;
            }

            if (!string.IsNullOrWhiteSpace(message.status))
            {
                StatusText = message.status;
            }

            if (!string.Equals(message.type, "motion_result", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var actionType = MotionActionMapper.FromExternalName(message.motion);
            if (actionType == MotionActionType.None)
            {
                StatusText = $"알 수 없는 동작: {message.motion}";
                return;
            }

            var timestamp = message.timestamp > 0d ? message.timestamp : Time.timeAsDouble;
            pendingFrames.Enqueue(new MotionInputFrame(actionType, message.confidence, timestamp));
            StatusText = $"최근 입력: {message.motion}";
        }
    }
}
