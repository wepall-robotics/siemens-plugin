using System.Collections.Generic;
using Godot;
using S7.Net;
using S7.Net.Types;

[Tool]
[GlobalClass]
public partial class DataGroup : Node
{
    [Export] 
    public Godot.Collections.Array<DataItem> Items { get; set; } = new();

    // Obtener si es posible el PlcNode que estará como padre
    public PlcNode? ParentPlcNode => GetParent<PlcNode>();

    public void ReadAll(Plc plc)
    {
        var s7Items = new List<DataItem>();
        
        foreach (var item in Items)
        {
            s7Items.Add(new DataItem
            {
                DataType = item.DataType,
                DB = (ushort)item.DB,
                StartByteAdr = item.StartByteAdr,
                VarType = item.VarType,
                Count = item.Count
            });
        }

    }
    public void WriteAll(Plc plc)
    {
        foreach (var item in Items)
        {
            var value = item.GDValue;
            plc.Write(item.DataType,item.DB, item.StartByteAdr, value);
        }
    }

}