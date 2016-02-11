# SharpQueue
Ultra simple multithread / multiprocess directory based persistent queue system for C#

- VNext ready
- Thread safe
- Fast and simple

## install-package sharpqueue

# Usage

## Enqueue

```
using (var q = new DirectoryQueue("queue")) {
  q.Enqueue(item);
}
```

## Dequeue
Other thread, process or application
```
using (var q = new DirectoryQueue("queue")) {
   var item = q.Dequeue<List<Item>>();
   ...
}
```




