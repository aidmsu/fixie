namespace Fixie.Assertions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using static System.Environment;

    public class AssertActualExpectedException : AssertException
    {
        public AssertActualExpectedException(object expected, object actual)
            : base(BuildMessage(expected, actual))
        {
        }

        public AssertActualExpectedException(object expected, object actual, string userMessage)
            : base(BuildMessage(expected, actual, userMessage))
        {
        }

        static string BuildMessage(object expected, object actual, string userMessage = null)
        {
            var message = new StringBuilder();

            if (userMessage != null)
            {
                message.AppendLine(userMessage);
                message.AppendLine();
            }

            if (actual is IEnumerable enumerableActual && expected is IEnumerable enumerableExpected)
            {
                var comparer = new EnumerableEqualityComparer();
                comparer.Equals(enumerableActual, enumerableExpected);

                message.AppendLine("First difference is at position " + comparer.Position);
            }

            var actualStr = actual == null ? null : ConvertToString(actual);
            var expectedStr = expected == null ? null : ConvertToString(expected);

            if (actual != null &&
                expected != null &&
                actual.ToString() == expected.ToString() &&
                actual.GetType() != expected.GetType())
            {
                actualStr += $" ({actual.GetType().FullName})";
                expectedStr += $" ({expected.GetType().FullName})";
            }
            
            message.AppendLine($"Expected: {FormatMultiLine(expectedStr ?? "(null)")}");
            message.Append($"Actual:   {FormatMultiLine(actualStr ?? "(null)")}");

            return message.ToString();
        }

        static string ConvertToString(object value)
        {
            if (value is Array valueArray)
            {
                var valueStrings = new List<string>();

                foreach (var valueObject in valueArray)
                    valueStrings.Add(valueObject?.ToString() ?? "(null)");

                return value.GetType().FullName +
                       $" {{{NewLine}{String.Join("," + NewLine, valueStrings.ToArray())}{NewLine}}}";
            }

            return value.ToString();
        }

        static string FormatMultiLine(string value)
            => value.Replace(NewLine, NewLine + "          ");
    }
}