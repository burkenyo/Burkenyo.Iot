namespace Burkenyo.Iot.Server;

static class AsyncEventExtensions
{
    static async Task PreserveAggregateException(Task task)
    {
        try
        {
            await task;
        }
        catch
        {
            throw task.Exception!;
        }
    }

    public static Task InvokeMulti(this Func<Task> @event) =>
        PreserveAggregateException(Task.WhenAll(@event.GetInvocationList().Select(@delegate =>
        {
            try
            {
                return ((Func<Task>)@delegate).Invoke();
            }
            catch (Exception ex)
            {
                // Handle synchronous exceptions
                return Task.FromException(ex);
            }
        })));

    public static Task InvokeMulti<T>(this Func<T, Task> @event, T arg) =>
        PreserveAggregateException(Task.WhenAll(@event.GetInvocationList().Select(@delegate =>
        {
            try
            {
                return ((Func<T, Task>)@delegate).Invoke(arg);
            }
            catch (Exception ex)
            {
                // Handle synchronous exceptions
                return Task.FromException(ex);
            }
        })));

    public static Task InvokeMulti<T1, T2>(this Func<T1, T2, Task> @event, T1 arg1, T2 arg2) =>
        PreserveAggregateException(Task.WhenAll(@event.GetInvocationList().Select(@delegate =>
        {
            try
            {
                return ((Func<T1, T2, Task>)@delegate).Invoke(arg1, arg2);
            }
            catch (Exception ex)
            {
                // Handle synchronous exceptions
                return Task.FromException(ex);
            }
        })));

    public static Task InvokeMulti<T1, T2, T3>(this Func<T1, T2, Task> @event, T1 arg1, T2 arg2, T3 arg3) =>
        PreserveAggregateException(Task.WhenAll(@event.GetInvocationList().Select(@delegate =>
        {
            try
            {
                return ((Func<T1, T2, T3, Task>)@delegate).Invoke(arg1, arg2, arg3);
            }
            catch (Exception ex)
            {
                // Handle synchronous exceptions
                return Task.FromException(ex);
            }
        })));
}