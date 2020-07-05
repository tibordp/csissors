using System.Threading.Tasks;

namespace Csissors.Repository
{
    public interface IRepositoryFactory
    {
        Task<IRepository> CreateRepositoryAsync();
    }

}