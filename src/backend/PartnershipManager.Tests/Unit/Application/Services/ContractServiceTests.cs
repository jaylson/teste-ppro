using Moq;
using PartnershipManager.Application.Features.Contracts.DTOs;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Exceptions;
using PartnershipManager.Domain.Interfaces;
using PartnershipManager.Infrastructure.Services;
using FluentAssertions;

public class ContractServiceTests
{
    private readonly Mock<IContractRepository> _contractRepositoryMock;
    private readonly Mock<IContractTemplateRepository> _templateRepositoryMock;
    private readonly Mock<IClauseRepository> _clauseRepositoryMock;
    private readonly Mock<ICompanyRepository> _companyRepositoryMock;
    private readonly ContractService _service;

    public ContractServiceTests()
    {
        _contractRepositoryMock = new Mock<IContractRepository>();
        _templateRepositoryMock = new Mock<IContractTemplateRepository>();
        _clauseRepositoryMock = new Mock<IClauseRepository>();
        _companyRepositoryMock = new Mock<ICompanyRepository>();

        _service = new ContractService(
            _contractRepositoryMock.Object,
            _templateRepositoryMock.Object,
            _clauseRepositoryMock.Object,
            _companyRepositoryMock.Object);
    }

    #region GetByIdAsync

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnContract()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var contract = Contract.Create(
            clientId,
            Guid.NewGuid(),
            "Test Contract",
            ContractTemplateType.NDA,
            description: "Test");

        _contractRepositoryMock
            .Setup(x => x.GetByIdAsync(contractId, clientId))
            .ReturnsAsync(contract);

        // Act
        var result = await _service.GetByIdAsync(contractId, clientId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(contract.Id);
        result.Title.Should().Be("Test Contract");
        _contractRepositoryMock.Verify(x => x.GetByIdAsync(contractId, clientId), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();

        _contractRepositoryMock
            .Setup(x => x.GetByIdAsync(contractId, clientId))
            .ReturnsAsync((Contract?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetByIdAsync(contractId, clientId));
    }

    #endregion

    #region GetWithDetailsAsync

    [Fact]
    public async Task GetWithDetailsAsync_WithValidId_ShouldReturnContractWithDetails()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var contract = Contract.Create(
            clientId,
            Guid.NewGuid(),
            "Test Contract",
            ContractTemplateType.NDA,
            description: "Test");

        _contractRepositoryMock
            .Setup(x => x.GetWithDetailsAsync(contractId, clientId))
            .ReturnsAsync(contract);

        // Act
        var result = await _service.GetWithDetailsAsync(contractId, clientId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(contract.Id);
        _contractRepositoryMock.Verify(x => x.GetWithDetailsAsync(contractId, clientId), Times.Once);
    }

    [Fact]
    public async Task GetWithDetailsAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();

        _contractRepositoryMock
            .Setup(x => x.GetWithDetailsAsync(contractId, clientId))
            .ReturnsAsync((Contract?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.GetWithDetailsAsync(contractId, clientId));
    }

    #endregion

    #region GetByCompanyAsync

    [Fact]
    public async Task GetByCompanyAsync_ShouldReturnContractsForCompany()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var contracts = new List<Contract>
        {
            Contract.Create(clientId, companyId, "Contract 1", ContractTemplateType.NDA),
            Contract.Create(clientId, companyId, "Contract 2", ContractTemplateType.Partnership)
        };

        _contractRepositoryMock
            .Setup(x => x.GetByCompanyAsync(companyId, clientId))
            .ReturnsAsync(contracts);

        // Act
        var result = await _service.GetByCompanyAsync(companyId, clientId);

        // Assert
        result.Should().HaveCount(2);
        _contractRepositoryMock.Verify(x => x.GetByCompanyAsync(companyId, clientId), Times.Once);
    }

    #endregion

    #region GetByStatusAsync

    [Fact]
    public async Task GetByStatusAsync_ShouldReturnContractsWithStatus()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var contracts = new List<Contract>
        {
            Contract.Create(clientId, Guid.NewGuid(), "Contract 1", ContractTemplateType.NDA),
            Contract.Create(clientId, Guid.NewGuid(), "Contract 2", ContractTemplateType.NDA)
        };

        _contractRepositoryMock
            .Setup(x => x.GetByStatusAsync(clientId, ContractStatus.Draft))
            .ReturnsAsync(contracts);

        // Act
        var result = await _service.GetByStatusAsync(clientId, ContractStatus.Draft);

        // Assert
        result.Should().HaveCount(2);
        _contractRepositoryMock.Verify(
            x => x.GetByStatusAsync(clientId, ContractStatus.Draft),
            Times.Once);
    }

