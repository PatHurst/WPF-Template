using System.Data.Common;

using LanguageExt;

using static LanguageExt.Prelude;

namespace WPFTemplate.Services.Database;

/// <summary>
/// Provides strongly-typed, null-safe extension methods for reading column values
/// from a <see cref="DbDataReader"/>.
/// </summary>
/// <remarks>
/// <para>
/// Methods without the <c>Opt</c> prefix assume the column is NOT NULL in the schema
/// and will throw if the value is <see cref="DBNull"/>. Use these for required columns.
/// </para>
/// <para>
/// Methods prefixed with <c>Opt</c> handle nullable columns and return
/// <see cref="Option{T}"/>: <c>Some(value)</c> when a value is present,
/// or <c>None</c> when the column is <c>NULL</c>.
/// </para>
/// <para>
/// Example usage inside a mapping function:
/// <code>
/// private static Customer Map(DbDataReader r) => new(
///     r.Int("Id"),
///     r.Str("Name"),
///     r.Str("Email"),
///     r.OptStr("Phone"),           // nullable in DB → Option&lt;string&gt;
///     r.OptDateTime("DeletedAt")   // nullable in DB → Option&lt;DateTime&gt;
/// );
/// </code>
/// </para>
/// </remarks>
public static class DbReader
{
    // -------------------------------------------------------------------------
    // Generic access
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads a non-null column value and casts it to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The expected CLR type of the column.</typeparam>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value cast to <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidCastException">
    /// Thrown if the value cannot be cast to <typeparamref name="T"/>.
    /// </exception>
    public static T Get<T>(this DbDataReader r, string col) => (T)r[col];

    /// <summary>
    /// Reads a nullable column value and wraps it in <see cref="Option{T}"/>.
    /// Returns <c>None</c> if the column is <see cref="DBNull"/> or null.
    /// </summary>
    /// <typeparam name="T">The expected CLR type of the column.</typeparam>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(value)</c> if the column has a value; otherwise <c>None</c>.</returns>
    public static Option<T> GetOptional<T>(this DbDataReader r, string col)
    {
        var value = r[col];
        return value is DBNull or null ? None : Some((T)value);
    }

    // -------------------------------------------------------------------------
    // Required (non-null) column readers
    // -------------------------------------------------------------------------

    /// <summary>Reads a required <see cref="string"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value as a <see cref="string"/>.</returns>
    public static string Str(this DbDataReader r, string col) => r.Get<string>(col);

    /// <summary>Reads a required <see cref="int"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value as an <see cref="int"/>.</returns>
    public static int Int(this DbDataReader r, string col) => r.Get<int>(col);

    /// <summary>Reads a required <see cref="long"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value as a <see cref="long"/>.</returns>
    public static long Long(this DbDataReader r, string col) => r.Get<long>(col);

    /// <summary>Reads a required <see cref="bool"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value as a <see cref="bool"/>.</returns>
    public static bool Bool(this DbDataReader r, string col) => r.Get<bool>(col);

    /// <summary>Reads a required <see cref="DateTime"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value as a <see cref="DateTime"/>.</returns>
    public static DateTime DateTime(this DbDataReader r, string col) => r.Get<DateTime>(col);

    /// <summary>Reads a required <see cref="Guid"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value as a <see cref="Guid"/>.</returns>
    public static Guid Guid(this DbDataReader r, string col) => r.Get<Guid>(col);

    /// <summary>Reads a required <see cref="decimal"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value as a <see cref="decimal"/>.</returns>
    public static decimal Decimal(this DbDataReader r, string col) => r.Get<decimal>(col);

    /// <summary>Reads a required <see cref="double"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value as a <see cref="double"/>.</returns>
    public static double Double(this DbDataReader r, string col) => r.Get<double>(col);

    /// <summary>Reads a required <see cref="byte"/> array column (e.g. <c>VARBINARY</c>).</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns>The column value as a <see cref="byte"/> array.</returns>
    public static byte[] Bytes(this DbDataReader r, string col) => r.Get<byte[]>(col);

    // -------------------------------------------------------------------------
    // Optional (nullable) column readers → Option<T>
    // -------------------------------------------------------------------------

    /// <summary>Reads a nullable <see cref="string"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(string)</c> if not NULL; otherwise <c>None</c>.</returns>
    public static Option<string> OptStr(this DbDataReader r, string col) =>
        r.GetOptional<string>(col);

    /// <summary>Reads a nullable <see cref="int"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(int)</c> if not NULL; otherwise <c>None</c>.</returns>
    public static Option<int> OptInt(this DbDataReader r, string col) =>
        r.GetOptional<int>(col);

    /// <summary>Reads a nullable <see cref="long"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(long)</c> if not NULL; otherwise <c>None</c>.</returns>
    public static Option<long> OptLong(this DbDataReader r, string col) =>
        r.GetOptional<long>(col);

    /// <summary>Reads a nullable <see cref="bool"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(bool)</c> if not NULL; otherwise <c>None</c>.</returns>
    public static Option<bool> OptBool(this DbDataReader r, string col) =>
        r.GetOptional<bool>(col);

    /// <summary>Reads a nullable <see cref="DateTime"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(DateTime)</c> if not NULL; otherwise <c>None</c>.</returns>
    public static Option<DateTime> OptDateTime(this DbDataReader r, string col) =>
        r.GetOptional<DateTime>(col);

    /// <summary>Reads a nullable <see cref="Guid"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(Guid)</c> if not NULL; otherwise <c>None</c>.</returns>
    public static Option<Guid> OptGuid(this DbDataReader r, string col) =>
        r.GetOptional<Guid>(col);

    /// <summary>Reads a nullable <see cref="decimal"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(decimal)</c> if not NULL; otherwise <c>None</c>.</returns>
    public static Option<decimal> OptDecimal(this DbDataReader r, string col) =>
        r.GetOptional<decimal>(col);

    /// <summary>Reads a nullable <see cref="double"/> column.</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(double)</c> if not NULL; otherwise <c>None</c>.</returns>
    public static Option<double> OptDouble(this DbDataReader r, string col) =>
        r.GetOptional<double>(col);

    /// <summary>Reads a nullable <see cref="byte"/> array column (e.g. <c>VARBINARY</c>).</summary>
    /// <param name="r">The current <see cref="DbDataReader"/> row.</param>
    /// <param name="col">The column name.</param>
    /// <returns><c>Some(byte[])</c> if not NULL; otherwise <c>None</c>.</returns>
    public static Option<byte[]> OptBytes(this DbDataReader r, string col) =>
        r.GetOptional<byte[]>(col);
}
