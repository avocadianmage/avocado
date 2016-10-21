using System.Threading;

namespace UtilityLib.Utilities
{
    public class ResetEventWithData<T>
    {        
        readonly ManualResetEvent mre = new ManualResetEvent(false);
        T data;

        public T Block()
        {
            mre.Reset();
            mre.WaitOne();
            mre.Reset();
            return data;
        }

        public void Signal(T data)
        {
            this.data = data;
            mre.Set();
        }
    }
}
