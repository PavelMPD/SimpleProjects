using System;
using System.Globalization;

namespace DebtCollection.Common.FilesHelper
{
    public class FilePath
    {
        private readonly string fileServer;
        private readonly long? entityId;
        private readonly string fileName;
        private readonly string fileLogicType;

        public FilePath(string fileServer, long? entityId, string fileName, string fileLogicType)
        {
            this.fileServer = fileServer;
            this.entityId = entityId;
            this.fileName = fileName;
            this.fileLogicType = fileLogicType;
        }

        public string FileFullPath
        {
            get
            {
                return String.Format(@"{0}\{1}\{2}{3}", fileServer, fileLogicType, entityId.HasValue ? (entityId.Value.ToString(CultureInfo.InvariantCulture) + "\\") : "", fileName);
            }
        }

        public string FileRelativePath
        {
            get
            {
                return String.Format(@"\{0}\{1}\{2}", fileLogicType, entityId.HasValue ? (entityId.Value.ToString(CultureInfo.InvariantCulture) + "\\") : "", fileName);
            }
        }

        public string PathOnly
        {
            get { return String.Format(@"{0}\{1}\{2}", fileServer, fileLogicType, entityId.HasValue ? (entityId.Value.ToString(CultureInfo.InvariantCulture) + "\\") : ""); }
        }

        public string ShortFileName
        {
            get { return fileName; }
        }
    }
}
