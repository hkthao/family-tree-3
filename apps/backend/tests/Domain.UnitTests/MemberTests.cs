using backend.Domain.Entities;
using backend.Domain.Enums;
using FluentAssertions;
using Xunit;
using System;

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
        var member = new Member(lastName, firstName, code, _familyId, isDeceased);

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
            placeOfBirth, placeOfDeath, phone, email, address, occupation, avatarUrl, biography, order, isDeceased
        );

        // Assert
        member.LastName.Should().Be(lastName);
        member.FirstName.Should().Be(firstName);
        member.Code.Should().Be(code);
        member.FamilyId.Should().Be(_familyId);
        member.Nickname.Should().Be(nickname);
        member.Gender.Should().Be(gender);
        member.DateOfBirth.Should().Be(dateOfBirth);
        member.DateOfDeath.Should().Be(dateOfDeath);
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
    public void Update_ShouldUpdateAllPropertiesCorrectly()
    {
        // Arrange
        var member = new Member("Old", "Name", "OLDNAME", _familyId);

        var newLastName = "New";
        var newFirstName = "User";
        var newCode = "NEWUSER";
        var newNickname = "Newbie";
        var newGender = "Male";
        var newDateOfBirth = new DateTime(1991, 2, 2);
        var newDateOfDeath = new DateTime(2051, 2, 2);
        var newPlaceOfBirth = "London";
        var newPlaceOfDeath = "Paris";
        var newPhone = "098-765-4321";
        var newEmail = "new.user@example.com";
        var newAddress = "456 New Rd";
        var newOccupation = "Developer";
        var newAvatarUrl = "http://example.com/newuser.jpg";
        var newBiography = "An updated bio.";
        var newOrder = 2;
        var newIsDeceased = true;

        // Act
        member.Update(
            newFirstName, newLastName, newCode, newNickname, newGender, newDateOfBirth, newDateOfDeath,
            newPlaceOfBirth, newPlaceOfDeath, newPhone, newEmail, newAddress, newOccupation, newAvatarUrl,
            newBiography, newOrder, newIsDeceased
        );

        // Assert
        member.LastName.Should().Be(newLastName);
        member.FirstName.Should().Be(newFirstName);
        member.Code.Should().Be(newCode);
        member.Nickname.Should().Be(newNickname);
        member.Gender.Should().Be(newGender);
        member.DateOfBirth.Should().Be(newDateOfBirth);
        member.DateOfDeath.Should().Be(newDateOfDeath);
        member.PlaceOfBirth.Should().Be(newPlaceOfBirth);
        member.PlaceOfDeath.Should().Be(newPlaceOfDeath);
        member.Phone.Should().Be(newPhone);
        member.Email.Should().Be(newEmail);
        member.Address.Should().Be(newAddress);
        member.Occupation.Should().Be(newOccupation);
        member.AvatarUrl.Should().Be(newAvatarUrl);
        member.Biography.Should().Be(newBiography);
        member.Order.Should().Be(newOrder);
        member.IsDeceased.Should().Be(newIsDeceased);
        member.FullName.Should().Be($"{newLastName} {newFirstName}");
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
        member.UpdateGender(newGender);

        // Assert
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
            FaceId = "face_id_123",
            OriginalImageUrl = "http://face.com/face.jpg",
            Confidence = 0.5f
        };

        // Act
        member.AddFace(memberFace);

        // Assert
        member.MemberFaces.Should().ContainSingle(f => f.FaceId == "face_id_123");
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
            FaceId = "face_id_123",
            OriginalImageUrl = "http://face.com/face.jpg",
            Confidence = 0.5f
        };

        // Act
        Action act = () => member.AddFace(memberFace);

        // Assert
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("MemberFace must belong to this Member.");
    }

    // [Fact]
    // public void AddFatherRelationship_ShouldCreateRelationshipAndAddToTargetRelationships()
    // {
    //     // Arrange
    //     var member = new Member("Child", "Member", "CM", _familyId);
    //     var fatherId = Guid.NewGuid();

    //     // Act
    //     var relationship = member.AddFatherRelationship(fatherId);

    //     // Assert
    //     relationship.Should().NotBeNull();
    //     relationship.FamilyId.Should().Be(_familyId);
    //     relationship.SourceMemberId.Should().Be(fatherId);
    //     relationship.TargetMemberId.Should().Be(member.Id);
    //     relationship.RelationshipType.Should().Be(RelationshipType.Father);
    //     member.TargetRelationships.Should().Contain(relationship);
    // }

    // [Fact]
    // public void AddMotherRelationship_ShouldCreateRelationshipAndAddToTargetRelationships()
    // {
    //     // Arrange
    //     var member = new Member("Child", "Member", "CM", _familyId);
    //     var motherId = Guid.NewGuid();

    //     // Act
    //     var relationship = member.AddMotherRelationship(motherId);

    //     // Assert
    //     relationship.Should().NotBeNull();
    //     relationship.FamilyId.Should().Be(_familyId);
    //     relationship.SourceMemberId.Should().Be(motherId);
    //     relationship.TargetMemberId.Should().Be(member.Id);
    //     relationship.RelationshipType.Should().Be(RelationshipType.Mother);
    //     member.TargetRelationships.Should().Contain(relationship);
    // }

    // [Fact]
    // public void AddHusbandRelationship_ShouldCreateRelationshipAndAddToSourceRelationships()
    // {
    //     // Arrange
    //     var member = new Member("Wife", "Member", "WM", _familyId); // Assuming female member
    //     var husbandId = Guid.NewGuid();

    //     // Act
    //     var relationship = member.AddHusbandRelationship(husbandId);

    //     // Assert
    //     relationship.Should().NotBeNull();
    //     relationship.FamilyId.Should().Be(_familyId);
    //     relationship.SourceMemberId.Should().Be(member.Id);
    //     relationship.TargetMemberId.Should().Be(husbandId);
    //     relationship.RelationshipType.Should().Be(RelationshipType.Wife);
    //     member.SourceRelationships.Should().Contain(relationship);
    // }

    // [Fact]
    // public void AddWifeRelationship_ShouldCreateRelationshipAndAddToSourceRelationships()
    // {
    //     // Arrange
    //     var member = new Member("Husband", "Member", "HM", _familyId); // Assuming male member
    //     var wifeId = Guid.NewGuid();

    //     // Act
    //     var relationship = member.AddWifeRelationship(wifeId);

    //     // Assert
    //     relationship.Should().NotBeNull();
    //     relationship.FamilyId.Should().Be(_familyId);
    //     relationship.SourceMemberId.Should().Be(member.Id);
    //     relationship.TargetMemberId.Should().Be(wifeId);
    //     relationship.RelationshipType.Should().Be(RelationshipType.Husband);
    //     member.SourceRelationships.Should().Contain(relationship);
    // }
}
