// using Godot;
// using S7.Net;
// using System;

// // Main class with tool support
// [Tool]
// [GlobalClass]
// [Icon("uid://dj2skfrj122mt")]
// public partial class PlcNode : Node
// {
//     #region Properties
//     [Export]
//     public Plc Plc { get; set; } = new Plc();

//     [Export]
//     public string Ip
//     {
//         get => Plc.IP;
//         set { Plc.IP = value;UpdateConfigurationWarnings(); }
//     }

//     [Export]
//     public CpuType Cpu
//     {
//         get => Plc.CPU;
//         set { Plc.CPU = value;}
//     }

//     [Export]
//     public Int16 Rack
//     {
//         get => Plc.Rack;
//         set { Plc.Rack = value; }
//     }

//     [Export]
//     public Int16 Slot
//     {
//         get => Plc.Slot;
//         set { Plc.Slot = value; }
//     }

//     [ExportCategory("PlcCommands")]
//     [Export]
//     public Plc.Status CurrentStatus
//     {
//         get => Plc.CurrentStatus;
//         set
//         {
//             Plc.CurrentStatus = value;
//             UpdateConfigurationWarnings();
//         }
//     }

//     [Export]
//     public bool ValidConfiguration
//     {
//         get => Plc.ValidConfiguration;
//         set { Plc.ValidConfiguration = value; }
//     }

//     [Export]
//     public bool IsOnline
//     {
//         get => Plc.IsOnline;
//         set { Plc.IsOnline = value; }
//     }

//     #endregion

//     #region Lifecycle Methods
//     public override void _Ready()
//     {
//         if (Engine.IsEditorHint() && Plc == null)
//             Plc = new Plc();
//     }

//     public override void _Process(double delta)
//     {
//         if (!Engine.IsEditorHint())
//         {
//             if (!Plc.IsConnected)
//             {
//                 GD.Print("Connecting to PLC...");
//                 Plc.Open();
//             }

//             if (Plc != null && Plc.IsOnline && Plc.CurrentStatus == Plc.Status.Connected)
//             {
//                 Plc.ProcessRegisteredActionsPublic();
                
//             }
//         }
//     }
//     #endregion

//     #region Inspector Integration
//     public override string[] _GetConfigurationWarnings()
//     {
//         var warnings = new System.Collections.Generic.List<string>();
//         ValidConfiguration = false;

//         if (string.IsNullOrEmpty(Ip) || !Plc.ValidateIP(Ip))
//         {
//             warnings.Add("Invalid IP address:\n1. Select this node in the Scene tree.\n" +
//                         "2. In the Inspector, see Plc group.\n" +
//                         "3. Enter a valid IP address in the field.");
//         }
//         else
//         {
//             var plcs = GetTree().GetNodesInGroup("Plcs");
//             var counter = 0;

//             foreach (var node in plcs)
//             {
//                 if (node is PlcNode plcNode && plcNode.Ip == Ip)
//                     counter++;
//             }

//             if (counter > 1)
//                 warnings.Add("There is another Plc with this IP.");
//         }

//         ValidConfiguration = warnings.Count == 0;
//         return warnings.ToArray();
//     }

//     public override void _ValidateProperty(Godot.Collections.Dictionary property)
//     {
//         var propertyName = (string)property["name"];
//         var usage = (PropertyUsageFlags)(long)property["usage"];

//         if (propertyName == "current_status" || propertyName == "GhostProp")
//             usage |= PropertyUsageFlags.NoEditor;

//         property["usage"] = (long)usage;
//     }
//     #endregion
// }
