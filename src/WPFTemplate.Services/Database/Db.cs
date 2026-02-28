using LanguageExt;
using LanguageExt.Common;

using Microsoft.Data.SqlClient;

using static LanguageExt.Prelude;

namespace InnoJob.Services.Database;

/// <summary>
/// Core database infrastructure providing connection and transaction management
/// for SQL Express using a functional-style <see cref="Either{L,R}"/> error model.
/// </summary>
/// <remarks>
/// <para>
/// Connections are created per-operation and disposed immediately after use.
/// This is safe and efficient because ADO.NET's built-in connection pooling
/// means <c>OpenAsync()</c> borrows a pre-warmed connection from the pool rather
/// than opening a raw socket, and <c>Dispose()</c> returns it to the pool rather
/// than closing it.
/// </para>
/// <para>
/// Configure once at application startup via <see cref="Configure"/>:
/// <code>
/// Db.Configure(@"Server=.\SQLEXPRESS;Database=MyDb;Trusted_Connection=True;TrustServerCertificate=True;");
/// </code>
/// </para>
/// </remarks>
public static class Db
{
    /// <summary>
    /// Gets the ADO.NET connection string used for all database operations.
    /// Set this once at startup via <see cref="Configure"/>.
    /// </summary>
    public static string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// Configures the connection string used by all database operations.
    /// Call this once during application startup before any queries are made.
    /// </summary>
    /// <param name="connectionString">
    /// A valid SQL Server / SQL Express connection string. Example:
    /// <c>Server=.\SQLEXPRESS;Database=MyDb;Trusted_Connection=True;TrustServerCertificate=True;</c>
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="connectionString"/> is null or empty.
    /// </exception>
    public static void Configure(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "Connection string must not be null or empty.");

        ConnectionString = connectionString;
    }

    /// <summary>
    /// Opens a pooled <see cref="SqlConnection"/>, executes the provided asynchronous
    /// function, then disposes the connection. Any exception is caught and returned
    /// as a <see cref="Left{Error}"/> rather than being re-thrown.
    /// </summary>
    /// <typeparam name="T">The type of the successful result value.</typeparam>
    /// <param name="f">
    /// An asynchronous function that receives the open connection and returns
    /// an <see cref="Either{Error, T}"/>. Exceptions thrown inside <paramref name="f"/>
    /// are caught and wrapped as <c>Left(Error)</c>.
    /// </param>
    /// <returns>
    /// <c>Right(T)</c> on success, or <c>Left(Error)</c> if an exception was thrown
    /// at any point (including <c>OpenAsync</c>).
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await Db.WithConnection(conn =>
    ///     DbQuery.QueryMany(conn, "SELECT * FROM Customers", Map));
    /// </code>
    /// </example>
    public static async Task<Either<Error, T>> WithConnection<T>(
        Func<SqlConnection, Task<Either<Error, T>>> f)
    {
        try
        {
            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();
            return await f(conn);
        }
        catch (Exception ex)
        {
            return Left(Error.New(ex));
        }
    }

    /// <summary>
    /// Opens a pooled <see cref="SqlConnection"/>, begins a <see cref="SqlTransaction"/>,
    /// executes the provided asynchronous function, and then either commits or rolls back
    /// the transaction based on whether the result is <c>Right</c> or <c>Left</c>.
    /// </summary>
    /// <typeparam name="T">The type of the successful result value.</typeparam>
    /// <param name="f">
    /// An asynchronous function that receives the open connection and active transaction,
    /// and returns an <see cref="Either{Error, T}"/>. Pass the transaction to every
    /// <see cref="DbQuery"/> call inside this function so they all participate in the
    /// same atomic unit of work.
    /// </param>
    /// <returns>
    /// <c>Right(T)</c> with the transaction committed on success, or <c>Left(Error)</c>
    /// with the transaction rolled back on failure or exception.
    /// </returns>
    /// <remarks>
    /// The transaction is automatically rolled back if:
    /// <list type="bullet">
    ///   <item>The function returns <c>Left(Error)</c>.</item>
    ///   <item>An unhandled exception is thrown anywhere in the pipeline.</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var result = await Db.WithTransaction(async (conn, tx) =>
    /// {
    ///     var insertResult = await DbQuery.Execute(conn, "INSERT INTO Orders ...", p => { ... }, tx);
    ///     if (insertResult.IsLeft) return insertResult;
    ///
    ///     return await DbQuery.Execute(conn, "UPDATE Stock ...", p => { ... }, tx);
    /// });
    /// </code>
    /// </example>
    public static async Task<Either<Error, T>> WithTransaction<T>(
        Func<SqlConnection, SqlTransaction, Task<Either<Error, T>>> f)
    {
        try
        {
            await using var conn = new SqlConnection(ConnectionString);
            await conn.OpenAsync();
            await using var tx = conn.BeginTransaction();

            var result = await f(conn, tx);

            if (result.IsRight)
                await tx.CommitAsync();
            else
                await tx.RollbackAsync();

            return result;
        }
        catch (Exception ex)
        {
            return Left(Error.New(ex));
        }
    }
}
