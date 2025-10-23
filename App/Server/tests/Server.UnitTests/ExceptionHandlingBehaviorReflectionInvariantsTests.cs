using System.Reflection;
using Ardalis.Result;
using Server.SharedKernel;

namespace Server.UnitTests;

/// <summary>
/// Tests to validate the reflection invariants required by ExceptionHandlingBehavior.
/// These tests ensure that the CustomArdalisResultFactory methods exist and have the correct signatures,
/// which is critical for the exception handling pipeline to function correctly.
/// 
/// If any of these tests fail, it indicates that:
/// 1. A required factory method was removed or renamed in CustomArdalisResultFactory
/// 2. The method signature was changed in an incompatible way
/// 3. ExceptionHandlingBehavior needs to be updated to match the new CustomArdalisResultFactory API
/// </summary>
public class ExceptionHandlingBehaviorReflectionInvariantsTests
{
  private readonly Type _factoryType = typeof(CustomArdalisResultFactory);

  [Fact]
  public void CustomArdalisResultFactory_ShouldHaveGenericCriticalErrorMethod()
  {
    // Arrange & Act
    var method = _factoryType.GetMethods(BindingFlags.Public | BindingFlags.Static)
      .FirstOrDefault(m =>
        m.Name == nameof(CustomArdalisResultFactory.CriticalError) &&
        m.IsGenericMethodDefinition &&
        m.GetParameters().Length == 1 &&
        m.GetParameters()[0].ParameterType == typeof(Exception));

    // Assert
    method.ShouldNotBeNull("CustomArdalisResultFactory must have a generic CriticalError<T>(Exception) method for ExceptionHandlingBehavior to function correctly.");
    method.IsGenericMethodDefinition.ShouldBeTrue("CriticalError<T> must be a generic method definition.");
    method.GetGenericArguments().Length.ShouldBe(1, "CriticalError<T> must have exactly one generic type parameter.");

    // Verify it returns Result<T>
    var genericParam = method.GetGenericArguments()[0];
    var returnType = method.ReturnType;
    returnType.IsGenericType.ShouldBeTrue("CriticalError<T> must return a generic Result<T>.");
    returnType.GetGenericTypeDefinition().ShouldBe(typeof(Result<>), "CriticalError<T> must return Result<T>.");
  }

  [Fact]
  public void CustomArdalisResultFactory_ShouldHaveGenericConflictMethod()
  {
    // Arrange & Act
    var method = _factoryType.GetMethods(BindingFlags.Public | BindingFlags.Static)
      .FirstOrDefault(m =>
        m.Name == nameof(CustomArdalisResultFactory.Conflict) &&
        m.IsGenericMethodDefinition &&
        m.GetParameters().Length == 1 &&
        m.GetParameters()[0].ParameterType == typeof(Exception));

    // Assert
    method.ShouldNotBeNull("CustomArdalisResultFactory must have a generic Conflict<T>(Exception) method for ExceptionHandlingBehavior to function correctly.");
    method.IsGenericMethodDefinition.ShouldBeTrue("Conflict<T> must be a generic method definition.");
    method.GetGenericArguments().Length.ShouldBe(1, "Conflict<T> must have exactly one generic type parameter.");

    // Verify it returns Result<T>
    var genericParam = method.GetGenericArguments()[0];
    var returnType = method.ReturnType;
    returnType.IsGenericType.ShouldBeTrue("Conflict<T> must return a generic Result<T>.");
    returnType.GetGenericTypeDefinition().ShouldBe(typeof(Result<>), "Conflict<T> must return Result<T>.");
  }

  [Fact]
  public void ExceptionHandlingBehavior_CanInvokeGenericCriticalErrorMethod()
  {
    // Arrange
    var exception = new InvalidOperationException("Test exception");
    var method = _factoryType.GetMethods(BindingFlags.Public | BindingFlags.Static)
      .First(m =>
        m.Name == nameof(CustomArdalisResultFactory.CriticalError) &&
        m.IsGenericMethodDefinition);

    // Act
    var genericMethod = method.MakeGenericMethod(typeof(string));
    var result = genericMethod.Invoke(null, new object[] { exception });

    // Assert
    result.ShouldNotBeNull("Invoking CriticalError<T> should return a result.");
    result.ShouldBeOfType<Result<string>>("Result should be of type Result<string>.");
    var typedResult = (Result<string>)result;
    typedResult.Status.ShouldBe(ResultStatus.CriticalError, "Result status should be CriticalError.");
  }

  [Fact]
  public void ExceptionHandlingBehavior_CanInvokeGenericConflictMethod()
  {
    // Arrange
    var exception = new InvalidOperationException("Test exception");
    var method = _factoryType.GetMethods(BindingFlags.Public | BindingFlags.Static)
      .First(m =>
        m.Name == nameof(CustomArdalisResultFactory.Conflict) &&
        m.IsGenericMethodDefinition);

    // Act
    var genericMethod = method.MakeGenericMethod(typeof(string));
    var result = genericMethod.Invoke(null, new object[] { exception });

    // Assert
    result.ShouldNotBeNull("Invoking Conflict<T> should return a result.");
    result.ShouldBeOfType<Result<string>>("Result should be of type Result<string>.");
    var typedResult = (Result<string>)result;
    typedResult.Status.ShouldBe(ResultStatus.Conflict, "Result status should be Conflict.");
  }
}
