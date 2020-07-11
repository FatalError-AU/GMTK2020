using System;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;

namespace GMTK2020.UI
{
    public class MenuController : MonoBehaviour
    {
        public static MenuController Instance { get; private set; }

        public GameObject menuMain;
        public GameObject menuPause;
        public GameObject menuSettings;
        public GameObject menuFail;
        public GameObject menuSuccess;

        public Menu startingMenu;
        public bool canClose = true;

        public UltEvent onMenuClose;

        private readonly Stack<Menu> _stateStack = new Stack<Menu>();
        private Menu _cachedMenu = Menu.None;

        private void Awake()
        {
            Instance = this;

            foreach (Menu state in Enum.GetValues(typeof(Menu)))
            {
                GameObject menu = GetStateMenu(state);

                if (menu)
                    menu.SetActive(false);
            }
        }

        private void Start()
        {
            Open(startingMenu);
        }

        private void Update()
        {
            Menu current = Menu.None;
            if (_stateStack.Count > 0)
                current = _stateStack.Peek();

            if (current == _cachedMenu) return;
            
            foreach (Menu state in Enum.GetValues(typeof(Menu)))
            {
                GameObject menu = GetStateMenu(state);

                if (!menu) continue;
                
                menu.SetActive(state == current);
                foreach (IMenuCallback callback in menu.GetComponentsInChildren<IMenuCallback>(true))
                    callback.OnMenuChanged(state == current);

                UiNavigation nav = menu.GetComponent<UiNavigation>();
                if(nav && nav.isActiveAndEnabled)
                    nav.OnMenuChanged(state == current);
            }

            _cachedMenu = current;
        }

        public void Open(Menu menu)
        {
            if (menu == Menu.None)
                return;
            _stateStack.Push(menu);
        }

        public void Close()
        {
            if (_stateStack.Count > 0 && (_stateStack.Count > 1 || canClose))
                _stateStack.Pop();

            if (_stateStack.Count != 0) return;
            
            Destroy(gameObject);
            onMenuClose?.InvokeX();
        }

        public void CloseAll()
        {
            while (_stateStack.Count > 0 && (_stateStack.Count > 1 || canClose))
                Close();
        }

        private GameObject GetStateMenu(Menu menu)
        {
            switch (menu)
            {
                case Menu.None:
                    return null;
                case Menu.MainMenu:
                    return menuMain;
                case Menu.PauseMenu:
                    return menuPause;
                case Menu.Settings:
                    return menuSettings;
                case Menu.Fail:
                    return menuFail;
                case Menu.Success:
                    return menuSuccess;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
            }
        }

        public GameObject GetCurrentMenu()
            => _stateStack.Count == 0 ? null : GetStateMenu(_stateStack.Peek());

        [Serializable]
        public enum Menu
        {
            None,
            MainMenu,
            PauseMenu,
            Settings,
            Fail,
            Success,
        }

        public interface IMenuCallback
        {
            void OnMenuChanged(bool activated);
        }
    }
}