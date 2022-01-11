using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PettingZoo.Core.ExportImport
{
    public class StreamProgressDecorator : BaseProgressDecorator
    {
        private readonly StreamWrapper streamWrapper;

        public Stream Stream => streamWrapper;


        /// <summary>
        /// Wraps a Stream and provides reports to the IProgress.
        /// </summary>
        /// <remarks>
        /// Use the Stream property to pass along to the method you want to monitor the progress on.
        /// If the consumer seeks around in the stream a lot the progress will not be linear, but that
        /// seems to be a trend anyways with progress bars, so enjoy your modern experience!
        /// </remarks>
        /// <param name="decoratedStream">The stream to decorate.</param>
        /// <param name="progress">Receives progress reports. The value will be a percentage (0 - 100).</param>
        /// <param name="reportInterval">The minimum time between reports.</param>
        public StreamProgressDecorator(Stream decoratedStream, IProgress<int> progress, TimeSpan? reportInterval = null)
            : base(progress, reportInterval)
        {
            streamWrapper = new StreamWrapper(this, decoratedStream);
        }


        protected override int GetProgress()
        {
            return streamWrapper.DecoratedStream.Length > 0
                ? (int)Math.Truncate((double)streamWrapper.DecoratedStream.Position / streamWrapper.DecoratedStream.Length * 100)
                : 0;
        }


        private class StreamWrapper : Stream
        {
            private readonly StreamProgressDecorator owner;
            public readonly Stream DecoratedStream;

            public override bool CanRead => DecoratedStream.CanRead;
            public override bool CanSeek => DecoratedStream.CanSeek;
            public override bool CanWrite => DecoratedStream.CanWrite;
            public override long Length => DecoratedStream.Length;

            public override long Position
            {
                get => DecoratedStream.Position;
                set => DecoratedStream.Position = value;
            }


            public StreamWrapper(StreamProgressDecorator owner, Stream decoratedStream)
            {
                this.owner = owner;
                this.DecoratedStream = decoratedStream;
            }


            public override void Flush()
            {
                DecoratedStream.Flush();
            }


            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                return DecoratedStream.FlushAsync(cancellationToken);
            }


            public override int Read(byte[] buffer, int offset, int count)
            {
                var result = DecoratedStream.Read(buffer, offset, count);
                owner.UpdateProgress();
                return result;
            }

            public override int Read(Span<byte> buffer)
            {
                var result = DecoratedStream.Read(buffer);
                owner.UpdateProgress();
                return result;
            }


            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                #pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
                var result = await DecoratedStream.ReadAsync(buffer, offset, count, cancellationToken);
                #pragma warning restore CA1835
                owner.UpdateProgress();
                return result;
            }


            public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new())
            {
                var result = DecoratedStream.ReadAsync(buffer, cancellationToken);
                owner.UpdateProgress();
                return result;
            }


            public override long Seek(long offset, SeekOrigin origin)
            {
                var result = DecoratedStream.Seek(offset, origin);
                owner.UpdateProgress();
                return result;
            }


            public override void SetLength(long value)
            {
                DecoratedStream.SetLength(value);
                owner.UpdateProgress();
            }


            public override void Write(byte[] buffer, int offset, int count)
            {
                DecoratedStream.Write(buffer, offset, count);
                owner.UpdateProgress();
            }


            public override void Write(ReadOnlySpan<byte> buffer)
            {
                DecoratedStream.Write(buffer);
                owner.UpdateProgress();
            }


            public override async Task WriteAsync(byte[] buffer, int offset, int count,
                CancellationToken cancellationToken)
            {
                #pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
                await DecoratedStream.WriteAsync(buffer, offset, count, cancellationToken);
                #pragma warning restore CA1835
                owner.UpdateProgress();
            }


            public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer,
                CancellationToken cancellationToken = new())
            {
                await DecoratedStream.WriteAsync(buffer, cancellationToken);
                owner.UpdateProgress();
            }
        }
    }
}
