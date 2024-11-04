using Gateway.Domain;
using Gateway.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gateway.BLL
{
    public class VehicleEventService : IVehicleEventService
    {
        public VehicleEventModel DeserializeVehicleEvent(byte[] data)
        {
            var vehicleEvent = new VehicleEventModel();

            using (var ms = new MemoryStream(data))
            using (var br = new BinaryReader(ms))
            {
                try
                {
                    while (ms.Position < ms.Length)
                    {
                        char dataType = (char)br.ReadByte();
                        byte[] dataValueBytes = br.ReadBytes(4);

                        switch (dataType)
                        {
                            case 'O':
                                int odometer = BitConverter.ToInt32(dataValueBytes, 0);
                                vehicleEvent.Odometer = odometer;
                                break;
                            case 'S': 
                                int speedometerInt = BitConverter.ToInt32(dataValueBytes, 0);
                                vehicleEvent.Speedometer = (short)speedometerInt;
                                break;
                            case 'F': 
                                float fuelLevel = BitConverter.ToSingle(dataValueBytes, 0);
                                vehicleEvent.FuelLevel = fuelLevel;
                                break;
                            case 'B': 
                                int statusInt = BitConverter.ToInt32(dataValueBytes, 0);
                                byte statusByte = (byte)statusInt;

                                vehicleEvent.IsKeysInIgnition = (statusByte & (1 << 0)) != 0;
                                vehicleEvent.IsDriverInSafe = (statusByte & (1 << 1)) != 0;
                                vehicleEvent.IsVehicleOnLine = (statusByte & (1 << 2)) != 0;
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidDataException("Ошибка при десериализации данных.", ex);
                }
            }

            return vehicleEvent;
        }
    }
}
