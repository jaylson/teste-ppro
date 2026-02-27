using Dapper;
using PartnershipManager.Domain.Entities;
using PartnershipManager.Domain.Enums;
using PartnershipManager.Domain.Interfaces;

namespace PartnershipManager.Infrastructure.Persistence.Repositories;

public class MilestoneProgressRepository : IMilestoneProgressRepository
{
    private readonly DapperContext _context;

    public MilestoneProgressRepository(DapperContext context)
    {
        _context = context;
    }

    private const string SelectColumns = @"
        id, client_id, grant_milestone_id, recorded_date, recorded_value,
        progress_percentage, notes, data_source, recorded_by, created_at";

    public async Task<IEnumerable<MilestoneProgress>> GetByMilestoneAsync(
        Guid clientId, Guid grantMilestoneId)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM milestone_progress
            WHERE client_id = @ClientId AND grant_milestone_id = @MilestoneId
            ORDER BY recorded_date DESC, created_at DESC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            MilestoneId = grantMilestoneId.ToString()
        }, _context.Transaction);

        return rows.Select(Map);
    }

    public async Task<MilestoneProgress?> GetLatestAsync(Guid clientId, Guid grantMilestoneId)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM milestone_progress
            WHERE client_id = @ClientId AND grant_milestone_id = @MilestoneId
            ORDER BY recorded_date DESC, created_at DESC
            LIMIT 1";

        var row = await _context.Connection.QueryFirstOrDefaultAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            MilestoneId = grantMilestoneId.ToString()
        }, _context.Transaction);

        return row is null ? null : Map(row);
    }

    public async Task<IEnumerable<MilestoneProgress>> GetTimeSeriesAsync(
        Guid clientId, Guid grantMilestoneId, DateTime from, DateTime to)
    {
        var sql = $@"
            SELECT {SelectColumns} FROM milestone_progress
            WHERE client_id = @ClientId AND grant_milestone_id = @MilestoneId
              AND recorded_date >= @From AND recorded_date <= @To
            ORDER BY recorded_date ASC, created_at ASC";

        var rows = await _context.Connection.QueryAsync<dynamic>(sql, new
        {
            ClientId = clientId.ToString(),
            MilestoneId = grantMilestoneId.ToString(),
            From = from.Date,
            To = to.Date
        }, _context.Transaction);

        return rows.Select(Map);
    }

    public async Task AddAsync(MilestoneProgress progress)
    {
        var sql = @"
            INSERT INTO milestone_progress
                (id, client_id, grant_milestone_id, recorded_date, recorded_value,
                 progress_percentage, notes, data_source, recorded_by, created_at)
            VALUES
                (@Id, @ClientId, @MilestoneId, @RecordedDate, @RecordedValue,
                 @ProgressPercentage, @Notes, @DataSource, @RecordedBy, @CreatedAt)";

        await _context.Connection.ExecuteAsync(sql, new
        {
            Id = progress.Id.ToString(),
            ClientId = progress.ClientId.ToString(),
            MilestoneId = progress.GrantMilestoneId.ToString(),
            progress.RecordedDate,
            progress.RecordedValue,
            progress.ProgressPercentage,
            progress.Notes,
            DataSource = progress.DataSource?.ToString(),
            RecordedBy = progress.RecordedBy.ToString(),
            progress.CreatedAt
        }, _context.Transaction);
    }

    private static MilestoneProgress Map(dynamic row)
    {
        static Guid ParseGuid(object v) => v is Guid g ? g : Guid.Parse(v.ToString()!);

        ProgressDataSource? dataSource = row.data_source == null || row.data_source is DBNull
            ? null
            : Enum.TryParse<ProgressDataSource>(row.data_source.ToString(), out ProgressDataSource ds) ? ds : (ProgressDataSource?)null;

        return MilestoneProgress.Reconstitute(
            id: ParseGuid(row.id),
            clientId: ParseGuid(row.client_id),
            grantMilestoneId: ParseGuid(row.grant_milestone_id),
            recordedDate: (DateTime)row.recorded_date,
            recordedValue: Convert.ToDecimal(row.recorded_value),
            progressPercentage: Convert.ToDecimal(row.progress_percentage),
            notes: row.notes == null || row.notes is DBNull ? null : (string)row.notes,
            dataSource: dataSource,
            recordedBy: ParseGuid(row.recorded_by),
            createdAt: (DateTime)row.created_at);
    }
}
