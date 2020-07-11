using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GMTK2020.UI.Components;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Action = GMTK2020.Controls.Action;

namespace GMTK2020.UI
{
    public class UiNavigation : UIBehaviour
    {
        public NavigationShouldCloseEvent closeAction;
        public UiModal modalTemplate;

        [Header("Tabs (optional)")]
        public GameObject[] tabs;
        public string[] tabNames;

        public RectTransform tabView;
        public GameObject tabTemplate;

        private Rewired.Player _controller;

        private int _currentTab;
        private Toggle[] _toggles;

        private UiComponent[] _components;
        private int[] _currentComponent;

        private UiComponent CurrentComponent
        {
            get
            {
                if (_components.Length == 0)
                    return null;

                List<UiComponent> active = ActiveComponents;

                if (_currentComponent[_currentTab] < 0 || _currentComponent[_currentTab] >= active.Count)
                    SetComponent(0);

                return active[_currentComponent[_currentTab]];
            }
        }

        private List<UiComponent> ActiveComponents => _components.Where(x => x && x.IsActive()).ToList();

        private UiModal modal;
        public bool IsModal => modal;

        protected override void Start()
        {
            base.Start();

            _controller = ReInput.players.GetPlayer(Controls.Player.MAINPLAYER);

            if (tabs == null || tabs.Length == 0)
                tabs = new[] {gameObject};

            _toggles = new Toggle[tabs.Length];
            _currentComponent = new int[tabs.Length > 0 ? tabs.Length : 1];

            if (tabView)
            {
                for (int i = 0; i < tabs.Length; i++)
                {
                    string name = tabs[i].name;
                    if (tabNames != null && i < tabNames.Length)
                        name = tabNames[i];

                    RectTransform tab = Instantiate(tabTemplate, tabView).GetComponent<RectTransform>();

                    TextMeshProUGUI text = tab.GetComponentInChildren<TextMeshProUGUI>();
                    if (text)
                    {
                        text.text = name;
                        text.ForceMeshUpdate();

                        tab.sizeDelta = new Vector2(text.preferredWidth + 40F, tab.sizeDelta.y);
                    }

                    _toggles[i] = tab.GetComponent<Toggle>();
                }
            }

            SetTab(0, false);
        }

        private void Update()
        {
            if (modal)
            {
                if (_controller.GetButtonDown(Action.UIVERTICAL))
                    modal.NavigateUp();
                else if (_controller.GetNegativeButtonDown(Action.UIVERTICAL))
                    modal.NavigateDown();

                if (_controller.GetButtonDown(Action.UISUBMIT))
                    modal.Submit();
                else if (_controller.GetButtonDown(Action.UICANCEL))
                    modal.Cancel();

                return;
            }

            int tab = _currentTab;

            if (_controller.GetButtonDown(Action.UIHORIZONTAL) && (!CurrentComponent || !CurrentComponent.NavigateRight()))
                tab++;
            else if (_controller.GetNegativeButtonDown(Action.UIHORIZONTAL) && (!CurrentComponent || !CurrentComponent.NavigateLeft()))
                tab--;

            if (tab != _currentTab)
            {
                SetTab(tab, true);
                return;
            }

            int component = _currentComponent[_currentTab];

            if (_controller.GetButtonDown(Action.UIVERTICAL) && (!CurrentComponent || !CurrentComponent.NavigateDown()))
                component--;
            else if (_controller.GetNegativeButtonDown(Action.UIVERTICAL) && (!CurrentComponent || !CurrentComponent.NavigateUp()))
                component++;

            if (component != _currentComponent[_currentTab])
            {
                SetComponent(component);
                return;
            }

            if (_controller.GetButtonDown(Action.UISUBMIT))
            {
                if (CurrentComponent)
                    CurrentComponent.OnSubmit();
            }

            if (_controller.GetButtonDown(Action.UICANCEL) && (!CurrentComponent || !CurrentComponent.OnCancel()))
            {
                if (closeAction != null)
                {
                    closeAction.Invoke();
                    MenuAudio.Back();
                }
            }
        }

        public void RefreshComponents(bool instantEffect = false)
        {
            if (tabs == null || tabs.Length == 0)
                return;

            _components = tabs[_currentTab].GetComponentsInChildren<UiComponent>(true);
            List<UiComponent> active = ActiveComponents;

            foreach (UiComponent component in active)
                component.OnDeselect(instantEffect);
            if (active.Count > 0)
                active[_currentComponent[_currentTab]].OnSelect(instantEffect);
        }

        public void OpenModal(string text, IEnumerable<string> buttons, Action<string> callback = null, int startIndex = 0)
            => StartCoroutine(ModalWindow(text, buttons, callback, startIndex));

        public IEnumerator ModalWindow(string text, IEnumerable<string> buttons, Action<string> callback, int startIndex = 0)
        {
            if (!modalTemplate)
            {
                Debug.LogError("No modal window template assigned");
                yield break;
            }

            modal = Instantiate(modalTemplate, transform);
            yield return modal.Show(text, buttons, startIndex);
            modal = null;

            if (callback != null)
                callback(UiModal.Result);
        }

        private void SetTab(int id, bool audio)
        {
            if (id < 0)
                id = tabs.Length - 1;
            else if (id >= tabs.Length)
                id = 0;

            if(audio && id != _currentTab)
                MenuAudio.Scroll();
            
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].SetActive(id == i);
                if (_toggles[i])
                    _toggles[i].isOn = id == i;
            }

            _currentTab = id;

            RefreshComponents(true);
        }

        private void SetComponent(int id, bool noTransition = false)
        {
            List<UiComponent> active = ActiveComponents;

            if (active.Count <= 0)
                return;

            if (id < 0)
                id = active.Count - 1;
            if (id >= active.Count)
                id = 0;

            if(!noTransition && id != _currentComponent[_currentTab])
                MenuAudio.Scroll();
            
            if (active[_currentComponent[_currentTab]])
                active[_currentComponent[_currentTab]].OnDeselect(noTransition);
            _currentComponent[_currentTab] = id;
            if (active[_currentComponent[_currentTab]])
                active[_currentComponent[_currentTab]].OnSelect(noTransition);
        }

        [Serializable]
        public class NavigationShouldCloseEvent : UnityEvent
        {
        }

        public void OnMenuChanged(bool activated)
        {
            if(_currentComponent != null)
                SetComponent(_currentComponent[_currentTab], true);
        }
    }
}