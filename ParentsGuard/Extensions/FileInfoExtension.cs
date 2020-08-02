using System.IO;

namespace ParentsGuard.Extensions
{
    public static class FileInfoExtension
    {
        public static byte[] ReadToEnd(this FileInfo fileInfo)
        {
            using (var fs = fileInfo.OpenRead())
            {
                using (var ms = new MemoryStream())
                {
                    fs.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
