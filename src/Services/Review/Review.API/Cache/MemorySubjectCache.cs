using System.Collections.Concurrent;

namespace Reviews.API.Cache
{
    public class MemorySubjectCache : ISubjectCache
    {
        private readonly ConcurrentDictionary<string, (bool exists, DateTimeOffset expiration)> _cache = new();

        public bool TryGetValue(string key, out bool exists)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.expiration > DateTimeOffset.UtcNow)
                {
                    exists = entry.exists;
                    return true;
                }
                _cache.TryRemove(key, out _);
            }
            exists = false;
            return false;
        }

        public void Set(string key, bool exists, TimeSpan? expiration = null)
        {
            var expirationTime = DateTimeOffset.UtcNow.Add(expiration ?? TimeSpan.FromHours(1));
            _cache.AddOrUpdate(key, (exists, expirationTime), (_, _) => (exists, expirationTime));
        }

        public void Remove(string key) => _cache.TryRemove(key, out _);
    }
}