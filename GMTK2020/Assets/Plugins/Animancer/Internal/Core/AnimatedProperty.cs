// Animancer // Copyright 2020 Kybernetik //

using System;
using UnityEngine;
using UnityEngine.Playables;

#if UNITY_2019_1_OR_NEWER
using UnityEngine.Animations;
using UnityEngine.Experimental.Animations;
#endif

namespace Animancer
{
    /// <summary>
    /// A wrapper which allows access to the value of a property that is controlled by animations.
    /// <para></para>
    /// Requires Unity 2019.1+.
    /// </summary>
#if !UNITY_2019_1_OR_NEWER
    [Obsolete(OldUnityError)]
#endif
    public sealed class AnimatedProperty
    {
        /************************************************************************************************************************/

        /// <summary>The type of property being wrapped.</summary>
        public enum Type
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member.
            Bool,
            Float,
            Int,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member.
        }

        /************************************************************************************************************************/

#if UNITY_2019_1_OR_NEWER
        private PlayableGraph _Graph;
        private AnimationScriptPlayable _Playable;
        private AnimationPlayableOutput _Output;
#endif

        /************************************************************************************************************************/

        /// <summary>The value of the target property when using <see cref="Type.Bool"/>.</summary>
        /// <exception cref="ArgumentException">Thrown if not using <see cref="Type.Bool"/>.</exception>
        public bool BoolValue
        {
#if UNITY_2019_1_OR_NEWER
            get { return _Playable.GetJobData<BoolJob>().value; }
#else
            get { throw new NotSupportedException(OldUnityError); }
#endif
        }

        /// <summary>The value of the target property when using <see cref="Type.Float"/>.</summary>
        /// <exception cref="ArgumentException">Thrown if not using <see cref="Type.Float"/>.</exception>
        public float FloatValue
        {
#if UNITY_2019_1_OR_NEWER
            get { return _Playable.GetJobData<FloatJob>().value; }
#else
            get { throw new NotSupportedException(OldUnityError); }
#endif
        }

        /// <summary>The value of the target property when using <see cref="Type.Int"/>.</summary>
        /// <exception cref="ArgumentException">Thrown if not using <see cref="Type.Int"/>.</exception>
        public int IntValue
        {
#if UNITY_2019_1_OR_NEWER
            get { return _Playable.GetJobData<IntJob>().value; }
#else
            get { throw new NotSupportedException(OldUnityError); }
#endif
        }

        /************************************************************************************************************************/

        /// <summary>The value of the target property when using <see cref="Type.Bool"/>.</summary>
        /// <exception cref="ArgumentException">Thrown if not using <see cref="Type.Bool"/>.</exception>
        public static implicit operator bool(AnimatedProperty property)
        {
            return property.BoolValue;
        }

        /// <summary>The value of the target property when using <see cref="Type.Float"/>.</summary>
        /// <exception cref="ArgumentException">Thrown if not using <see cref="Type.Float"/>.</exception>
        public static implicit operator float(AnimatedProperty property)
        {
            return property.FloatValue;
        }

        /// <summary>The value of the target property when using <see cref="Type.Int"/>.</summary>
        /// <exception cref="ArgumentException">Thrown if not using <see cref="Type.Int"/>.</exception>
        public static implicit operator int(AnimatedProperty property)
        {
            return property.IntValue;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="AnimatedProperty"/> to wrap a property with the specified `name`.</summary>
        public AnimatedProperty(IAnimancerComponent animancer, Type type, string name)
            : this(animancer.Playable._Graph, animancer.Animator, type, name) { }

        /// <summary>Creates a new <see cref="AnimatedProperty"/> to wrap a property with the specified `name`.</summary>
        public AnimatedProperty(AnimancerPlayable animancer, Type type, string name)
            : this(animancer._Graph, animancer.Component.Animator, type, name) { }

        /// <summary>Creates a new <see cref="AnimatedProperty"/> to wrap a property with the specified `name`.</summary>
        public AnimatedProperty(PlayableGraph graph, Animator animator, Type type, string name)
        {
#if UNITY_2019_1_OR_NEWER
            var property = animator.BindStreamProperty(animator.transform, typeof(Animator), name);

            switch (type)
            {
                case Type.Bool:
                    _Playable = AnimationScriptPlayable.Create(graph, new BoolJob() { property = property });
                    break;

                case Type.Float:
                    _Playable = AnimationScriptPlayable.Create(graph, new FloatJob() { property = property });
                    break;

                case Type.Int:
                    _Playable = AnimationScriptPlayable.Create(graph, new IntJob() { property = property });
                    break;

                default:
                    throw new ArgumentException("Unsupported AnimatedProperty.Type: " + type, "type");
            }

            _Graph = graph;
            _Playable.Pause();

            _Output = AnimationPlayableOutput.Create(graph, name, animator);
            _Output.SetSourcePlayable(_Playable);
            _Output.SetAnimationStreamSource(AnimationStreamSource.PreviousInputs);
#endif
        }

        /************************************************************************************************************************/

        /// <summary>Cleans up this <see cref="AnimatedProperty"/>.</summary>
        public void Destroy()
        {
#if UNITY_2019_1_OR_NEWER
            if (!_Output.IsOutputValid())
                return;

            _Graph.DestroyOutput(_Output);
            _Graph.DestroyPlayable(_Playable);
#endif
        }

        /************************************************************************************************************************/
#if UNITY_2019_1_OR_NEWER
        /************************************************************************************************************************/

        private struct BoolJob : IAnimationJob
        {
            public PropertyStreamHandle property;
            public bool value;

            public void ProcessRootMotion(AnimationStream stream) { }

            public void ProcessAnimation(AnimationStream stream)
            {
                value = property.GetBool(stream);
            }
        }

        /************************************************************************************************************************/

        private struct FloatJob : IAnimationJob
        {
            public PropertyStreamHandle property;
            public float value;

            public void ProcessRootMotion(AnimationStream stream) { }

            public void ProcessAnimation(AnimationStream stream)
            {
                value = property.GetFloat(stream);
            }
        }

        /************************************************************************************************************************/

        private struct IntJob : IAnimationJob
        {
            public PropertyStreamHandle property;
            public int value;

            public void ProcessRootMotion(AnimationStream stream) { }

            public void ProcessAnimation(AnimationStream stream)
            {
                value = property.GetInt(stream);
            }
        }
    
        /************************************************************************************************************************/
#else
        /************************************************************************************************************************/

        private const string OldUnityError = "AnimatedProperty requires the Animation Job system implemented in Unity 2019.1.";

        /************************************************************************************************************************/
#endif
        /************************************************************************************************************************/
    }
}

