using System;
using System.Collections.Generic;

namespace SharingService.Dtos
{
    public class ShareCreateDto
    {
        public string Tip { get; set; } = "View";

        // Obavezno za EDIT: nalozi (email) sa pravom izmene.
        public List<string>? Emails { get; set; }
    }

    public class ShareDto
    {
        public Guid Id { get; set; }
        public string Kod { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;
        public Guid PlanId { get; set; }
        public List<string> DozvoljeniEmails { get; set; } = new();
        public DateTime? IstekDatum { get; set; }
        public bool Opozvan { get; set; }
        public DateTime DatumKreiranja { get; set; }
    }
}
