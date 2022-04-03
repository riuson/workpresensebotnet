using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ServerApp.Defs;

namespace ServerApp.DB;

/// <summary>
/// Persistent data storage implementation.
/// </summary>
public class Database : IDatabase
{
    private readonly IConfiguration config;
    private readonly ILogger<Database> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Database"/> class.
    /// </summary>
    /// <param name="config">Configuration.</param>
    /// <param name="logger">Logger service.</param>
    public Database(
        IConfiguration config,
        ILogger<Database> logger)
    {
        this.config = config;
        this.logger = logger;
    }

    private string DatabasePath => this.config.GetValue<string>("Database:Path");

    /// <inheritdoc />
    public async Task<IEnumerable<UserInfo>> GetUsersAsync(
        IEnumerable<long> userIds,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Filename={this.DatabasePath}");
        var result = new List<UserInfo>();

        try
        {
            await connection.OpenAsync(cancellationToken);

            var listOfIds = string.Join(", ", userIds.Select(x => x.ToString()));
            var command =
                new SqliteCommand(
                    $"select user_id, state, state_timestamp, phone_number, web_hook_id from users where user_id in ({listOfIds});",
                    connection);
            var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (reader.HasRows)
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var userId = await reader.GetFieldValueAsync<long>(0, cancellationToken);
                    var state = await reader.GetFieldValueAsync<UserState>(1, cancellationToken);
                    var stateTimeStamp = await reader.GetFieldValueAsync<DateTime>(2, cancellationToken);

                    var phoneNumber = await reader.IsDBNullAsync(3, cancellationToken)
                        ? string.Empty
                        : await reader.GetFieldValueAsync<string>(3, cancellationToken);

                    var webHookId = await reader.GetFieldValueAsync<Guid>(4, cancellationToken);

                    result.Add(new UserInfo()
                    {
                        UserId = userId,
                        State = state,
                        StateTimeStamp = stateTimeStamp,
                        PhoneNumber = phoneNumber,
                        WebHookId = webHookId,
                    });
                }
            }
        }
        catch (Exception exc)
        {
            this.logger.LogCritical(exc, "An error was occur while loading users list!");
        }
        finally
        {
            await connection.CloseAsync();
        }

        return result;
    }
}