    #endregion

    #region GetExpiredContractsAsync

    [Fact]
    public async Task GetExpiredContractsAsync_ShouldReturnExpiredContracts()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var expiredContracts = new List<Contract>
        {
            Contract.Create(
                clientId,
                Guid.NewGuid(),
                "Expired 1",
                ContractTemplateType.NDA,
                expirationDate: DateTime.UtcNow.AddDays(-1))
        };

        _contractRepositoryMock
            .Setup(x => x.GetExpiredContractsAsync(clientId))
            .ReturnsAsync(expiredContracts);

        // Act
        var result = await _service.GetExpiredContractsAsync(clientId);

        // Assert
        result.Should().HaveCount(1);
        _contractRepositoryMock.Verify(x => x.GetExpiredContractsAsync(clientId), Times.Once);
    }

    #endregion

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidRequest_ShouldCreateContract()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var company = Company.Create(
            clientId,
            "Test Company",
            "11.222.333/0001-81",
            LegalForm.LTDA,
            DateTime.UtcNow.AddYears(-1),
            1000m,
            100m);

        var request = new CreateContractRequest
        {
            Title = "New Contract",
            CompanyId = companyId,
            ContractType = ContractTemplateType.NDA,
            Description = "Description",
            TemplateId = null,
            ExpirationDate = DateTime.UtcNow.AddDays(30)
        };

        _companyRepositoryMock
            .Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync(company);

        _contractRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Contract>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateAsync(clientId, request, userId);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Contract");
        _companyRepositoryMock.Verify(x => x.GetByIdAsync(companyId), Times.Once);
        _contractRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Contract>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithInvalidCompany_ShouldThrowNotFoundException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var request = new CreateContractRequest
        {
            Title = "New Contract",
            CompanyId = companyId,
            ContractType = ContractTemplateType.NDA,
            TemplateId = null
        };

        _companyRepositoryMock
            .Setup(x => x.GetByIdAsync(companyId))
            .ReturnsAsync((Company?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.CreateAsync(clientId, request, null));
    }

    #endregion

    #region UpdateStatusAsync

    [Fact]
    public async Task UpdateStatusAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();

        var request = new UpdateContractStatusRequest
        {
            Status = ContractStatus.PendingReview
        };

        _contractRepositoryMock
            .Setup(x => x.GetByIdAsync(contractId, clientId))
            .ReturnsAsync((Contract?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.UpdateStatusAsync(contractId, clientId, request, null));
    }

    #endregion

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldDeleteContract()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();
        var contract = Contract.Create(
            clientId,
            Guid.NewGuid(),
            "Test Contract",
            ContractTemplateType.NDA);

        _contractRepositoryMock
            .Setup(x => x.GetByIdAsync(contractId, clientId))
            .ReturnsAsync(contract);

        _contractRepositoryMock
            .Setup(x => x.SoftDeleteAsync(contractId, clientId, null))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(contractId, clientId, null);

        // Assert
        _contractRepositoryMock.Verify(x => x.SoftDeleteAsync(contractId, clientId, null), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldThrowNotFoundException()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var contractId = Guid.NewGuid();

        _contractRepositoryMock
            .Setup(x => x.GetByIdAsync(contractId, clientId))
            .ReturnsAsync((Contract?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.DeleteAsync(contractId, clientId, null));
    }

    #endregion

    #region GetPagedAsync

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedContracts()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var contracts = new List<Contract>
        {
            Contract.Create(clientId, Guid.NewGuid(), "Contract 1", ContractTemplateType.NDA),
            Contract.Create(clientId, Guid.NewGuid(), "Contract 2", ContractTemplateType.Partnership)
        };

        _contractRepositoryMock
            .Setup(x => x.GetPagedAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Guid?>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>()))
            .ReturnsAsync((contracts, 2));

        // Act
        var result = await _service.GetPagedAsync(
            clientId,
            null,
            1,
            10,
            null,
            null,
            null,
            null,
            null);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.TotalPages.Should().Be(1);
    }

    #endregion
}
