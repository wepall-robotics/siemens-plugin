using Godot;
using System;

namespace S7.Net.Types
{
    public partial class DataItem : Node
    {
        public enum AccessMode { ReadFromPlc, WriteToPlc }
        [Export]
        public AccessMode Mode { get; set; }

        public virtual Node VisualComponent { get; set; }
        public virtual string VisualProperty { get; set; }
        public virtual void UpdateGDValue() { }
        public virtual void UpdateValue() { }    
    }
}