using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GMTK2020.UI.Components
{
    public class UiModal : MonoBehaviour
    {
        public RectTransform windowAnchor;

        public TextMeshProUGUI textDisplay;
        public RectTransform scrollRect;
        public RectTransform buttonsAnchor;
        public GameObject buttonTemplate;

        public static string Result { get; private set; }
        private bool _isDone;

        private readonly List<UiButton> _buttons = new List<UiButton>();
        private int _selectedButton;

        public IEnumerator Show(string text, IEnumerable<string> buttons, int startIndex)
        {
            float height = 25F;
            float textBottom = 10F;

            if (textDisplay)
            {
                textDisplay.text = text;
                textDisplay.ForceMeshUpdate();

                height += textDisplay.preferredHeight;
                textBottom += textDisplay.preferredHeight;
            }

            if (buttonTemplate && buttonsAnchor)
            {
                foreach (string button in buttons)
                {
                    GameObject ob = Instantiate(buttonTemplate, buttonsAnchor);
                    RectTransform trs = (RectTransform) ob.transform;

                    height += trs.sizeDelta.y + 5F;

                    TextMeshProUGUI textCmp = ob.GetComponentInChildren<TextMeshProUGUI>();
                    if (textCmp)
                        textCmp.text = button;

                    UiButton btn = ob.GetComponent<UiButton>();
                    if (!btn) continue;
                    
                    btn.onClick.DynamicCalls += () => OnButton(button);
                    _buttons.Add(btn);
                    btn.OnDeselect(true);
                }

                if (startIndex >= _buttons.Count)
                    startIndex = 0;

                if (_buttons.Count > startIndex)
                {
                    _selectedButton = startIndex;
                    _buttons[startIndex].OnSelect(true);
                }
            }

            height = Mathf.Min(height, Screen.height * .85F);
            
            if (windowAnchor)
                windowAnchor.sizeDelta = new Vector2(windowAnchor.sizeDelta.x, height);
            scrollRect.anchoredPosition = new Vector2(buttonsAnchor.anchoredPosition.x, -textBottom);
            scrollRect.sizeDelta = new Vector2(scrollRect.sizeDelta.x, height - 25F);

            while (!_isDone)
                yield return null;

            Destroy(gameObject);
        }

        public void NavigateUp() => UpdateSelection(_selectedButton - 1);
        public void NavigateDown() => UpdateSelection(_selectedButton + 1);

        public void Submit()
        {
            if (_isDone || _buttons.Count == 0)
                return;

            if (!_buttons[_selectedButton]) return;
            
            _buttons[_selectedButton].OnSubmit();
            MenuAudio.Accept();
        }

        public void Cancel()
        {
            OnButton("Cancelled");
            MenuAudio.Back();
        }

        private void UpdateSelection(int newSelection)
        {
            if (_isDone || _buttons.Count == 0)
                return;

            if (newSelection < 0)
                newSelection = _buttons.Count - 1;
            if (newSelection >= _buttons.Count)
                newSelection = 0;

            if (newSelection == _selectedButton)
                return;

            if (_selectedButton < _buttons.Count)
                _buttons[_selectedButton].OnDeselect(false);

            _selectedButton = newSelection;

            if (_selectedButton < _buttons.Count)
                _buttons[_selectedButton].OnSelect(false);

            // Scroll
            RectTransform rect = (RectTransform) _buttons[_selectedButton].transform;
            float scrolledPos = rect.anchoredPosition.y + buttonsAnchor.anchoredPosition.y;

            if (scrolledPos > -rect.sizeDelta.y)
                buttonsAnchor.anchoredPosition = new Vector2(0F, -rect.anchoredPosition.y - rect.sizeDelta.y);
            else if (scrolledPos < -scrollRect.sizeDelta.y)
                buttonsAnchor.anchoredPosition = new Vector2(0F, -scrollRect.sizeDelta.y - rect.anchoredPosition.y);
        }

        public void OnButton(string button)
        {
            if (_isDone)
                return;

            Result = button;
            _isDone = true;
        }
    }
}