#if ENABLE_VALUEFIELD_INTERFACE
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.AppUI.Core
{
    /// <summary>
    /// Phases of a drag operation.
    /// </summary>
    public enum DragPhase
    {
        /// <summary>
        /// The drag operation has started.
        /// </summary>
        Started,

        /// <summary>
        /// The drag operation is ongoing.
        /// </summary>
        Dragging,

        /// <summary>
        /// The drag operation has ended.
        /// </summary>
        Ended
    }

    /// <summary>
    /// Context for drag operations on input labels.
    /// </summary>
    /// <param name="phase">The current phase of the drag operation.</param>
    /// <param name="delta">The delta of the device since the last drag event.</param>
    /// <param name="speed">The speed modifier for the drag operation.</param>
    public record DragContext(DragPhase phase, Vector3 delta, DeltaSpeed speed) : IContext
    {
        /// <summary>
        /// The current phase of the drag operation.
        /// </summary>
        public DragPhase phase { get; set; } = phase;

        /// <summary>
        /// The delta in pixels since the last drag event.
        /// </summary>
        public Vector3 delta { get; } = delta;


        /// <summary>
        /// THe speed modifier for the drag operation.
        /// </summary>
        public DeltaSpeed speed { get; } = speed;
    }
}
#endif
