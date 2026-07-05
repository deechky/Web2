using System.Collections.Generic;

namespace TripService.Dtos
{
    public class FullPlanDto
    {
        public PlanDto Plan { get; set; } = null!;
        public List<DestinacijaDto> Destinacije { get; set; } = new();
        public List<AktivnostDto> Aktivnosti { get; set; } = new();
        public List<TrosakDto> Troskovi { get; set; } = new();
        public List<ChecklistStavkaDto> ChecklistStavke { get; set; } = new();
        public List<BeleskaDto> Beleske { get; set; } = new();
        public List<PodsetnikDto> Podsetnici { get; set; } = new();
        public BudgetDto Budzet { get; set; } = null!;
    }
}
