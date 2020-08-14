using System.IO;
using System.Threading;

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

        /// <summary>
        /// Waits for a file to be released.
        /// </summary>
        /// <param name="fileName">Target file's full path.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="LockTimeoutException"/>
        public static void WaitFileRelease(string fileName, CancellationToken cancellationToken = default)
        {
            while (IsLocked(fileName))
            {
                if (cancellationToken.IsCancellationRequested)
                    throw new LockTimeoutException(fileName);
                Thread.Sleep(200);
            }
        }
    }
}
