using System.Text;
using EmployeeManagementSys.BL.Utils;
using Xunit;

namespace EmployeeManagementSys.Tests;

public class CsvExportTests
{
    [Theory]
    [InlineData("Alice", "Alice")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Field_PlainValues_PassThrough(string? input, string expected)
        => Assert.Equal(expected, CsvExport.Field(input));

    [Fact]
    public void Field_WithComma_IsQuoted()
        => Assert.Equal("\"Doe, John\"", CsvExport.Field("Doe, John"));

    [Fact]
    public void Field_WithEmbeddedQuote_IsDoubledAndWrapped()
        => Assert.Equal("\"She said \"\"hi\"\"\"", CsvExport.Field("She said \"hi\""));

    [Fact]
    public void Field_WithNewline_IsQuoted()
        => Assert.Equal("\"line1\nline2\"", CsvExport.Field("line1\nline2"));

    [Theory]
    [InlineData("=1+1", "'=1+1")]
    [InlineData("+cmd", "'+cmd")]
    [InlineData("-2", "'-2")]
    [InlineData("@SUM", "'@SUM")]
    public void Field_FormulaInjection_IsNeutralized(string input, string expected)
        => Assert.Equal(expected, CsvExport.Field(input));

    [Fact]
    public void Field_FormulaPlusSpecialChar_IsPrefixedAndWrapped()
        => Assert.Equal("\"'=a,b\"", CsvExport.Field("=a,b"));

    [Fact]
    public void Row_JoinsEscapedFields()
        => Assert.Equal("Alice,\"Doe, John\",'=x", CsvExport.Row("Alice", "Doe, John", "=x"));

    [Fact]
    public void ToUtf8Bytes_EmitsBomAndCrlfRows()
    {
        var bytes = CsvExport.ToUtf8Bytes(new[] { "a,b", "c,d" });

        // UTF-8 BOM prefix
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3).ToArray());

        var text = new UTF8Encoding(true).GetString(bytes);
        Assert.Equal("﻿a,b\r\nc,d\r\n", text);
    }
}
