using System.Collections;
using UnityEngine;

namespace GMTK2020.FX
{
    public class UnfoldController : MonoBehaviour
    {
        public RectTransform[] unfoldables = { };
        public float transitionDuration = .15F;
        
        private int _currentIndex = 0;

        private bool _reverse;
        
        private void Awake()
        {
            if (unfoldables == null || unfoldables.Length == 0)
                return;

            foreach(RectTransform trs in unfoldables)
                trs.gameObject.SetActive(false);
            
            StartCoroutine(Unfold());
        }

        private IEnumerator Unfold()
        {
            float start = _reverse ? 0F : -90F;
            float end = _reverse ? -90F : 0F;
            
            Vector3 rot = unfoldables[_currentIndex].localEulerAngles;
            rot.y = start;
            unfoldables[_currentIndex].localEulerAngles = rot;
            
            unfoldables[_currentIndex].gameObject.SetActive(true);

            float time = 0F;
            while (time < transitionDuration)
            {
                time += Time.deltaTime;

                rot.y = Mathf.Lerp(start, end, time / transitionDuration);
                unfoldables[_currentIndex].localEulerAngles = rot;

                yield return null;
            }

            if(_reverse)
                unfoldables[_currentIndex].gameObject.SetActive(false);

            if (_reverse)
                _currentIndex--;
            else
                _currentIndex++;
            
            if((_reverse && _currentIndex >= 0) || (!_reverse && _currentIndex < unfoldables.Length))
                StartCoroutine(Unfold());
            else if (_reverse)
                Destroy(gameObject);
        }

        public void Reverse()
        {
            if (_reverse)
                return;
            
            StopAllCoroutines();
            
            _reverse = true;

            _currentIndex = Mathf.Max(0, _currentIndex - 1);
            
            StartCoroutine(Unfold());
        }
    }
}
