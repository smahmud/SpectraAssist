using System;
using System.Drawing;
using System.Security.Cryptography;

namespace CortexView
{

    internal enum ChangeDecision
    {
        NoChange,
        MinorChangeBelowThreshold,
        SignificantChange
    }

    internal class ChangeDetection
    {
        private byte[]? _lastImageHash;
        private byte[]? _lastDownsampled;
        private int _downsampledWidth = 64;
        private int _downsampledHeight = 36;

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

        public double ComputeChangedFraction(Bitmap currentBitmap)
        {
            if (currentBitmap == null) throw new ArgumentNullException(nameof(currentBitmap));

            byte[] currentDownsampled = DownsampleToGrayscale(currentBitmap, _downsampledWidth, _downsampledHeight);

            if (_lastDownsampled == null)
            {
                _lastDownsampled = currentDownsampled;
                return 1.0; // treat first frame as 100% changed
            }

            int changed = 0;
            int total = currentDownsampled.Length;

            const int noiseThreshold = 10; // brightness difference 0–255

            for (int i = 0; i < total; i++)
            {
                int diff = Math.Abs(currentDownsampled[i] - _lastDownsampled[i]);
                if (diff > noiseThreshold)
                    changed++;
            }

            _lastDownsampled = currentDownsampled;

            return total == 0 ? 0.0 : (double)changed / total;
        }

        private static byte[] DownsampleToGrayscale(Bitmap source, int targetWidth, int targetHeight)
        {
            using var resized = new Bitmap(targetWidth, targetHeight);
            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
                g.DrawImage(source, 0, 0, targetWidth, targetHeight);
            }

            var pixels = new byte[targetWidth * targetHeight];
            int idx = 0;

            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    var c = resized.GetPixel(x, y);
                    byte gray = (byte)((c.R * 0.3) + (c.G * 0.59) + (c.B * 0.11));
                    pixels[idx++] = gray;
                }
            }

            return pixels;
        }

        public ChangeDecision DecideChange(double changedFraction, double sensitivityThresholdFraction)
        {
            // First capture or no previous downsampled data already yields changedFraction = 1.0
            if (changedFraction <= 0)
                return ChangeDecision.NoChange;

            if (changedFraction < sensitivityThresholdFraction)
                return ChangeDecision.MinorChangeBelowThreshold;

            return ChangeDecision.SignificantChange;
        }

        public string? TryExtractOcrText(Bitmap bitmap)
        {
            // Milestone 3 placeholder: return null or a simple marker.
            // Later this will call a real OCR engine.
            return null;
        }
    }
}
