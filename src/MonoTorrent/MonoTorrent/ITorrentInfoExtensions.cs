using System;

namespace MonoTorrent;

public static class ITorrentInfoExtensions
{
    static bool IsV1Only (this ITorrentInfo self)
        => self.InfoHashes.V1 != null && self.InfoHashes.V2 == null;

    static bool IsV2Only (this ITorrentInfo self)
        => self.InfoHashes.V1 == null && self.InfoHashes.V2 != null;

    static bool IsHybrid (this ITorrentManagerInfo self)
        => self.InfoHashes.IsHybrid;

    static bool IsHybrid (this ITorrentInfo self)
        => self.InfoHashes.IsHybrid;

    public static int BlocksPerPiece (this ITorrentInfo self, int pieceIndex)
        => (BytesPerPiece (self, pieceIndex) + Constants.BlockSize - 1) / Constants.BlockSize;

    public static int BytesPerBlock (this ITorrentInfo self, int pieceIndex, int blockIndex)
        => Math.Min (Constants.BlockSize, self.BytesPerPiece (pieceIndex) - blockIndex * Constants.BlockSize);

    public static int BytesPerPiece (this ITorrentInfo self, int pieceIndex)
    {
        if (self.IsV2Only ()) {
            return BytesPerPieceV2 (self, pieceIndex);
        } else {
            return BytesPerPieceV1 (self, pieceIndex);
        }
    }

    public static int BytesPerPieceV1 (this ITorrentInfo self, int pieceIndex)
    {
        if (self.InfoHashes.V1 != null) {
            // Hybrid torrents always have padding files, and v1 torrents do not have
            // piece aligned files, so it's fine.
            if (pieceIndex < self.PieceCount () - 1)
                return self.PieceLength;
            return (int) (self.Size - self.PieceIndexToByteOffset (pieceIndex));
        } else {
            throw new NotSupportedException ();
        }
    }

    public static int BytesPerPieceV2 (this ITorrentInfo self, int pieceIndex)
    {
        if (self.InfoHashes.V2 != null) {
            // V2 only torrents aren't padded and so may have smaller
            // pieces at the end of each file.
            for (int i = 0; i < self.Files.Count; i++) {
                var file = self.Files[i];
                if (pieceIndex < file.StartPieceIndex || pieceIndex > file.EndPieceIndex || file.Length == 0)
                    continue;
                var remainder = file.Length - (pieceIndex - file.StartPieceIndex) * (long) self.PieceLength;
                return (int) (remainder > self.PieceLength ? self.PieceLength : remainder);
            }
            throw new ArgumentOutOfRangeException (nameof (pieceIndex));
        } else {
            throw new NotSupportedException ();
        }
    }

    public static int ByteOffsetToPieceIndex (this ITorrentInfo self, long offset)
    {
        if (self.IsV2Only ()) {
            for (int i = 0; i < self.Files.Count; i++) {
                var file = self.Files[i];
                if (offset < file.OffsetInTorrent || offset >= file.OffsetInTorrent + file.Length)
                    continue;
                return file.StartPieceIndex + (int) ((offset - file.OffsetInTorrent) / self.PieceLength);
            }
            throw new ArgumentOutOfRangeException (nameof (offset));
        } else {
            // Works for padded, and unpadded, V1 torrents. including hybrid v1/v2 torrents.
            if (offset < 0 || offset >= self.Size)
                throw new ArgumentOutOfRangeException (nameof (offset));
            return (int) (offset / self.PieceLength);
        }
    }

    /// <summary>
    /// The number of pieces in the torrent
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static int PieceCount (this ITorrentInfo self)
    {
        if (self.IsV2Only ())
            return self.Files[self.Files.Count - 1].EndPieceIndex + 1;
        else
            return (int) ((self.Size + self.PieceLength - 1) / self.PieceLength);
    }

    public static long PieceIndexToByteOffset (this ITorrentInfo self, int pieceIndex)
    {
        if (self.IsV2Only ()) {
            for (int i = 0; i < self.Files.Count; i++) {
                var file = self.Files[i];
                if (pieceIndex < file.StartPieceIndex || pieceIndex > file.EndPieceIndex)
                    continue;
                return file.OffsetInTorrent + ((pieceIndex - file.StartPieceIndex) * (long) self.PieceLength);
            }
            throw new ArgumentOutOfRangeException (nameof (pieceIndex));
        } else {
            return (long) self.PieceLength * pieceIndex;
        }
    }
}