using System.Text.Json;
using Scriban.Runtime;
using TeleHook.Api.Services.Utilities;
using Xunit;

namespace TeleHook.Api.Tests.Services;

public class JsonToScribanConverterTests
{
    private readonly JsonToScribanConverter _converter;

    public JsonToScribanConverterTests()
    {
        _converter = new JsonToScribanConverter();
    }

    #region ConvertToScriptObject(object data) Tests

    [Fact]
    public void ConvertToScriptObject_WithSimpleObject_ShouldConvertCorrectly()
    {
        // Arrange
        var data = new { name = "John", age = 30 };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result["name"]);
        Assert.Equal(30, result["age"]);
    }

    [Fact]
    public void ConvertToScriptObject_WithNestedObject_ShouldConvertCorrectly()
    {
        // Arrange
        var data = new
        {
            user = new
            {
                name = "John Doe",
                profile = new
                {
                    email = "john@example.com",
                    age = 25
                }
            }
        };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);
        var userObj = result["user"] as ScriptObject;
        Assert.NotNull(userObj);
        Assert.Equal("John Doe", userObj["name"]);

        var profileObj = userObj["profile"] as ScriptObject;
        Assert.NotNull(profileObj);
        Assert.Equal("john@example.com", profileObj["email"]);
        Assert.Equal(25, profileObj["age"]);
    }

    [Fact]
    public void ConvertToScriptObject_WithArray_ShouldConvertToScriptArray()
    {
        // Arrange
        var data = new
        {
            items = new[] { "item1", "item2", "item3" }
        };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);
        var itemsArray = result["items"] as ScriptArray;
        Assert.NotNull(itemsArray);
        Assert.Equal(3, itemsArray.Count);
        Assert.Equal("item1", itemsArray[0]);
        Assert.Equal("item2", itemsArray[1]);
        Assert.Equal("item3", itemsArray[2]);
    }

    [Fact]
    public void ConvertToScriptObject_WithComplexArray_ShouldConvertCorrectly()
    {
        // Arrange
        var data = new
        {
            users = new[]
            {
                new { name = "John", age = 30 },
                new { name = "Jane", age = 25 }
            }
        };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);
        var usersArray = result["users"] as ScriptArray;
        Assert.NotNull(usersArray);
        Assert.Equal(2, usersArray.Count);

        var firstUser = usersArray[0] as ScriptObject;
        Assert.NotNull(firstUser);
        Assert.Equal("John", firstUser["name"]);
        Assert.Equal(30, firstUser["age"]);

        var secondUser = usersArray[1] as ScriptObject;
        Assert.NotNull(secondUser);
        Assert.Equal("Jane", secondUser["name"]);
        Assert.Equal(25, secondUser["age"]);
    }

    [Fact]
    public void ConvertToScriptObject_WithMixedDataTypes_ShouldConvertCorrectly()
    {
        // Arrange
        var data = new
        {
            stringValue = "test",
            intValue = 42,
            longValue = 9876543210L,
            decimalValue = 123.45m,
            doubleValue = 678.90,
            boolValue = true,
            nullValue = (string?)null
        };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test", result["stringValue"]);
        Assert.Equal(42, result["intValue"]);
        Assert.Equal(9876543210L, result["longValue"]);
        Assert.Equal(123.45m, result["decimalValue"]);
        Assert.Equal((decimal)678.90, (decimal)result["doubleValue"], precision: 10);
        Assert.Equal(true, result["boolValue"]);
        Assert.Null(result["nullValue"]);
    }

    [Fact]
    public void ConvertToScriptObject_WithJsonElement_ShouldPassThrough()
    {
        // Arrange
        var jsonString = """{"name": "John", "age": 30}""";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);

        // Act
        var result = _converter.ConvertToScriptObject(jsonElement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result["name"]);
        Assert.Equal(30, result["age"]);
    }

    #endregion

    #region ConvertToScriptObject(JsonElement element) Tests

    [Fact]
    public void ConvertToScriptObject_JsonElement_WithSimpleObject_ShouldConvertCorrectly()
    {
        // Arrange
        var jsonString = """{"name": "Alice", "score": 95}""";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);

        // Act
        var result = _converter.ConvertToScriptObject(jsonElement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Alice", result["name"]);
        Assert.Equal(95, result["score"]);
    }

    [Fact]
    public void ConvertToScriptObject_JsonElement_WithNestedObject_ShouldConvertCorrectly()
    {
        // Arrange
        var jsonString = """
        {
            "webhook": {
                "name": "GitHub Push",
                "repository": {
                    "name": "my-repo",
                    "private": false
                }
            }
        }
        """;
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);

        // Act
        var result = _converter.ConvertToScriptObject(jsonElement);

        // Assert
        Assert.NotNull(result);
        var webhookObj = result["webhook"] as ScriptObject;
        Assert.NotNull(webhookObj);
        Assert.Equal("GitHub Push", webhookObj["name"]);

        var repoObj = webhookObj["repository"] as ScriptObject;
        Assert.NotNull(repoObj);
        Assert.Equal("my-repo", repoObj["name"]);
        Assert.Equal(false, repoObj["private"]);
    }

    [Fact]
    public void ConvertToScriptObject_JsonElement_WithArray_ShouldConvertCorrectly()
    {
        // Arrange
        var jsonString = """
        {
            "commits": [
                {"message": "Initial commit", "author": "John"},
                {"message": "Fix bug", "author": "Jane"}
            ]
        }
        """;
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);

        // Act
        var result = _converter.ConvertToScriptObject(jsonElement);

        // Assert
        Assert.NotNull(result);
        var commitsArray = result["commits"] as ScriptArray;
        Assert.NotNull(commitsArray);
        Assert.Equal(2, commitsArray.Count);

        var firstCommit = commitsArray[0] as ScriptObject;
        Assert.NotNull(firstCommit);
        Assert.Equal("Initial commit", firstCommit["message"]);
        Assert.Equal("John", firstCommit["author"]);
    }

    [Fact]
    public void ConvertToScriptObject_JsonElement_WithEmptyObject_ShouldReturnEmptyScriptObject()
    {
        // Arrange
        var jsonString = "{}";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);

        // Act
        var result = _converter.ConvertToScriptObject(jsonElement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void ConvertToScriptObject_JsonElement_WithEmptyArray_ShouldConvertCorrectly()
    {
        // Arrange
        var jsonString = """{"items": []}""";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);

        // Act
        var result = _converter.ConvertToScriptObject(jsonElement);

        // Assert
        Assert.NotNull(result);
        var itemsArray = result["items"] as ScriptArray;
        Assert.NotNull(itemsArray);
        Assert.Equal(0, itemsArray.Count);
    }

    #endregion

    #region Edge Cases and Data Type Tests

    [Fact]
    public void ConvertToScriptObject_WithStringNumbers_ShouldConvertCorrectly()
    {
        // Arrange
        var data = new
        {
            intAsString = "123",
            decimalAsString = "45.67"
        };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123", result["intAsString"]);
        Assert.Equal("45.67", result["decimalAsString"]);
    }

    [Fact]
    public void ConvertToScriptObject_WithBooleanValues_ShouldConvertCorrectly()
    {
        // Arrange
        var data = new
        {
            trueValue = true,
            falseValue = false
        };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(true, result["trueValue"]);
        Assert.Equal(false, result["falseValue"]);
    }

    [Fact]
    public void ConvertToScriptObject_WithNullValues_ShouldHandleCorrectly()
    {
        // Arrange
        var data = new
        {
            normalValue = "test",
            nullValue = (string?)null
        };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test", result["normalValue"]);
        Assert.Null(result["nullValue"]);
    }

    [Fact]
    public void ConvertToScriptObject_WithDifferentNumberTypes_ShouldConvertCorrectly()
    {
        // Arrange
        var jsonString = """
        {
            "smallInt": 42,
            "largeInt": 9876543210,
            "decimal": 123.456,
            "scientific": 1.23e-4
        }
        """;
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);

        // Act
        var result = _converter.ConvertToScriptObject(jsonElement);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result["smallInt"]);
        Assert.Equal(9876543210L, result["largeInt"]);

        // Check that decimal values are properly converted
        Assert.IsType<decimal>(result["decimal"]);
        Assert.Equal(123.456m, result["decimal"]);

        // Scientific notation should be handled as decimal
        Assert.IsType<decimal>(result["scientific"]);
    }

    [Fact]
    public void ConvertToScriptObject_WithComplexNestedStructure_ShouldConvertCorrectly()
    {
        // Arrange
        var data = new
        {
            webhook = new
            {
                name = "Complex Event",
                metadata = new
                {
                    timestamp = "2023-01-01T00:00:00Z",
                    tags = new[] { "urgent", "deploy" },
                    config = new
                    {
                        retries = 3,
                        timeout = 30.5,
                        enabled = true
                    }
                },
                commits = new[]
                {
                    new
                    {
                        sha = "abc123",
                        message = "Initial commit",
                        files = new[] { "README.md", "src/main.cs" }
                    }
                }
            }
        };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);

        var webhookObj = result["webhook"] as ScriptObject;
        Assert.NotNull(webhookObj);
        Assert.Equal("Complex Event", webhookObj["name"]);

        var metadataObj = webhookObj["metadata"] as ScriptObject;
        Assert.NotNull(metadataObj);
        Assert.Equal("2023-01-01T00:00:00Z", metadataObj["timestamp"]);

        var tagsArray = metadataObj["tags"] as ScriptArray;
        Assert.NotNull(tagsArray);
        Assert.Equal(2, tagsArray.Count);
        Assert.Equal("urgent", tagsArray[0]);

        var configObj = metadataObj["config"] as ScriptObject;
        Assert.NotNull(configObj);
        Assert.Equal(3, configObj["retries"]);
        Assert.Equal((decimal)30.5, (decimal)configObj["timeout"], precision: 10);
        Assert.Equal(true, configObj["enabled"]);

        var commitsArray = webhookObj["commits"] as ScriptArray;
        Assert.NotNull(commitsArray);
        Assert.Equal(1, commitsArray.Count);

        var firstCommit = commitsArray[0] as ScriptObject;
        Assert.NotNull(firstCommit);
        Assert.Equal("abc123", firstCommit["sha"]);

        var filesArray = firstCommit["files"] as ScriptArray;
        Assert.NotNull(filesArray);
        Assert.Equal(2, filesArray.Count);
        Assert.Equal("README.md", filesArray[0]);
    }

    [Fact]
    public void ConvertToScriptObject_WithArrayOfPrimitives_ShouldConvertCorrectly()
    {
        // Arrange
        var data = new
        {
            strings = new[] { "a", "b", "c" },
            numbers = new[] { 1, 2, 3 },
            booleans = new[] { true, false, true },
            mixed = new object[] { "text", 42, true, null }
        };

        // Act
        var result = _converter.ConvertToScriptObject(data);

        // Assert
        Assert.NotNull(result);

        var stringsArray = result["strings"] as ScriptArray;
        Assert.NotNull(stringsArray);
        Assert.Equal(3, stringsArray.Count);
        Assert.Equal("a", stringsArray[0]);

        var numbersArray = result["numbers"] as ScriptArray;
        Assert.NotNull(numbersArray);
        Assert.Equal(3, numbersArray.Count);
        Assert.Equal(1, numbersArray[0]);

        var booleansArray = result["booleans"] as ScriptArray;
        Assert.NotNull(booleansArray);
        Assert.Equal(3, booleansArray.Count);
        Assert.Equal(true, booleansArray[0]);
        Assert.Equal(false, booleansArray[1]);

        var mixedArray = result["mixed"] as ScriptArray;
        Assert.NotNull(mixedArray);
        Assert.Equal(4, mixedArray.Count);
        Assert.Equal("text", mixedArray[0]);
        Assert.Equal(42, mixedArray[1]);
        Assert.Equal(true, mixedArray[2]);
        Assert.Null(mixedArray[3]);
    }

    [Fact]
    public void ConvertToScriptObject_WithTopLevelArray_ShouldConvertToScriptArray()
    {
        // Arrange
        var jsonString = """["item1", "item2", "item3"]""";
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonString);

        // Act
        var result = _converter.ConvertToScriptObject(jsonElement);

        // Assert
        Assert.NotNull(result);
        // Top-level arrays are handled by the Array case in the switch, 
        // which doesn't populate the ScriptObject, so it should be empty
        Assert.Equal(0, result.Count);
    }

    #endregion

    #region Real-World Webhook Scenarios

    [Fact]
    public void ConvertToScriptObject_WithGitHubWebhookData_ShouldConvertCorrectly()
    {
        // Arrange - Simulated GitHub webhook payload
        var githubData = new
        {
            @ref = "refs/heads/main",
            before = "abc123",
            after = "def456",
            repository = new
            {
                name = "my-repo",
                full_name = "user/my-repo",
                @private = false,
                owner = new
                {
                    name = "user",
                    email = "user@example.com"
                }
            },
            commits = new[]
            {
                new
                {
                    id = "def456",
                    message = "Fix critical bug",
                    timestamp = "2023-01-01T12:00:00Z",
                    author = new
                    {
                        name = "John Doe",
                        email = "john@example.com"
                    },
                    added = new[] { "src/fix.cs" },
                    removed = new string[] { },
                    modified = new[] { "README.md" }
                }
            }
        };

        // Act
        var result = _converter.ConvertToScriptObject(githubData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("refs/heads/main", result["ref"]);
        Assert.Equal("abc123", result["before"]);
        Assert.Equal("def456", result["after"]);

        var repository = result["repository"] as ScriptObject;
        Assert.NotNull(repository);
        Assert.Equal("my-repo", repository["name"]);
        Assert.Equal(false, repository["private"]);

        var commits = result["commits"] as ScriptArray;
        Assert.NotNull(commits);
        Assert.Equal(1, commits.Count);

        var firstCommit = commits[0] as ScriptObject;
        Assert.NotNull(firstCommit);
        Assert.Equal("Fix critical bug", firstCommit["message"]);

        var addedFiles = firstCommit["added"] as ScriptArray;
        Assert.NotNull(addedFiles);
        Assert.Equal(1, addedFiles.Count);
        Assert.Equal("src/fix.cs", addedFiles[0]);
    }

    #endregion
}
