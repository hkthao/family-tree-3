namespace backend.Application.Common.Interfaces.Core;

/// <summary>
/// Định nghĩa giao diện để lấy thời gian hiện tại.
/// </summary>
public interface IDateTime
{
    /// <summary>
    /// Lấy thời gian hiện tại.
    /// </summary>
    DateTime Now { get; }
}
