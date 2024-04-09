using UnityEngine;
using UnityEngine.EventSystems;

namespace Sja_UI
{
    class Sja_MainWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        private RectTransform mainRect;

        public void Awake()
        {
            // To transform the window we need to get the transform component of the type RectTransform!
            mainRect = GetComponent(typeof(RectTransform)) as RectTransform;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // When user clicks and drags. The mouse delta position is added to the transform component.
            // This makes the window follow the mouse exactly!
            mainRect.anchoredPosition += eventData.delta;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // When the user clicks the window we want it to appear in front of all other windows.
            // We therefor want it to render last. The last sibling renders last! 
            mainRect.SetAsLastSibling();
        }
    }
}
