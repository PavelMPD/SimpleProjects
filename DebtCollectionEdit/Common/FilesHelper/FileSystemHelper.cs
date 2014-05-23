using System;
using System.IO;

namespace DebtCollection.Common.FilesHelper
{
    public static class FileSystemHelper
    {
        public static void SaveFile(byte[] fileData, FilePath filePath)
        {
            if (!Directory.Exists(filePath.PathOnly))
            {
                Directory.CreateDirectory(filePath.PathOnly);
            }

            File.WriteAllBytes(filePath.FileFullPath, fileData);
        }

        public static void SaveFile(byte[] fileData, string path, string fileName)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            File.WriteAllBytes(path + @"\" + fileName, fileData);
        }

        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
