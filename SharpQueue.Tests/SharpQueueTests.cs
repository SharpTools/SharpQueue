using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Sharp.Queue.Tests {
    public class SharpQueueTests {

        private string _dir = "dir";
        private SharpQueue _queue;

        [SetUp]
        public void SetUp() {
            _queue = new SharpQueue(_dir);
            EmptyDir();
        }

        [Test]
        public void Should_create_base_directory() {
            Assert.IsTrue(Directory.Exists(_dir));
        }

        [Test]
        public async Task Should_enqueue() {
            await _queue.EnqueueAsync("test");
            var filename = GetFirstDirFile();
            var text = File.ReadAllText(Path.Combine(_dir, filename));
            Assert.AreEqual("\"test\"", text);
        }

        [Test]
        public async Task Should_dequeue() {
            await _queue.EnqueueAsync("test");
            var item = await _queue.DequeueAsync<string>();
            Assert.AreEqual("test", item);
        }

        [TearDown]
        public void TearDown() {
            EmptyDir();
            Directory.Delete(_dir);
            _queue.Dispose();
        }

        private void EmptyDir() {
            var dir = new DirectoryInfo(_dir);
            dir.EnumerateFiles().ToList().ForEach(f => f.Delete());
        }

        private string GetFirstDirFile() {
            var dir = new DirectoryInfo(_dir);
            return dir.EnumerateFiles().First().Name;
        }
    }
}
