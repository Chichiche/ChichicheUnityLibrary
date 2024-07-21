using System.Buffers;
using BinDI;
using Cysharp.Text;
using TMPro;
using UnityEngine;

namespace Chichiche
{
    public class CharArrayView : MonoBehaviour, IPublishable<ForTextMeshProCharArray>
    {
        [SerializeField] TMP_Text _text;

        void Reset()
        {
            _text = GetComponent<TMP_Text>();
        }

        public void Publish(ForTextMeshProCharArray value)
        {
            value.ToTMP(_text);
        }
    }

    public sealed class ForTextMeshProCharArray
    {
        readonly ArrayPool<char> _pool = ArrayPool<char>.Shared;
        char[] _chars;
        int _length;

        public ForTextMeshProCharArray(int defaultCapacity = 256)
        {
            _chars = _pool.Rent(defaultCapacity);
        }

        public void Update(Utf16ValueStringBuilder stringBuilder)
        {
            var newLength = stringBuilder.Length;
            if (_chars.Length < newLength)
            {
                _pool.Return(_chars);
                _chars = ArrayPool<char>.Shared.Rent(newLength);
            }
            stringBuilder.TryCopyTo(_chars, out _length);
        }

        public void ToTMP(TMP_Text text)
        {
            text.SetText(_chars, 0, _length);
        }
    }
}