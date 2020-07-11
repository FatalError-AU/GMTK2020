// Animancer // Copyright 2020 Kybernetik //

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>[Pro-Only]
    /// Base class for mixers which blend an array of child states together based on a <see cref="Parameter"/>.
    /// </summary>
    public abstract class MixerState<TParameter> : ManualMixerState
    {
        /************************************************************************************************************************/
        #region Properties
        /************************************************************************************************************************/

        /// <summary>The parameter values at which each of the child states are used and blended.</summary>
        private TParameter[] _Thresholds;

        /************************************************************************************************************************/

        private TParameter _Parameter;

        /// <summary>The value used to calculate the weights of the child states.</summary>
        public TParameter Parameter
        {
            get { return _Parameter; }
            set
            {
                _Parameter = value;
                WeightsAreDirty = true;
                RequireUpdate();
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Thresholds
        /************************************************************************************************************************/

        /// <summary>
        /// Returns the value of the threshold associated with the specified index.
        /// </summary>
        public TParameter GetThreshold(int index)
        {
            return _Thresholds[index];
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Sets the value of the threshold associated with the specified index.
        /// </summary>
        public void SetThreshold(int index, TParameter threshold)
        {
            _Thresholds[index] = threshold;
            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Assigns the specified array as the thresholds to use for blending.
        /// <para></para>
        /// WARNING: if you keep a reference to the `thresholds` array you must call <see cref="OnThresholdsChanged"/>
        /// whenever any changes are made to it, otherwise this mixer may not blend correctly.
        /// </summary>
        public void SetThresholds(params TParameter[] thresholds)
        {
            if (thresholds.Length != ChildCount)
                throw new ArgumentOutOfRangeException("thresholds", "Incorrect threshold count. There are " + ChildCount +
                    " states, but the specified thresholds array contains " + thresholds.Length + " elements.");

            _Thresholds = thresholds;
            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="_Thresholds"/> don't have the same <see cref="Array.Length"/> as the
        /// <see cref="ManualMixerState._States"/>, this method allocates and assigns a new array of that size.
        /// </summary>
        public bool ValidateThresholdCount()
        {
            if (!IsInitialised)
                return false;

            if (_Thresholds == null || _Thresholds.Length != ChildCount)
            {
                _Thresholds = new TParameter[ChildCount];
                return true;
            }

            return false;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Called whenever the thresholds are changed. By default this method simply indicates that the blend weights
        /// need recalculating but it can be overridden by child classes to perform validation checks or optimisations.
        /// </summary>
        public virtual void OnThresholdsChanged()
        {
            WeightsAreDirty = true;
            RequireUpdate();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls `calculate` for each of the <see cref="ManualMixerState._States"/> and stores the returned value as
        /// the threshold for that state.
        /// </summary>
        public void CalculateThresholds(Func<AnimancerState, TParameter> calculate)
        {
            ValidateThresholdCount();

            for (int i = ChildCount - 1; i >= 0; i--)
            {
                var state = GetChild(i);
                if (state == null)
                    continue;

                _Thresholds[i] = calculate(state);
            }

            OnThresholdsChanged();
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Initialisation
        /************************************************************************************************************************/

        /// <summary>
        /// Initialises this mixer with the specified number of ports which can be filled individually by <see cref="CreateState"/>.
        /// </summary>
        public override void Initialise(int portCount)
        {
            base.Initialise(portCount);
            _Thresholds = new TParameter[portCount];
            OnThresholdsChanged();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> and <see cref="ManualMixerState._States"/> with one
        /// state per clip and assigns the `thresholds`.
        /// <para></para>
        /// WARNING: if you keep a reference to the `thresholds` array, you must call
        /// <see cref="OnThresholdsChanged"/> whenever any changes are made to it, otherwise this mixer may not blend
        /// correctly.
        /// </summary>
        public void Initialise(AnimationClip[] clips, TParameter[] thresholds)
        {
            Initialise(clips);
            _Thresholds = thresholds;
            OnThresholdsChanged();
        }

        /// <summary>
        /// Initialises the <see cref="AnimationMixerPlayable"/> and <see cref="ManualMixerState._States"/> with one
        /// state per clip and assigns the thresholds by calling `calculateThreshold` for each state.
        /// </summary>
        public void Initialise(AnimationClip[] clips, Func<AnimancerState, TParameter> calculateThreshold)
        {
            Initialise(clips);
            CalculateThresholds(calculateThreshold);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Creates and returns a new <see cref="ClipState"/> to play the `clip` with this
        /// <see cref="MixerState"/> as its parent, connects it to the specified `index`, and assigns the
        /// `threshold` for it.
        /// </summary>
        public ClipState CreateState(int index, AnimationClip clip, TParameter threshold)
        {
            SetThreshold(index, threshold);
            return CreateState(index, clip);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Descriptions
        /************************************************************************************************************************/

        /// <summary>Gets a user-friendly key to identify the `state` in the Inspector.</summary>
        public override string GetDisplayKey(AnimancerState state)
        {
            return string.Concat("[", state.Index.ToString(), "] ", _Thresholds[state.Index].ToString());
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Called by <see cref="AnimancerNode.AppendDescription"/> to append the details of this node.
        /// </summary>
        protected override void AppendDetails(StringBuilder text, string delimiter)
        {
            text.Append(delimiter);
            text.Append("Parameter: ");
            AppendParameter(text, Parameter);

            text.Append(delimiter).Append("Thresholds: ");
            if (_Thresholds == null)
            {
                text.Append("null");
            }
            else
            {
                for (int i = 0; i < _Thresholds.Length; i++)
                {
                    if (i > 0)
                        text.Append(", ");

                    AppendParameter(text, _Thresholds[i]);
                }
            }

            base.AppendDetails(text, delimiter);
        }

        /************************************************************************************************************************/

        /// <summary>Appends the `parameter` in a viewer-friendly format.</summary>
        public virtual void AppendParameter(StringBuilder description, TParameter parameter)
        {
            description.Append(parameter);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

