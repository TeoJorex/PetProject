using ServerStorage.Domain.Models;

namespace ServerStorage.Domain.Interfaces
{
    public interface IVehicleEventRepository
    {
        Task SaveVehicleEventAsync(VehicleEventRecord record);
    }
}
