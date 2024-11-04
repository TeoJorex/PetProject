namespace ServerStorage.Domain.Models
{
    public class VehicleEventRecord
    {
        public Guid Id { get; set; }

        public DateTime RecordDate { get; set; }

        public string JsonData { get; set; }
    }
}
