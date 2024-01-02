// GenerationControl.cs

using System.Threading;

public static class GenerationControl
{
    public static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

    public static void CancelGeneration()
    {
        if (!CancellationTokenSource.IsCancellationRequested)
        {
            CancellationTokenSource.Cancel();
            // Recreate the CancellationTokenSource for the next operation
            CancellationTokenSource = new CancellationTokenSource();
        }
    }
}
