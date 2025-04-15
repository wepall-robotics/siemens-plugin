using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using S7.Net;
using S7.Net.Types;

[Tool]
[GlobalClass]
public partial class DataGroup : Node, IPlcAction
{
    [Export]
    public Godot.Collections.Array<DataItem> Items { get; set; } = new();
    public Plc? ParentPlc => GetParent() as Plc;

    private bool _isRegistered;
    // Class field to store items
    private List<DataItem> _itemsToProcess;

    public void ReadAll()
    {
        if (ParentPlc == null)
        {
            GD.PrintErr("PLC is null, cannot read data items.");
            return;
        }
        List<DataItem> s7Items = Items
            .Where(item => item.Mode != DataItem.AccessMode.Write)
            .ToList();

        if (s7Items.Count == 0) return;

        ParentPlc.ReadMultipleVars(s7Items);
        s7Items.ForEach(item => item.UpdateGDValue());
    }

    public void WriteAll()
    {
        if (ParentPlc == null)
        {
            GD.PrintErr("PLC is null, cannot write data items.");
            return;
        }

        // Store items in a class field instead of passing as parameter
        _itemsToProcess = Items
            .Where(item => item.Mode != DataItem.AccessMode.Read)
            .ToList();

        if (_itemsToProcess.Count == 0) return;

        // Call the method without parameters
        CallDeferred(nameof(DeferredWriteAll));
    }

    // Public method that uses the class field
    public void DeferredWriteAll()
    {
        foreach (var item in _itemsToProcess)
        {
            if (item.VisualComponent != null && !string.IsNullOrEmpty(item.VisualProperty))
                item.UpdateValue();
        }

        ParentPlc.Write(_itemsToProcess.ToArray());
    }

    private void UpdateItemList()
    {
        Items.Clear();
        foreach (Node child in GetChildren())
        {
            if (child is DataItem item)
                if (!Items.Contains(item))
                    Items.Add(item);
        }
    }

    public override void _Ready()
    {
        if (ParentPlc != null && !_isRegistered)
        {
            ParentPlc.RegisterAction(this);
            _isRegistered = true;
        }
    }

    public override void _Notification(int what)
    {
        base._Notification(what);

        if (what == NotificationChildOrderChanged)
            UpdateItemList();

        if (what == NotificationParented)
        {
            if (GetParent() is Plc plc)
            {
                if (!_isRegistered)
                {
                    plc.RegisterAction(this);
                    _isRegistered = true;
                }
            }
        }
        else if (what == NotificationUnparented)
        {
            if (_isRegistered && ParentPlc != null)
            {
                ParentPlc.RemoveAction(this);
                _isRegistered = false;
            }
        }
    }

    public override void _ExitTree()
    {
        if (ParentPlc != null && _isRegistered)
        {
            ParentPlc.RemoveAction(this);
            _isRegistered = false;
        }
    }

    public void Execute()
    {
        if (ParentPlc == null)
        {
            GD.PrintErr("PLC instance is null. Cannot execute action.");
            return;
        }

        try
        {
            ReadAll();
            WriteAll();
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error executing action: {ex.Message}");
        }
    }
}