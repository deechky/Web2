using System;

namespace TripService.Dtos
{
    public class ChecklistStavkaCreateDto
    {
        public string Naziv { get; set; } = string.Empty;
    }

    public class ChecklistStavkaUpdateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public bool Zavrseno { get; set; }
    }

    public class ChecklistStavkaDto
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public bool Zavrseno { get; set; }
    }
}
