using Xunit;
using ECEngine.AST;
using ECEngine.Runtime;
using System.IO;
using RuntimeInterpreter = ECEngine.Runtime.Interpreter;

namespace ECEngine.Tests.Runtime;

public class DateObjectTests
{
    #region Static Methods Tests

    [Fact]
    public void Evaluate_DateNow_ReturnsCurrentTimestamp()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var memberExpression = new MemberExpression(dateIdentifier, "now");
        var callExpression = new CallExpression(memberExpression, new List<Expression>());
        var timestampBefore = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Act
        var result = interpreter.Evaluate(callExpression, "Date.now()");

        // Assert
        Assert.IsType<double>(result);
        var timestamp = (double)result;
        var timestampAfter = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        Assert.True(timestamp >= timestampBefore);
        Assert.True(timestamp <= timestampAfter + 1000); // Allow 1 second tolerance
    }

    [Fact]
    public void Evaluate_DateParse_ReturnsCorrectTimestamp()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var memberExpression = new MemberExpression(dateIdentifier, "parse");
        var dateString = new StringLiteral("2023-12-25T10:30:00.000Z");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { dateString });

        // Act
        var result = interpreter.Evaluate(callExpression, "Date.parse('2023-12-25T10:30:00.000Z')");

        // Assert
        Assert.IsType<double>(result);
        var timestamp = (double)result;
        
        // December 25, 2023 10:30:00 UTC
        var expectedDate = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc);
        var expectedTimestamp = ((DateTimeOffset)expectedDate).ToUnixTimeMilliseconds();
        
        Assert.Equal(expectedTimestamp, timestamp);
    }

    [Fact]
    public void Evaluate_DateUTC_ReturnsCorrectTimestamp()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var memberExpression = new MemberExpression(dateIdentifier, "UTC");
        var args = new List<Expression>
        {
            new NumberLiteral(2023),    // year
            new NumberLiteral(11),      // month (0-indexed, so 11 = December)
            new NumberLiteral(25),      // day
            new NumberLiteral(10),      // hour
            new NumberLiteral(30),      // minute
            new NumberLiteral(0),       // second
            new NumberLiteral(0)        // millisecond
        };
        var callExpression = new CallExpression(memberExpression, args);

        // Act
        var result = interpreter.Evaluate(callExpression, "Date.UTC(2023, 11, 25, 10, 30, 0, 0)");

        // Assert
        Assert.IsType<double>(result);
        var timestamp = (double)result;
        
        // December 25, 2023 10:30:00 UTC
        var expectedDate = new DateTime(2023, 12, 25, 10, 30, 0, DateTimeKind.Utc);
        var expectedTimestamp = ((DateTimeOffset)expectedDate).ToUnixTimeMilliseconds();
        
        Assert.Equal(expectedTimestamp, timestamp);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Evaluate_DateConstructorWithNoArgs_ReturnsDateObject()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var callExpression = new CallExpression(dateIdentifier, new List<Expression>());
        var beforeTime = DateTime.Now; // Use local time since DateConstructorFunction uses DateTime.Now

        // Act
        var result = interpreter.Evaluate(callExpression, "Date()");

        // Assert
        Assert.IsType<DateObject>(result);
        var dateObj = (DateObject)result;
        var afterTime = DateTime.Now; // Use local time for comparison
        
        Assert.True(dateObj.DateTime >= beforeTime.AddSeconds(-1));
        Assert.True(dateObj.DateTime <= afterTime.AddSeconds(1));
    }

    [Fact]
    public void Evaluate_DateConstructorWithTimestamp_ReturnsCorrectDate()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var timestamp = 1703505000000; // December 25, 2023 10:30:00 UTC in milliseconds
        var args = new List<Expression> { new NumberLiteral(timestamp) };
        var callExpression = new CallExpression(dateIdentifier, args);

        // Act
        var result = interpreter.Evaluate(callExpression, "Date(1703505000000)");

        // Assert
        Assert.IsType<DateObject>(result);
        var dateObj = (DateObject)result;
        
        var expectedDate = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;
        Assert.Equal(expectedDate.Year, dateObj.DateTime.Year);
        Assert.Equal(expectedDate.Month, dateObj.DateTime.Month);
        Assert.Equal(expectedDate.Day, dateObj.DateTime.Day);
        Assert.Equal(expectedDate.Hour, dateObj.DateTime.Hour);
        Assert.Equal(expectedDate.Minute, dateObj.DateTime.Minute);
    }

    [Theory]
    [InlineData(2023, 11, 25)] // December 25, 2023 (month is 0-indexed)
    [InlineData(2024, 0, 1)]   // January 1, 2024
    [InlineData(2022, 5, 15)]  // June 15, 2022
    public void Evaluate_DateConstructorWithYearMonthDay_ReturnsCorrectDate(int year, int month, int day)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression>
        {
            new NumberLiteral(year),
            new NumberLiteral(month),
            new NumberLiteral(day)
        };
        var callExpression = new CallExpression(dateIdentifier, args);

        // Act
        var result = interpreter.Evaluate(callExpression, $"Date({year}, {month}, {day})");

        // Assert
        Assert.IsType<DateObject>(result);
        var dateObj = (DateObject)result;
        Assert.Equal(year, dateObj.DateTime.Year);
        Assert.Equal(month + 1, dateObj.DateTime.Month); // Convert from 0-indexed to 1-indexed
        Assert.Equal(day, dateObj.DateTime.Day);
    }

    [Fact]
    public void Evaluate_DateConstructorWithFullArgs_ReturnsCorrectDate()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression>
        {
            new NumberLiteral(2023),    // year
            new NumberLiteral(11),      // month (0-indexed)
            new NumberLiteral(25),      // day
            new NumberLiteral(14),      // hour
            new NumberLiteral(30),      // minute
            new NumberLiteral(45),      // second
            new NumberLiteral(123)      // millisecond
        };
        var callExpression = new CallExpression(dateIdentifier, args);

        // Act
        var result = interpreter.Evaluate(callExpression, "Date(2023, 11, 25, 14, 30, 45, 123)");

        // Assert
        Assert.IsType<DateObject>(result);
        var dateObj = (DateObject)result;
        Assert.Equal(2023, dateObj.DateTime.Year);
        Assert.Equal(12, dateObj.DateTime.Month); // December (converted from 0-indexed)
        Assert.Equal(25, dateObj.DateTime.Day);
        Assert.Equal(14, dateObj.DateTime.Hour);
        Assert.Equal(30, dateObj.DateTime.Minute);
        Assert.Equal(45, dateObj.DateTime.Second);
        Assert.Equal(123, dateObj.DateTime.Millisecond);
    }

    #endregion

    #region Instance Method Tests

    [Fact]
    public void Evaluate_DateGetTime_ReturnsTimestamp()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var timestamp = 1703505000000; // December 25, 2023 10:30:00 UTC
        var dateIdentifier = new Identifier("Date");
        var dateConstructor = new CallExpression(dateIdentifier, new List<Expression> { new NumberLiteral(timestamp) });
        
        // First create the date object
        var dateObject = interpreter.Evaluate(dateConstructor, "Date(1703505000000)");
        Assert.IsType<DateObject>(dateObject);
        
        // Now test getTime method
        var getTimeMember = new MemberExpression(new Identifier("testDate"), "getTime");
        var getTimeCall = new CallExpression(getTimeMember, new List<Expression>());
        
        // Set the date object as a variable for testing
        var dateObj = (DateObject)dateObject;
        
        // Act - Call getTime directly on the DateObject
        var result = dateObj.GetTime(new List<object?>());

        // Assert
        Assert.IsType<double>(result);
        Assert.Equal((double)timestamp, (double)result);
    }

    [Fact]
    public void Evaluate_DateGetFullYear_ReturnsCorrectYear()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression>
        {
            new NumberLiteral(2023),
            new NumberLiteral(11),
            new NumberLiteral(25)
        };
        var dateConstructor = new CallExpression(dateIdentifier, args);
        
        // Act
        var dateObject = interpreter.Evaluate(dateConstructor, "Date(2023, 11, 25)");
        Assert.IsType<DateObject>(dateObject);
        
        var dateObj = (DateObject)dateObject;
        var result = dateObj.GetFullYear(new List<object?>());

        // Assert
        Assert.Equal(2023.0, result);
    }

    [Theory]
    [InlineData(0, 0)]   // January -> 0
    [InlineData(5, 5)]   // June -> 5  
    [InlineData(11, 11)] // December -> 11
    public void Evaluate_DateGetMonth_ReturnsZeroIndexedMonth(int inputMonth, int expectedMonth)
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression>
        {
            new NumberLiteral(2023),
            new NumberLiteral(inputMonth),
            new NumberLiteral(15)
        };
        var dateConstructor = new CallExpression(dateIdentifier, args);
        
        // Act
        var dateObject = interpreter.Evaluate(dateConstructor, $"Date(2023, {inputMonth}, 15)");
        Assert.IsType<DateObject>(dateObject);
        
        var dateObj = (DateObject)dateObject;
        var result = dateObj.GetMonth(new List<object?>());

        // Assert
        Assert.Equal((double)expectedMonth, result);
    }

    [Fact]
    public void Evaluate_DateToString_ReturnsFormattedString()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression>
        {
            new NumberLiteral(2023),
            new NumberLiteral(11),
            new NumberLiteral(25)
        };
        var dateConstructor = new CallExpression(dateIdentifier, args);
        
        // Act
        var dateObject = interpreter.Evaluate(dateConstructor, "Date(2023, 11, 25)");
        Assert.IsType<DateObject>(dateObject);
        
        var dateObj = (DateObject)dateObject;
        var result = dateObj.ToString(new List<object?>());

        // Assert
        Assert.IsType<string>(result);
        var dateString = (string)result;
        Assert.Contains("2023", dateString);
        Assert.Contains("25", dateString); // Day should be present
        // Note: Different systems may format dates differently, so we check for key components
    }

    [Fact]
    public void Evaluate_DateToISOString_ReturnsISOFormat()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var timestamp = 1703505000000; // December 25, 2023 10:30:00 UTC
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression> { new NumberLiteral(timestamp) };
        var dateConstructor = new CallExpression(dateIdentifier, args);
        
        // Act
        var dateObject = interpreter.Evaluate(dateConstructor, "Date(1703505000000)");
        Assert.IsType<DateObject>(dateObject);
        
        var dateObj = (DateObject)dateObject;
        var result = dateObj.ToISOString(new List<object?>());

        // Assert
        Assert.IsType<string>(result);
        var isoString = (string)result;
        Assert.Contains("2023-12-25", isoString);
        Assert.EndsWith("Z", isoString);
        // Note: Time might be adjusted for local timezone, so we check key components
    }

    #endregion

    #region UTC Method Tests

    [Fact]
    public void Evaluate_DateGetUTCFullYear_ReturnsCorrectUTCYear()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var timestamp = 1703505000000; // December 25, 2023 10:30:00 UTC
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression> { new NumberLiteral(timestamp) };
        var dateConstructor = new CallExpression(dateIdentifier, args);
        
        // Act
        var dateObject = interpreter.Evaluate(dateConstructor, "Date(1703505000000)");
        Assert.IsType<DateObject>(dateObject);
        
        var dateObj = (DateObject)dateObject;
        var result = dateObj.GetUTCFullYear(new List<object?>());

        // Assert
        Assert.Equal(2023.0, result);
    }

    [Fact]
    public void Evaluate_DateGetUTCMonth_ReturnsCorrectUTCMonth()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var timestamp = 1703505000000; // December 25, 2023 10:30:00 UTC
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression> { new NumberLiteral(timestamp) };
        var dateConstructor = new CallExpression(dateIdentifier, args);
        
        // Act
        var dateObject = interpreter.Evaluate(dateConstructor, "Date(1703505000000)");
        Assert.IsType<DateObject>(dateObject);
        
        var dateObj = (DateObject)dateObject;
        var result = dateObj.GetUTCMonth(new List<object?>());

        // Assert
        Assert.Equal(11.0, result); // December is month 11 (0-indexed)
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void Evaluate_DateParseWithInvalidString_ReturnsNaN()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var memberExpression = new MemberExpression(dateIdentifier, "parse");
        var invalidString = new StringLiteral("invalid-date-string");
        var callExpression = new CallExpression(memberExpression, new List<Expression> { invalidString });

        // Act
        var result = interpreter.Evaluate(callExpression, "Date.parse('invalid-date-string')");

        // Assert
        Assert.IsType<double>(result);
        Assert.True(double.IsNaN((double)result));
    }

    [Fact]
    public void Evaluate_DateConstructorWithInvalidArgs_HandlesGracefully()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression>
        {
            new NumberLiteral(2023),
            new NumberLiteral(13), // Invalid month (should be 0-11)
            new NumberLiteral(32)  // Invalid day
        };
        var callExpression = new CallExpression(dateIdentifier, args);

        // Act & Assert
        // Should not throw an exception, DateTime constructor handles invalid values
        var result = interpreter.Evaluate(callExpression, "Date(2023, 13, 32)");
        Assert.IsType<DateObject>(result);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Performance_CreateManyDateObjects_CompletesInReasonableTime()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 1000;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var dateIdentifier = new Identifier("Date");
            var callExpression = new CallExpression(dateIdentifier, new List<Expression>());
            interpreter.Evaluate(callExpression, "Date()");
        }
        
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 2000, 
            $"Creating {iterations} Date objects took {stopwatch.ElapsedMilliseconds}ms, expected less than 2000ms");
    }

    [Fact]
    public void Performance_DateNowCalls_CompletesInReasonableTime()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        const int iterations = 1000;

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var dateIdentifier = new Identifier("Date");
            var memberExpression = new MemberExpression(dateIdentifier, "now");
            var callExpression = new CallExpression(memberExpression, new List<Expression>());
            interpreter.Evaluate(callExpression, "Date.now()");
        }
        
        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Calling Date.now() {iterations} times took {stopwatch.ElapsedMilliseconds}ms, expected less than 1000ms");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Integration_DateObjectWorkflow_AllMethodsWork()
    {
        // Arrange
        var interpreter = new RuntimeInterpreter();
        var dateIdentifier = new Identifier("Date");
        var args = new List<Expression>
        {
            new NumberLiteral(2023),
            new NumberLiteral(11),
            new NumberLiteral(25),
            new NumberLiteral(14),
            new NumberLiteral(30),
            new NumberLiteral(45)
        };
        var dateConstructor = new CallExpression(dateIdentifier, args);

        // Act - Create date object
        var dateObject = interpreter.Evaluate(dateConstructor, "Date(2023, 11, 25, 14, 30, 45)");
        Assert.IsType<DateObject>(dateObject);
        
        var dateObj = (DateObject)dateObject;

        // Test multiple methods
        var year = dateObj.GetFullYear(new List<object?>());
        var month = dateObj.GetMonth(new List<object?>());
        var day = dateObj.GetDate(new List<object?>());
        var timestamp = dateObj.GetTime(new List<object?>());
        var isoString = dateObj.ToISOString(new List<object?>());

        // Assert
        Assert.Equal(2023.0, year);
        Assert.Equal(11.0, month); // December (0-indexed)
        Assert.Equal(25.0, day);
        Assert.IsType<double>(timestamp);
        Assert.IsType<string>(isoString);
        Assert.Contains("2023-12-25", (string)isoString);
    }

    #endregion
}
