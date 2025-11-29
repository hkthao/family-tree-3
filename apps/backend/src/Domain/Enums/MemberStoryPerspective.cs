using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace backend.Domain.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum MemberStoryPerspective
{
    [EnumMember(Value = "firstPerson")]
    FirstPerson,

    [EnumMember(Value = "thirdPerson")]
    ThirdPerson,

    [EnumMember(Value = "familyMember")]
    FamilyMember,

    [EnumMember(Value = "neutralPersonal")]
    NeutralPersonal,

    [EnumMember(Value = "fullyNeutral")]
    FullyNeutral,
}