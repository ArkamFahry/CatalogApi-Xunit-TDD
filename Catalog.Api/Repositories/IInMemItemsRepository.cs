using Catalog.Api.Entities;

namespace Catalog.Api.Repositories
{
    public interface IInMemItemsRepository
    {
        Task<item> GetItemAsync(Guid id);
        Task<IEnumerable<item>> GetItemsAsync();
        Task CreateItemAsync(item item);
        Task UpdateItemAsync(item item);
        Task DeleteItemAsync(Guid id);
    }
}