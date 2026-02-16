using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;

namespace PartnershipManager.Tests.Unit.Domain.Contracts;

public class ContractTests
{
    [Fact]
    public void Create_ShouldInitializeWithDefaults()
    {
        var contract = Contract.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Contrato Teste",
            ContractTemplateType.NDA,
            description: "Descricao");

        contract.Id.Should().NotBeEmpty();
        contract.Status.Should().Be(ContractStatus.Draft);
        contract.Parties.Should().NotBeNull();
        contract.Clauses.Should().NotBeNull();
    }

    [Fact]
    public void SubmitForReview_ShouldMoveToPendingReview()
    {
        var contract = Contract.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Contrato",
            ContractTemplateType.NDA);

        contract.SubmitForReview();

        contract.Status.Should().Be(ContractStatus.PendingReview);
    }

    [Fact]
    public void Approve_ShouldMoveToApproved()
    {
        var contract = Contract.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Contrato",
            ContractTemplateType.NDA);

        contract.SubmitForReview();
        contract.Approve();

        contract.Status.Should().Be(ContractStatus.Approved);
    }

    [Fact]
    public void SendForSignature_ShouldMoveToSentForSignature()
    {
        var contract = Contract.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Contrato",
            ContractTemplateType.NDA);

        contract.SendForSignature();

        contract.Status.Should().Be(ContractStatus.SentForSignature);
    }

    [Fact]
    public void UpdateSignatureProgress_ShouldSetPartiallySigned()
    {
        var contract = Contract.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Contrato",
            ContractTemplateType.NDA);

        var partyOne = ContractParty.Create(contract.Id, "A", "a@email.com");
        var partyTwo = ContractParty.Create(contract.Id, "B", "b@email.com");

        contract.AddParty(partyOne);
        contract.AddParty(partyTwo);

        partyOne.MarkAsSigned();
        contract.UpdateSignatureProgress();

        contract.Status.Should().Be(ContractStatus.PartiallySigned);
    }

    [Fact]
    public void UpdateSignatureProgress_ShouldSetSigned_WhenAllSigned()
    {
        var contract = Contract.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Contrato",
            ContractTemplateType.NDA);

        var partyOne = ContractParty.Create(contract.Id, "A", "a@email.com");
        var partyTwo = ContractParty.Create(contract.Id, "B", "b@email.com");

        contract.AddParty(partyOne);
        contract.AddParty(partyTwo);

        partyOne.MarkAsSigned();
        partyTwo.MarkAsSigned();

        contract.UpdateSignatureProgress();

        contract.Status.Should().Be(ContractStatus.Signed);
    }
}
