using System;

namespace TripService.Models
{
    public class ChecklistStavka
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public bool Zavrseno { get; set; }
    }
}
