using Ardalis.Result;
using FluentAssertions;
using Xunit;
using Result = Ardalis.Result.Result;

namespace Server.SharedKernel.ResultUnitTests;

public class ResultImplicitOperators
{
  private string successMessage = "Success";
  private string expectedString = "test string";
  private int expectedInt = 123;
  private TestObject expectedObject = new TestObject();
  private TestObject expectedNullObject = null;

  [Fact]
  public void ConvertFromStringValue()
  {
    var result = DoBusinessOperationExample(expectedString);

    result.Value.Should().Be(expectedString);
    result.Status.Should().Be(ResultStatus.Ok);
  }
  [Fact]
  public void ConvertToStringValue()
  {
    var result = GetValueForResultExample(Result<string>.Success(expectedString));

    result.Should().Be(expectedString);
  }

  [Fact]
  public void SuccessWithSuccessMessage()
  {
    var result = Result<string>.Success(expectedString, successMessage);

    result.SuccessMessage.Should().Be(successMessage);
    result.Value.Should().Be(expectedString);
  }

  [Fact]
  public void ConvertFromIntValue()
  {
    var result = DoBusinessOperationExample(expectedInt);

    result.Value.Should().Be(expectedInt);
    result.Status.Should().Be(ResultStatus.Ok);
  }
  [Fact]
  public void ConvertToIntValue()
  {
    var result = GetValueForResultExample(Result<int>.Success(expectedInt));

    result.Should().Be(expectedInt);
  }

  [Fact]
  public void ConvertFromObjectValue()
  {
    var result = DoBusinessOperationExample(expectedObject);

    result.Value.Should().Be(expectedObject);
    result.Status.Should().Be(ResultStatus.Ok);
  }
  [Fact]
  public void ConvertToObjectValue()
  {
    var result = GetValueForResultExample(Result<TestObject>.Success(expectedObject));

    result.Should().Be(expectedObject);
  }

  [Fact]
  public void ConvertFromNullObjectValue()
  {
    var result = DoBusinessOperationExample(expectedNullObject);

    result.Value.Should().Be(expectedNullObject);
    result.Status.Should().Be(ResultStatus.Ok);
  }
  [Fact]
  public void ConvertToNullObjectValue()
  {
    var result = GetValueForResultExample(Result<TestObject>.Success(expectedNullObject));

    result.Should().Be(expectedNullObject);
  }

  [Fact]
  public void ConvertResultResultToResult()
  {
    var result = Result.Error(expectedString);

    Result convertedResult = result;

    convertedResult.Should().NotBeNull();
    convertedResult.Errors.First().Should().Be(expectedString);
  }

  public Ardalis.Result.Result<T> DoBusinessOperationExample<T>(T testValue) => testValue;
  public T GetValueForResultExample<T>(Ardalis.Result.Result<T> testResult) => testResult;

  private class TestObject { }
}
