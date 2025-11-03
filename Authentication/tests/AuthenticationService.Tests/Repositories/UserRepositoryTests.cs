using AuthenticationService.Models;
using AuthenticationService.Repositories;
using FluentAssertions;

namespace AuthenticationService.Tests.Repositories;

/// <summary>
/// Unit tests for UserRepository (InMemoryUserRepository) following xUnit best practices
/// Tests user storage, retrieval, and data persistence
/// </summary>
public class UserRepositoryTests
{
    #region GetByEmailAsync Tests

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
      // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
        {
          Name = "John Doe",
            Email = "john@example.com",
            PasswordHash = "hashedPassword"
        };
        await repository.CreateAsync(user);

      // Act
        var result = await repository.GetByEmailAsync("john@example.com");

 // Assert
    result.Should().NotBeNull();
        result!.Email.Should().Be("john@example.com");
      result.Name.Should().Be("John Doe");
        result.PasswordHash.Should().Be("hashedPassword");
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task GetByEmailAsync_NonExistentUser_ReturnsNull()
    {
    // Arrange
        var repository = new InMemoryUserRepository();

  // Act
   var result = await repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        result.Should().BeNull();
    }

 [Fact]
    [Trait("Category", "UserRepository")]
    public async Task GetByEmailAsync_CaseInsensitiveEmail_ReturnsUser()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
{
   Name = "Test User",
        Email = "Test@Example.COM",
            PasswordHash = "hash"
   };
    await repository.CreateAsync(user);

      // Act
        var result1 = await repository.GetByEmailAsync("test@example.com");
        var result2 = await repository.GetByEmailAsync("TEST@EXAMPLE.COM");
        var result3 = await repository.GetByEmailAsync("Test@Example.COM");

