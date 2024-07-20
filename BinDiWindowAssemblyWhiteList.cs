using System.Collections.Generic;
using UnityEngine;

namespace BinDI
{
    [CreateAssetMenu]
    public sealed class BinDiWindowAssemblyWhiteList : ScriptableObject, IBinDiWindowAssemblyWhiteList
    {
        [SerializeField] string[] _whiteList = { "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" };

        public IEnumerable<string> WhiteList => _whiteList;
    }
}