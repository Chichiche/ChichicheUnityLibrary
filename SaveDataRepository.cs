using System;
using R3;
using VContainer.Unity;

namespace Chichiche
{
    public abstract class SaveDataRepository<T> : Observable<T>, ILateTickable, IDisposable where T : new()
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

        protected override IDisposable SubscribeCore(Observer<T> observer)
        {
            observer.OnNext(_data);
            return Disposable.Empty;
        }

        public void Update<TValue>(TValue value, Action<T, TValue> update)
        {
            update(_data, value);
            _isDirty = true;
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