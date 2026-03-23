using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cs2Overlay;

public sealed class GameStateListener : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly int _port;
    private CancellationTokenSource? _cts;

    public event EventHandler<GameState>? GameStateUpdated;

    public GameStateListener(int port)
    {
        _port = port;
        _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
    }

    public void Start()
    {
        if (_cts != null)
            return;

        _cts = new CancellationTokenSource();
        _listener.Start();
        _ = Task.Run(() => ListenLoopAsync(_cts.Token));
    }

    private async Task ListenLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            HttpListenerContext? ctx = null;
            try
            {
                ctx = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch when (token.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                continue;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    using var reader = new System.IO.StreamReader(ctx.Request.InputStream, Encoding.UTF8);
                    var body = await reader.ReadToEndAsync().ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        var state = JsonSerializer.Deserialize<GameState>(body);
                        if (state != null)
                            GameStateUpdated?.Invoke(this, state);
                    }

                    ctx.Response.StatusCode = 200;
                    await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("OK")).ConfigureAwait(false);
                }
                catch
                {
                    try { ctx.Response.StatusCode = 500; } catch { }
                }
                finally
                {
                    try { ctx.Response.OutputStream.Close(); } catch { }
                }
            }, token);
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _listener.Close();
    }
}

using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Cs2Overlay;

public sealed class GameStateListener : IDisposable
{
    private readonly HttpListener _listener = new();
    private readonly int _port;
    private CancellationTokenSource? _cts;

    public event EventHandler<GameState>? GameStateUpdated;

    public GameStateListener(int port)
    {
        _port = port;
        _listener.Prefixes.Add($"http://127.0.0.1:{_port}/");
    }

    public void Start()
    {
        if (_cts != null)
            return;

        _cts = new CancellationTokenSource();
        _listener.Start();
        _ = Task.Run(() => ListenLoopAsync(_cts.Token));
    }

    private async Task ListenLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            HttpListenerContext? ctx = null;
            try
            {
                ctx = await _listener.GetContextAsync().ConfigureAwait(false);
            }
            catch when (token.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                continue;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    using var reader = new System.IO.StreamReader(ctx.Request.InputStream, Encoding.UTF8);
                    var body = await reader.ReadToEndAsync().ConfigureAwait(false);

                    if (!string.IsNullOrWhiteSpace(body))
                    {
                        var state = JsonSerializer.Deserialize<GameState>(body);
                        if (state != null)
                        {
                            GameStateUpdated?.Invoke(this, state);
                        }
                    }

                    ctx.Response.StatusCode = 200;
                    await ctx.Response.OutputStream.WriteAsync(Encoding.UTF8.GetBytes("OK")).ConfigureAwait(false);
                }
                catch
                {
                    if (ctx != null)
                    {
                        try
                        {
                            ctx.Response.StatusCode = 500;
                        }
                        catch { /* ignore */ }
                    }
                }
                finally
                {
                    try { ctx.Response.OutputStream.Close(); } catch { /* ignore */ }
                }
            }, token);
        }
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _listener.Close();
    }
}

