namespace Reviews.API.Cache
{
    public interface ISubjectCache
    {
        bool TryGetValue(string key, out bool exists);

        void Set(string key, bool exists, TimeSpan? expiration = null);

        void Remove(string key);
    }
}