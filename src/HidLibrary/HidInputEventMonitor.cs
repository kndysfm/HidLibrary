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

        public bool IsMonitoring { get; private set; } = false;

        public void Init()
        {
#if NET20 || NET35 || NET5_0_OR_GREATER
            Task task = Task.Factory.StartNew(() => InputEventMonitor());
#else
            var eventMonitor = new Action(InputEventMonitor);
            eventMonitor.BeginInvoke(DisposeInputEventMonitor, eventMonitor);
#endif
            IsMonitoring = true;
        }

        public int Timeout { get; set; } = 1000;

        private void InputEventMonitor()
        {
            if (OnInput != null)
            {
                HidDeviceData data = _device.Read(Timeout);
                if (data.Status == HidDeviceData.ReadStatus.Success)
                {
                    OnInput(data);
                }
            }
            else
            {
                Thread.Sleep(Timeout);
            }

            if (_device.MonitorInputEvents) Init();
            else IsMonitoring = false;
        }

        private static void DisposeInputEventMonitor(IAsyncResult ar)
        {
            ((Action)ar.AsyncState).EndInvoke(ar);
        }
    }
}
