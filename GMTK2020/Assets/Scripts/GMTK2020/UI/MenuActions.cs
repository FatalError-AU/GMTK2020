using System;
using System.Collections;
using GMTK2020.UI.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GMTK2020.UI
{
    public class MenuActions : MonoBehaviour
    {
        public const int MAIN_MENU = 1;
        
        public static bool pauseLocked;
        public const string FIRST_SCENE = "Assets/Scenes/GameScene.unity";

        public MenuController controller;

        public void StartGame()
        {
            SceneTransition.LoadScene(FIRST_SCENE);
        }

        public void OpenMenu(MenuController.Menu menu)
        {
            if (controller)
                controller.Open(menu);
        }

        public void RestartLevel(bool bypassConfirm = false)
        {
            StartCoroutine(Confirm(() => SceneTransition.LoadScene(SceneManager.GetActiveScene().buildIndex), bypassConfirm, "restart the current level from the beginning"));
        }

        public void MainMenu(bool bypassConfirm = false)
        {
            StartCoroutine(Confirm(() => SceneTransition.LoadScene(MAIN_MENU), bypassConfirm, "return to main menu"));
        }

        public IEnumerator Confirm(Action action, bool bypass, string message)
        {
            if (!bypass && controller)
            {
                UiNavigation menu = controller.GetCurrentMenu().GetComponent<UiNavigation>();

                if (menu)
                    yield return menu.ModalWindow($"Are you sure you want to {message}?", new[] {"No", "Yes"}, null);

                if (!"Yes".Equals(UiModal.Result))
                    yield break;
            }

            action?.Invoke();
        }

        public static void SetPause(bool pause) => SetPause(pause, true);
        
        public static void SetPause(bool pause, bool doBlur)
        {
            if (pauseLocked)
                return;
            
            Time.timeScale = pause ? 0F : 1F;
            // ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER).controllers.maps.SetMapsEnabled(!pause, Controls.Category.DEFAULT);
        }

        public void ExitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}