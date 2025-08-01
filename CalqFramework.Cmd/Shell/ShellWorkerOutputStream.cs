﻿namespace CalqFramework.Cmd.Shell {

    // INFO throwing with only ErrorCode is OK, stderr might contain diagnostics/info instead of error errorMessage so don't throw just because not empty
    public abstract class ShellWorkerOutputStream : Stream {
        protected readonly record struct Error(long? ErrorCode, Exception? Exception);

        public override bool CanRead => InnerStream.CanRead;

        public override bool CanSeek => InnerStream.CanSeek;

        public override bool CanWrite => InnerStream.CanWrite;

        public override long Length => InnerStream.Length;

        public override long Position { get => InnerStream.Position; set => InnerStream.Position = value; }

        protected abstract Stream InnerStream { get; }

        public override void Flush() {
            InnerStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int bytesRead;
            try {
                bytesRead = TryRead(buffer, offset, count);
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                throw new ShellWorkerException(null, $"Error code: {null}", ex);
            }

            if (bytesRead == 0) {
                Error error = GetError();

                if (error.ErrorCode != 0) {
                    throw new ShellWorkerException(error.ErrorCode, $"Error code: {error.ErrorCode}", error.Exception);
                }
            }

            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            int bytesRead;
            try {
                bytesRead = await TryReadAsync(buffer, offset, count, cancellationToken);
            } catch (OperationCanceledException) {
                throw;
            } catch (Exception ex) {
                throw new ShellWorkerException(null, $"Error code: {null}", ex);
            }

            if (bytesRead == 0) {
                Error error = await GetErrorAsync();

                if (error.ErrorCode != 0) {
                    throw new ShellWorkerException(error.ErrorCode, $"Error code: {error.ErrorCode}", error.Exception);
                }
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            return InnerStream.Seek(offset, origin);
        }

        public override void SetLength(long value) {
            InnerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count) {
            InnerStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                InnerStream.Dispose();
            }
            base.Dispose(disposing);
        }

        protected abstract Error GetError();

        protected abstract Task<Error> GetErrorAsync();

        protected abstract int TryRead(byte[] buffer, int offset, int count);

        protected abstract Task<int> TryReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    }
}
