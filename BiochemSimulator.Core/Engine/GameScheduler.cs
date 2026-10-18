using System;
using System.Threading.Tasks;

namespace BiochemSimulator.Engine
{
    /// <summary>
    /// Abstraction for running an action after a delay. The core uses this for
    /// timed phase transitions instead of depending on a UI framework's
    /// dispatcher. A desktop UI supplies an implementation that marshals the
    /// callback onto its UI thread; headless hosts can use the default.
    /// </summary>
    public interface IGameScheduler
    {
        void ScheduleDelayed(TimeSpan delay, Action action);
    }

    /// <summary>
    /// Default scheduler: runs the action on a thread-pool thread after the
    /// delay. Engine events raised from the callback are the host's to marshal.
    /// </summary>
    public sealed class DefaultGameScheduler : IGameScheduler
    {
        public void ScheduleDelayed(TimeSpan delay, Action action)
        {
            _ = Task.Delay(delay).ContinueWith(_ => action());
        }
    }
}
