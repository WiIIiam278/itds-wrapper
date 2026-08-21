using System.Collections.Generic;
using System.Linq;

namespace ITDSWrapper.Core;

public class DropOutQueue<T>(int maxSize)
{
    private Queue<T> _queue = new();
    public int MaxSize { get; } = maxSize;

    public void Add(T item)
    {
        _queue.Enqueue(item);
        if (_queue.Count > MaxSize)
            _queue.Dequeue();
    }

    public T? Dequeue()
    {
        return _queue.Count == 0 ? default : _queue.Dequeue();
    }

    public List<T> GetList()
    {
        return [.. _queue];
    }
}