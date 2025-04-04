using Godot;
using S7.Net.Protocol.S7;
using System;

namespace S7.Net.Types
{
    /// <summary>
    /// Create an instance of a memory block that can be read by using ReadMultipleVars
    /// </summary>
    [GlobalClass]
    [Tool]
    public partial class DataItem : Node
    {
        /// <summary>
        /// Memory area to read 
        /// </summary>
        [Export]
        public DataType DataType { get; set; }

        /// <summary>
        /// Type of data to be read (default is bytes)
        /// </summary>
        [Export]
        public VarType VarType { get; set; }

        /// <summary>
        /// Address of memory area to read (example: for DB1 this value is 1, for T45 this value is 45)
        /// </summary>
        [Export]
        public int DB { get; set; }

        /// <summary>
        /// Address of the first byte to read
        /// </summary>
        [Export]
        public int StartByteAdr { get; set; }

        /// <summary>
        /// Addess of bit to read from StartByteAdr
        /// </summary>
        [Export]
        public byte BitAdr { get; set; }

        /// <summary>
        /// Number of variables to read
        /// </summary>
        [Export]
        public int Count { get; set; }

        /// <summary>
        /// Contains the value of the memory area after the read has been executed
        /// </summary>
        public object? Value { get; set; }

        [Export]
        public Variant GDValue
        {
            get => S7TypeConverter.ConvertToVariant(Value, VarType);
            set => Value = S7TypeConverter.ConvertFromVariant(value, VarType);
        }

        /// <summary>
        /// Create an instance of DataItem
        /// </summary>
        public DataItem()
        {
            VarType = VarType.Byte;
            Count = 1;
        }

        /// <summary>
        /// Create an instance of <see cref="DataItem"/> from the supplied address.
        /// </summary>
        /// <param name="address">The address to create the DataItem for.</param>
        /// <returns>A new <see cref="DataItem"/> instance with properties parsed from <paramref name="address"/>.</returns>
        /// <remarks>The <see cref="Count" /> property is not parsed from the address.</remarks>
        public static DataItem FromAddress(string address)
        {
            PLCAddress.Parse(address, out var dataType, out var dbNumber, out var varType, out var startByte,
                out var bitNumber);

            return new DataItem
            {
                DataType = dataType,
                DB = dbNumber,
                VarType = varType,
                StartByteAdr = startByte,
                BitAdr = (byte)(bitNumber == -1 ? 0 : bitNumber)
            };
        }

        /// <summary>
        /// Create an instance of <see cref="DataItem"/> from the supplied address and value.
        /// </summary>
        /// <param name="address">The address to create the DataItem for.</param>
        /// <param name="value">The value to be applied to the DataItem.</param>
        /// <returns>A new <see cref="DataItem"/> instance with properties parsed from <paramref name="address"/> and the supplied value set.</returns>
        public static DataItem FromAddressAndValue<T>(string address, T value)
        {
            var dataItem = FromAddress(address);
            dataItem.Value = value;

            if (typeof(T).IsArray)
            {
                var array = ((Array?)dataItem.Value);
                if (array != null)
                {
                    dataItem.Count = array.Length;
                }
            }

            return dataItem;
        }

        internal static DataItemAddress GetDataItemAddress(DataItem dataItem)
        {
            return new DataItemAddress(dataItem.DataType, dataItem.DB, dataItem.StartByteAdr, Plc.VarTypeToByteLength(dataItem.VarType, dataItem.Count));
        }

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
