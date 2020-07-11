using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace GMTK2020.Data.Settings
{
    [AddComponentMenu("")]
    public sealed class SettingsApplier : MonoBehaviour
    {
        private VolumeProfile _profile;

        private ColorAdjustments _colorAdjustments;
        private LiftGammaGain _gammaAdjustments;

        private AudioMixer _mixer;

        private float _frameDelta;

        private void Awake()
        {
            Volume pp = gameObject.AddComponent<Volume>();

            pp.isGlobal = true;

            _profile = ScriptableObject.CreateInstance<VolumeProfile>();
            pp.profile = _profile;

            _colorAdjustments = _profile.Add<ColorAdjustments>();
            _gammaAdjustments = _profile.Add<LiftGammaGain>();

            _colorAdjustments.postExposure.overrideState = true;
            _gammaAdjustments.gamma.overrideState = true;

            _mixer = Resources.Load<AudioMixer>("Mixer");
            if(!_mixer)
                Debug.LogError("Audio mixer not found");
        }

        private void Update()
        {
            DisplaySettings display = SettingsData.Get<DisplaySettings>();

            if (display != null)
            {
                _colorAdjustments.postExposure.value = Mathf.Clamp(display.brightness, -2F, 2F);
                
                Vector4 gamma = _gammaAdjustments.gamma.value;
                gamma.w = Mathf.Clamp(display.gamma, -1F, 1F);
                _gammaAdjustments.gamma.value = gamma;
                
                QualitySettings.vSyncCount = display.vsync ? 1 : 0;
            }

            SoundSettings sound = SettingsData.Get<SoundSettings>();
            
            if (sound != null)
            {
                foreach (KeyValuePair<SoundSettings.Channel, int> channel in sound.volume)
                {
                    // Formula for percentage to audio scaling: Log10(percentage) * 20 where percentage > 0
                    // Source: https://johnleonardfrench.com/articles/the-right-way-to-make-a-volume-slider-in-unity-using-logarithmic-conversion/
                    _mixer.SetFloat($"Volume{channel.Key}", Mathf.Log10(Mathf.Max(0.0001F, channel.Value / 100F)) * 20F);
                }
            }
            
            _frameDelta += (Time.unscaledDeltaTime - _frameDelta) * 0.1F;
        }

        private void OnDestroy()
        {
            DestroyImmediate(_profile);
        }

        #region Obsolete UI Code

        private void OnGUI()
        {
            DisplaySettings display = SettingsData.Get<DisplaySettings>();

            if (display == null || display.fpsDisplay == DisplaySettings.FpsDisplay.None)
                return;

            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();
            Rect rect = default;

            style.fontSize = h * 2 / 100;

            switch (display.fpsDisplay)
            {
                case DisplaySettings.FpsDisplay.None:
                    break;
                case DisplaySettings.FpsDisplay.TopLeft:
                    style.alignment = TextAnchor.UpperLeft;
                    rect = new Rect(0, 0, w, style.fontSize);
                    break;
                case DisplaySettings.FpsDisplay.TopRight:
                    style.alignment = TextAnchor.UpperRight;
                    rect = new Rect(0, 0, w, style.fontSize);
                    break;
                case DisplaySettings.FpsDisplay.BottomLeft:
                    style.alignment = TextAnchor.LowerLeft;
                    rect = new Rect(0, h - style.fontSize, w, style.fontSize);
                    break;
                case DisplaySettings.FpsDisplay.BottomRight:
                    style.alignment = TextAnchor.LowerRight;
                    rect = new Rect(0, h - style.fontSize, w, style.fontSize);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            style.normal.textColor = Color.yellow;

            float msec = _frameDelta * 1000F;
            float fps = 1F / _frameDelta;

            string text = $"{msec:0.0} ms ({fps:0.} fps)";
            GUI.Label(rect, text, style);
        }

        #endregion

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            ApplyResolution();
            ApplyQualitySettings();
            
            GameObject go = new GameObject("Settings Applier");
            DontDestroyOnLoad(go);

            go.AddComponent<SettingsApplier>();
        }

        public static void ApplyResolution()
        {
            DisplaySettings display = SettingsData.Get<DisplaySettings>();
            Resolution res = display.resolution;
            Screen.SetResolution(res.width, res.height, DisplaySettings.ModeToUnity(display.fullscreenMode), res.refreshRate);
        }

        public static void ApplyQualitySettings()
        {
            DisplaySettings display = SettingsData.Get<DisplaySettings>();
            QualitySettings.SetQualityLevel(display.qualityLevel, true);
        }
    }
}