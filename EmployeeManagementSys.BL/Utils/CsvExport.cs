using System.Text;

namespace EmployeeManagementSys.BL.Utils
{
    /// <summary>
    /// Minimal RFC-4180 CSV assembly. Kept dependency-free on purpose
    /// (see docs/agdr/AgDR-0002-attendance-report-csv.md). Escaping lives here
    /// so it is tested once and reused by every export.
    /// </summary>
    public static class CsvExport
    {
        /// <summary>
        /// Escapes a single field per RFC 4180: wrap in double quotes when the
        /// value contains a comma, double quote, CR, or LF, doubling any inner
        /// quotes. Null becomes an empty field.
        ///
        /// Also neutralizes CSV formula injection (CWE-1236): a value beginning
        /// with = + - @ (or tab/CR) is prefixed with a single quote so Excel/
        /// Sheets treats it as text rather than executing it as a formula.
        /// </summary>
        public static string Field(string? value)
        {
            value ??= string.Empty;

            if (value.Length > 0 && "=+-@\t\r".IndexOf(value[0]) >= 0)
            {
                value = "'" + value;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>Joins already-escaped-or-plain fields into one CSV row.</summary>
        public static string Row(params string?[] fields)
            => string.Join(",", fields.Select(Field));

        /// <summary>
        /// Builds a full CSV document (header + rows) and returns UTF-8 bytes
        /// with a BOM so Excel opens non-ASCII (e.g. Arabic names) correctly.
        /// </summary>
        public static byte[] ToUtf8Bytes(IEnumerable<string> lines)
        {
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                sb.Append(line).Append("\r\n");
            }
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(sb.ToString());
        }
    }
}
