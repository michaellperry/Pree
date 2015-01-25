using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UpdateControls.Fields;

namespace Pree.CSP
{
    class Process
    {
        private AsyncSemaphore _lock = new AsyncSemaphore();
        private Independent<Exception> _exception = new Independent<Exception>();

        public Exception Exception
        {
            get
            {
                lock (_exception)
                {
                    return _exception;
                }
            }
        }

        public Task JoinAsync()
        {
            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();
            Enqueue(async delegate
            {
                completion.SetResult(true);
            });
            return completion.Task;
        }

        protected async void Enqueue(Func<Task> request)
        {
            await _lock.WaitAsync();
            try
            {
                await request();
                lock (_exception)
                {
                    _exception.Value = null;
                }
            }
            catch (Exception x)
            {
                lock (_exception)
                {
                    _exception.Value = x;
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        protected void Enqueue(Action request)
        {
            Enqueue(() => Task.Run(request));
        }
    }
}
