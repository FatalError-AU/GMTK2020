using UltEvents;

namespace GMTK2020.UI.Components
{
    public class UiButton : UiComponent
    {
        public UltEvent onClick;

        public override void OnSubmit()
        {
            base.OnSubmit();
            
            onClick.InvokeX();
            
            MenuAudio.Accept();
        }
    }
}