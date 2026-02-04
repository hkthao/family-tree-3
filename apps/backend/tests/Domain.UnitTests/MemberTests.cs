using backend.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace backend.Domain.UnitTests;

public class MemberTests
{
    private readonly Guid _familyId = Guid.NewGuid();

    [Fact]
    public void Member_DefaultConstructor_ShouldCreateInstance()
    {
        // Act
        var member = new Member();

        // Assert
        member.Should().NotBeNull();
    }

    [Fact]
    public void Member_PrimaryConstructor_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var lastName = "Doe";
        var firstName = "John";
        var code = "JOHNDOE";
        var isDeceased = true;

        // Act
        var member = new Member(lastName, firstName, code, _familyId, isDeceased: isDeceased);

        // Assert
        member.LastName.Should().Be(lastName);
        member.FirstName.Should().Be(firstName);
        member.Code.Should().Be(code);
        member.FamilyId.Should().Be(_familyId);
        member.IsDeceased.Should().Be(isDeceased);
        member.FullName.Should().Be($"{lastName} {firstName}");
        member.Id.Should().NotBe(Guid.Empty); // BaseAuditableEntity property
    }

    [Fact]
    public void Member_FullConstructor_ShouldSetAllPropertiesCorrectly()
    {
        // Arrange
        var lastName = "Doe";
        var firstName = "Jane";
        var code = "JANEDOE";
        var nickname = "Janey";
        var gender = "Female";
        var dateOfBirth = new DateTime(1990, 1, 1);
        var dateOfDeath = new DateTime(2050, 1, 1);
        var placeOfBirth = "New York";
        var placeOfDeath = "London";
        var phone = "123-456-7890";
        var email = "jane.doe@example.com";
        var address = "123 Main St";
        var occupation = "Engineer";
        var avatarUrl = "http://example.com/jane.jpg";
        var biography = "A short bio.";
        var order = 1;
        var isDeceased = false;

        // Act
        var member = new Member(
            lastName, firstName, code, _familyId, nickname, gender, dateOfBirth, dateOfDeath,
            null, // lunarDateOfDeath
            placeOfBirth, placeOfDeath, phone, email, address, occupation, avatarUrl, biography, order, isDeceased
        );

        // Assert
        member.LastName.Should().Be(lastName);
        member.FirstName.Should().Be(firstName);
        member.Code.Should().Be(code);
        member.Gender.Should().Be(gender);
        member.PlaceOfBirth.Should().Be(placeOfBirth);
        member.PlaceOfDeath.Should().Be(placeOfDeath);
        member.Phone.Should().Be(phone);
        member.Email.Should().Be(email);
        member.Address.Should().Be(address);
        member.Occupation.Should().Be(occupation);
        member.AvatarUrl.Should().Be(avatarUrl);
        member.Biography.Should().Be(biography);
        member.Order.Should().Be(order);
        member.IsDeceased.Should().Be(isDeceased);
        member.FullName.Should().Be($"{lastName} {firstName}");
    }



    [Fact]
    public void SetAsRoot_ShouldSetIsRootToTrue()
    {
        // Arrange
        var member = new Member("Test", "Member", "TM", _familyId);
        member.UnsetAsRoot(); // Ensure it's false initially

        // Act
        member.SetAsRoot();

        // Assert
        member.IsRoot.Should().BeTrue();
    }

    [Fact]
    public void UnsetAsRoot_ShouldSetIsRootToFalse()
    {
        // Arrange
        var member = new Member("Test", "Member", "TM", _familyId);
        member.SetAsRoot(); // Ensure it's true initially

        // Act
        member.UnsetAsRoot();

        // Assert
        member.IsRoot.Should().BeFalse();
    }

    [Fact]
    public void UpdateAvatar_ShouldUpdateAvatarUrl()
    {
        // Arrange
        var member = new Member("Test", "Member", "TM", _familyId);
        var newAvatarUrl = "http://example.com/new_avatar.jpg";

        // Act
        member.UpdateAvatar(newAvatarUrl);

        // Assert
        member.AvatarUrl.Should().Be(newAvatarUrl);
    }

    [Fact]
    public void UpdateBiography_ShouldUpdateBiography()
    {
        // Arrange
        var member = new Member("Test", "Member", "TM", _familyId);
        var newBiography = "An updated biography.";

        // Act
        member.UpdateBiography(newBiography);

        // Assert
        member.Biography.Should().Be(newBiography);
    }

    [Fact]
    public void UpdateGender_ShouldUpdateGender()
    {
        // Arrange
        var member = new Member("Test", "Member", "TM", _familyId);
        var newGender = "Female";

        // Act
        member.Gender.Should().Be(newGender);
    }

    [Fact]
    public void UpdateRelationShip_ShouldUpdateDenormalizedRelationshipProperties()
    {
        // Arrange
        var member = new Member("Test", "Member", "TM", _familyId);
        var fatherId = Guid.NewGuid();
        var motherId = Guid.NewGuid();
        var husbandId = Guid.NewGuid();
        var wifeId = Guid.NewGuid();

        // Act
        member.UpdateRelationShip(
            fatherId, "Father FullName", "FatherAvatar",
            motherId, "Mother FullName", "MotherAvatar",
            husbandId, "Husband FullName", "HusbandAvatar",
            wifeId, "Wife FullName", "WifeAvatar"
        );

        // Assert
        member.FatherId.Should().Be(fatherId);
        member.FatherFullName.Should().Be("Father FullName");
        member.FatherAvatarUrl.Should().Be("FatherAvatar");
        member.MotherId.Should().Be(motherId);
        member.MotherFullName.Should().Be("Mother FullName");
        member.MotherAvatarUrl.Should().Be("MotherAvatar");
        member.HusbandId.Should().Be(husbandId);
        member.HusbandFullName.Should().Be("Husband FullName");
        member.HusbandAvatarUrl.Should().Be("HusbandAvatar");
        member.WifeId.Should().Be(wifeId);
        member.WifeFullName.Should().Be("Wife FullName");
        member.WifeAvatarUrl.Should().Be("WifeAvatar");
    }

    [Fact]
    public void AddFace_ShouldAddMemberFace()
    {
        // Arrange
        var member = new Member("Test", "Member", "TM", _familyId);
        var memberFace = new MemberFace
        {
            MemberId = member.Id,
            OriginalImageUrl = "http://face.com/face.jpg",
            Confidence = 0.5f
        };

        // Act
        member.AddFace(memberFace);

        // Assert
        member.MemberFaces.Should().ContainSingle(f => f.OriginalImageUrl == "http://face.com/face.jpg");
    }

    [Fact]
    public void AddFace_ShouldThrowInvalidOperationException_WhenMemberIdMismatch()
    {
        // Arrange
        var member = new Member("Test", "Member", "TM", _familyId);
        var differentMemberId = Guid.NewGuid();
        var memberFace = new MemberFace
        {
            MemberId = differentMemberId,
            OriginalImageUrl = "http://face.com/face.jpg",
            Confidence = 0.5f
        };

        // Act
        Action act = () => member.AddFace(memberFace);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("MemberFace must belong to this Member.");
    }


}
