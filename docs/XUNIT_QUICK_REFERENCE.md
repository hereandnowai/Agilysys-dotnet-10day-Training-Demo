# XUnit Quick Reference Guide

## ?? **Getting Started**

### **Test Class Structure**
```csharp
using Xunit;

public class MyTests
{
    // Constructor runs before each test
    public MyTests()
    {
     // Setup code here
    }

    [Fact]
    public void MyTest()
    {
      // Test code
    }
}
```

---

## ?? **Common Attributes**

| Attribute | Purpose | Example |
|-----------|---------|---------|
| `[Fact]` | Simple test | `[Fact]`<br>`public void Test1() { }` |
| `[Theory]` | Parameterized test | `[Theory]`<br>`[InlineData(1, 2, 3)]`<br>`public void Add(int a, int b, int expected) { }` |
| `[InlineData]` | Test data | `[InlineData(1, 2)]`<br>`[InlineData(3, 4)]` |
| `[MemberData]` | Complex data | `[MemberData(nameof(TestData))]` |
| `[ClassData]` | Class-based data | `[ClassData(typeof(MyTestData))]` |
| `[Trait]` | Test categorization | `[Trait("Category", "Unit")]` |

---

## ? **Assertions Cheat Sheet**

### **Equality**
```csharp
Assert.Equal(expected, actual);
Assert.NotEqual(expected, actual);
Assert.Same(expected, actual);      // Reference equality
Assert.NotSame(expected, actual);
```

### **Null/Not Null**
```csharp
Assert.Null(value);
Assert.NotNull(value);
```

### **Boolean**
```csharp
Assert.True(condition);
Assert.False(condition);
```

### **Collections**
```csharp
Assert.Empty(collection);
Assert.NotEmpty(collection);
Assert.Contains(item, collection);
Assert.DoesNotContain(item, collection);
Assert.All(collection, item => Assert.True(item > 0));
```

### **Strings**
```csharp
Assert.Equal("expected", actual);
Assert.Contains("substring", actual);
Assert.DoesNotContain("substring", actual);
Assert.StartsWith("prefix", actual);
Assert.EndsWith("suffix", actual);
Assert.Matches(@"regex", actual);
```

### **Numbers**
```csharp
Assert.Equal(expected, actual);
Assert.InRange(actual, low, high);
Assert.NotInRange(actual, low, high);
```

### **Types**
```csharp
Assert.IsType<ExpectedType>(actual);
Assert.IsNotType<UnexpectedType>(actual);
Assert.IsAssignableFrom<BaseType>(actual);
```

### **Exceptions**
```csharp
// Sync
var ex = Assert.Throws<ArgumentException>(() => MethodThatThrows());
Assert.Equal("paramName", ex.ParamName);

// Async
var ex = await Assert.ThrowsAsync<ArgumentException>(
    async () => await AsyncMethodThatThrows());
Assert.Contains("error message", ex.Message);

// Doesn't throw
var exception = Record.Exception(() => Method());
Assert.Null(exception);
```

---

## ?? **NUnit ? XUnit Migration**

| NUnit | XUnit |
|-------|-------|
| `[TestFixture]` | (Remove - not needed) |
| `[SetUp]` | Constructor |
| `[TearDown]` | `Dispose()` |
| `[OneTimeSetUp]` | `IClassFixture<T>` |
| `[OneTimeTearDown]` | `IClassFixture<T>` with Dispose |
| `[Test]` | `[Fact]` |
| `[TestCase(1, 2)]` | `[Theory]` + `[InlineData(1, 2)]` |
| `[Category("Unit")]` | `[Trait("Category", "Unit")]` |
| `[Ignore("reason")]` | `[Fact(Skip = "reason")]` |

---

## ?? **Common Patterns**

### **Constructor Setup**
```csharp
public class MyTests
{
    private readonly MyService _service;
    private readonly Mock<IDependency> _mockDep;

    public MyTests()
    {
        _mockDep = new Mock<IDependency>();
        _service = new MyService(_mockDep.Object);
    }
}
```

