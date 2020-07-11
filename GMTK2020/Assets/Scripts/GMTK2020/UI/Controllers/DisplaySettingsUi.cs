using System.Collections.Generic;
using System.Linq;
using GMTK2020.Attributes;
using GMTK2020.Data.Settings;
using GMTK2020.UI.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace GMTK2020.UI.Controllers
{
    public class DisplaySettingsUi : MonoBehaviour
    {
        public UiSlider brightnessSlider;
        public UiSlider gammaSlider;
        public UiCheckbox vsyncSlider;
        public UiSelection fpsDisplay;

        private UiNavigation _navigation;

        private Dictionary<int, DisplaySettings.FpsDisplay> _fpsValueMap;

        private Dictionary<string, Resolution> _resolutions;
        private static readonly List<string> ScreenModes = FriendlyNameAttribute.GetNames<DisplaySettings.FullscreenMode>(false).ToList();
        
        private void Awake()
        {
            _navigation = GetComponentInParent<UiNavigation>();
            
            DisplaySettings settings = SettingsData.Get<DisplaySettings>();

            if (brightnessSlider)
            {
                brightnessSlider.SetInstant(settings.brightness);
                brightnessSlider.onValueChange.AddListener(ob => settings.brightness = brightnessSlider.Value);
            }

            if (gammaSlider)
            {
                gammaSlider.SetInstant(settings.gamma);
                gammaSlider.onValueChange.AddListener(ob => settings.gamma = gammaSlider.Value);
            }

            if (vsyncSlider)
            {
                vsyncSlider.SetInstant(settings.vsync);
                vsyncSlider.onValueChange.AddListener(ob => settings.vsync = vsyncSlider.IsChecked);
            }

            if (fpsDisplay)
            {
                _fpsValueMap = fpsDisplay.SetEnum<DisplaySettings.FpsDisplay>();

                fpsDisplay.Value = _fpsValueMap.SingleOrDefault(x => x.Value.Equals(settings.fpsDisplay)).Key;
                fpsDisplay.onValueChange.AddListener(ob => settings.fpsDisplay = _fpsValueMap[fpsDisplay.Value]);
            }
        }

        [UsedImplicitly]
        public void OpenResolutionDialog()
        {
            _resolutions = Screen.resolutions.Reverse().ToDictionary(x => x.ToString());
            
            int index = 0;

            if (_resolutions.ContainsValue(Screen.currentResolution))
                index = _resolutions.Values.ToList().IndexOf(Screen.currentResolution);
            
            _navigation.OpenModal("Display Resolution", _resolutions.Keys, SetResolution, index);
        }

        private void SetResolution(string result)
        {
            Debug.Log(_resolutions.ContainsKey(result));
            if (_resolutions.ContainsKey(result))
            {
                DisplaySettings settings = SettingsData.Get<DisplaySettings>();
                settings.resolution = _resolutions[result];
                SettingsApplier.ApplyResolution();
            }
        }
        
        [UsedImplicitly]
        public void OpenDisplayModeDialog()
        {
            DisplaySettings settings = SettingsData.Get<DisplaySettings>();
            
            _navigation.OpenModal("Fullscreen Mode", ScreenModes, SetDisplayMode, (int)settings.fullscreenMode);
        }

        private void SetDisplayMode(string result)
        {
            if (ScreenModes.Contains(result))
            {
                DisplaySettings settings = SettingsData.Get<DisplaySettings>();
                settings.fullscreenMode = (DisplaySettings.FullscreenMode) ScreenModes.IndexOf(result);
                SettingsApplier.ApplyResolution();
            }
        }

        [UsedImplicitly]
        public void OpenQualityDialog()
        {
            _navigation.OpenModal("Quality", QualitySettings.names, SetQualityLevel, QualitySettings.GetQualityLevel());
        }

        private void SetQualityLevel(string quality)
        {
            DisplaySettings settings = SettingsData.Get<DisplaySettings>();
            settings.qualityLevel = QualitySettings.names.ToList().IndexOf(quality);
            SettingsApplier.ApplyQualitySettings();
        }
    }
}