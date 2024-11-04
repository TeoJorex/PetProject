namespace Gateway.Domain.Interfaces
{
    public interface IVehicleEventService
    {
        VehicleEventModel DeserializeVehicleEvent(byte[] data);
    }
}
