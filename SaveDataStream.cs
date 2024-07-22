using System;
using System.IO;
using MessagePack;
using UnityEngine.Assertions;

namespace Chichiche
{
    public interface ISaveDataStream<out T> : IDisposable
    {
        T Load();
        void Save();
        void Delete();
    }

    public sealed class VariableLengthSaveDataStream<T> : ISaveDataStream<T> where T : new()
    {
        readonly string _filePath;
        readonly FileStream _stream;
        T _data = new();
        bool _disposed;

        public VariableLengthSaveDataStream(string filePath)
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
            var binary = MessagePackSerializer.Serialize(_data);
            _stream.Position = 0;
            _stream.Write(binary);
            _stream.SetLength(binary.Length);
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

        ~VariableLengthSaveDataStream() => Dispose();
    }

    public sealed class FixedLengthSaveDataStream<T> : ISaveDataStream<T> where T : new()
    {
        readonly string _filePath;
        readonly FileStream _stream;
        T _data = new();
        bool _disposed;

        public FixedLengthSaveDataStream(string filePath)
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

        ~FixedLengthSaveDataStream() => Dispose();
    }
}