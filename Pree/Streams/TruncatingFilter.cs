using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Pree.Streams
{
    public class TruncatingFilter : Stream
    {
        private readonly Stream _targetStream;
        private readonly int _bytesToTruncate;
        private readonly long _bytesAvailable;

        private int _skipCount;
        private long _takeCount;
        
        public TruncatingFilter(Stream targetStream, int bytesToTruncate, long bytesAvailable)
        {
            _targetStream = targetStream;
            _bytesToTruncate = bytesToTruncate;
            _bytesAvailable = bytesAvailable;

            _skipCount = _bytesToTruncate;
            _takeCount = bytesAvailable - _bytesToTruncate * 2;
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return _targetStream.CanWrite; }
        }

        public override void Flush()
        {
            _targetStream.Flush();
        }

        public override long Length
        {
            get { return _targetStream.Length; }
        }

        public override long Position
        {
            get { return _targetStream.Position; }
            set { throw new NotImplementedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int availableSkipCount = Math.Min(_skipCount, count);

            offset += availableSkipCount;
            count -= availableSkipCount;
            _skipCount -= availableSkipCount;

            int availableTakeCount = (int)Math.Min(_takeCount, count);

            count = availableTakeCount;
            _takeCount -= availableTakeCount;

            if (count > 0)
                _targetStream.Write(buffer, offset, count);
        }
    }
}
