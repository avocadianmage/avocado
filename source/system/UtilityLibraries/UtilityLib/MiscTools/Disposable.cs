using System;

namespace UtilityLib.MiscTools
{
    public abstract class Disposable : IDisposable
    {
        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);
        }

        ~Disposable()
        {
            dispose(false);
        }

        void dispose(bool safeToFreeManagedResources)
        {
            FreeUnmanagedResources();

            if (!safeToFreeManagedResources) return;
            FreeManagedResources();
        }

        protected virtual void FreeUnmanagedResources() { }
        protected virtual void FreeManagedResources() { }
    }
}