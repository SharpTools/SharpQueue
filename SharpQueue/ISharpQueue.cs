using System;
using System.Threading.Tasks;

namespace Sharp.Queue {
    public interface ISharpQueue : IDisposable {
        void Enqueue<T>(T item) where T : class;
        T Dequeue<T>() where T : class;
        QueueInfo GetQueueInfo();
        void CleanQueue();
    }
}