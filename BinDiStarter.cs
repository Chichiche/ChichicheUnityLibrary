using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace BinDI
{
    public sealed class BinDiStarter : MonoBehaviour
    {
        [SerializeField] BinDiWindowAssemblyWhiteList _whiteList;
        [SerializeField] GameObject[] _entryScopes;
        [SerializeField] bool _collectAssemblyLogEnabled;
        [SerializeField] bool _domainRegistrationLogEnabled;
        [SerializeField] bool _pubSubConnectionLogEnabled;
        IObjectResolver _rootScope;

        void Start()
        {
            _rootScope = CreateRootScope();
            BuildEntryScopes(_rootScope);
        }

        void OnDestroy()
        {
            _rootScope?.Dispose();
        }

        IObjectResolver CreateRootScope()
        {
            var options = new BinDiOptions
            {
                CollectAssemblyLogEnabled = _collectAssemblyLogEnabled,
                DomainRegistrationLogEnabled = _domainRegistrationLogEnabled,
                PubSubConnectionLogEnabled = _pubSubConnectionLogEnabled,
            };

            var filter = new AssemblyWhiteListFilter(_whiteList.WhiteList);

            return new ContainerBuilder().RegisterBinDi(options, filter).Build();
        }

        void BuildEntryScopes(IObjectResolver rootScope)
        {
            foreach (var entryScope in _entryScopes)
            {
                rootScope.InjectGameObject(entryScope);
                rootScope.BuildGameObjectScope(entryScope);
            }
        }
    }
}