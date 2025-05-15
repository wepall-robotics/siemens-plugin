![Godot](https://img.shields.io/badge/godot-4.x-blue)  
A powerful plugin to connect **Siemens S7 PLCs** with the **Godot Engine**, enabling real-time industrial automation and visualization.
## Features

- **Connect** to Siemens S7 PLCs (S7-300, S7-400, S7-1200, S7-1500)
- **Read and write** PLC variables directly from Godot
- **Support for multiple data types**: Bool, Byte, Int, DInt, Real, LReal, String, etc.
- **Data grouping** for efficient batch operations
- **Visual binding**: Link PLC variables to Godot UI components and properties
- **Robust error handling** and automatic reconnection logic
- **Extensible**: Easily add new variable types and behaviors

![[Siemens-Plugin.gif]]

---

## VarTypes and DataItems Explained

**VarTypes** define the type of PLC variable you wish to read or write (e.g., Bit, Int, Real). In this plugin, each variable you interact with is represented by a **DataItem** node, which you configure with the appropriate VarType, memory area, address, and access mode.

**Supported VarTypes:**

| VarType | Description                  |
| :------ | :--------------------------- |
| Bit     | Single boolean bit           |
| Byte    | 8-bit unsigned integer       |
| Word    | 16-bit unsigned integer      |
| DWord   | 32-bit unsigned integer      |
| Int     | 16-bit signed integer        |
| DInt    | 32-bit signed integer        |
| Real    | 32-bit floating-point number |
| LReal   | 64-bit floating-point number |
| String  | Character string             |

![[Pasted image 20250515093146.png]]

**DataItem Properties:**

| Property | Description |
| :-- | :-- |
| DataType | PLC memory area (Memory, DataBlock, Input, Output) |
| VarType | Variable type (see above) |
| DB | DataBlock number (0 for main memory, or specific DB) |
| StartByte Adr | Starting byte address |
| Bit Adr | Bit address (for Bit VarType) |
| Count | Number of variables to read/write |
| Mode | "ReadFromPlc" or "WriteToPlc" |

---

## Installation

1. **Copy** the `siemens_plugin` folder into your project's `addons` directory.
2. **Enable** the plugin in **Project > Project Settings > Plugins**.
3. Ensure your project targets **Godot 4.x** and has **.NET** support enabled.
4. **Build** the project.
5. **Reload** your project.

---

## Quick Start

1. Add a **S7_1500 escene** to your scene (`Plc`).
2. Configure the PLC's **IP**, **CPU model**, **rack**, and **slot**.
3. Add **GroupData** nodes as children of the **PLC Node**.
4. Add **Data Items** (e.g., `BoolItem`, `IntItem`) as children of the **GroupData Node**.
5. Use the provided scripts to read/write variables or bind them to UI components.

---

## Example Scenes

You can use these example scenes as a guide to create your own demos and documentation for the plugin. Each example includes a brief description, step-by-step instructions, and suggestions for visual integration.

---

### 1. Basic Connection to a Siemens PLC

**Description:**
A simple scene allowing you to configure the PLC's **IP address**, **CPU type**, **rack**, and **slot**, then **connect** or **disconnect** and view the **connection status**.

**Features:**

- Input fields for **IP**, **rack**, and **slot**.
- Dropdown for **CPU type** selection.
- Button to **connect**/**disconnect** from the PLC.
- Label or indicator showing the current **connection status**.

**Instructions (as shown in a Label3D in the scene):**

1. Add **Plc scene** to tree.
2. Enter the PLC's **IP address**.
3. Select the **CPU type**.
4. Set **rack** and **slot**.
5. Click the **Connect** button.
6. Check the **connection status**.

![[1.Basic-Connection.gif]]
---

### 2. Reading a Boolean Variable

**Description:**
A scene with a visual switch (checkbox or button) that reflects the state of a **boolean** **variable** in the **PLC**.

**Features:**

- Periodically reads a **boolean variable** from the **PLC**.
- Updates the **visual component** in Godot to match the **PLC** **value**.

**Instructions:**

1. Configure the **Plc node** as shown in the previous example.
2. Add a **GroupData** node as a **child** of the **Plc node**.
3. Add a **BoolItem** as a **child** of the **GroupData node**.
4. Set the **DataType** to "**Memory**" to read a bit from the **PLC**.
5. Set the **VarType** to "**Bit**".
6. Set **DB to 0** if it is in the **Main DB**, or to the number of the DB where the boolean is stored.
7. Set the **StartByte** **Adr** and **Bit Adr** according to the address of the bit in the **PLC**.
8. Set Count to **1**.
9. Set **Mode** to "**ReadFromPlc**".
10. Click the "**Online**" button.
11. **Run** the current scene.
12. Select "**BoolItem**".
13. View the value in the "**Gd** **Value**" property.

![[2.Read-Bool.gif]]
---

### 3. Writing Numeric Variables

**Description:**
A scene with **numeric input fields** for **writing integer** and **floating-point** values (Int, DInt, Real, LReal) to the **PLC**.

**Features:**

- Demonstrates mapping of different data types.
- Real-time updates between **Godot** and the **PLC**.

**Instructions:**

1. Configure the **Plc node** as shown in the previous example.
2. Add a **GroupData node** as a child of the **Plc node**.
3. Add an **IntItem**, **RealItem**, and **LRealItem** as children of the **GroupData node**.
4. Set the **DataType** to "**Memory**" to write to the **PLC**.
5. Set **VarType** to "**Int**", "**Real**", or "**LReal**" depending on the item.
6. Set **DB to 0** if it is in the **Main DB**, or to the number of the DB where the variable is stored.
7. Set the **StartByte Adr** and **Bit Adr** according to the address in the **PLC**.
8. Set Count to 1.
9. Set **Mode** to "**WriteFromPlc**".
10. Click the "**Online**" button.
11. **Run** the current scene.
12. Select "**IntItem**", "**RealItem**", or "**LRealItem**" and change the "**GdValue**".
13. View the value changing in **TIA Portal**.

![[3.Write-Nums.gif]]
---

### 4. Advanced Visual Integration

**Description:**
An example showing how to bind **PLC variables** to **advanced visual properties**, such as colors or animations.

**Features:**

- Dynamic UI driven by **PLC data**.
- Integration with **Godot's visual** and animation systems.

**Instructions:**

1. Configure the **Plc node** as shown in the previous example.
2. Add a **GroupData node** as a child of the **Plc node**.
3. Add a **BoolItem** as a child of the **GroupData node**.
4. Set the **DataType** to "**Memory**" to write a bit from **Godot** to the **PLC**.
5. Set the **VarType** to "**Bit**".
6. Set **DB to 0** if it is in the **Main DB**, or to the number of the DB where the boolean is stored.
7. Set the **StartByte Adr** and **Bit Adr** according to the address in the **PLC**.
8. Set Count to 1.
9. Set **Mode** to "**WriteToPlc**".
10. Select **BoolItem**.
11. Drag and drop your **visual component** (e.g., "CSGCylinder3D") to the "**Visual Component**" property in the inspector.
12. Select the **property to bind** (e.g., "on_blocked") in the "**Visual Property**" field.
13. Click the "**Online**" button.
14. **Run** the current scene.
15. The "**on_blocked**" property will write to memory (e.g., M0.0) in the PLC.
16. View the value changing in **TIA Portal**.

![[4.Advanced-Visual-Integration.gif]]
---

## Reference

- **Plc**: Main node for PLC connection and communication.
- **GroupData**: Manages groups of variables for efficient batch operations.
- **DataItem**: Base class for all variable types.
- **BoolItem, IntItem, DIntItem, RealItem, LRealItem, etc.**: Specialized variable nodes for each supported VarType.

---

## Troubleshooting

- Ensure your PLC is reachable from your network.
- Check firewall settings and PLC configuration.
- In TIA Portal, allow access with PUT/GET communication from remote partners.
![[Pasted image 20250508124504.png]]
- Use the Godot output console for error messages.

---

## Contributing

Pull requests and suggestions are welcome! Please open an issue for bugs or feature requests.

---

## License

MIT License.

---

## Credits

- [S7NetPlus](https://github.com/S7NetPlus/s7netplus) library for PLC communication.
- Godot Engine community.

---

## Contact

For questions or support, open an issue or contact the maintainer:
Álex Pérez, CTO at [Wepall](https://wepall.com)
alex.perez@wepall.com

---
