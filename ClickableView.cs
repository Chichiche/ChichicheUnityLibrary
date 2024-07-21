using System;
using BinDI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Chichiche
{
    public class ClickableView : MonoBehaviour, IPointerClickHandler, ISubscribable
    {
        readonly Broker _onClick = new();

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick.Publish();
        }

        public IDisposable Subscribe(IPublishable publishable)
        {
            return _onClick.Subscribe(publishable);
        }
    }
}