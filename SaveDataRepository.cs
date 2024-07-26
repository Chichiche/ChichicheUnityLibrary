using System;
using BinDI;
using VContainer.Unity;

namespace Chichiche
{
    public abstract class SaveDataRepository<T> : ILateTickable, IDisposable where T : new()
    {
        readonly SaveDataStream<T> _stream;
        readonly T _data;
        bool _isDirty;

        protected SaveDataRepository()
        {
            var saveDataPath = PathUtil.GetSaveDataFilePath(typeof( T ).Name);
            _stream = new SaveDataStream<T>(saveDataPath);
            _data = _stream.Load();
        }

        public void LoadTo(Action<T> publish)
        {
            publish(_data);
        }

        public void SaveFrom<TValue>(ISubscribable<TValue> subscribable, Action<TValue, T> update)
        {
            subscribable.Subscribe(value =>
            {
                update(value, _data);
                _isDirty = true;
            });
        }

        void ILateTickable.LateTick()
        {
            if (! _isDirty) return;
            _isDirty = false;
            _stream.Save();
        }

        void IDisposable.Dispose()
        {
            _stream?.Dispose();
        }
    }
}