using System.Collections.Generic;
using System.Linq;
using Godot;
using S7.Net;
using S7.Net.Types;

[Tool]
[GlobalClass]
public partial class DataGroup : Node
{
    public Godot.Collections.Array<DataItem> Items { get; set; } = new();
    public PlcNode? ParentPlcNode => GetParent<PlcNode>();


    public void ReadAll(Plc plc)
    {
        // List<DataItem> s7Items = new List<DataItem>();
        List<DataItem> s7Items = Items.Where(item => item.Mode != DataItem.AccessMode.Write).ToList();
        // foreach (var item in Items)
        // {
        //     if (item.Mode != DataItem.AccessMode.Write)
        //     {
        //         s7Items.Add(new DataItem
        //         {
        //             DataType = item.DataType,
        //             DB = (ushort)item.DB,
        //             StartByteAdr = item.StartByteAdr,
        //             VarType = item.VarType,
        //             Count = item.Count
        //         });
        //     }
        // }
        plc.ReadMultipleVars(s7Items);

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