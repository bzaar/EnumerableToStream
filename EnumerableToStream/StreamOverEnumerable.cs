using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EnumerableToStream
{
    internal class StreamOverEnumerable : Stream
    {
        private int _currentIndex;
        private IEnumerator<char[]>? _enumerator;
        private readonly Encoder _encoder;

        public StreamOverEnumerable(IEnumerable<string?> input, Encoder encoder)
        {
            _encoder = encoder;
            _enumerator = input
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => s!.ToCharArray())
                .GetEnumerator();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_enumerator == null)
            {
                throw new ObjectDisposedException($"The {nameof(StreamOverEnumerable)} has been disposed.");
            }

            int totalBytesRead = 0;
            bool spaceInBuffer = true;

            while (spaceInBuffer && totalBytesRead < count && (_currentIndex != 0 || _enumerator.MoveNext()))
            {
                _encoder.Convert(CurrentBytes, _currentIndex, CurrentBytes.Length - _currentIndex, 
                    buffer, offset + totalBytesRead, count - totalBytesRead,
                    false, out int charsUsed, out int bytesUsed, out spaceInBuffer);

                totalBytesRead += bytesUsed;
                _currentIndex += charsUsed;

                if (_currentIndex == CurrentBytes.Length) _currentIndex = 0;
            }

            return totalBytesRead;
        }

        char[] CurrentBytes => _enumerator!.Current!;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _enumerator?.Dispose();
                _enumerator = null;
            }
            base.Dispose(disposing);
        }
        
        public override void Flush()
            => throw new NotImplementedException();

        public override long Seek(long offset, SeekOrigin origin)
            => throw new NotImplementedException();

        public override void SetLength(long value)
            => throw new NotImplementedException();

        public override void Write(byte[] buffer, int offset, int count) 
            => throw new NotImplementedException();

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotImplementedException();
        public override long Position
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}
