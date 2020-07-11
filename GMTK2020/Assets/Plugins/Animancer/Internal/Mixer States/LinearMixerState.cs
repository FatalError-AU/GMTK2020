// Animancer // Copyright 2020 Kybernetik //

using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// An <see cref="AnimancerState"/> which blends an array of other states together using linear interpolation
    /// between the specified thresholds.
    /// <para></para>
    /// This mixer type is similar to the 1D Blend Type in Mecanim Blend Trees.
    /// </summary>
    public sealed class LinearMixerState : MixerState<float>
    {
        /************************************************************************************************************************/

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> and <see cref="ManualMixerState._States"/> with one
        /// state per clip and assigns thresholds evenly spaced between the specified min and max (inclusive).
        /// </summary>
        public void Initialise(AnimationClip[] clips, float minThreshold = 0, float maxThreshold = 1)
        {
#if UNITY_ASSERTIONS
            if (minThreshold >= maxThreshold)
                throw new ArgumentException("minThreshold must be less than maxThreshold");
#endif

            Initialise(clips);
            AssignLinearThresholds(minThreshold, maxThreshold);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> with two ports and connects two states to them for
        /// the specified clips at the specified thresholds (default 0 and 1).
        /// </summary>
        public void Initialise(AnimationClip clip0, AnimationClip clip1,
            float threshold0 = 0, float threshold1 = 1)
        {
            Initialise(2);
            CreateState(0, clip0);
            CreateState(1, clip1);
            SetThresholds(threshold0, threshold1);
#if UNITY_ASSERTIONS
            AssertThresholdsSorted();
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> with three ports and connects three states to them for
        /// the specified clips at the specified thresholds (default -1, 0, and 1).
        /// </summary>
        public void Initialise(AnimationClip clip0, AnimationClip clip1, AnimationClip clip2,
            float threshold0 = -1, float threshold1 = 0, float threshold2 = 1)
        {
            Initialise(3);
            CreateState(0, clip0);
            CreateState(1, clip1);
            CreateState(2, clip2);
            SetThresholds(threshold0, threshold1, threshold2);
#if UNITY_ASSERTIONS
            AssertThresholdsSorted();
#endif
        }

        /************************************************************************************************************************/
#if UNITY_ASSERTIONS
        /************************************************************************************************************************/

        private bool _NeedToCheckThresholdSorting;

        /// <summary>
        /// Called whenever the thresholds are changed. Indicates that <see cref="AssertThresholdsSorted"/> needs to
        /// be called by the next <see cref="RecalculateWeights"/> if UNITY_ASSERTIONS is defined, then calls
        /// <see cref="MixerState{TParameter}.OnThresholdsChanged"/>.
        /// </summary>
        public override void OnThresholdsChanged()
        {
            _NeedToCheckThresholdSorting = true;

            base.OnThresholdsChanged();
        }

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the thresholds are not sorted from lowest to highest without
        /// any duplicates.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="InvalidOperationException">Thrown if the thresholds have not been initialised.</exception>
        public void AssertThresholdsSorted()
        {
#if UNITY_ASSERTIONS
            _NeedToCheckThresholdSorting = false;
#endif

            if (!IsInitialised)
                throw new InvalidOperationException("Thresholds have not been initialised");

            var previous = float.NegativeInfinity;

            var childCount = ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                var state = GetChild(i);
                if (state == null)
                    continue;

                var next = GetThreshold(i);
                if (next > previous)
                    previous = next;
                else
                    throw new ArgumentException("Thresholds are out of order." +
                        " They must be sorted from lowest to highest with no equal values.");
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Recalculates the weights of all <see cref="ManualMixerState._States"/> based on the current value of the
        /// <see cref="MixerState{TParameter}.Parameter"/> and the thresholds.
        /// </summary>
        public override void RecalculateWeights()
        {
            WeightsAreDirty = false;

#if UNITY_ASSERTIONS
            if (_NeedToCheckThresholdSorting)
                AssertThresholdsSorted();
#endif

            // Go through all states, figure out how much weight to give those with thresholds adjacent to the
            // current parameter value using linear interpolation, and set all others to 0 weight.

            var index = 0;
            var previousState = GetNextState(ref index);
            if (previousState == null)
                return;

            var previousThreshold = GetThreshold(index);

            if (Parameter <= previousThreshold)
            {
                previousState.Weight = 1;
                DisableRemainingStates(index);
                return;
            }

            var childCount = ChildCount;
            while (++index < childCount)
            {
                var nextState = GetNextState(ref index);
                if (nextState == null)
                    break;

                var nextThreshold = GetThreshold(index);

                if (Parameter > previousThreshold && Parameter <= nextThreshold)
                {
                    var t = (Parameter - previousThreshold) / (nextThreshold - previousThreshold);
                    previousState.Weight = 1 - t;
                    nextState.Weight = t;
                    DisableRemainingStates(index);
                    return;
                }
                else
                {
                    previousState.Weight = 0;
                }

                previousState = nextState;
                previousThreshold = nextThreshold;
            }

            previousState.Weight = Parameter > previousThreshold ? 1 : 0;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Assigns the thresholds to be evenly spaced between the specified min and max (inclusive).
        /// </summary>
        public void AssignLinearThresholds(float min = 0, float max = 1)
        {
            var childCount = ChildCount;

            var thresholds = new float[childCount];

            var increment = (max - min) / (childCount - 1);

            for (int i = 0; i < childCount; i++)
            {
                thresholds[i] =
                    i < childCount - 1 ?
                    min + i * increment :// Assign each threshold linearly spaced between the min and max.
                    max;// and ensure that the last one is exactly at the max (to avoid floating-point error).
            }

            SetThresholds(thresholds);
        }

        /************************************************************************************************************************/
        #region Inspector
        /************************************************************************************************************************/

        /// <summary>The number of parameters being managed by this state.</summary>
        protected override int ParameterCount { get { return 1; } }

        /// <summary>Returns the name of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">Thrown if this state doesn't manage any parameters.</exception>
        protected override string GetParameterName(int index) { return "Parameter"; }

        /// <summary>Returns the type of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">Thrown if this state doesn't manage any parameters.</exception>
        protected override AnimatorControllerParameterType GetParameterType(int index) { return AnimatorControllerParameterType.Float; }

        /// <summary>Returns the value of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">Thrown if this state doesn't manage any parameters.</exception>
        protected override object GetParameterValue(int index) { return Parameter; }

        /// <summary>Sets the value of a parameter being managed by this state.</summary>
        /// <exception cref="NotSupportedException">Thrown if this state doesn't manage any parameters.</exception>
        protected override void SetParameterValue(int index, object value) { Parameter = (float)value; }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Transition
        /************************************************************************************************************************/

        /// <summary>
        /// A serializable <see cref="ITransition"/> which can create a <see cref="LinearMixerState"/> when
        /// passed into <see cref="AnimancerPlayable.Play(ITransition)"/>.
        /// </summary>
        /// <remarks>
        /// Unfortunately the tool used to generate this documentation does not currently support nested types with
        /// identical names, so only one <c>Transition</c> class will actually have a documentation page.
        /// </remarks>
        [Serializable]
        public new class Transition : Transition<LinearMixerState, float>
        {
            /************************************************************************************************************************/

            /// <summary>
            /// Creates and returns a new <see cref="LinearMixerState"/> connected to the `layer`.
            /// <para></para>
            /// This method also assigns it as the <see cref="AnimancerState.Transition{TState}.State"/>.
            /// </summary>
            public override LinearMixerState CreateState()
            {
                State = new LinearMixerState();
                InitialiseState();
                return State;
            }

            /************************************************************************************************************************/

            /// <summary>
            /// Sorts all states so that their thresholds go from lowest to highest.
            /// <para></para>
            /// This method uses Bubble Sort which is inefficient for large numbers of states.
            /// </summary>
            public void SortByThresholds()
            {
                var thresholdCount = Thresholds.Length;
                if (thresholdCount <= 1)
                    return;

                var speedCount = Speeds.Length;
                var syncCount = SynchroniseChildren.Length;

                var previousThreshold = Thresholds[0];

                for (int i = 1; i < thresholdCount; i++)
                {
                    var threshold = Thresholds[i];
                    if (threshold >= previousThreshold)
                    {
                        previousThreshold = threshold;
                        continue;
                    }

                    Thresholds.Swap(i, i - 1);
                    Clips.Swap(i, i - 1);

                    if (i < speedCount)
                        Speeds.Swap(i, i - 1);

                    if (i == syncCount && !SynchroniseChildren[i - 1])
                    {
                        var sync = SynchroniseChildren;
                        Array.Resize(ref sync, ++syncCount);
                        sync[i - 1] = true;
                        sync[i] = false;
                        SynchroniseChildren = sync;
                    }
                    else if (i < syncCount)
                    {
                        SynchroniseChildren.Swap(i, i - 1);
                    }

                    if (i == 1)
                    {
                        i = 0;
                        previousThreshold = float.NegativeInfinity;
                    }
                    else
                    {
                        i -= 2;
                        previousThreshold = Thresholds[i];
                    }
                }
            }

            /************************************************************************************************************************/
            #region Drawer
#if UNITY_EDITOR
            /************************************************************************************************************************/

            /// <summary>[Editor-Only] Draws the Inspector GUI for a <see cref="Transition"/>.</summary>
            [UnityEditor.CustomPropertyDrawer(typeof(Transition), true)]
            public class Drawer : TransitionDrawer
            {
                /************************************************************************************************************************/

                private static GUIContent _SortingErrorContent;
                private static GUIStyle _SortingErrorStyle;

                /// <summary>Draws the GUI of the threshold at the specified `index`.</summary>
                protected override void DoThresholdGUI(Rect area, int index)
                {
                    var color = GUI.color;

                    if (index > 0)
                    {
                        var previousThreshold = CurrentThresholds.GetArrayElementAtIndex(index - 1);
                        var currentThreshold = CurrentThresholds.GetArrayElementAtIndex(index);
                        if (previousThreshold.floatValue >= currentThreshold.floatValue)
                        {
                            if (_SortingErrorContent == null)
                            {
                                _SortingErrorContent = UnityEditor.EditorGUIUtility.IconContent("console.erroricon.sml");

                                _SortingErrorContent.tooltip = "Linear Mixer Thresholds must always be sorted in ascending order (click to sort)";

                                _SortingErrorStyle = new GUIStyle(GUI.skin.label)
                                {
                                    padding = new RectOffset(),
                                };
                            }

                            var iconArea = Editor.AnimancerGUI.StealFromRight(ref area, area.height, Editor.AnimancerGUI.StandardSpacing);
                            if (GUI.Button(iconArea, _SortingErrorContent, _SortingErrorStyle))
                            {
                                var transition = (Transition)Context.Transition;
                                UnityEditor.Undo.RecordObjects(Context.Property.serializedObject.targetObjects, "Inspector");
                                transition.SortByThresholds();
                            }

                            GUI.color = Editor.AnimancerGUI.ErrorFieldColor;
                        }
                    }

                    base.DoThresholdGUI(area, index);

                    GUI.color = color;
                }

                /************************************************************************************************************************/

                /// <summary><see cref="AddThresholdFunctionsToMenu"/> will add some functions to the menu.</summary>
                protected override bool HasThresholdContextMenu { get { return true; } }

                /// <summary>Adds functions to the `menu` relating to the thresholds.</summary>
                protected override void AddThresholdFunctionsToMenu(UnityEditor.GenericMenu menu)
                {
                    AddPropertyModifierFunction(menu, "Evenly Spaced", (_) =>
                    {
                        var count = CurrentThresholds.arraySize;
                        if (count <= 1)
                            return;

                        var first = CurrentThresholds.GetArrayElementAtIndex(0).floatValue;
                        var last = CurrentThresholds.GetArrayElementAtIndex(count - 1).floatValue;
                        for (int i = 0; i < count; i++)
                        {
                            CurrentThresholds.GetArrayElementAtIndex(i).floatValue = Mathf.Lerp(first, last, i / (float)(count - 1));
                        }
                    });

                    AddCalculateThresholdsFunction(menu, "From Speed",
                        (clip, threshold) => clip.apparentSpeed);
                    AddCalculateThresholdsFunction(menu, "From Velocity X",
                        (clip, threshold) => clip.averageSpeed.x);
                    AddCalculateThresholdsFunction(menu, "From Velocity Y",
                        (clip, threshold) => clip.averageSpeed.z);
                    AddCalculateThresholdsFunction(menu, "From Velocity Z",
                        (clip, threshold) => clip.averageSpeed.z);
                    AddCalculateThresholdsFunction(menu, "From Angular Speed (Rad)",
                        (clip, threshold) => clip.averageAngularSpeed);
                    AddCalculateThresholdsFunction(menu, "From Angular Speed (Deg)",
                        (clip, threshold) => clip.averageAngularSpeed * Mathf.Rad2Deg);
                }

                /************************************************************************************************************************/

                private static void AddCalculateThresholdsFunction(UnityEditor.GenericMenu menu, string label,
                    Func<AnimationClip, float, float> calculateThreshold)
                {
                    AddPropertyModifierFunction(menu, label, (property) =>
                    {
                        var count = CurrentClips.arraySize;
                        for (int i = 0; i < count; i++)
                        {
                            var clip = CurrentClips.GetArrayElementAtIndex(i).objectReferenceValue as AnimationClip;
                            if (clip == null)
                                continue;

                            var threshold = CurrentThresholds.GetArrayElementAtIndex(i);

                            threshold.floatValue = calculateThreshold(clip, threshold.floatValue);
                        }
                    });
                }

                /************************************************************************************************************************/
            }

            /************************************************************************************************************************/
#endif
            #endregion
            /************************************************************************************************************************/
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

