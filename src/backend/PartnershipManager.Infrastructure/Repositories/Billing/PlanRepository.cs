using Dapper;
using PartnershipManager.Domain.Entities.Billing;
using PartnershipManager.Domain.Interfaces.Billing;
using PartnershipManager.Infrastructure.Persistence;

namespace PartnershipManager.Infrastructure.Repositories.Billing;

public class PlanRepository : IPlanRepository
{
    private readonly DapperContext _context;

    public PlanRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM BillingPlans
            WHERE Id = @Id AND DeletedAt IS NULL";

        return await _context.Connection.QueryFirstOrDefaultAsync<Plan>(sql, new { Id = id });
    }

    public async Task<IEnumerable<Plan>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM BillingPlans
            WHERE DeletedAt IS NULL
            ORDER BY Price ASC";

        return await _context.Connection.QueryAsync<Plan>(sql);
    }

    public async Task<IEnumerable<Plan>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT * FROM BillingPlans
            WHERE IsActive = TRUE AND DeletedAt IS NULL
            ORDER BY Price ASC";

        return await _context.Connection.QueryAsync<Plan>(sql);
    }

    public async Task<Guid> CreateAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        plan.Id = Guid.NewGuid();
        plan.CreatedAt = DateTime.UtcNow;
        plan.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            INSERT INTO BillingPlans (Id, Name, Description, Price, BillingCycle, MaxCompanies, MaxUsers, Features, IsActive, CreatedAt, UpdatedAt)
            VALUES (@Id, @Name, @Description, @Price, @BillingCycle, @MaxCompanies, @MaxUsers, @Features, @IsActive, @CreatedAt, @UpdatedAt)";

        await _context.Connection.ExecuteAsync(sql, plan);
        return plan.Id;
    }

    public async Task<bool> UpdateAsync(Plan plan, CancellationToken cancellationToken = default)
    {
        plan.UpdatedAt = DateTime.UtcNow;

        const string sql = @"
            UPDATE BillingPlans
            SET Name = @Name,
                Description = @Description,
                Price = @Price,
                BillingCycle = @BillingCycle,
                MaxCompanies = @MaxCompanies,
                MaxUsers = @MaxUsers,
                Features = @Features,
                IsActive = @IsActive,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND DeletedAt IS NULL";

        var rows = await _context.Connection.ExecuteAsync(sql, plan);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            UPDATE BillingPlans
            SET DeletedAt = @DeletedAt
            WHERE Id = @Id AND DeletedAt IS NULL";

        var rows = await _context.Connection.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
        return rows > 0;
    }
}
