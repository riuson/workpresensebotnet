using System.Globalization;
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
            await using var command =
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

    /// <inheritdoc />
    public async Task UpdateUserState(
        long userId,
        UserState state,
        CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection($"Filename={this.DatabasePath}");

        try
        {
            await connection.OpenAsync(cancellationToken);

            await this.EnsureUserExists(userId, connection, cancellationToken);

            await using var command =
                new SqliteCommand(
                    "update users set (state, state_timestamp) = (@state, @timestamp) where user_id = @user_id;",
                    connection);
            command.Parameters.Add("@state", SqliteType.Integer).Value = (int)state;
            command.Parameters.Add("@timestamp", SqliteType.Text).Value =
                DateTime.Now.ToString("O", CultureInfo.InvariantCulture);
            command.Parameters.Add("@user_id", SqliteType.Integer).Value = userId;
            await command.ExecuteNonQueryAsync(cancellationToken);

            this.logger.LogInformation($"Successfully changed state of user {userId} to {state}");
        }
        catch (Exception exc)
        {
            this.logger.LogCritical(exc, "An error was occur while loading users list!");
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    private async Task EnsureUserExists(
        long userId,
        SqliteConnection connection,
        CancellationToken cancellationToken)
    {
        await using var commandCheck =
            new SqliteCommand(
                "select count(*) from users where user_id = @user_id;",
                connection);
        commandCheck.Parameters.Add("@user_id", SqliteType.Integer).Value = userId;
        var count = Convert.ToInt32(await commandCheck.ExecuteScalarAsync(cancellationToken));

        if (count > 0)
        {
            return;
        }

        await using var commandInsert =
            new SqliteCommand(
                "insert into users (user_id, state, state_timestamp, web_hook_id) values (@user_id, @state, @state_timestamp, @web_hook_id)",
                connection);
        commandInsert.Parameters.Add("@user_id", SqliteType.Integer).Value = userId;
        commandInsert.Parameters.Add("@state", SqliteType.Integer).Value = (int)UserState.Unknown;
        commandInsert.Parameters.Add("@state_timestamp", SqliteType.Text).Value =
            DateTime.Now.ToString("O", CultureInfo.InvariantCulture);
        commandInsert.Parameters.Add("@web_hook_id", SqliteType.Integer).Value = Guid.NewGuid().ToString();
        await commandInsert.ExecuteNonQueryAsync(cancellationToken);
    }
}