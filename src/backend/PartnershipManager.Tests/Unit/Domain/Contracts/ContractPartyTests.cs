using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Tests.Unit.Domain.Contracts;

public class ContractPartyTests
{
    [Fact]
    public void Create_ShouldInitializeDefaults()
    {
        var party = ContractParty.Create(
            Guid.NewGuid(),
            "Maria",
            "maria@email.com");

        party.Id.Should().NotBeEmpty();
        party.SignatureStatus.Should().Be(SignatureStatus.Pending);
    }

    [Fact]
    public void SetWaitingSignature_ShouldUpdateStatusAndToken()
    {
        var party = ContractParty.Create(Guid.NewGuid(), "Maria", "maria@email.com");

        party.SetWaitingSignature("token-123", "external-1");

        party.SignatureStatus.Should().Be(SignatureStatus.WaitingSignature);
        party.SignatureToken.Should().Be("token-123");
        party.ExternalId.Should().Be("external-1");
    }

    [Fact]
    public void MarkAsSigned_ShouldUpdateStatusAndDate()
    {
        var party = ContractParty.Create(Guid.NewGuid(), "Maria", "maria@email.com");

        party.MarkAsSigned("external-2");

        party.SignatureStatus.Should().Be(SignatureStatus.Signed);
        party.SignatureDate.Should().NotBeNull();
        party.ExternalId.Should().Be("external-2");
    }

    [Fact]
    public void RejectSignature_ShouldSetRejectedStatus()
    {
        var party = ContractParty.Create(Guid.NewGuid(), "Maria", "maria@email.com");

        party.RejectSignature();

        party.SignatureStatus.Should().Be(SignatureStatus.Rejected);
    }

    [Fact]
    public void ExpireSignature_ShouldSetExpiredStatus()
    {
        var party = ContractParty.Create(Guid.NewGuid(), "Maria", "maria@email.com");

        party.ExpireSignature();

        party.SignatureStatus.Should().Be(SignatureStatus.Expired);
    }
}
