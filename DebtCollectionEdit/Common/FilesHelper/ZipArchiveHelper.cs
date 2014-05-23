using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DebtCollection.Common.FilesHelper
{
    public static class ZipArchiveHelper
    {
        public static MemoryStream CreateZipArchivStream(IEnumerable<ZipArchiveHelperInput> files)
        {
            MemoryStream zipStream = new MemoryStream();

            using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                int fileId = 0;
                foreach (var file in files)
                {
                    fileId++;
                    var entry = zipArchive.CreateEntry(fileId + "_" + file.ResultFileName);
                    using (var entryStream = entry.Open())
                    {
                        FileStream fileStream = new FileStream(file.InputFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                        fileStream.CopyTo(entryStream);
                    }
                }
            }
            zipStream.Position = 0;
            return zipStream;
        }
    }
}
