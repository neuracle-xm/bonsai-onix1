using System.Collections.Concurrent;

namespace OpenEphys.Onix1;

public class MatCache<T> where T : class
{
    /// <summary>
    /// 缓存数据的字典，键为设备名，值为对应的队列。
    /// </summary>
    public static readonly ConcurrentDictionary<string, ConcurrentQueue<T>> DataQueueDict = new();
    public static void AddToQueue(string deviceName, T item)
    {
        // 如果设备名对应队列不存在则创建一个
        if (!DataQueueDict.TryGetValue(deviceName, out var queue))
        {
            queue = new ConcurrentQueue<T>();
            DataQueueDict[deviceName] = queue;
        }

        // 将数据添加到对应队列中
        queue.Enqueue(item);
    }
}

