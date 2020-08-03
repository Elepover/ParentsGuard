using System.IO;

namespace ParentsGuard.Utilities
{
    public class FileHelper
    {
        public static bool IsLocked(string fileName)
        {
            try
            {
                using (var stream = File.Open(fileName, FileMode.Open, FileAccess.ReadWrite)) { }
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }
    }
}
