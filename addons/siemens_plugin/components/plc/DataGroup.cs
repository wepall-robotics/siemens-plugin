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
    public PlcNode? ParentPlcNode => GetParent() as PlcNode;
    public Plc? ParentPlc => ParentPlcNode?.Data;

    private bool _isRegistered;

    public void ReadAll()
    {
        if (ParentPlc == null)
        {
            GD.PrintErr("PLC is null, cannot read data items.");
            return;
        }
        // var groupedItems = Items
        //         .Where(item => item.Mode != DataItem.AccessMode.Write)
        //         .GroupBy(item => new { item.DataType, item.DB })
        //         .ToList();

        // foreach (var group in groupedItems)
        // {
        //     try
        //     {
        //         // Leer cada grupo por separado
        //         ParentPlc.ReadMultipleVars(group.ToList());
        //         // Actualizar valores después de cada lectura grupal
        //         group.ToList().ForEach(item => item.UpdateGDValue());
        //     }
        //     catch (Exception ex)
        //     {
        //         GD.PrintErr($"Error reading group: {ex.Message}");
        //     }
        // }
        List<DataItem> s7Items = Items
            .Where(item => item.Mode != DataItem.AccessMode.Write)
            .ToList();

        ParentPlc.ReadMultipleVars(s7Items);
        s7Items.ForEach(item => item.UpdateGDValue());
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
        {
            UpdateItemList();
        }

        if (what == NotificationParented)
        {
            if (GetParent() is PlcNode plcNode)
            {
                if (!_isRegistered)
                {
                    plcNode.Data?.RegisterAction(this);
                    _isRegistered = true;
                }
            }
        }
        else if (what == NotificationUnparented)
        {
            if (_isRegistered && ParentPlcNode != null)
            {
                ParentPlcNode.Data?.RemoveAction(this);
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
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error executing action: {ex.Message}");
        }
    }


    // public void WriteAll(Plc plc)
    // {
    //     foreach (var item in Items)
    //     {
    //         var value = item.GDValue;
    //         plc.Write(item.DataType,item.DB, item.StartByteAdr, value);
    //     }
    // }

}