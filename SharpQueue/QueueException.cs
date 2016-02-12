using System;

namespace SharpQueue {
    public class QueueException : Exception {
        public QueueException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