### **Shared Context (Fixture)**
```csharp
public class DatabaseFixture : IDisposable
{
    public DbConnection Connection { get; }

    public DatabaseFixture()
    {
        Connection = CreateConnection();
    }

    public void Dispose()
    {
        Connection?.Dispose();
    }
}

public class MyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### **Parameterized Tests**
```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(2, 3, 5)]
[InlineData(0, 0, 0)]
public void Add_ReturnsCorrectSum(int a, int b, int expected)
{
    var result = Calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

### **Member Data**
```csharp
public static IEnumerable<object[]> TestData()
{
    yield return new object[] { 1, 2, 3 };
    yield return new object[] { 2, 3, 5 };
}

[Theory]
[MemberData(nameof(TestData))]
public void Add_WithMemberData(int a, int b, int expected)
{
    var result = Calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

### **Async Tests**
```csharp
[Fact]
public async Task GetCustomerAsync_ReturnsCustomer()
{
    // Arrange
    var service = new CustomerService();

    // Act
    var result = await service.GetCustomerAsync(1);

    // Assert
    Assert.NotNull(result);
}
```

### **Exception Testing**
```csharp
[Fact]
public async Task CreateCustomer_ThrowsException_WhenEmailExists()
{
    // Arrange
    var service = new CustomerService();
  var customer = new Customer { Email = "test@test.com" };

    // Act & Assert
 var exception = await Assert.ThrowsAsync<InvalidOperationException>(
   async () => await service.CreateAsync(customer));

    Assert.Contains("Email already exists", exception.Message);
}
```

---

## ?? **Test Organization**

### **Using Regions**
```csharp
public class CustomerServiceTests
{
 #region GetAllAsync Tests
    
    [Fact]
    public void GetAllAsync_ReturnsAll() { }
    
    [Fact]
 public void GetAllAsync_ReturnsEmpty() { }
    
    #endregion
    
    #region GetByIdAsync Tests
    
    [Fact]
    public void GetByIdAsync_ReturnsCustomer() { }
    
    [Fact]
    public void GetByIdAsync_ReturnsNull() { }
    
    #endregion
}
```

### **Nested Classes**
```csharp
public class CustomerServiceTests
{
    public class GetAllAsyncTests
    {
        [Fact]
    public void ReturnsAllCustomers() { }
        
        [Fact]
        public void ReturnsEmptyList() { }
    }
    
    public class GetByIdAsyncTests
    {
        [Fact]
     public void ReturnsCustomer() { }
        
        [Fact]
        public void ReturnsNull() { }
    }
}
```

---

## ?? **Best Practices**

### **1. Test Naming**
```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Fact]
public void GetCustomerById_WhenCustomerExists_ReturnsCustomer() { }

[Fact]
public void CreateCustomer_WhenEmailExists_ThrowsException() { }
```

### **2. Arrange-Act-Assert**
```csharp
[Fact]
public void ExampleTest()
{
    // Arrange
    var service = new MyService();
    var input = "test";

    // Act
    var result = service.Process(input);

    // Assert
    Assert.Equal("expected", result);
}
```

### **3. One Assertion Per Test (when possible)**
```csharp
// Good
[Fact]
public void GetCustomer_ReturnsCorrectName()
{
    var result = _service.GetCustomer(1);
    Assert.Equal("John", result.Name);
}

// Also OK - Multiple related assertions
[Fact]
public void GetCustomer_ReturnsCompleteCustomer()
{
    var result = _service.GetCustomer(1);
    Assert.NotNull(result);
    Assert.Equal("John", result.Name);
    Assert.Equal("john@test.com", result.Email);
}
```

### **4. Use Theory for Similar Tests**
```csharp
// Instead of this:
[Fact] public void Add_1_2_Returns_3() { /* ... */ }
[Fact] public void Add_2_3_Returns_5() { /* ... */ }
[Fact] public void Add_0_0_Returns_0() { /* ... */ }

// Do this:
[Theory]
[InlineData(1, 2, 3)]
[InlineData(2, 3, 5)]
[InlineData(0, 0, 0)]
public void Add_ReturnsCorrectSum(int a, int b, int expected)
{
    var result = Calculator.Add(a, b);
    Assert.Equal(expected, result);
}
```

---

## ?? **Running Tests**

### **Command Line**
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~MyTest"

# Run tests in a category
dotnet test --filter "Category=Unit"

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Watch mode
dotnet watch test
```

### **Visual Studio**
- **Test Explorer**: View ? Test Explorer
- **Run All**: Ctrl + R, A
- **Run Selected**: Ctrl + R, T
- **Debug Selected**: Ctrl + R, Ctrl + T

---

## ?? **Filtering Tests**

```bash
# By name
dotnet test --filter "Name~Customer"

# By full name
dotnet test --filter "FullyQualifiedName~GetCustomer"

# By trait
dotnet test --filter "Category=Integration"

# By class
dotnet test --filter "ClassName~CustomerTests"

# Multiple filters
dotnet test --filter "(Category=Unit)|(Category=Integration)"
```

---

## ?? **Additional Resources**

- [XUnit Documentation](https://xunit.net/)
- [Assertion Library](https://xunit.net/docs/assertions)
- [Comparisons with other frameworks](https://xunit.net/docs/comparisons)
- [Migration Guide](https://xunit.net/docs/migration-guide)

---

**Happy Testing with XUnit!** ??
