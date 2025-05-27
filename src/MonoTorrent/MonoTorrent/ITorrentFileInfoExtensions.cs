namespace MonoTorrent;

public static class ITorrentFileInfoExtensions
{
    public static long BytesDownloaded (this ITorrentManagerFile info)
        => (long) (info.BitField.PercentComplete * info.Length / 100.0);

    public static bool Overlaps (this ITorrentManagerFile self, ITorrentManagerFile file)
        => self.Length > 0 &&
           file.Length > 0 &&
           self.StartPieceIndex <= file.EndPieceIndex &&
           file.StartPieceIndex <= self.EndPieceIndex;
}
