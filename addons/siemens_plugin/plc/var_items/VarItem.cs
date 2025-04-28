using Godot;
using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Represents a base class for data items that interact with the PLC and visual components in the Godot engine.
    /// Provides properties and methods for managing access modes and synchronization with visual components.
    /// </summary>
    public partial class DataItem : Node
    {
        #region Enums

        /// <summary>
        /// Defines the access mode for the data item.
        /// </summary>
        public enum AccessMode
        {
            /// <summary>
            /// Indicates that the data item is read from the PLC.
            /// </summary>
            ReadFromPlc,

            /// <summary>
            /// Indicates that the data item is written to the PLC.
            /// </summary>
            WriteToPlc
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the access mode for the data item.
        /// </summary>
        [Export]
        public AccessMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the visual component associated with this data item.
        /// </summary>
        public virtual Node VisualComponent { get; set; }

        /// <summary>
        /// Gets or sets the name of the visual property to bind to.
        /// </summary>
        public virtual string VisualProperty { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Updates the Godot value based on the PLC value.
        /// This method should be overridden in derived classes to implement specific behavior.
        /// </summary>
        public virtual void UpdateGDValue() { }

        /// <summary>
        /// Updates the PLC value based on the Godot value.
        /// This method should be overridden in derived classes to implement specific behavior.
        /// </summary>
        public virtual void UpdateValue() { }

        #endregion
    }
}