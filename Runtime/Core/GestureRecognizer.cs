using System;
using UnityEngine;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// Enumerator that represent the current state a gesture recognizer is in.
    /// <para />
    /// Gesture recognizers recognize a discrete event such as a tap or a swipe but don’t report changes within
    /// the gesture.
    /// In other words, discrete gestures don’t transition through the Began and Changed
    /// states and they can’t fail or be canceled.
    /// </summary>
    public enum GestureRecognizerState
    {
        /// <summary>
        /// The current touch configuration is not recognized but still possible.
        /// </summary>
        /// <remarks>
        /// This state is applicable to discrete gestures only.
        /// </remarks>
        Possible,
        
        /// <summary>
        /// The gesture recognizer has received touches recognized as a continuous gesture.
        /// </summary>
        /// <remarks>
        /// This state is applicable to continuous gestures only.
        /// </remarks>
        Began,
        
        /// <summary>
        /// The gesture recognizer has received touches recognized as a change to a continuous gesture.
        /// </summary>
        /// <remarks>
        /// This state is applicable to continuous gestures only.
        /// </remarks>
        Changed,
        
        /// <summary>
        /// The gesture has ended/failed/been cancelled.
        /// </summary>
        /// <remarks>
        /// This state is applicable to continuous gestures only.
        /// </remarks>
        Ended,
        
        /// <summary>
        /// The gesture recognizer has received touches resulting in the cancellation of a continuous gesture.
        /// </summary>
        /// <remarks>
        /// This state is applicable to any kind of gesture.
        /// </remarks>
        Canceled,
        
        /// <summary>
        /// The gesture recognizer has received a multi-touch sequence that it can’t recognize as its gesture.
        /// </summary>
        /// <remarks>
        /// This state is applicable to discrete gestures only.
        /// </remarks>
        Failed,
        
        /// <summary>
        /// The current touch configuration is recognized.
        /// </summary>
        /// <remarks>
        /// This state is applicable to discrete gestures only.
        /// </remarks>
        Recognized,
    }
    
    /// <summary>
    /// Interface for gesture recognizers.
    /// <para />
    /// A gesture recognizer decouples the logic for recognizing a sequence of touches (or other input) and acting on
    /// that recognition.
    /// When one of these objects recognizes a common gesture or, in some cases, a change in the gesture,
    /// it sends an action message to each designated target object.
    /// </summary>
    public interface IGestureRecognizer
    {
        /// <summary>
        /// The current state of the gesture recognizer.
        /// <para />
        /// The possible states a gesture recognizer can be in are represented by the values of type
        /// <see cref="GestureRecognizerState"/>.
        /// Some of these states aren’t applicable to discrete gestures.
        /// </summary>
        GestureRecognizerState state { get; }
        
        /// <summary>
        /// Whether the gesture recognizer state or outgoing value has changed this frame.
        /// </summary>
        bool hasChangedThisFrame { get; }
        
        /// <summary>
        /// Main method to recognize a gesture.
        /// <para/>
        /// A concrete implementation of a gesture recognizer should override this method to recognize a gesture.
        /// The implementation should also assume that this method will be called every frame.
        /// </summary>
        /// <remarks>
        /// Sometimes, touch inputs can be lost from a frame to another (no <see cref="AppUITouch"/> event with an
        /// <see cref="TouchPhase.Ended"/> or <see cref="TouchPhase.Canceled"/> phase will be sent).
        /// The concrete implementation must aknowledge this and reset its state accordingly.
        /// </remarks>
        /// <param name="appuiTouches"> The current touch inputs for the current frame.</param>
        void Recognize(AppUITouch[] appuiTouches);

        /// <summary>
        /// Resets the gesture recognizer to its initial state.
        /// </summary>
        void Reset();
    }
    
    /// <summary>
    /// The base class for concrete gesture recognizers.
    /// <para />
    /// A gesture recognizer decouples the logic for recognizing a sequence of touches (or other input) and acting on
    /// that recognition.
    /// When one of these objects recognizes a common gesture or, in some cases, a change in the gesture,
    /// it sends an action message to each designated target object.
    /// </summary>
    /// <typeparam name="T"> The type of value the gesture recognizer will output when the gesture is recognized.
    /// Very often this value will be of type <see cref="Vector2"/> or <see cref="float"/>.
    /// </typeparam>
    public abstract class GestureRecognizer<T> : IGestureRecognizer
    {
        GestureRecognizerState m_State = GestureRecognizerState.Possible;

        /// <inheritdoc />
        public GestureRecognizerState state
        {
            get => m_State;
            set
            {
                if (value != m_State)
                {
                    m_State = value;
                    hasChangedThisFrame = true;
                }
            }
        }
        
        /// <inheritdoc />
        public bool hasChangedThisFrame { get; protected set; }
        
        /// <inheritdoc />
        public virtual void Recognize(AppUITouch[] appuiTouches)
        {
            hasChangedThisFrame = false;
        }
        
        /// <summary>
        /// The current value of type <typeparamref name="T"/> for the gesture recognizer.
        /// <para/>
        /// Concrete implementations of gesture recognizers should set this value when the gesture is recognized.
        /// Very often this value will be of type <see cref="Vector2"/> or <see cref="float"/>.
        /// </summary>
        public T value { get; protected set; }
        
        /// <summary>
        /// Resets the gesture recognizer to its initial state.
        /// </summary>
        public virtual void Reset()
        {
            m_State = GestureRecognizerState.Possible;
            hasChangedThisFrame = false;
        }
    }
}
