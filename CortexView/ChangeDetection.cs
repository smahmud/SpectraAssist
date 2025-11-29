using System;
using System.Drawing;
using System.Security.Cryptography;

namespace CortexView
{
    internal class ChangeDetection
    {
        private byte[]? _lastImageHash;

        public bool HasMeaningfulChange(Bitmap currentBitmap)
        {
            if (currentBitmap == null) throw new ArgumentNullException(nameof(currentBitmap));

            byte[] currentHash = ComputeImageHash(currentBitmap);

            bool isFirstCapture = _lastImageHash == null;
            bool isDifferent = !AreHashesEqual(_lastImageHash, currentHash);

            _lastImageHash = currentHash;

            // For now, “meaningful change” == “hash is different” (Milestone 2 behavior).
            return isFirstCapture || isDifferent;
        }

        private static byte[] ComputeImageHash(Bitmap bitmap)
        {
            using var ms = new System.IO.MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            ms.Position = 0;

            using var sha1 = SHA1.Create();
            return sha1.ComputeHash(ms);
        }

        private static bool AreHashesEqual(byte[]? a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            return true;
        }
    }
}
