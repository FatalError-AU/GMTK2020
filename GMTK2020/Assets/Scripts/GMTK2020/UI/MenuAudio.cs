using UnityEngine;

namespace GMTK2020.UI
{
    public class MenuAudio : MonoBehaviour
    {
        private static MenuAudio instance;

        [Header("Audio")]
        public AudioSource accept;
        public AudioSource back;
        public AudioSource scroll;

        private void Awake()
        {
            if (instance)
                return;

            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (accept)
                accept.ignoreListenerPause = true;
            if (back)
                back.ignoreListenerPause = true;
            if (scroll)
                scroll.ignoreListenerPause = true;
        }

        public static void Accept()
            => Play(instance ? instance.accept : null);

        public static void Back()
            => Play(instance ? instance.back : null);   
        
        public static void Scroll()
            => Play(instance ? instance.scroll : null);

        private static void Play(AudioSource source)
        {
            if (!source) return;
            source.time = 0F;
            source.Play();
        }
    }
}