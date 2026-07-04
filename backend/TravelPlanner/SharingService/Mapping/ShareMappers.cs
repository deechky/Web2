using SharingService.Dtos;
using SharingService.Models;

namespace SharingService.Mapping
{
    public static class ShareMappers
    {
        public static ShareDto ToDto(this Share share) => new ShareDto
        {
            Id = share.Id,
            Kod = share.Kod,
            Tip = share.Tip.ToString(),
            PlanId = share.PlanId,
            IstekDatum = share.IstekDatum,
            Opozvan = share.Opozvan,
            DatumKreiranja = share.DatumKreiranja
        };
    }
}
