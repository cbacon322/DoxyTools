using System.Threading;

namespace DoxyTools
{
    public static class GenerationControl
    {
        public static CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static void CancelGeneration()
        {
            if (!CancellationTokenSource.IsCancellationRequested)
            {
                CancellationTokenSource.Cancel();
            }
        }
    }
}