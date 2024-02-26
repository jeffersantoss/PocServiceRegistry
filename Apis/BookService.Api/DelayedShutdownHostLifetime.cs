using System.Runtime.InteropServices;

namespace BookService.Api;

public class DelayedShutdownHostLifetime(IHostApplicationLifetime applicationLifetime, TimeSpan delay) : IHostLifetime, IDisposable
{
    private IEnumerable<IDisposable>? _disposables;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        _disposables =
        [
        PosixSignalRegistration.Create(PosixSignal.SIGINT, HandleSignal),
        PosixSignalRegistration.Create(PosixSignal.SIGQUIT, HandleSignal),
        PosixSignalRegistration.Create(PosixSignal.SIGTERM, HandleSignal)
        ];
        return Task.CompletedTask;
    }

    protected void HandleSignal(PosixSignalContext ctx)
    {
        ctx.Cancel = true;
        Task.Delay(delay).ContinueWith(t => applicationLifetime.StopApplication());
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables ?? Enumerable.Empty<IDisposable>())
        {
            disposable.Dispose();
        }
    }
}
