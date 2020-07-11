using UnityEngine;

namespace GMTK2020.UI.Components
{
    public abstract class UiSelectable : UiComponent
    {
        [ColorUsage(false)]
        public Color lockedMultiply = Color.red;

        private bool _locked;

        protected override void UpdateState(bool instant)
        {
            HighlightMultiply = _locked ? lockedMultiply : Color.white;

            base.UpdateState(instant);
        }

        public override void OnSubmit()
        {
            base.OnSelect();

            _locked = !_locked;
            if(_locked)
                MenuAudio.Back();
            else
                MenuAudio.Accept();
            
            UpdateState_Pre();
        }

        public override bool OnCancel()
        {
            if (_locked)
            {
                _locked = false;
                MenuAudio.Back();
                
                UpdateState_Pre();
                return true;
            }

            return base.OnCancel();
        }

        protected override void OnDeselect()
        {
            base.OnDeselect();

            _locked = false;
        }

        public override bool NavigateLeft() => _locked ? DirectionPressed(Direction.Left) : _locked;
        public override bool NavigateRight() => _locked ? DirectionPressed(Direction.Right) : _locked;
        public override bool NavigateUp() => _locked ? DirectionPressed(Direction.Up) : _locked;
        public override bool NavigateDown() => _locked ? DirectionPressed(Direction.Down) : _locked;

        public abstract bool DirectionPressed(Direction dir);

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
    }
}