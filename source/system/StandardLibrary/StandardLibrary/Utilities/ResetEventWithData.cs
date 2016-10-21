using System.Threading;

namespace StandardLibrary.Utilities
{
    public class ResetEventWithData<T>
    {
        readonly AutoResetEvent resetEvent = new AutoResetEvent(false);
        T data;

        public T Block()
        {
            resetEvent.WaitOne();
            return data;
        }

        public void Signal(T data)
        {
            this.data = data;
            resetEvent.Set();
        }
    }
}
