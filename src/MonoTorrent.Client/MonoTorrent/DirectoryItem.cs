using System.Threading;

namespace MonoTorrent;

public class DirectoryItem
{
    static long _id = 0;

    public long Id { get; private set; }

    public string Path { get; set; } = string.Empty;

    public int Count { get; set; }

    public DirectoryItem ()
    {
        Id = Interlocked.Increment (ref _id);
    }
}
