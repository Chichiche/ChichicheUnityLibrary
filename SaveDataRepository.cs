using System;
using R3;
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

        public Observable<T> AsObservable()
        {
            return Observable.Return(_data);
        }

        public void Update<TValue>(TValue value, Action<T, TValue> update)
        {
            update(_data, value);
            _isDirty = true;
        }

        public void LateTick()
        {
            if (! _isDirty) return;
            _isDirty = false;
            _stream.Save();
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}