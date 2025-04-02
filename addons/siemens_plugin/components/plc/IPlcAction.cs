using S7.Net;


// ========================
// PLC Action Interfaces
// ========================
/// <summary>
/// Base interface for all PLC actions
/// </summary>
public interface IPlcAction
{
    void Execute();
}

/// <summary>
/// Marker interface for read operations
/// </summary>
public interface IReadAction : IPlcAction { }

/// <summary>
/// Marker interface for write operations
/// </summary>
public interface IWriteAction : IPlcAction { }