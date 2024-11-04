namespace VehicleEmulator.Domain.Interfaces
{
    public interface IVehicleEventService
    {
        VehicleEventModel GenerateVehicleEvent();
        byte[] SerializeVehicleEvent(VehicleEventModel vehicleEvent);
    }
}
