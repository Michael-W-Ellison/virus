using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using BiochemSimulator.Engine;

namespace BiochemSimulator
{
    /// <summary>
    /// WPF implementation of the core's <see cref="IGameScheduler"/>. Runs the
    /// delayed callback back on the UI thread so timed phase transitions can
    /// safely raise events the UI is bound to — preserving the behavior the
    /// engine previously hard-coded via Application.Current.Dispatcher.
    /// </summary>
    public sealed class DispatcherGameScheduler : IGameScheduler
    {
        private readonly Dispatcher _dispatcher;

        public DispatcherGameScheduler(Dispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void ScheduleDelayed(TimeSpan delay, Action action)
        {
            _ = Task.Delay(delay).ContinueWith(_ => _dispatcher.Invoke(action));
        }
    }
}
