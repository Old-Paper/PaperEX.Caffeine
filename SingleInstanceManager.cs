namespace PaperEX.Caffeine;

/// <summary>
/// Guarantees that only one instance of the app runs per user session,
/// so two processes never call SetThreadExecutionState concurrently.
/// </summary>
internal sealed class SingleInstanceManager : IDisposable
{
    private readonly Mutex _mutex;
    private readonly bool _isFirstInstance;

    public SingleInstanceManager(string mutexName)
    {
        _mutex = new Mutex(initiallyOwned: true, mutexName, out bool createdNew);

        // If a previous instance crashed, the mutex is abandoned; claim it then.
        try
        {
            _isFirstInstance = createdNew || _mutex.WaitOne(0);
        }
        catch (AbandonedMutexException)
        {
            _isFirstInstance = true;
        }
    }

    public bool IsFirstInstance => _isFirstInstance;

    public void Dispose()
    {
        if (_isFirstInstance)
        {
            _mutex.ReleaseMutex();
        }

        _mutex.Dispose();
    }
}
