using System.Collections.Generic;
using System.Linq;

namespace MonoTorrent;

public static class DirectoryManager
{
    private static readonly List<DirectoryItem> Directories = [];

    public static long Add (string directoryPath)
    {
        if (string.IsNullOrEmpty (directoryPath)) {
            return -1;
        }

        var item = Directories.SingleOrDefault (x => x.Path == directoryPath);

        if (item == null) {
            item = new DirectoryItem { Count = 1, Path = directoryPath };
            Directories.Add (item);
        } else {
            item.Count++;
        }

        return item.Id;
    }

    public static string Get (long id)
    {
        return id <= 0 ? string.Empty : Directories.Single (x => x.Id == id).Path;
    }
}
