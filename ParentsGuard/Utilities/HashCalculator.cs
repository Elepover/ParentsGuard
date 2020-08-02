using ParentsGuard.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ParentsGuard.Utilities
{
    public class HashCalculator
    {
        public static string GetHash<TAlgorithm>(byte[] data, bool upperCase = false)
            where TAlgorithm : HashAlgorithm, new()
        {
            using (var algorithm = new TAlgorithm())
            {
                var hash = algorithm.ComputeHash(data);
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString(upperCase ? "X2" : "x2"));
                }

                return sb.ToString();
            }
        }

        public static string GetHash<TAlgorithm>(FileInfo file, bool upperCase = false)
            where TAlgorithm : HashAlgorithm, new()
            => GetHash<TAlgorithm>(file.ReadToEnd(), upperCase);

        public static string GetHash<TAlgorithm>(string str, Encoding encoding, bool upperCase = false)
            where TAlgorithm : HashAlgorithm, new()
            => GetHash<TAlgorithm>(encoding.GetBytes(str), upperCase);

        public static string GetHash<TAlgorithm>(string str, bool upperCase = false)
            where TAlgorithm : HashAlgorithm, new()
            => GetHash<TAlgorithm>(str, Encoding.UTF8, upperCase);

        public static string GetSha1Hash(FileInfo file, bool upperCase = false)
            => GetHash<SHA1Managed>(file, upperCase);

        public static string GetSha256Hash(FileInfo file, bool upperCase = false)
            => GetHash<SHA256Managed>(file, upperCase);

        public static string GetSha384Hash(FileInfo file, bool upperCase = false)
            => GetHash<SHA384Managed>(file, upperCase);

        public static string GetSha512Hash(FileInfo file, bool upperCase = false)
            => GetHash<SHA512Managed>(file, upperCase);
    }
}
