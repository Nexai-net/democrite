// Copyright (c) Nexai.
// The Democrite licenses this file to you under the MIT license.
// Produce by nexai & community (cf. docs/Teams.md)

namespace Democrite.Framework.Toolbox.UnitTests.Extensions
{
    using NFluent;

    /// <summary>
    /// Test for <see cref="ExceptionExtensions"/>
    /// </summary>
    public class ExceptionExtensionUTest
    {
        /// <summary>
        /// Test <see cref="ExceptionExtensions.GetFullString"/> with a simple exception
        /// </summary>
        [Fact]
        public void GetFullString_Simple()
        {
            const string MESSAGE = "Simple Test Message";
            var ex = new Exception(MESSAGE);

            var str = ExceptionExtensions.GetFullString(ex);

            Check.That(str).IsNotNull().And.IsEqualTo(ex.GetType().Name + " : " + MESSAGE);
        }

        /// <summary>
        /// Test <see cref="ExceptionExtensions.GetFullString"/> with a simple exception
        /// </summary>
        [Fact]
        public void GetFullString_Simple_WithData()
        {
            const string MESSAGE = "Simple Test Message";
            var ex = new Exception(MESSAGE);

            var data = new Dictionary<object, object>()
            {
                { "Arg1", 42 },
                { 42.5d, "Arg2Value" }
            };

            foreach (var kv in data)
                ex.Data.Add(kv.Key, kv.Value);  

            // Get Full String without data
            var str = ExceptionExtensions.GetFullString(ex);
            
            var expectedMessage = ex.GetType().Name + " : " + MESSAGE;
            Check.That(str).IsNotNull().And.IsEqualTo(expectedMessage);

            // Get Full String with data

            str = ExceptionExtensions.GetFullString(ex, includeData: true);

            expectedMessage += Environment.NewLine;
            expectedMessage += "-- Data ({0}): --".WithArguments(data.Count);

            /*
             * Loop through keys to have the same order that GetFullString that are based only on IDictionary type
             */
            foreach (var key in data.Keys)
            {
                var value = data[key];
                expectedMessage += Environment.NewLine;
                expectedMessage += "  {0} : {1}".WithArguments(key, value?.ToString() ?? "null");
            }

            Check.That(str).IsNotNull().And.IsEqualTo(expectedMessage);
        }

        /// <summary>
        /// Test <see cref="ExceptionExtensions.GetFullString"/> with a exception and inner
        /// </summary>
        [Fact]
        public void GetFullString_With_Inner()
        {
            const string MESSAGE = "Simple Test Message";
            const string INNER_MESSAGE = "Inner Simple\nTest Message";

            var ex = new Exception(MESSAGE, new InvalidDataException(INNER_MESSAGE));

            var str = ExceptionExtensions.GetFullString(ex);

            var expectedMessage = ex.GetType().Name + " : " + MESSAGE;
            expectedMessage += Environment.NewLine;
            expectedMessage += "    " + ex.InnerException!.GetType().Name + " : " + INNER_MESSAGE.Replace("\n", "\n        ");

            Check.That(str).IsNotNull().And.IsEqualTo(expectedMessage);
        }

        /// <summary>
        /// Test <see cref="ExceptionExtensions.GetFullString"/> with an exception with inner exception and data
        /// </summary>
        [Fact]
        public void GetFullString_Simple_WithData_With_Inner()
        {
            const string MESSAGE = "Simple Test Message";
            const string INNER_MESSAGE = "Inner Simple\nTest Message";

            var ex = new Exception(MESSAGE, new InvalidCastException(INNER_MESSAGE));

            var data = new Dictionary<object, object>()
            {
                { "Arg1", 42 },
                { 42.5d, "Arg2Value" }
            };

            foreach (var kv in data)
            {
                ex.Data.Add(kv.Key, kv.Value);
                ex.InnerException!.Data.Add(kv.Key, kv.Value);
            }

            // Get Full String without data
            var str = ExceptionExtensions.GetFullString(ex);

            var expectedMessage = ex.GetType().Name + " : " + MESSAGE;
            expectedMessage += Environment.NewLine;
            expectedMessage += "    " + ex.InnerException!.GetType().Name + " : " + INNER_MESSAGE.Replace("\n", "\n        ");

            Check.That(str).IsNotNull().And.IsEqualTo(expectedMessage);

            // Get Full String with data

            str = ExceptionExtensions.GetFullString(ex, includeData: true);

            // Reset
            expectedMessage = FormatExceptionWithData(MESSAGE, ex, data);
            expectedMessage += Environment.NewLine + "    " + FormatExceptionWithData(INNER_MESSAGE.Replace("\n", "\n    "), ex.InnerException!, data).Replace("\n", "\n    ");

            Check.That(str).IsNotNull().And.IsEqualTo(expectedMessage);
        }

        private static string FormatExceptionWithData(string MESSAGE, Exception ex, Dictionary<object, object> data)
        {
            var expectedMessage = ex.GetType().Name + " : " + MESSAGE;
            expectedMessage += Environment.NewLine;
            expectedMessage += "-- Data ({0}): --".WithArguments(data.Count);

            /*
             * Loop through keys to have the same order that GetFullString that are based only on IDictionary type
             */
            foreach (var key in data.Keys)
            {
                var value = data[key];
                expectedMessage += Environment.NewLine;
                expectedMessage += "  {0} : {1}".WithArguments(key, value?.ToString() ?? "null");
            }

            return expectedMessage;
        }

        /// <summary>
        /// Test <see cref="ExceptionExtensions.GetFullString"/> with a null exception
        /// </summary>
        [Fact]
        public void GetFullString_Null_Exception()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            var str = ExceptionExtensions.GetFullString((Exception)null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            Check.That(str).IsNotNull().And.IsEqualTo(string.Empty);
        }
    }
}
