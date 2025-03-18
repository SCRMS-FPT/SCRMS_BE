using Reviews.API.Cache;

namespace Reviews.Test.Cache
{
    public class MemorySubjectCacheTests
    {
        [Fact]
        public void TryGetValue_KeyNotInCache_ReturnsFalseAndSetsExistsToFalse()
        {
            // Arrange
            var cache = new MemorySubjectCache();
            string key = "nonexistent_key";

            // Act
            bool result = cache.TryGetValue(key, out bool exists);

            // Assert
            Assert.False(result);
            Assert.False(exists);
        }

        [Fact]
        public void TryGetValue_KeyInCacheAndNotExpired_ReturnsTrueAndSetsExistsToCachedValue()
        {
            // Arrange
            var cache = new MemorySubjectCache();
            string key = "existing_key";
            bool expectedExists = true;
            cache.Set(key, expectedExists, TimeSpan.FromMinutes(5));

            // Act
            bool result = cache.TryGetValue(key, out bool exists);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedExists, exists);
        }

        [Fact]
        public async Task TryGetValue_KeyInCacheButExpired_ReturnsFalseAndRemovesKey()
        {
            // Arrange
            var cache = new MemorySubjectCache();
            string key = "expiring_key";
            cache.Set(key, true, TimeSpan.FromMilliseconds(100));

            // Wait for expiration
            await Task.Delay(150);

            // Act
            bool result = cache.TryGetValue(key, out bool exists);

            // Assert
            Assert.False(result);
            Assert.False(exists);

            // Verify the key is removed
            bool stillExists = cache.TryGetValue(key, out _);
            Assert.False(stillExists);
        }

        [Fact]
        public void Set_WithExpiration_SetsValueCorrectly()
        {
            // Arrange
            var cache = new MemorySubjectCache();
            string key = "test_key";
            bool expectedExists = true;
            TimeSpan expiration = TimeSpan.FromMinutes(10);

            // Act
            cache.Set(key, expectedExists, expiration);

            // Assert
            bool result = cache.TryGetValue(key, out bool actualExists);
            Assert.True(result);
            Assert.Equal(expectedExists, actualExists);
        }

        [Fact]
        public void Set_WithoutExpiration_UsesDefaultExpiration()
        {
            // Arrange
            var cache = new MemorySubjectCache();
            string key = "default_expiration_key";
            bool expectedExists = true;

            // Act
            cache.Set(key, expectedExists); // No expiration specified, defaults to 1 hour

            // Assert
            bool result = cache.TryGetValue(key, out bool actualExists);
            Assert.True(result);
            Assert.Equal(expectedExists, actualExists);
        }

        [Fact]
        public void Remove_KeyExists_RemovesKeySuccessfully()
        {
            // Arrange
            var cache = new MemorySubjectCache();
            string key = "removable_key";
            cache.Set(key, true, TimeSpan.FromMinutes(5));

            // Act
            cache.Remove(key);

            // Assert
            bool result = cache.TryGetValue(key, out bool exists);
            Assert.False(result);
            Assert.False(exists);
        }

        [Fact]
        public void Set_UpdatesExistingKey_CorrectlyUpdatesValueAndExpiration()
        {
            // Arrange
            var cache = new MemorySubjectCache();
            string key = "update_key";
            cache.Set(key, false, TimeSpan.FromMinutes(5));

            // Act
            cache.Set(key, true, TimeSpan.FromMinutes(10));

            // Assert
            bool result = cache.TryGetValue(key, out bool exists);
            Assert.True(result);
            Assert.True(exists); // Updated value
        }
    }
}
