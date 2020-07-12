using System;
using System.Collections;
using GMTK2020.ActionTimeline;
using UnityEngine;

namespace GMTK2020.UI.Ingame
{
    public class PlayerCircleController : MonoBehaviour
    {
        public int index;
        public float transitionDuration = .5F;
        
        private RectTransform _trs;
        private bool _isSelected;

        private Vector2 _naturalSize;
        
        private void Awake()
        {
            _trs = GetComponent<RectTransform>();
            _naturalSize = _trs.sizeDelta;
            
            _trs.sizeDelta = Vector2.zero;
        }

        private void Update()
        {
            bool selected = TimelineManager.Instance.SelectedActor == index;

            if (selected == _isSelected)
                return;

            _isSelected = selected;
            
            StopAllCoroutines();
            StartCoroutine(TransitionSize(selected ? _naturalSize : Vector2.zero));
        }

        private IEnumerator TransitionSize(Vector2 size)
        {
            Vector2 start = _trs.sizeDelta;
            float time = .0F;

            while (time <= transitionDuration)
            {
                time += Time.deltaTime;
                _trs.sizeDelta = Vector2.Lerp(start, size, time / transitionDuration);

                yield return null;
            }
        }
    }
}
