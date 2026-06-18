using System.Collections.Concurrent;

namespace __NS__;

// Referenz der wichtigsten Synchronisations-Muster (anpassen / loeschen).
// In einem eigenen Thread starten z.B. mit:  new Thread(demo.Kritisch).Start();
internal class ThreadingDemo
{
    // --- lock: NUR auf ein privates readonly-Objekt sperren (nie this/typeof/string) ---
    private readonly object _lock = new object();
    public void Kritisch()
    {
        lock (_lock)
        {
            // kritischer Abschnitt
        }
    }

    // --- SemaphoreSlim: max. N gleichzeitig (z.B. N Landebahnen) ---
    private readonly SemaphoreSlim _slots = new SemaphoreSlim(3, 3);
    public void MitSlot()
    {
        _slots.Wait();
        try
        {
            // Arbeit (max. 3 Threads gleichzeitig)
        }
        finally
        {
            _slots.Release();   // IMMER im finally freigeben!
        }
    }

    // --- Monitor: auf eine Bedingung warten -> IMMER in while(), nicht if() ---
    private readonly object _cond = new object();
    public void WarteAuf(Func<bool> bedingung)
    {
        lock (_cond)
        {
            while (!bedingung()) Monitor.Wait(_cond);
        }
    }
    public void Signalisieren()
    {
        lock (_cond) { Monitor.PulseAll(_cond); }
    }

    // --- Producer/Consumer (thread-safe Queue) ---
    private readonly BlockingCollection<string> _queue = new BlockingCollection<string>();
    public void Produce(string item) => _queue.Add(item);
    public void ConsumeLoop()
    {
        foreach (string item in _queue.GetConsumingEnumerable())
        {
            // item verarbeiten
        }
    }
}
