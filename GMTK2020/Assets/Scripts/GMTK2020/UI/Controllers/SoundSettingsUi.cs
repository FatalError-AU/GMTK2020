using GMTK2020.Data.Settings;
using GMTK2020.UI.Components;
using UnityEngine;

namespace GMTK2020.UI.Controllers
{
    public class SoundSettingsUi : MonoBehaviour
    {
        public UiSlider masterSlider;
        public UiSlider musicSlider;
        public UiSlider sfxSlider;

        private void Awake()
        {
            SoundSettings settings = SettingsData.Get<SoundSettings>();

            if (masterSlider)
            {
                masterSlider.SetInstant(settings.volume[SoundSettings.Channel.Master]);
                masterSlider.onValueChange.AddListener(ob => settings.volume[SoundSettings.Channel.Master] = Mathf.RoundToInt(masterSlider.Value));
            }

            if (musicSlider)
            {
                musicSlider.SetInstant(settings.volume[SoundSettings.Channel.Music]);
                musicSlider.onValueChange.AddListener(ob => settings.volume[SoundSettings.Channel.Music] = Mathf.RoundToInt(musicSlider.Value));
            }

            if (sfxSlider)
            {
                sfxSlider.SetInstant(settings.volume[SoundSettings.Channel.Sfx]);
                sfxSlider.onValueChange.AddListener(ob => settings.volume[SoundSettings.Channel.Sfx] = Mathf.RoundToInt(sfxSlider.Value));
            }
        }
    }
}