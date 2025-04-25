// using Godot;
// using S7.Net;
// using S7.Net.Types;
// using System;

// [Tool]
// [Icon("uid://cw2vaurpx2nc2")]
// [GlobalClass]
// public partial class TimerItem : DataItem
// {
//     [Signal]
//     public delegate void ValueChangedEventHandler(int newValue); // milisegundos

//     [Export]
//     public override Node VisualComponent
//     {
//         get => _visualComponent;
//         set
//         {
//             _visualComponent = value;
//             NotifyPropertyListChanged();
//         }
//     }

//     [Export]
//     public override string VisualProperty
//     {
//         get => _visualProperty;
//         set
//         {
//             if (_visualProperty == value) return;
//             _visualProperty = value;
//         }
//     }

//     private Node _visualComponent;
//     private string _visualProperty;
//     private int _gdValue; // S5TIME convertido a milisegundos

//     [Export]
//     public int GDValue
//     {
//         get => _gdValue;
//         set
//         {
//             if (_gdValue != value)
//             {
//                 _gdValue = value;
//                 Value = value;
//                 EmitSignal(SignalName.ValueChanged, value);
//             }
//         }
//     }

//     private void UpdateVisualComponent(int value)
//     {
//         if (VisualComponent != null && !string.IsNullOrEmpty(VisualProperty))
//         {
//             VisualComponent.Set(VisualProperty, value);
//         }
//     }

//     public override void UpdateGDValue()
//     {
//         try
//         {
//             // S7.NetPlus devuelve el timer como int (milisegundos)
//             if (Value is int msValue)
//             {
//                 _gdValue = msValue;
//                 UpdateVisualComponent(_gdValue);
//             }
//         }
//         catch (Exception ex)
//         {
//             GD.PrintErr($"Error updating GDValue: {ex.Message}");
//         }
//     }

//     public override void UpdateValue()
//     {
//         try
//         {
//             if (VisualComponent != null && !string.IsNullOrEmpty(VisualProperty))
//             {
//                 var value = VisualComponent.Get(VisualProperty);
//                 GDValue = value.AsInt32();
//             }
//         }
//         catch (Exception ex)
//         {
//             GD.PrintErr($"Error updating Value: {ex.Message}");
//         }
//     }

//     public TimerItem()
//     {
//         VarType = VarType.Timer;
//         DataType = DataType.Timer;
//         Count = 1;
//         Value = 0; // milisegundos
//     }
// }
