using System;
using System.Threading;
using System.Threading.Tasks;

namespace HidLibrary
{
    internal class HidInputEventMonitor
    {
        public event InputEventHandler OnInput;

        private readonly HidDevice _device;

        public HidInputEventMonitor(HidDevice device)
        {
            _device = device;
        }

        public void Init()
        {
#if NET20 || NET35 || NET5_0_OR_GREATER
            Task task = Task.Factory.StartNew(() => InputEventMonitor());
#else
            var eventMonitor = new Action(InputEventMonitor);
            eventMonitor.BeginInvoke(DisposeInputEventMonitor, eventMonitor);
#endif
        }

        private void InputEventMonitor()
        {
            const int Timeout = 1;
            if (OnInput != null)
            {
                HidDeviceData data = _device.Read(Timeout);
                if (data.Status == HidDeviceData.ReadStatus.Success)
                {
                    OnInput(data);
                }
            }
            Thread.Yield();

            if (_device.MonitorInputEvents) Init();
        }

        private static void DisposeInputEventMonitor(IAsyncResult ar)
        {
            ((Action)ar.AsyncState).EndInvoke(ar);
        }
    }
}
