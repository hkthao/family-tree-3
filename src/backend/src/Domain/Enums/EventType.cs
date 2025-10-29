namespace backend.Domain.Enums;

/// <summary>
/// Đại diện cho các loại sự kiện trong cây gia phả.
/// </summary>
public enum EventType
{
    /// <summary>
    /// Sự kiện sinh.
    /// </summary>
    Birth,
    /// <summary>
    /// Sự kiện kết hôn.
    /// </summary>
    Marriage,
    /// <summary>
    /// Sự kiện qua đời.
    /// </summary>
    Death,
    /// <summary>
    /// Sự kiện di cư.
    /// </summary>
    Migration,
    /// <summary>
    /// Các loại sự kiện khác.
    /// </summary>
    Other
}
