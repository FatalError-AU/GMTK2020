// Animancer // Copyright 2020 Kybernetik //

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Animancer
{
    /// <summary>
    /// Enforces various rules throughout the system, most of which are compiled out if UNITY_ASSERTIONS is not defined
    /// (by default, it is defined in the Unity Editor and in Development Builds).
    /// </summary>
    public static class Validate
    {
        /************************************************************************************************************************/

        /// <summary>[Assert]
        /// Throws if the `clip` is marked as <see cref="AnimationClip.legacy"/>.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        [System.Diagnostics.Conditional(Strings.Assert)]
        public static void NotLegacy(AnimationClip clip)
        {
#if UNITY_ASSERTIONS
            if (clip.legacy)
                throw new ArgumentException("Legacy clip '" + clip + "' cannot be used by Animancer." +
                    " Set the legacy property to false before using this clip." +
                    " If it was imported as part of a model then the model's Rig type must be changed to Humanoid or Generic." +
                    " Otherwise you can use the 'Toggle Legacy' function in the clip's context menu" +
                    " (via the cog icon in the top right of its Inspector).");
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert]
        /// Throws if the <see cref="AnimancerNode.Root"/> is not the `root`.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        [System.Diagnostics.Conditional(Strings.Assert)]
        public static void Root(AnimancerNode node, AnimancerPlayable root)
        {
#if UNITY_ASSERTIONS
            if (node == null)
                throw new ArgumentNullException("node");

            if (node.Root != root)
                throw new ArgumentException("AnimancerNode.Root mismatch:" +
                    " cannot use a node in an AnimancerPlayable that is not its Root: " + node.GetDescription());
#endif
        }

        /************************************************************************************************************************/

        /// <summary>[Assert]
        /// Throws if the `state` was not actually assigned to its specified <see cref="AnimancerNode.Index"/> in
        /// the `states`.
        /// </summary>
        /// <exception cref="InvalidOperationException"/>
        /// <exception cref="IndexOutOfRangeException">
        /// Thrown if the <see cref="AnimancerNode.Index"/> is larger than the number of `states`.
        /// </exception>
        [System.Diagnostics.Conditional(Strings.Assert)]
        public static void RemoveChild(AnimancerState state, IList<AnimancerState> states)
        {
#if UNITY_ASSERTIONS
            var index = state.Index;

            if (index < 0)
                throw new InvalidOperationException(
                    "Cannot remove a child state that did not have an Index assigned");

            if (index > states.Count)
                throw new IndexOutOfRangeException(
                    "AnimancerState.Index (" + state.Index + ") is outside the collection of states (count " + states.Count + ")");

            if (states[state.Index] != state)
                throw new InvalidOperationException(
                    "Cannot remove a child state that was not actually connected to its port on " + state.Parent + ":" +
                    "\n    Port: " + state.Index +
                    "\n    Connected Child: " + AnimancerUtilities.ToStringOrNull(states[state.Index]) +
                    "\n    Disconnecting Child: " + AnimancerUtilities.ToStringOrNull(state));
#endif
        }

        /************************************************************************************************************************/
    }
}

