namespace backend.Domain.Enums;

public enum LocationLinkType
{
    /// <summary>
    /// Địa điểm liên kết là nơi sinh của thành viên.
    /// </summary>
    Birth,

    /// <summary>
    /// Địa điểm liên kết là nơi mất của thành viên.
    /// </summary>
    Death,

    /// <summary>
    /// Địa điểm liên kết là nơi cư trú của thành viên.
    /// </summary>
    Residence,

    /// <summary>
    /// Địa điểm liên kết là địa điểm chung của gia đình hoặc các địa điểm khác không thuộc các loại trên.
    /// </summary>
    General
}
