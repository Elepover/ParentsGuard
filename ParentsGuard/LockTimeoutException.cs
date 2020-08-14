using System;

namespace ParentsGuard
{
    public class LockTimeoutException : TimeoutException
    {
        public LockTimeoutException(string message, Exception innerException) : base(message, innerException) { }
        public LockTimeoutException(string fileName)
            : base($"File is still locked by another process after timeout. Will not delete the file (skipped). Affected file is: {fileName}")
        { }
    }
}
