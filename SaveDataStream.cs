using System;
using System.IO;
using MessagePack;
using UnityEngine.Assertions;

namespace Chichiche
{
    public sealed class SaveDataStream<T> : IDisposable where T : new()
    {
        readonly string _filePath;
        readonly FileStream _stream;
        T _data = new();
        bool _disposed;

        public SaveDataStream(string filePath)
        {
            _filePath = filePath;
            var directoryPath = Path.GetDirectoryName(filePath);
            Assert.IsNotNull(directoryPath);
            if (! Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);
            _stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }

        public T Load()
        {
            if (new FileInfo(_filePath).Length == 0) return _data;
            _stream.Position = 0;
            _data = MessagePackSerializer.Deserialize<T>(_stream);
            return _data;
        }

        public void Save()
        {
            _stream.Position = 0;
            MessagePackSerializer.Serialize(_stream, _data);
            _stream.SetLength(_stream.Position);
        }

        public void Delete()
        {
            Dispose();
            File.Delete(_filePath);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _stream?.Dispose();
            GC.SuppressFinalize(this);
        }

        ~SaveDataStream() => Dispose();
    }
}