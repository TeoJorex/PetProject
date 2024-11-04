namespace ServerStorage.Domain.Models
{
    public class VehicleEventModel
    {
        public int Odometer { get; set; }

        public short Speedometer { get; set; }

        public float FuelLevel { get; set; }

        public bool IsKeysInIgnition { get; set; }

        public bool IsDriverInSafe { get; set; }

        public bool IsVehicleOnLine { get; set; }
    }
}
