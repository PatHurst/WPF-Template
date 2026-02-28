using LanguageExt;
using LanguageExt.Common;

using Microsoft.Data.SqlClient;

using static LanguageExt.Prelude;

namespace InnoJob.Services.Database;

/// <summary>
/// Provides functional-style helper methods for executing SQL queries and commands
/// against a <see cref="SqlConnection"/>.
/// </summary>
/// <remarks>
/// All methods return <see cref="Either{Error, T}"/> and never throw — exceptions
/// are caught internally and surfaced as <c>Left(Error)</c>.
/// <para>
/// Obtain a connection via <see cref="Db.WithConnection{T}"/> or
/// <see cref="Db.WithTransaction{T}"/> and pass it into these methods.
/// Optionally pass a <see cref="SqlTransaction"/> so the command participates
/// in an ambient unit of work.
/// </para>
/// </remarks>
public static class DbQuery
{
    /// <summary>
    /// Executes a SELECT query and maps each row of the result set to an instance
    /// of <typeparamref name="T"/> using the provided mapping function.
    /// </summary>
    /// <typeparam name="T">The type each row is mapped to.</typeparam>
    /// <param name="conn">An open <see cref="SqlConnection"/>.</param>
    /// <param name="sql">The SQL query text. Use named parameters (e.g. <c>@Id</c>).</param>
    /// <param name="map">
    /// A function that receives the current row of a <see cref="SqlDataReader"/>
    /// and returns a <typeparamref name="T"/>. Use the extension methods on
    /// <see cref="DbReader"/> for null-safe column access.
    /// </param>
    /// <param name="parameterize">
    /// Optional. An action that adds <see cref="SqlParameter"/> values to the command,
    /// e.g. <c>cmd => cmd.Parameters.AddWithValue("@Id", id)</c>.
    /// </param>
    /// <param name="tx">
    /// Optional. A <see cref="SqlTransaction"/> to enlist this command in.
    /// </param>
    /// <returns>
    /// <c>Right(Seq&lt;T&gt;)</c> containing all mapped rows (empty if none matched),
    /// or <c>Left(Error)</c> if an exception occurred.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await Db.WithConnection(conn =>
    ///     DbQuery.QueryMany(conn,
    ///         "SELECT * FROM Customers WHERE Active = 1",
    ///         r => new Customer(r.Int("Id"), r.Str("Name"))));
    /// </code>
    /// </example>
    public static async Task<Either<Error, Seq<T>>> QueryMany<T>(
        SqlConnection conn,
        string sql,
        Func<SqlDataReader, T> map,
        Action<SqlCommand>? parameterize = null,
        SqlTransaction? tx = null)
    {
        try
        {
            await using var cmd = new SqlCommand(sql, conn, tx);
            parameterize?.Invoke(cmd);

            await using var reader = await cmd.ExecuteReaderAsync();
            var results = new List<T>();

            while (await reader.ReadAsync())
                results.Add(map(reader));

            return Right(Seq(results.AsEnumerable()));
        }
        catch (Exception ex)
        {
            return Left(Error.New(ex));
        }
    }

    /// <summary>
    /// Executes a SELECT query and maps the first row of the result set to an instance
    /// of <typeparamref name="T"/>. Remaining rows, if any, are ignored.
    /// </summary>
    /// <typeparam name="T">The type the row is mapped to.</typeparam>
    /// <param name="conn">An open <see cref="SqlConnection"/>.</param>
    /// <param name="sql">
    /// The SQL query text. Use named parameters (e.g. <c>@Id</c>). Consider adding
    /// <c>TOP 1</c> to avoid reading unnecessary rows from the server.
    /// </param>
    /// <param name="map">
    /// A function that receives the first row of a <see cref="SqlDataReader"/>
    /// and returns a <typeparamref name="T"/>.
    /// </param>
    /// <param name="parameterize">
    /// Optional. An action that adds <see cref="SqlParameter"/> values to the command.
    /// </param>
    /// <param name="tx">
    /// Optional. A <see cref="SqlTransaction"/> to enlist this command in.
    /// </param>
    /// <returns>
    /// <c>Right(Some(T))</c> if a row was found, <c>Right(None)</c> if the query
    /// returned no rows, or <c>Left(Error)</c> if an exception occurred.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await Db.WithConnection(conn =>
    ///     DbQuery.QuerySingle(conn,
    ///         "SELECT * FROM Customers WHERE Id = @Id",
    ///         r => new Customer(r.Int("Id"), r.Str("Name")),
    ///         cmd => cmd.Parameters.AddWithValue("@Id", customerId)));
    ///
    /// result.Match(
    ///     Right: opt => opt.Match(Some: c => Show(c), None: () => Show404()),
    ///     Left:  err => ShowError(err.Message));
    /// </code>
    /// </example>
    public static async Task<Either<Error, Option<T>>> QuerySingle<T>(
        SqlConnection conn,
        string sql,
        Func<SqlDataReader, T> map,
        Action<SqlCommand>? parameterize = null,
        SqlTransaction? tx = null)
    {
        try
        {
            await using var cmd = new SqlCommand(sql, conn, tx);
            parameterize?.Invoke(cmd);

            await using var reader = await cmd.ExecuteReaderAsync();

            return Right(await reader.ReadAsync()
                ? Some(map(reader))
                : None);
        }
        catch (Exception ex)
        {
            return Left(Error.New(ex));
        }
    }

