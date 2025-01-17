using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace HealthChecks.NpgSql
{
    public class NpgSqlHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;
        private readonly string _sql;
        private readonly Action<NpgsqlConnection>? _connectionAction;

        public NpgSqlHealthCheck(string npgsqlConnectionString, string sql, Action<NpgsqlConnection>? connectionAction = null)
        {
            _connectionString = Guard.ThrowIfNull(npgsqlConnectionString);
            _sql = Guard.ThrowIfNull(sql);
            _connectionAction = connectionAction;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    _connectionAction?.Invoke(connection);

                    await connection.OpenAsync(cancellationToken);

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = _sql;
                        await command.ExecuteScalarAsync(cancellationToken);
                    }

                    return HealthCheckResult.Healthy();
                }
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, description: ex.Message, exception: ex);
            }
        }
    }
}
