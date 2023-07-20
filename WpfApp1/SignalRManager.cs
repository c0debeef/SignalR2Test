using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;

namespace WpfApp1
{
    public class SignalRManager
    {
        private static readonly TimeSpan CONNECTION_RESTART_DELAY = TimeSpan.FromSeconds(30);

        private static CancellationTokenSource _cancellationTokenSource = null;
        private static Task _task = null;

        private const string HubUrl = @"http://localhost:8888/";

        static SignalRManager()
        {
            ServicePointManagerConfig.Initialize();
        }

        public static HubConnection GetStraxeHubConnection()
        {
            HubConnection result = null;

            try
            {
                result = new HubConnection(HubUrl);
            }
            catch (Exception)
            {
                ((IDisposable)result)?.Dispose();
                throw;
            }

            return result;
        }

        public static /*async Task*/ void Start(string username)
        {
            if (null != _task) return;

            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                CancellationToken cancellationToken = _cancellationTokenSource.Token;

                _task = Task.Run(async () => await Launch(username, cancellationToken), CancellationToken.None);

                //await Task.Yield();
            }
            catch (Exception exception)
            {
                ;
            }
        }

        public static async Task Stop()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                await Task.Yield();

                if (null != _task) await _task;
            }
            catch (Exception exception)
            {
                ;
            }
            finally
            {
                _task?.Dispose();
                _cancellationTokenSource?.Dispose();
            }

            _task = null;
            _cancellationTokenSource = null;
        }

        private static async Task Launch(string username, CancellationToken cancellationToken = default)
        {
            bool first = true;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Yield();

                    if (!cancellationToken.IsCancellationRequested && !first) await Task.WhenAny(Task.Delay(CONNECTION_RESTART_DELAY, cancellationToken));

                    first = false;

                    using (var connection = GetStraxeHubConnection())
                    {
#if DEBUG
                        connection.TraceLevel = TraceLevels.All;
                        connection.TraceWriter = new DebugTextWriter(); //Console.Out;

                        connection.Error += (ex) => Debug.WriteLine($@"#[Error: {ex.Message}]");

                        connection.Closed += () => Debug.WriteLine(@"#[Closed]");

                        connection.ConnectionSlow += () => Debug.WriteLine(@"#[ConnectionSlow]");

                        connection.StateChanged += (state) => Debug.WriteLine($@"#[StateChanged: {state.OldState}->{state.NewState}]");
                        connection.Received += (data) => Debug.WriteLine($@"#[Received: {data}]");

                        connection.Reconnecting += () => Debug.WriteLine(@"#[Reconnecting]");
                        connection.Reconnected += () => Debug.WriteLine(@"#[Reconnected]");
#endif
                        connection.Headers.Add("Username", username);

                        var proxy = connection.CreateHubProxy("NHub");

                        using (var heartbeat = proxy.On("Heartbeat", Heartbeat))
                        using (var message = proxy.On<string>("NewMessage", (msg) => NewMessage(msg)))
                        {
                            await connection.Start();

                            while (!cancellationToken.IsCancellationRequested)
                            {
                                await Task.WhenAny(Task.Delay(Timeout.Infinite, cancellationToken));
                                //try
                                //{
                                //    //await proxy.Invoke("Heartbeat").CAF();


                                //    await Task.Delay(CONNECTION_HEARTBEAT_DELAY, cancellationToken).CAF();
                                //}
                                //catch (TaskCanceledException) { }
                            }
                        }

                        connection.Stop(); //TODO: FIXME!!! <-- I AM CRASH HERE!!!
                    }
                }
                catch (Exception exception)
                {
                    ;
                }
                finally
                {
                    ;
                }
            }
        }
        private static void Heartbeat()
        {
            ;
        }
        private static void NewMessage(string message)
        {
            ;
        }
    }

#if DEBUG
    public class DebugTextWriter : TextWriter
    {
        private StringBuilder buffer;

        public DebugTextWriter()
        {
            buffer = new StringBuilder();
        }

        public override void Write(char value)
        {
            switch (value)
            {
                case '\n':
                    return;
                case '\r':
                    Debug.WriteLine(buffer.ToString());
                    buffer.Clear();
                    return;
                default:
                    buffer.Append(value);
                    break;
            }
        }

        public override void Write(string value)
        {
            Debug.WriteLine(value);

        }
        public override Encoding Encoding
        {
            get { throw new NotImplementedException(); }
        }
    }
#endif
}
