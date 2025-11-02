using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class AdminDashboardViewModel
    {
        public IEnumerable<LockerRequest> LockerRequests { get; set; }
        public IEnumerable<GatePassModel> GatePasses { get; set; }
        public IEnumerable<Reservation> Reservations { get; set; }
        public IEnumerable<Student> Students { get; set; }
    }
}