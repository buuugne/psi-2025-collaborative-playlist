using Xunit;
using MyApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestProject.Utils
{
    // Simple test classes for testing the converter
    public class TestSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestTarget
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
    }

    public class GenericConverterTests
    {
        [Fact]
        public void ConvertAll_WithValidList_ShouldConvertSuccessfully()
        {
            // Arrange
            var converter = new GenericConverter<TestSource, TestTarget>();
            var sourceList = new List<TestSource>
            {
                new TestSource { Id = 1, Name = "Item 1" },
                new TestSource { Id = 2, Name = "Item 2" },
                new TestSource { Id = 3, Name = "Item 3" }
            };

            // Act
            var result = converter.ConvertAll(sourceList, src => new TestTarget
            {
                Id = src.Id,
                DisplayName = src.Name.ToUpper()
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].Id);
            Assert.Equal("ITEM 1", result[0].DisplayName);
            Assert.Equal(2, result[1].Id);
            Assert.Equal("ITEM 2", result[1].DisplayName);
        }

        [Fact]
        public void ConvertAll_WithEmptyList_ShouldReturnEmptyList()
        {
            // Arrange
            var converter = new GenericConverter<TestSource, TestTarget>();
            var sourceList = new List<TestSource>();

            // Act
            var result = converter.ConvertAll(sourceList, src => new TestTarget
            {
                Id = src.Id,
                DisplayName = src.Name
            });

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ConvertAll_WithNullList_ShouldThrowArgumentNullException()
        {
            // Arrange
            var converter = new GenericConverter<TestSource, TestTarget>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                converter.ConvertAll(null!, src => new TestTarget())
            );

            Assert.Contains("Source list or converter function cannot be null", exception.Message);
        }

        [Fact]
        public void ConvertAll_WithNullConverter_ShouldThrowArgumentNullException()
        {
            // Arrange
            var converter = new GenericConverter<TestSource, TestTarget>();
            var sourceList = new List<TestSource>
            {
                new TestSource { Id = 1, Name = "Item 1" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                converter.ConvertAll(sourceList, null!)
            );

            Assert.Contains("Source list or converter function cannot be null", exception.Message);
        }

        [Fact]
        public void ConvertOne_WithValidSource_ShouldConvertSuccessfully()
        {
            // Arrange
            var converter = new GenericConverter<TestSource, TestTarget>();
            var source = new TestSource { Id = 42, Name = "Test Item" };

            // Act
            var result = converter.ConvertOne(source, src => new TestTarget
            {
                Id = src.Id,
                DisplayName = $"Converted: {src.Name}"
            });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(42, result.Id);
            Assert.Equal("Converted: Test Item", result.DisplayName);
        }

        [Fact]
        public void ConvertOne_WithNullSource_ShouldThrowArgumentNullException()
        {
            // Arrange
            var converter = new GenericConverter<TestSource, TestTarget>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                converter.ConvertOne(null!, src => new TestTarget())
            );

            Assert.Contains("Source or converter function cannot be null", exception.Message);
        }

        [Fact]
        public void ConvertOne_WithNullConverter_ShouldThrowArgumentNullException()
        {
            // Arrange
            var converter = new GenericConverter<TestSource, TestTarget>();
            var source = new TestSource { Id = 1, Name = "Test" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                converter.ConvertOne(source, null!)
            );

            Assert.Contains("Source or converter function cannot be null", exception.Message);
        }

        [Fact]
        public void ConvertAll_WithComplexTransformation_ShouldApplyCorrectly()
        {
            // Arrange
            var converter = new GenericConverter<TestSource, TestTarget>();
            var sourceList = new List<TestSource>
            {
                new TestSource { Id = 1, Name = "alpha" },
                new TestSource { Id = 2, Name = "beta" },
                new TestSource { Id = 3, Name = "gamma" }
            };

            // Act - complex transformation: uppercase name and multiply ID
            var result = converter.ConvertAll(sourceList, src => new TestTarget
            {
                Id = src.Id * 10,
                DisplayName = src.Name.ToUpper()
            });

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(10, result[0].Id);
            Assert.Equal("ALPHA", result[0].DisplayName);
            Assert.Equal(20, result[1].Id);
            Assert.Equal("BETA", result[1].DisplayName);
            Assert.Equal(30, result[2].Id);
            Assert.Equal("GAMMA", result[2].DisplayName);
        }

        [Fact]
        public void ConvertOne_WithComplexTransformation_ShouldApplyCorrectly()
        {
            // Arrange
            var converter = new GenericConverter<TestSource, TestTarget>();
            var source = new TestSource { Id = 5, Name = "test" };

            // Act - complex transformation
            var result = converter.ConvertOne(source, src => new TestTarget
            {
                Id = src.Id * 100,
                DisplayName = $"#{src.Id}: {src.Name.ToUpper()}"
            });

            // Assert
            Assert.Equal(500, result.Id);
            Assert.Equal("#5: TEST", result.DisplayName);
        }
    }
}