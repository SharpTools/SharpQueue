using System.IO;
using System.Linq;
using NUnit.Framework;
using SharpQueue;

namespace Sharp.Queue.Tests {
    public class SharpQueueTests {

        private string _dir = "dir";
        private string _deepDir = "secondDir\\test";
        private DirectoryQueue _queue;

        [SetUp]
        public void SetUp() {
            EmptyDirs(_dir, _deepDir);
            _queue = new DirectoryQueue(_dir);
        }

        [Test]
        public void Should_create_base_directory() {
            Assert.IsTrue(Directory.Exists(_dir));
        }

        [Test]
        public void Should_enqueue() {
            _queue.Enqueue("test");
            var filename = GetFirstDirFile(_dir);
            var text = File.ReadAllText(Path.Combine(_dir, filename));
            Assert.AreEqual("\"test\"", text);
        }

        [Test]
        public void Mutex_names_dont_allow_backslash() {
            _queue = new DirectoryQueue(_deepDir);
            _queue.Enqueue("test");
            var filename = GetFirstDirFile(_deepDir);
            var text = File.ReadAllText(Path.Combine(_deepDir, filename));
            Assert.AreEqual("\"test\"", text);
        }

        [Test]
        public void Should_dequeue() {
            _queue.Enqueue("test");
            var item = _queue.Dequeue<string>();
            Assert.AreEqual("test", item);
        }

        [TearDown]
        public void TearDown() {
            EmptyDirs(_dir, _deepDir);
            _queue.Dispose();
        }

        [Test]
        public void Should_count_queue_length() {
            _queue.Enqueue("teste");
            _queue.Enqueue("teste");
            _queue.Enqueue("teste");
            Assert.AreEqual(3, _queue.GetQueueInfo().Enqueued);
        }

        [Test]
        public void When_queue_is_empty__returns_null() {
            _queue.Enqueue("teste");
            _queue.Dequeue<string>();
            var item = _queue.Dequeue<string>();
            Assert.IsNull(item);
        }

        private void EmptyDirs(params string[] dirs) {
            foreach (var d in dirs) {
                var dir = new DirectoryInfo(d);
                if (!dir.Exists) {
                    continue;
                }
                dir.EnumerateFiles().ToList().ForEach(f => f.Delete());
                dir.Delete();
            }
        }

        private string GetFirstDirFile(string dir) {
            var d = new DirectoryInfo(dir);
            return d.EnumerateFiles().First().Name;
        }
    }
}
