using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace backend.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum MemberStoryStyle
{
    [EnumMember(Value = "nostalgic")]
    Nostalgic,

    [EnumMember(Value = "warm")]
    Warm,

    [EnumMember(Value = "formal")]
    Formal,

    [EnumMember(Value = "folk")]
    Folk,
}
