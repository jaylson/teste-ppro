using System.Data;
using MySqlConnector;

namespace PartnershipManager.Infrastructure.Persistence;

/// <summary>
/// Interface para factory de conexões
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateConnectionAsync();
}

/// <summary>
/// Factory para conexões MySQL
/// </summary>
public class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}

/// <summary>
/// Contexto do Dapper para gerenciamento de conexões e transações
/// </summary>
public class DapperContext : IDisposable
{
    private readonly IDbConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public DapperContext(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = _connectionFactory.CreateConnection();
                _connection.Open();
            }
            return _connection;
        }
    }

    public IDbTransaction? Transaction => _transaction;

    public async Task<IDbTransaction> BeginTransactionAsync()
    {
        if (_connection == null || _connection.State != ConnectionState.Open)
        {
            _connection = await _connectionFactory.CreateConnectionAsync();
        }
        _transaction = _connection.BeginTransaction();
        return _transaction;
    }

    public void CommitTransaction()
    {
        _transaction?.Commit();
        _transaction?.Dispose();
        _transaction = null;
    }

    public Task CommitTransactionAsync()
    {
        CommitTransaction();
        return Task.CompletedTask;
    }

    public void RollbackTransaction()
    {
        _transaction?.Rollback();
        _transaction?.Dispose();
        _transaction = null;
    }

    public Task RollbackTransactionAsync()
    {
        RollbackTransaction();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
    }
}
