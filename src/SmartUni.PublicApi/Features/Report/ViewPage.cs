using static SmartUni.PublicApi.Common.Domain.Enums;

namespace SmartUni.PublicApi.Features.Report
{
    public class Page
    {
        public Guid Id { get; set; }
        public MostViewPage PageName { get; set; }
        public int ViewCount { get; set; }
    }
}
