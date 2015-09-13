using System;
using System.Threading.Tasks;

namespace Sharp.Queue {
    public interface ISharpQueue : IDisposable {
        Task EnqueueAsync<T>(T item) where T : class;
        Task<T> DequeueAsync<T>() where T : class;
        Task<QueueInfo> GetQueueInfoAsync();
        void CleanQueue();
    }
}