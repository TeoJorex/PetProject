using Npgsql;
using ServerStorage.Domain.Interfaces;
using ServerStorage.Domain.Models;
using System.Data;
using Dapper;

namespace ServerStorage.BLL
{
    public class VehicleEventRepository : IVehicleEventRepository
    {
        private readonly string _connectionString;

        public VehicleEventRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SaveVehicleEventAsync(VehicleEventRecord record)
        {
            using (IDbConnection db = new NpgsqlConnection(_connectionString))
            {
                string insertQuery = @"
                    INSERT INTO vehicle_event_records (id, record_date, json_data)
                    VALUES (@Id, @RecordDate, @JsonData);";

                await db.ExecuteAsync(insertQuery, record);
            }
        }
    }
}
