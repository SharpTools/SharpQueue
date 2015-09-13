namespace Sharp.Queue {
    public class QueueInfo {
        public int Enqueued { get; }
        public int Error { get; }

        public QueueInfo(int enqueued, int error) {
            Enqueued = enqueued;
            Error = error;
        }
    }
}