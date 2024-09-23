namespace CarSharing
{
    internal abstract class SafeDisposable: IDisposable
    {
        volatile int _isDisposed = 0;

        protected SafeDisposable() { }

        abstract protected void DisposeConcrete();

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                DisposeConcrete();
                GC.SuppressFinalize(this);
            }
        }

        public bool IsDisposed => Interlocked.CompareExchange(ref _isDisposed, 1, 1) == 1;

        protected void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(IsDisposed, this);
    }
}
