using System.Collections;
using GMTK2020.ActionTimeline;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GMTK2020.UI.Ingame
{
    public class ArchetypeUi : MonoBehaviour
    {
        [Header("ID")]
        public int playerIndex;

        [Header("References")]
        public RectTransform popout;
        public Image selectionHighlight;
        public Image healthSlider;
        public TextMeshProUGUI healthPercentage;

        [Header("Properties")]
        public float transitionDuration = .5F;
        public Color normalColor;
        public Color highlightColor;

        private bool _cachedIsSelected;
        private Vector2 _naturalSize;

        private void Awake()
        {
            if (popout)
                _naturalSize = popout.sizeDelta;

            if (selectionHighlight)
                selectionHighlight.CrossFadeColor(normalColor, 0F, false, true, true);
        }

        private void Update()
        {
            bool selected = TimelineManager.Instance.SelectedActor == playerIndex;
            
            if(selected == _cachedIsSelected)
                return;

            _cachedIsSelected = selected;
            
            StopAllCoroutines();
            StartCoroutine(ResizeTo(_naturalSize * (selected ? 1.15F : 1F)));
            if(selectionHighlight)
                selectionHighlight.CrossFadeColor(selected ? highlightColor : normalColor, transitionDuration, false, true, true);
        }

        private IEnumerator ResizeTo(Vector2 size)
        {
            if (!popout)
                yield break;

            Vector2 from = popout.sizeDelta;
            
            float time = 0F;

            while (time <= transitionDuration)
            {
                time += Time.deltaTime;
                popout.sizeDelta = Vector2.Lerp(from, size, time / transitionDuration);

                yield return null;
            }
        }
    }
}