        // Assert
        result1.Should().NotBeNull();
      result2.Should().NotBeNull();
        result3.Should().NotBeNull();
        result1!.Email.Should().Be("Test@Example.COM");
        result2!.Email.Should().Be("Test@Example.COM");
        result3!.Email.Should().Be("Test@Example.COM");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("admin@test.org")]
    [InlineData("test+tag@email.com")]
 [InlineData("user.name@sub.domain.com")]
    [Trait("Category", "UserRepository")]
    public async Task GetByEmailAsync_DifferentEmailFormats_ReturnsCorrectUser(string email)
    {
        // Arrange
     var repository = new InMemoryUserRepository();
        var user = new User
        {
 Name = "Test User",
         Email = email,
            PasswordHash = "hash"
      };
 await repository.CreateAsync(user);

        // Act
        var result = await repository.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task GetByEmailAsync_MultipleUsers_ReturnsCorrectUser()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        
  var user1 = new User { Name = "User 1", Email = "user1@example.com", PasswordHash = "hash1" };
      var user2 = new User { Name = "User 2", Email = "user2@example.com", PasswordHash = "hash2" };
        var user3 = new User { Name = "User 3", Email = "user3@example.com", PasswordHash = "hash3" };

        await repository.CreateAsync(user1);
        await repository.CreateAsync(user2);
        await repository.CreateAsync(user3);

        // Act
   var result = await repository.GetByEmailAsync("user2@example.com");

      // Assert
        result.Should().NotBeNull();
    result!.Name.Should().Be("User 2");
    result.Email.Should().Be("user2@example.com");
   result.PasswordHash.Should().Be("hash2");
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task GetByEmailAsync_EmptyString_ReturnsNull()
    {
     // Arrange
        var repository = new InMemoryUserRepository();

  // Act
        var result = await repository.GetByEmailAsync(string.Empty);

      // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAsync Tests

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_NewUser_AssignsId()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
  {
 Name = "John Doe",
  Email = "john@example.com",
            PasswordHash = "hashedPassword"
 };

    // Act
        var result = await repository.CreateAsync(user);

        // Assert
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_NewUser_SetsCreatedAt()
    {
  // Arrange
        var repository = new InMemoryUserRepository();
        var beforeCreation = DateTime.UtcNow;
      var user = new User
        {
  Name = "John Doe",
    Email = "john@example.com",
            PasswordHash = "hashedPassword"
      };

        // Act
        var result = await repository.CreateAsync(user);
        var afterCreation = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeOnOrAfter(beforeCreation);
     result.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_NewUser_PreservesUserData()
    {
        // Arrange
   var repository = new InMemoryUserRepository();
      var user = new User
        {
            Name = "Jane Doe",
 Email = "jane@example.com",
            PasswordHash = "secureHash123"
        };

        // Act
    var result = await repository.CreateAsync(user);

        // Assert
        result.Name.Should().Be("Jane Doe");
        result.Email.Should().Be("jane@example.com");
        result.PasswordHash.Should().Be("secureHash123");
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_MultipleUsers_AssignsSequentialIds()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user1 = new User { Name = "User 1", Email = "user1@example.com", PasswordHash = "hash1" };
        var user2 = new User { Name = "User 2", Email = "user2@example.com", PasswordHash = "hash2" };
        var user3 = new User { Name = "User 3", Email = "user3@example.com", PasswordHash = "hash3" };

        // Act
     var result1 = await repository.CreateAsync(user1);
        var result2 = await repository.CreateAsync(user2);
        var result3 = await repository.CreateAsync(user3);

      // Assert
        result1.Id.Should().Be(1);
      result2.Id.Should().Be(2);
     result3.Id.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
 public async Task CreateAsync_NewUser_CanBeRetrievedByEmail()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
        {
            Name = "Test User",
            Email = "test@example.com",
      PasswordHash = "hash"
        };

     // Act
      var createdUser = await repository.CreateAsync(user);
        var retrievedUser = await repository.GetByEmailAsync("test@example.com");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Id.Should().Be(createdUser.Id);
   retrievedUser.Name.Should().Be("Test User");
        retrievedUser.Email.Should().Be("test@example.com");
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_SameUserObject_ModifiesOriginalObject()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
        {
      Name = "John Doe",
  Email = "john@example.com",
     PasswordHash = "hash"
        };

  // Act
     var originalId = user.Id;
        var originalCreatedAt = user.CreatedAt;
     
        await repository.CreateAsync(user);

   // Assert
  user.Id.Should().NotBe(originalId);
        user.Id.Should().BeGreaterThan(0);
        user.CreatedAt.Should().BeAfter(originalCreatedAt);
    }

    [Theory]
    [InlineData("", "test@example.com", "hash")]
    [InlineData("Test User", "", "hash")]
    [InlineData("Test User", "test@example.com", "")]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_EmptyFields_StillCreatesUser(string name, string email, string passwordHash)
  {
   // Arrange
        var repository = new InMemoryUserRepository();
        var user = new User
        {
     Name = name,
    Email = email,
            PasswordHash = passwordHash
        };

    // Act
 var result = await repository.CreateAsync(user);

   // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    result.Name.Should().Be(name);
result.Email.Should().Be(email);
        result.PasswordHash.Should().Be(passwordHash);
    }

    #endregion

    #region Integration Tests

 [Fact]
    [Trait("Category", "UserRepository")]
    public async Task Repository_CreateMultipleAndRetrieve_WorksCorrectly()
    {
        // Arrange
     var repository = new InMemoryUserRepository();
        var users = new[]
   {
            new User { Name = "Alice", Email = "alice@example.com", PasswordHash = "hash1" },
            new User { Name = "Bob", Email = "bob@example.com", PasswordHash = "hash2" },
 new User { Name = "Charlie", Email = "charlie@example.com", PasswordHash = "hash3" }
        };

     // Act
        foreach (var user in users)
        {
 await repository.CreateAsync(user);
        }

      var alice = await repository.GetByEmailAsync("alice@example.com");
 var bob = await repository.GetByEmailAsync("bob@example.com");
        var charlie = await repository.GetByEmailAsync("charlie@example.com");
        var nonExistent = await repository.GetByEmailAsync("david@example.com");

        // Assert
        alice.Should().NotBeNull();
        alice!.Name.Should().Be("Alice");
        
    bob.Should().NotBeNull();
        bob!.Name.Should().Be("Bob");
        
charlie.Should().NotBeNull();
        charlie!.Name.Should().Be("Charlie");
        
        nonExistent.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task Repository_CreateWithSameEmail_AllowsDuplicates()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var user1 = new User { Name = "User 1", Email = "duplicate@example.com", PasswordHash = "hash1" };
 var user2 = new User { Name = "User 2", Email = "duplicate@example.com", PasswordHash = "hash2" };

        // Act
        await repository.CreateAsync(user1);
        await repository.CreateAsync(user2);
        var result = await repository.GetByEmailAsync("duplicate@example.com");

        // Assert
        // Note: In-memory repository returns first match
        result.Should().NotBeNull();
  result!.Name.Should().Be("User 1"); // Returns first user with this email
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task Repository_NewInstance_IsEmpty()
    {
        // Arrange
    var repository = new InMemoryUserRepository();

        // Act
    var result = await repository.GetByEmailAsync("any@example.com");

  // Assert
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task Repository_MultipleInstances_AreIndependent()
    {
        // Arrange
      var repository1 = new InMemoryUserRepository();
        var repository2 = new InMemoryUserRepository();
        
        var user = new User { Name = "Test User", Email = "test@example.com", PasswordHash = "hash" };

        // Act
        await repository1.CreateAsync(user);
        var resultFromRepo1 = await repository1.GetByEmailAsync("test@example.com");
        var resultFromRepo2 = await repository2.GetByEmailAsync("test@example.com");

        // Assert
     resultFromRepo1.Should().NotBeNull();
    resultFromRepo2.Should().BeNull(); // Different repository instance
    }

    #endregion

    #region Edge Cases

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_VeryLongEmail_StoresCorrectly()
    {
   // Arrange
        var repository = new InMemoryUserRepository();
     var longEmail = new string('a', 200) + "@example.com";
     var user = new User
    {
       Name = "Test User",
      Email = longEmail,
            PasswordHash = "hash"
        };

        // Act
        var created = await repository.CreateAsync(user);
    var retrieved = await repository.GetByEmailAsync(longEmail);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be(longEmail);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_VeryLongName_StoresCorrectly()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var longName = new string('N', 500);
        var user = new User
        {
      Name = longName,
    Email = "test@example.com",
          PasswordHash = "hash"
    };

        // Act
        var created = await repository.CreateAsync(user);
        var retrieved = await repository.GetByEmailAsync("test@example.com");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be(longName);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_SpecialCharactersInEmail_StoresCorrectly()
    {
      // Arrange
      var repository = new InMemoryUserRepository();
        var specialEmail = "test+tag.name_123@sub-domain.example.com";
 var user = new User
        {
     Name = "Test User",
     Email = specialEmail,
            PasswordHash = "hash"
    };

        // Act
     await repository.CreateAsync(user);
        var result = await repository.GetByEmailAsync(specialEmail);

        // Assert
      result.Should().NotBeNull();
        result!.Email.Should().Be(specialEmail);
    }

    [Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_UnicodeCharactersInName_StoresCorrectly()
    {
        // Arrange
  var repository = new InMemoryUserRepository();
        var unicodeName = "José García ?? Müller";
      var user = new User
        {
     Name = unicodeName,
            Email = "test@example.com",
      PasswordHash = "hash"
        };

        // Act
        await repository.CreateAsync(user);
        var result = await repository.GetByEmailAsync("test@example.com");

     // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be(unicodeName);
    }

[Fact]
    [Trait("Category", "UserRepository")]
    public async Task CreateAsync_LargeNumberOfUsers_AssignsCorrectIds()
    {
     // Arrange
  var repository = new InMemoryUserRepository();
        var userCount = 100;

 // Act
        var createdUsers = new List<User>();
        for (int i = 1; i <= userCount; i++)
   {
        var user = new User
  {
                Name = $"User {i}",
Email = $"user{i}@example.com",
          PasswordHash = $"hash{i}"
         };
   var created = await repository.CreateAsync(user);
    createdUsers.Add(created);
    }

        // Assert
        for (int i = 0; i < userCount; i++)
        {
            createdUsers[i].Id.Should().Be(i + 1);
        }

      // Verify retrieval
    var randomUser = await repository.GetByEmailAsync("user50@example.com");
    randomUser.Should().NotBeNull();
        randomUser!.Name.Should().Be("User 50");
        randomUser.Id.Should().Be(50);
    }

    #endregion
}
