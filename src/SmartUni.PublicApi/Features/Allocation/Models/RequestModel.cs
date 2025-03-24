namespace SmartUni.PublicApi.Features.Allocation.Models
{
    public class RequestModel
    {
        public Guid TutorID { get; set; }
        public List<RequestAllocationModel> requestAllocationModels { get; set; }
        
    }
    public class RequestAllocationModel
    {
        public Guid StudentID { get; set; }
    }
}
