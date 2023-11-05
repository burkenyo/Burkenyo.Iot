using System.Collections;
using System.Threading;

namespace Blinky;

static class EventQueue
{
    readonly static AutoResetEvent s_signaler = new(false);
    readonly static Queue s_queue = new();

    public static void Enqueue(object @event)
    {
        s_queue.Enqueue(@event);
        s_signaler.Set();
    }

    public static object Dequeue()
    {
        while (s_queue.Count == 0)
        {
            s_signaler.WaitOne();
        }
        
        return s_queue.Dequeue();
    }
}
