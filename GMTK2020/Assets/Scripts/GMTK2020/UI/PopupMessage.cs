using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace GMTK2020.UI
{
    public class PopupMessage : MonoBehaviour
    {
        private static PopupMessage _Instance;

        private CanvasGroup _canvas;
        private TextMeshProUGUI _text;

        private bool _running;
        private float _timer;
        
        private void Awake()
        {
            _Instance = this;

            _canvas = gameObject.AddComponent<CanvasGroup>();
            _canvas.alpha = 0F;
            
            _text = GetComponentInChildren<TextMeshProUGUI>();
            
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_running)
            {
                _timer -= Time.deltaTime;
                if (_timer < .0F)
                {
                    StopAllCoroutines();
                    StartCoroutine(Fade(0F));
                }
            }
        }

        public static void Show(string message, float timeout = 3F)
        {
            _Instance.ShowInternal(message, timeout);
        }

        private void ShowInternal(string message, float timeout)
        {
            _text.text = message;
            _timer = timeout;
            
            StopAllCoroutines();
            StartCoroutine(Fade(1F));
        }

        private IEnumerator Fade(float to)
        {
            float from = _canvas.alpha;
            float time = 0F;

            while (time <= .5F)
            {
                time += Time.deltaTime;

                _canvas.alpha = Mathf.Lerp(from, to, time / .5F);
                yield return null;
            }
            
            if(to > .1F)
                _running = true;
        }
    }
}
