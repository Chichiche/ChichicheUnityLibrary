using System;
using UnityEngine.AddressableAssets;

namespace Chichiche.AddressableUtilities
{
    public interface IAddressableReference
    {
        IDisposable Load();
    }

    [Serializable]
    public class AddressableReference<TObject> : AssetReferenceT<TObject>, IAddressableReference
        where TObject : UnityEngine.Object
    {
        int _loadCount;

        protected AddressableReference(string guid) : base(guid) { }

        public new TObject Asset => base.Asset as TObject;

        public IDisposable Load()
        {
            if (_loadCount == 0) LoadAssetAsync().WaitForCompletion();
            _loadCount++;
            return new UnloadDisposable(this);
        }

        public void ForceUnload()
        {
            _loadCount = 0;
            if (! IsValid()) return;
            ReleaseAsset();
        }

        class UnloadDisposable : IDisposable
        {
            readonly AddressableReference<TObject> _reference;
            bool _disposed;

            public UnloadDisposable(AddressableReference<TObject> reference)
            {
                _reference = reference;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _reference._loadCount--;
                if (_reference._loadCount > 0) return;
                _reference.ReleaseAsset();
            }
        }
    }
}