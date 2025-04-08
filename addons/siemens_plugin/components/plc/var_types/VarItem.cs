using Godot;
using System;

namespace S7.Net.Types
{
public partial class DataItem : Node
{
    public enum AccessMode { Read, Write,ReadWrite }
    [Export]
    public AccessMode Mode { get; set; }
    // [Export]
        // public Variant GDValue
        // {
        //     get =>S7TypeConverter.ConvertToVariant(Value, VarType);
        //     set => Value = S7TypeConverter.ConvertFromVariant(value, VarType);
        // }

         private static class S7TypeConverter
        {
            public static object ConvertFromVariant(Variant godotValue, VarType type)
            {
                return type switch
                {
                    VarType.Bit => godotValue.AsBool(),
                    VarType.Byte => godotValue.AsByte(),
                    VarType.Word => godotValue.AsUInt16(),
                    VarType.Int => godotValue.AsInt16(),
                    VarType.DWord => godotValue.AsUInt32(),
                    VarType.DInt => godotValue.AsInt32(),
                    VarType.Real => (float)godotValue.AsDouble(),
                    VarType.LReal => godotValue.AsDouble(),
                    VarType.String => godotValue.AsString(),
                    VarType.Time => godotValue.AsInt32(),
                    VarType.Date => godotValue.AsInt32(),
                    VarType.DateTime => godotValue.AsInt32(),
                    VarType.Counter => godotValue.AsUInt32(),
                    VarType.Timer => godotValue.AsUInt32(),
                    _ => godotValue
                };
            }

            public static Variant ConvertToVariant(object? value, VarType type)
            {
                return type switch
                {
                    VarType.Bit => Variant.From<bool>(Convert.ToBoolean(value)),
                    VarType.Byte => Variant.From<byte>(Convert.ToByte(value)),
                    VarType.Word => Variant.From<ushort>(Convert.ToUInt16(value)),
                    VarType.Int => Variant.From<short>(Convert.ToInt16(value)),
                    VarType.DWord => Variant.From<uint>(Convert.ToUInt32(value)),
                    VarType.DInt => Variant.From<int>(Convert.ToInt32(value)),
                    VarType.Real => Variant.From<float>(Convert.ToSingle(value)),
                    VarType.LReal => Variant.From<double>(Convert.ToDouble(value)),
                    VarType.String => Variant.From<string>(Convert.ToString(value)),
                    VarType.Time => Variant.From<int>(Convert.ToInt32(value)),
                    VarType.Date => Variant.From<int>(Convert.ToInt32(value)),
                    VarType.DateTime => Variant.From<int>(Convert.ToInt32(value)),
                    VarType.Counter => Variant.From<uint>(Convert.ToUInt32(value)),
                    VarType.Timer => Variant.From<uint>(Convert.ToUInt32(value)),
                    _ => default
                };
            }
        }
}
}