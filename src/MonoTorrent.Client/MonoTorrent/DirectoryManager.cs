using System.Collections.Generic;

namespace MonoTorrent;

public static class DirectoryManager
{
    private static readonly List<DirectoryItem> Directories = [];

    public static int Add (string directoryPath)
    {
        if (string.IsNullOrEmpty (directoryPath)) {
            return -1;
        }

        for (int i = 0; i < Directories.Count; i++) {
            if (Directories[i].Path == directoryPath) {
                Directories[i].Count++;
                return i;
            }
        }

        var index = Directories.Count;

        Directories.Add (new DirectoryItem { Count = 1, Path = directoryPath });

        return index;
    }

    public static string Get (int index)
    {
        return index < 0 ? string.Empty : Directories[index].Path;
    }
}
