// Animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// Base class for <see cref="Playable"/> wrapper objects in <see cref="Animancer"/>.
    /// </summary>
    public abstract class AnimancerNode : Key, IEnumerable<AnimancerState>, IEnumerator, IPlayableWrapper
    {
        /************************************************************************************************************************/
        #region Playable
        /************************************************************************************************************************/

        /// <summary>
        /// The internal object this node manages in the <see cref="PlayableGraph"/>.
        /// <para></para>
        /// Must be set by <see cref="CreatePlayable()"/>. Failure to do so will throw the following exception throughout
        /// the system when using this node: "<see cref="ArgumentException"/>: The playable passed as an argument is
        /// invalid. To create a valid playable, please use the appropriate Create method".
        /// </summary>
        protected internal Playable _Playable;

        /// <summary>[Internal] The <see cref="Playable"/> managed by this node.</summary>
        Playable IPlayableWrapper.Playable { get { return _Playable; } }

        /// <summary>
        /// Indicates whether the <see cref="_Playable"/> is usable (properly initialised and not destroyed).
        /// </summary>
        public bool IsValid { get { return _Playable.IsValid(); } }

        /************************************************************************************************************************/

#if UNITY_EDITOR
        /// <summary>[Editor-Only] [Internal] Indicates whether the Inspector details for this node are expanded.</summary>
        internal bool _IsInspectorExpanded;
#endif

        /************************************************************************************************************************/

        /// <summary>
        /// If the <see cref="Speed"/> was set before <see cref="CreatePlayable()"/> was called, this method applies it.
        /// </summary>
        protected void CreatePlayable()
        {
#if UNITY_ASSERTIONS
            if (Root == null)
                throw new InvalidOperationException("AnimancerNode.Root is null when attempting to create its Playable: " + this);
            if (_Playable.IsValid())
                Debug.LogWarning("AnimancerNode.CreatePlayable was called before destroying the previous Playable:" + this);
#endif

            CreatePlayable(out _Playable);

#if UNITY_ASSERTIONS
            if (!_Playable.IsValid())
                throw new InvalidOperationException("AnimancerNode.CreatePlayable did not create a valid Playable:" + this);
#endif

            if (_Speed != 1)
                _Playable.SetSpeed(_Speed);

            var parent = Parent;
            if (parent != null)
                ApplyConnectedState(parent);
        }

        /// <summary>Creates and assigns the <see cref="Playable"/> managed by this node.</summary>
        protected abstract void CreatePlayable(out Playable playable);

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="Playable"/>.</summary>
        public void DestroyPlayable()
        {
            if (_Playable.IsValid())
                Root._Graph.DestroyPlayable(_Playable);
        }

        /************************************************************************************************************************/

        /// <summary>Destroys the <see cref="Playable"/> and creates a new one using <see cref="CreatePlayable()"/>.</summary>
        public void RecreatePlayable()
        {
            DestroyPlayable();
            CreatePlayable();
        }

        /// <summary>Calls <see cref="RecreatePlayable"/> on this node and all its children recursively.</summary>
        public void RecreatePlayableRecursive()
        {
            RecreatePlayable();

            for (int i = ChildCount - 1; i >= 0; i--)
            {
                var child = GetChild(i);
                if (child != null)
                    child.RecreatePlayableRecursive();
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Graph
        /************************************************************************************************************************/

        /// <summary>The <see cref="AnimancerPlayable"/> at the root of the graph.</summary>
        public AnimancerPlayable Root { get; internal set; }

        /// <summary>The <see cref="PlayableGraph"/> managed by the <see cref="Root"/>.</summary>
        /// <remarks>This allows nodes defined outside the Animancer assembly to access the graph since it is internal.</remarks>
        protected PlayableGraph Graph
        {
            get { return Root._Graph; }
        }

        /************************************************************************************************************************/

        /// <summary>The root <see cref="AnimancerLayer"/> which this node is connected to.</summary>
        public abstract AnimancerLayer Layer { get; }

        /// <summary>The object which receives the output of this node.</summary>
        public abstract IPlayableWrapper Parent { get; }

        /************************************************************************************************************************/

        /// <summary>
        /// The index of the port this node is connected to on the parent's <see cref="Playable"/>.
        /// <para></para>
        /// A negative value indicates that it is not assigned to a port.
        /// </summary>
        /// <remarks>
        /// Indices are generally assigned starting from 0, ascending in the order they are connected to their layer.
        /// They will not usually change unless the <see cref="Parent"/> changes or another state on the same layer is
        /// destroyed so the last state is swapped into its place to avoid shuffling everything down to cover the gap.
        /// <para></para>
        /// The setter is internal so user defined states cannot set it incorrectly. Ideally,
        /// <see cref="AnimancerLayer"/> should be able to set the port in its constructor and
        /// <see cref="AnimancerState.SetParent"/> should also be able to set it, but classes that further inherit from
        /// there should not be able to change it without properly calling that method.
        /// </remarks>
        public int Index { get; internal set; }

        /************************************************************************************************************************/

        /// <summary>Constructs a new <see cref="AnimancerNode"/>.</summary>
        protected AnimancerNode()
        {
            Index = int.MinValue;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Connects the <see cref="_Playable"/> to the <see cref="Parent"/>.
        /// </summary>
        public void ConnectToGraph()
        {
            var parent = Parent;
            if (parent == null)
                return;

#if UNITY_ASSERTIONS
            if (Index < 0)
                throw new InvalidOperationException("Invalid AnimancerNode.Index when attempting to connect to its parent:" +
                    "\n    Node: " + this +
                    "\n    Parent: " + parent);
#endif

            Root._Graph.Connect(_Playable, 0, parent.Playable, Index);
            if (_IsWeightDirty)
                Root.RequireUpdate(this);
        }

        /// <summary>
        /// Disconnects the <see cref="_Playable"/> from the <see cref="Parent"/>.
        /// </summary>
        public void DisconnectFromGraph()
        {
            var parent = Parent;
            if (parent == null)
                return;

            var parentMixer = parent.Playable;
            if (parentMixer.GetInput(Index).IsValid())
                Root._Graph.Disconnect(parentMixer, Index);
        }

        /************************************************************************************************************************/

        private void ApplyConnectedState(IPlayableWrapper parent)
        {
#if UNITY_ASSERTIONS
            if (Index < 0)
                throw new InvalidOperationException("Invalid AnimancerNode.Index when attempting to connect to its parent:" +
                    "\n    Node: " + this +
                    "\n    Parent: " + parent);
#endif

            _IsWeightDirty = true;

            if (_Weight != 0 || parent.KeepChildrenConnected)
            {
                ConnectToGraph();
            }
            else
            {
                Root.RequireUpdate(this);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="AnimancerPlayable.RequireUpdate(AnimancerNode)"/> as long as the <see cref="Root"/> is not
        /// null.
        /// </summary>
        protected void RequireUpdate()
        {
            if (Root != null)
                Root.RequireUpdate(this);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Updates the <see cref="Weight"/> for fading, applies it to this state's port on the parent mixer, and plays
        /// or pauses the <see cref="Playable"/> if its state is dirty.
        /// <para></para>
        /// If the <see cref="Parent"/>'s <see cref="KeepChildrenConnected"/> is set to false, this method will
        /// also connect/disconnect this node from the <see cref="Parent"/> in the playable graph.
        /// </summary>
        protected internal virtual void Update(out bool needsMoreUpdates)
        {
            UpdateFade(out needsMoreUpdates);

            ApplyWeight();

        }

        /************************************************************************************************************************/
        // IEnumerator for yielding in a coroutine to wait until animations have stopped.
        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the animation is playing and hasn't yet reached its end.
        /// <para></para>
        /// This method is called by <see cref="IEnumerator.MoveNext"/> so this object can be used as a custom yield
        /// instruction to wait until it finishes.
        /// </summary>
        protected internal abstract bool IsPlayingAndNotEnding();

        /// <summary>Calls <see cref="IsPlayingAndNotEnding"/>.</summary>
        bool IEnumerator.MoveNext() { return IsPlayingAndNotEnding(); }

        /// <summary>Returns null.</summary>
        object IEnumerator.Current { get { return null; } }

        /// <summary>Does nothing.</summary>
        void IEnumerator.Reset() { }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Children
        /************************************************************************************************************************/

        /// <summary>The number of states using this node as their <see cref="AnimancerState.Parent"/>.</summary>
        public virtual int ChildCount { get { return 0; } }

        /// <summary>
        /// Returns the state connected to the specified `index` as a child of this node.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if this node cannot have children.</exception>
        public virtual AnimancerState GetChild(int index)
        {
            throw new NotSupportedException(this + " cannot have children.");
        }

        /// <summary>
        /// Called when a child is connected with this node as its <see cref="AnimancerState.Parent"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if this node cannot have children.</exception>
        protected internal virtual void OnAddChild(AnimancerState state)
        {
            throw new NotSupportedException(this + " cannot have children.");
        }

        /// <summary>
        /// Called when a child's <see cref="AnimancerState.Parent"/> is changed from this node to something else.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown if this node cannot have children.</exception>
        protected internal virtual void OnRemoveChild(AnimancerState state)
        {
            throw new NotSupportedException(this + " cannot have children.");
        }

        /************************************************************************************************************************/

        /// <summary>Connects the `state` to this node at its <see cref="Index"/>.</summary>
        /// <exception cref="InvalidOperationException">Thrown if the <see cref="Index"/> was already occupied.</exception>
        protected void OnAddChild(IList<AnimancerState> states, AnimancerState state)
        {
            var index = state.Index;

            if (states[index] != null)
            {
                state.ClearParent();
                throw new InvalidOperationException(
                    "Tried to add a state to an already occupied port on " + this + ":" +
                    "\n    Index: " + index +
                    "\n    Old State: " + states[index] +
                    "\n    New State: " + state);
            }

#if UNITY_ASSERTIONS
            if (state.Root != Root)
                Debug.LogError("AnimancerNode.Root mismatch:" +
                    "\n    State: " + state +
                    "\n    State.Root: " + state.Root +
                    "\n    Parent.Root: " + Root);
#endif

            states[index] = state;

            if (Root != null)
                state.ApplyConnectedState(this);
        }

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// Called by <see cref="AnimancerState.Destroy"/> for any states connected to this mixer.
        /// Adds the `state`s port to a list of spares to be reused by another state and notifies the root
        /// <see cref="AnimancerPlayable"/>.
        /// </summary>
        protected internal virtual void OnChildDestroyed(AnimancerState state) { }

        /************************************************************************************************************************/

        /// <summary>
        /// Indicates whether child playables should stay connected to this mixer at all times (default false).
        /// </summary>
        public virtual bool KeepChildrenConnected { get { return false; } }

        /// <summary>
        /// Ensures that all children of this node are connected to the <see cref="_Playable"/>.
        /// </summary>
        internal void ConnectAllChildrenToGraph()
        {
            if (!Parent.Playable.GetInput(Index).IsValid())
                ConnectToGraph();

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i).ConnectAllChildrenToGraph();
        }

        /// <summary>
        /// Ensures that all children of this node which have zero weight are disconnected from the
        /// <see cref="_Playable"/>.
        /// </summary>
        internal void DisconnectWeightlessChildrenFromGraph()
        {
            if (Weight == 0)
                DisconnectFromGraph();

            for (int i = ChildCount - 1; i >= 0; i--)
                GetChild(i).DisconnectWeightlessChildrenFromGraph();
        }

        /************************************************************************************************************************/
        // IEnumerable for 'foreach' statements.
        /************************************************************************************************************************/

        /// <summary>Gets an enumerator for all of this node's child states.</summary>
        public virtual IEnumerator<AnimancerState> GetEnumerator()
        {
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Weight
        /************************************************************************************************************************/

        /// <summary>The current blend weight of this node. Accessed via <see cref="Weight"/>.</summary>
        private float _Weight;

        /// <summary>Indicates whether the weight has changed and should be applied to the parent mixer.</summary>
        private bool _IsWeightDirty = true;

        /************************************************************************************************************************/

        /// <summary>
        /// The current blend weight of this node which determines how much it affects the final output. 0 has no
        /// effect while 1 applies the full effect of this node and values inbetween apply a proportional effect.
        /// <para></para>
        /// Setting this property cancels any fade currently in progress. If you don't wish to do that, you can use
        /// <see cref="SetWeight"/> instead.
        /// <para></para>
        /// Animancer Lite only allows this value to be set to 0 or 1 in a runtime build.
        /// </summary>
        ///
        /// <example>
        /// Calling <see cref="AnimancerPlayable.Play(AnimationClip)"/> immediately sets the weight of all states to 0
        /// and the new state to 1. Note that this is separate from other values like
        /// <see cref="AnimancerState.IsPlaying"/> so a state can be paused at any point and still show its pose on the
        /// character or it could be still playing at 0 weight if you want it to still trigger events (though states
        /// are normally stopped when they reach 0 weight so you would need to explicitly set it to playing again).
        /// <para></para>
        /// Calling <see cref="AnimancerPlayable.Play(AnimationClip, float, FadeMode)"/> does not immediately change
        /// the weights, but instead calls <see cref="StartFade"/> on every state to set their
        /// <see cref="TargetWeight"/> and <see cref="FadeSpeed"/>. Then every update each state's weight will move
        /// towards that target value at that speed.
        /// </example>
        public float Weight
        {
            get { return _Weight; }
            set
            {
                SetWeight(value);
                TargetWeight = value;
                FadeSpeed = 0;
            }
        }

        /// <summary>
        /// Sets the current blend weight of this node which determines how much it affects the final output.
        /// 0 has no effect while 1 applies the full effect of this node.
        /// <para></para>
        /// This method allows any fade currently in progress to continue. If you don't wish to do that, you can set
        /// the <see cref="Weight"/> property instead.
        /// <para></para>
        /// Animancer Lite only allows this value to be set to 0 or 1 in a runtime build.
        /// </summary>
        public void SetWeight(float value)
        {
            if (_Weight == value)
                return;

#if UNITY_ASSERTIONS
            if (!(value >= 0) || value == float.PositiveInfinity)// Reversed comparison includes NaN.
                throw new ArgumentOutOfRangeException("value", value, "Weight must be a finite positive value");
#endif

            _Weight = value;
            SetWeightDirty();
        }

        /// <summary>
        /// Flags this node as having a dirty weight that needs to be applied next update.
        /// </summary>
        protected internal void SetWeightDirty()
        {
            _IsWeightDirty = true;
            RequireUpdate();
        }

        /************************************************************************************************************************/

        /// <summary>[Internal]
        /// Applies the <see cref="Weight"/> to the connection between this node and its <see cref="Parent"/>.
        /// </summary>
        internal void ApplyWeight()
        {
            if (!_IsWeightDirty)
                return;

            _IsWeightDirty = false;

            var parent = Parent;
            if (parent == null)
                return;

            Playable parentMixer;

            if (!parent.KeepChildrenConnected)
            {
                if (_Weight == 0)
                {
                    DisconnectFromGraph();
                    return;
                }

                parentMixer = parent.Playable;
                if (!parentMixer.GetInput(Index).IsValid())
                    ConnectToGraph();
            }
            else parentMixer = parent.Playable;

            parentMixer.SetInputWeight(Index, _Weight);
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Fading
        /************************************************************************************************************************/

        /// <summary>
        /// The desired <see cref="Weight"/> which this node is fading towards according to the
        /// <see cref="FadeSpeed"/>.
        /// </summary>
        public float TargetWeight { get; set; }

        /// <summary>
        /// The speed at which this node is fading towards the <see cref="TargetWeight"/>.
        /// </summary>
        public float FadeSpeed { get; set; }

        /************************************************************************************************************************/

        /// <summary>
        /// Calls <see cref="OnStartFade"/> and starts fading the <see cref="Weight"/> over the course
        /// of the `fadeDuration` (in seconds).
        /// <para></para>
        /// If the `targetWeight` is 0 then <see cref="Stop"/> will be called when the fade is complete.
        /// <para></para>
        /// If the <see cref="Weight"/> is already equal to the `targetWeight` then the fade will end
        /// immediately.
        /// <para></para>
        /// Animancer Lite only allows a `targetWeight` of 0 or 1 and the default `fadeDuration` in a runtime build.
        /// </summary>
        public void StartFade(float targetWeight, float fadeDuration = AnimancerPlayable.DefaultFadeDuration)
        {

            TargetWeight = targetWeight;

            if (targetWeight == Weight)
            {
                if (targetWeight == 0)
                {
                    Stop();
                }
                else
                {
                    FadeSpeed = 0;
                    OnStartFade();
                }

                return;
            }

            // Duration 0 = Instant.
            if (fadeDuration <= 0)
            {
                FadeSpeed = float.PositiveInfinity;
            }
            else// Otherwise determine how fast we need to go to cover the distance in the specified time.
            {
                FadeSpeed = Math.Abs(Weight - targetWeight) / fadeDuration;
            }

            OnStartFade();
            RequireUpdate();
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Called by <see cref="StartFade"/>.
        /// </summary>
        protected internal abstract void OnStartFade();

        /************************************************************************************************************************/

        /// <summary>
        /// Stops the animation and makes it inactive immediately so it no longer affects the output.
        /// Sets <see cref="Weight"/> = 0 by default.
        /// </summary>
        public virtual void Stop()
        {
            Weight = 0;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Moves the <see cref="Weight"/> towards the <see cref="TargetWeight"/> according to the
        /// <see cref="FadeSpeed"/>.
        /// </summary>
        private void UpdateFade(out bool needsMoreUpdates)
        {
            var fadeSpeed = FadeSpeed;
            if (fadeSpeed == 0)
            {
                needsMoreUpdates = false;
                return;
            }

            _IsWeightDirty = true;

            fadeSpeed *= ParentEffectiveSpeed * AnimancerPlayable.DeltaTime;
            if (fadeSpeed < 0)
                fadeSpeed = -fadeSpeed;

            var target = TargetWeight;
            var current = _Weight;

            var delta = target - current;
            if (delta > 0)
            {
                if (delta > fadeSpeed)
                {
                    _Weight = current + fadeSpeed;
                    needsMoreUpdates = true;
                    return;
                }
            }
            else
            {
                if (-delta > fadeSpeed)
                {
                    _Weight = current - fadeSpeed;
                    needsMoreUpdates = true;
                    return;
                }
            }

            _Weight = target;
            needsMoreUpdates = false;

            if (target == 0)
            {
                Stop();
            }
            else
            {
                FadeSpeed = 0;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Inverse Kinematics
        /************************************************************************************************************************/

#if !UNITY_2018_1_OR_NEWER
        private const string IKNotSupported = "ApplyAnimatorIK is not supported by this version of Unity." +
            " Please upgrade to Unity 2018.1 or newer."
            ;
#endif

        /// <summary>
        /// Determines whether <c>OnAnimatorIK(int layerIndex)</c> will be called on the animated object while this
        /// node is active. The initial value is determined by <see cref="AnimancerPlayable.DefaultApplyAnimatorIK"/>.
        /// <para></para>
        /// This is equivalent to the "IK Pass" toggle in Animator Controller layers.
        /// <para></para>
        /// It requires Unity 2018.1 or newer, however 2018.3 or newer is recommended because a bug in earlier versions
        /// of the Playables API caused this value to only take effect while a state was at <see cref="Weight"/> == 1
        /// which meant that IK would not work while fading between animations.
        /// </summary>
        public virtual bool ApplyAnimatorIK
        {
#if UNITY_2018_1_OR_NEWER
            get
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    if (state.ApplyAnimatorIK)
                        return true;
                }

                return false;
            }
            set
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    state.ApplyAnimatorIK = value;
                }
            }
#else
            get { return false; }
            set { throw new NotSupportedException(IKNotSupported); }
#endif
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Determines whether this node is applying IK to the character's feet.
        /// The initial value is determined by <see cref="AnimancerPlayable.DefaultApplyFootIK"/>.
        /// <para></para>
        /// This is equivalent to the "Foot IK" toggle in Animator Controller states.
        /// </summary>
        public virtual bool ApplyFootIK
        {
            get
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    if (state.ApplyFootIK)
                        return true;
                }

                return false;
            }
            set
            {
                for (int i = ChildCount - 1; i >= 0; i--)
                {
                    var state = GetChild(i);
                    if (state == null)
                        continue;

                    state.ApplyFootIK = value;
                }
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Speed
        /************************************************************************************************************************/

        private float _Speed = 1;

        /// <summary>
        /// How fast the <see cref="AnimancerState.Time"/> is advancing every frame.
        /// <para></para>
        /// 1 is the normal speed.
        /// <para></para>
        /// A negative value will play the animation backwards.
        /// <para></para>
        /// Animancer Lite does not allow this value to be changed in a runtime build.
        /// </summary>
        ///
        /// <example>
        /// <code>
        /// void PlayAnimation(AnimancerComponent animancer, AnimationClip clip)
        /// {
        ///     var state = animancer.Play(clip);
        ///
        ///     state.Speed = 1;// Normal speed.
        ///     state.Speed = 2;// Double speed.
        ///     state.Speed = 0.5f;// Half speed.
        ///     state.Speed = -1;// Normal speed playing backwards.
        /// }
        /// </code>
        /// </example>
        public float Speed
        {
            get { return _Speed; }
            set
            {
#if UNITY_ASSERTIONS
                if (!value.IsFinite())
                    throw new ArgumentOutOfRangeException("value", value, "Speed must be finite");
#endif
                _Speed = value;

                if (_Playable.IsValid())
                    _Playable.SetSpeed(value);
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The <see cref="Speed"/> of each of this node's parents down the hierarchy, including the root
        /// <see cref="AnimancerPlayable"/>.
        /// </summary>
        private float ParentEffectiveSpeed
        {
            get
            {
                var speed = Root.Speed;

                var parent = Parent;
                while (parent != null)
                {
                    speed *= parent.Speed;
                    parent = parent.Parent;
                }

                return speed;
            }
        }

        /// <summary>
        /// The <see cref="Speed"/> of this node multiplied by the <see cref="Speed"/> of each of its parents down the
        /// hierarchy (including the root <see cref="AnimancerPlayable"/>) to determine the actual speed its output is
        /// being played at.
        /// </summary>
        public float EffectiveSpeed
        {
            get
            {
                return Speed * ParentEffectiveSpeed;
            }
            set
            {
                Speed = value / ParentEffectiveSpeed;
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
        #region Descriptions
        /************************************************************************************************************************/

        /// <summary>Returns a detailed descrption of the current details of this node.</summary>
        public string GetDescription(int maxChildDepth = 10, string delimiter = "\n")
        {
            var text = new StringBuilder();
            AppendDescription(text, maxChildDepth, delimiter);
            return text.ToString();
        }

        /************************************************************************************************************************/

        /// <summary>Appends a detailed descrption of the current details of this node.</summary>
        public void AppendDescription(StringBuilder text, int maxChildDepth = 10, string delimiter = "\n")
        {
            if (text.Length > 0)
                text.Append(delimiter);

            text.Append(ToString());

            delimiter += "    ";
            AppendDetails(text, delimiter);

            if (maxChildDepth-- > 0 && ChildCount > 0)
            {
                text.Append(delimiter).Append("ChildCount: ").Append(ChildCount);
                var indentedDelimiter = delimiter + "    ";

                var i = 0;
                foreach (var childState in this)
                {
                    text.Append(delimiter).Append('[').Append(i++).Append(']');
                    childState.AppendDescription(text, maxChildDepth, indentedDelimiter);
                }
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Called by <see cref="AppendDescription"/> to append the details of this node.
        /// </summary>
        protected virtual void AppendDetails(StringBuilder text, string delimiter)
        {
            text.Append(delimiter).Append("Playable: ");
            if (_Playable.IsValid())
                text.Append(_Playable.GetPlayableType());
            else
                text.Append("Invalid");

            text.Append(delimiter).Append("Index: ").Append(Index);

            var realSpeed = _Playable.IsValid() ? _Playable.GetSpeed() : _Speed;
            if (realSpeed == _Speed)
            {
                text.Append(delimiter).Append("Speed: ").Append(_Speed);
            }
            else
            {
                text.Append(delimiter).Append("Speed (Real): ").Append(_Speed)
                    .Append(" (").Append(realSpeed).Append(')');
            }

            text.Append(delimiter).Append("Weight: ").Append(Weight);

            if (Weight != TargetWeight)
            {
                text.Append(delimiter).Append("TargetWeight: ").Append(TargetWeight);
                text.Append(delimiter).Append("FadeSpeed: ").Append(FadeSpeed);
            }
        }

        /************************************************************************************************************************/
        #endregion
        /************************************************************************************************************************/
    }
}

