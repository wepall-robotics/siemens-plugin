using Godot;
using Godot.Collections;
using S7.Net;
using System;
using System.Collections.Generic;

[Tool]
[Icon("uid://dj2skfrj122mt")]
[GlobalClass]
public partial class PlcNode : Node
{
    public enum Status
    {
        Connected,
        Disconnected,
        Unknown
    }

    private Plc _data;
    private Status _currentStatus = Status.Unknown;
    private bool _validConfiguration;

    [ExportCategory("PLC Configuration")]
    [Export]
    public Plc Data
    {
        get => _data;
        set
        {
            if (_data != null)
            {
                _data.Changed -= UpdateConfigurationWarnings;
            }

            _data = value;

            if (_data != null)
            {
                _data.Changed += UpdateConfigurationWarnings;
            }

            UpdateConfigurationWarnings();
        }
    }


    [ExportCategory("PlcCommands")]
    [Export]
    public Status CurrentStatus
    {
        get => _currentStatus;
        set
        {
            _currentStatus = value;
            UpdateConfigurationWarnings();
        }
    }

    // Ghost property para organización del inspector
    private bool GhostProp { get; set; }
    [Export]
    public bool ValidConfiguration { get => _validConfiguration; set => _validConfiguration = value; }

    [Export]
    public DataGroup DataGroup { get; set; }
    public override string[] _GetConfigurationWarnings()
    {
        List<string> warnings = [];
        _validConfiguration = false;

        if (Data == null)
        {
            warnings.Add("PLC configuration required:\n1. Select this node in the Scene tree.\n" +
                        "2. In the Inspector, find the data property.\n" +
                        "3. Assign or create a new PLC configuration.");
        }
        else
        {
            if (string.IsNullOrEmpty(Data.IP) || !Plc.ValidateIP(Data.IP))
            {
                warnings.Add("Invalid IP address:\n1. Select this node in the Scene tree.\n" +
                            "2. In the Inspector, expand the data property.\n" +
                            "3. Enter a valid IP address in the field.");
            }
            else
            {
                var plcNodes = GetTree().GetNodesInGroup("PlcNodes");
                int counter = 0;

                foreach (var node in plcNodes)
                {
                    if (node is PlcNode plc && plc.Data.IP == Data.IP)
                    {
                        counter++;
                    }
                }

                if (counter > 1)
                {
                    warnings.Add("There is another PLC with this IP.");
                }
            }
        }

        _validConfiguration = warnings.Count == 0;
        return warnings.ToArray();
    }

    /// <summary>
    /// Called when the Inspector needs to validate a property.
    /// This implementation makes the <see cref="CurrentStatus"/> property read-only and hides the ghost property.
    /// </summary>
    /// <param name="property">Dictionary containing the property name and value.</param>
    /// 
    public override void _ValidateProperty(Godot.Collections.Dictionary property)
    {
        // Acceder al nombre de la propiedad usando la key del diccionario
        StringName propertyName = property["name"].AsStringName();

        if (propertyName == nameof(CurrentStatus))
        {
            // Modificar el flag de uso (casting necesario)
            PropertyUsageFlags usage = property["usage"].As<PropertyUsageFlags>();
            usage = PropertyUsageFlags.NoEditor;
            property["usage"] = (int)usage;
        }
        else if (propertyName == nameof(GhostProp))
        {
            PropertyUsageFlags usage = property["usage"].As<PropertyUsageFlags>();
            usage = PropertyUsageFlags.NoEditor;
            property["usage"] = (int)usage;
        }
    }
}
