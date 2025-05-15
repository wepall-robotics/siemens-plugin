using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using S7.Net;
using S7.Net.Types;

[Tool]
[GlobalClass]
[Icon("uid://2lx0drx4kc8g")]
/// <summary>
/// Represents a group of data items that interact with a parent PLC node.
/// This class provides functionality to read and write data items to the PLC,
/// manage the lifecycle of the node within the Godot engine, and handle
/// notifications related to child and parent changes.
/// </summary>
public partial class GroupData : Node, IPlcAction
{
    #region Fields

    /// <summary>
    /// Indicates whether this action is registered with the parent PLC.
    /// </summary>
    private bool _isRegistered;

    /// <summary>
    /// Temporary storage for items to process during write operations.
    /// </summary>
    private List<DataItem> _itemsToProcess;

    #endregion

    #region Properties

    /// <summary>
    /// List of data items managed by this group.
    /// </summary>
    [Export]
    public Godot.Collections.Array<DataItem> Items { get; set; } = new();
    /// <summary>
    /// Reference to the parent PLC node.
    /// </summary>
    public Plc? ParentPlc => GetParent() as Plc;
    #endregion

    #region Public Methods
    /// <summary>
    /// Executes the action by reading and writing data items.
    /// </summary>
    public void Execute()
    {
        if (ParentPlc == null || !ParentPlc.IsOnline)
        {
            GD.PrintErr("Plc instance is null or is Offline. Cannot execute action.");
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
    /// <summary>
    /// Reads all data items from the PLC.
    /// </summary>
    public void ReadAll()
    {
        if (ParentPlc == null || !IsInstanceValid(ParentPlc))
        {
            GD.PrintErr("Plc is null, cannot read data items.");
            return;
        }

        List<DataItem> s7Items = Items
            .Where(item => item.Mode != DataItem.AccessMode.WriteToPlc)
            .ToList();

        if (s7Items.Count == 0) return;

        ParentPlc.ReadMultipleVars(s7Items);
        s7Items.ForEach(item => item.UpdateGDValue());
    }

    /// <summary>
    /// Writes all data items to the PLC.
    /// </summary>
    public void WriteAll()
    {
        if (ParentPlc == null || !IsInstanceValid(ParentPlc))
        {
            GD.PrintErr("Plc is null, cannot write data items.");
            return;
        }

        // Store items in a class field instead of passing as parameter
        _itemsToProcess = Items
            .Where(item => item.Mode != DataItem.AccessMode.ReadFromPlc)
            .ToList();

        if (_itemsToProcess.Count == 0) return;

        // Call the method without parameters
        DeferredWriteAll();

    }

    #endregion

    #region Private Methods
    /// <summary>
    /// Deferred method to write data items to the PLC.
    /// </summary>
    private void DeferredWriteAll()
    {
        foreach (var item in _itemsToProcess)
        {
            if (item.VisualComponent != null && !string.IsNullOrEmpty(item.VisualProperty))
                item.UpdateValue();
        }

        ParentPlc.Write(_itemsToProcess.ToArray());
    }

    /// <summary>
    /// Updates the list of data items by scanning child nodes.
    /// </summary>
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

    #endregion

    #region Godot Lifecycle Methods
    /// <summary>
    /// Called when the node is removed from the scene tree.
    /// </summary>
    public override void _ExitTree()
    {
        if (ParentPlc != null && _isRegistered)
        {
            ParentPlc.RemoveAction(this);
            _isRegistered = false;
        }
    }

    /// <summary>
    /// Handles notifications from the Godot engine.
    /// </summary>
    /// <param name="what">Notification type.</param>
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
    
    /// <summary>
    /// Called when the node is added to the scene tree.
    /// </summary>
    public override void _Ready()
    {
        if (ParentPlc != null && !_isRegistered)
        {
            ParentPlc.RegisterAction(this);
            _isRegistered = true;
        }
    }
    #endregion
}