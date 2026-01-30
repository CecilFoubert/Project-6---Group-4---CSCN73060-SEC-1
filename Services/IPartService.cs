namespace Project_6___Group_4___CSCN73060_SEC_1.Services
{
    public interface IPartService
    {
        Task<IEnumerable<object>> GetAllAsync(string partType);
        Task<object?> GetByIdAsync(string partType, int id);
        Task<object> CreateAsync(string partType, Dictionary<string, object> data);
        Task<object?> UpdateAsync(string partType, int id, Dictionary<string, object> data);
        Task<bool> DeleteAsync(string partType, int id);
    }
}