    /// <summary>
    /// Executes a non-query SQL statement (INSERT, UPDATE, DELETE, etc.) and returns
    /// the number of rows affected.
    /// </summary>
    /// <param name="conn">An open <see cref="SqlConnection"/>.</param>
    /// <param name="sql">
    /// The SQL statement text. Use named parameters (e.g. <c>@Name</c>).
    /// </param>
    /// <param name="parameterize">
    /// Optional. An action that adds <see cref="SqlParameter"/> values to the command.
    /// </param>
    /// <param name="tx">
    /// Optional. A <see cref="SqlTransaction"/> to enlist this command in.
    /// Required when calling from within <see cref="Db.WithTransaction{T}"/>.
    /// </param>
    /// <returns>
    /// <c>Right(int)</c> with the number of affected rows, or <c>Left(Error)</c>
    /// if an exception occurred.
    /// </returns>
    /// <example>
    /// <code>
    /// var result = await Db.WithConnection(conn =>
    ///     DbQuery.Execute(conn,
    ///         "DELETE FROM Customers WHERE Id = @Id",
    ///         cmd => cmd.Parameters.AddWithValue("@Id", id)));
    /// </code>
    /// </example>
    public static async Task<Either<Error, int>> Execute(
        SqlConnection conn,
        string sql,
        Action<SqlCommand>? parameterize = null,
        SqlTransaction? tx = null)
    {
        try
        {
            await using var cmd = new SqlCommand(sql, conn, tx);
            parameterize?.Invoke(cmd);
            return Right(await cmd.ExecuteNonQueryAsync());
        }
        catch (Exception ex)
        {
            return Left(Error.New(ex));
        }
    }

    /// <summary>
    /// Executes a SQL statement and returns the first column of the first row as
    /// <typeparamref name="T"/>. Useful for scalar results such as newly inserted
    /// identity values or aggregate counts.
    /// </summary>
    /// <typeparam name="T">
    /// The expected type of the scalar result. A type mismatch returns
    /// <c>Left(Error)</c> rather than throwing.
    /// </typeparam>
    /// <param name="conn">An open <see cref="SqlConnection"/>.</param>
    /// <param name="sql">
    /// The SQL statement text. For inserts, use <c>OUTPUT INSERTED.Id</c> to
    /// return the new identity value in a single round-trip.
    /// </param>
    /// <param name="parameterize">
    /// Optional. An action that adds <see cref="SqlParameter"/> values to the command.
    /// </param>
    /// <param name="tx">
    /// Optional. A <see cref="SqlTransaction"/> to enlist this command in.
    /// </param>
    /// <returns>
    /// <c>Right(T)</c> on success, or <c>Left(Error)</c> if an exception occurred
    /// or the result could not be cast to <typeparamref name="T"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// // Insert a row and get back the generated Id in one round-trip
    /// var result = await Db.WithConnection(conn =>
    ///     DbQuery.Scalar&lt;int&gt;(conn,
    ///         "INSERT INTO Customers (Name) OUTPUT INSERTED.Id VALUES (@Name)",
    ///         cmd => cmd.Parameters.AddWithValue("@Name", name)));
    /// </code>
    /// </example>
    public static async Task<Either<Error, T>> Scalar<T>(
        SqlConnection conn,
        string sql,
        Action<SqlCommand>? parameterize = null,
        SqlTransaction? tx = null)
    {
        try
        {
            await using var cmd = new SqlCommand(sql, conn, tx);
            parameterize?.Invoke(cmd);
            var result = await cmd.ExecuteScalarAsync();
            return result is T t
                ? Right(t)
                : Left(Error.New($"Unexpected scalar type: expected {typeof(T).Name}, got {result?.GetType().Name ?? "null"}."));
        }
        catch (Exception ex)
        {
            return Left(Error.New(ex));
        }
    }
}
