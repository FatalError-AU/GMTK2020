using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GMTK2020.UI
{
    public class SceneTransition : MonoBehaviour
    {
        private const string Loading = "Assets/Scenes/Utility/Loading.unity";

        private static SceneTransition _Instance;

        public float transitionTime = 1F;
        private CanvasGroup _render;

        public static bool AlreadyLoading { get; private set; } = false;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            _Instance = this;
            _render = GetComponentInChildren<CanvasGroup>();
            Debug.Assert(_render);
            _render.alpha = 0F;
        }

        public static void LoadScene(string name)
        {
            int index = SceneUtility.GetBuildIndexByScenePath(name);
            
            if(index == -1)
                throw new FileNotFoundException($"Scene {name} not found");
            
            LoadScene(SceneUtility.GetBuildIndexByScenePath(name));
        }
        
        public static void LoadScene(int index, Action sceneLoaded = null)
        {
            if (!_Instance)
            {
                Debug.LogError("No scene transition object exists");
                SceneManager.LoadScene(index);
                sceneLoaded?.Invoke();
                return;
            }

            if (AlreadyLoading)
                throw new Exception("Attempted to load a scene whilst a scene was already loading");
            
            _Instance.StartCoroutine(_Instance._LoadScene(index, sceneLoaded));
        }

        public static IEnumerator LoadSceneManual(int index, Action sceneLoaded = null)
        {
            if (!_Instance)
            {
                Debug.LogError("No scene transition object exists");
                SceneManager.LoadScene(index);
                sceneLoaded?.Invoke();
                return null;
            }

            if (AlreadyLoading)
                throw new Exception("Attempted to load a scene whilst a scene was already loading");

            return _Instance._LoadScene(index, sceneLoaded);
        }

        private IEnumerator _LoadScene(int index, Action sceneLoaded)
        {
            MenuActions.SetPause(true);
            AlreadyLoading = true;

            yield return StartCoroutine(Fade(1F));
            yield return SceneManager.LoadSceneAsync(Loading);
            yield return StartCoroutine(Fade(0F));

            AsyncOperation mainSceneLoad = SceneManager.LoadSceneAsync(index);
            mainSceneLoad.allowSceneActivation = false;
            while (mainSceneLoad.progress < 0.9F)
                yield return null;

            yield return StartCoroutine(Fade(1F));
            mainSceneLoad.allowSceneActivation = true;

            yield return mainSceneLoad;
            sceneLoaded?.Invoke();

            yield return new WaitForSecondsRealtime(.25F);
            MenuActions.SetPause(false);
            yield return StartCoroutine(Fade(0F));
            AlreadyLoading = false;
        }

        private IEnumerator Fade(float alpha)
        {
            float time = 0F;
            
            float startA = _render.alpha;

            while (time < transitionTime)
            {
                time += Time.unscaledDeltaTime;
                _render.alpha = Mathf.Lerp(startA, alpha, time / transitionTime);
                yield return null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Instantiate(Resources.Load<GameObject>("UI/Transition"));
        }
    }
}