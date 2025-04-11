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