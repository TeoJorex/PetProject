using VehicleEmulator.Domain;
using VehicleEmulator.Domain.Interfaces;

namespace VehicleEmulator.BLL
{
    public class VehicleEventService : IVehicleEventService
    {
        public VehicleEventModel GenerateVehicleEvent()
        {
            var random = new Random();
            var vehicleEvent = new VehicleEventModel
            {
                Odometer = random.Next(0, 100000),
                Speedometer = (short)random.Next(0, 200),
                FuelLevel = (float)(random.NextDouble() * 100),
                IsKeysInIgnition = random.Next(0, 2) == 1,
                IsDriverInSafe = random.Next(0, 2) == 1,
                IsVehicleOnLine = random.Next(0, 2) == 1
            };
            return vehicleEvent;
        }

        public byte[] SerializeVehicleEvent(VehicleEventModel vehicleEvent)
        {
            using (var ms = new MemoryStream())
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write((byte)'O'); 
                bw.Write(BitConverter.GetBytes(vehicleEvent.Odometer));

                bw.Write((byte)'S'); 
                bw.Write(BitConverter.GetBytes((int)vehicleEvent.Speedometer));

                bw.Write((byte)'F'); 
                bw.Write(BitConverter.GetBytes(vehicleEvent.FuelLevel));

                bw.Write((byte)'B'); 
                byte statusByte = 0;
                if (vehicleEvent.IsKeysInIgnition) statusByte |= 1 << 0;
                if (vehicleEvent.IsDriverInSafe) statusByte |= 1 << 1;
                if (vehicleEvent.IsVehicleOnLine) statusByte |= 1 << 2;
                bw.Write(BitConverter.GetBytes((int)statusByte));

                return ms.ToArray();
            }
        }
    }
}
