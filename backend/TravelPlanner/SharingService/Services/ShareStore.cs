using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using SharingService.Data;
using SharingService.Models;

namespace SharingService.Services
{
    public class ShareStore
    {
        private const string CacheDictionaryName = "ShareCache";

        private readonly SharingDbContext _db;
        private readonly IReliableStateManager _stateManager;

        public ShareStore(SharingDbContext db, IReliableStateManager stateManager)
        {
            _db = db;
            _stateManager = stateManager;
        }

        public async Task<Share> CreateAsync(Guid planId, Guid kreatorId, ShareTip tip, int defaultExpiryDays, string? dozvoljeniEmails = null)
        {
            var kod = await GenerateUniqueCodeAsync();

            var share = new Share
            {
                Id = Guid.NewGuid(),
                Kod = kod,
                PlanId = planId,
                KreatorId = kreatorId,
                Tip = tip,
                DozvoljeniEmails = tip == ShareTip.Edit ? dozvoljeniEmails : null,
                DatumKreiranja = DateTime.UtcNow,
                IstekDatum = DateTime.UtcNow.AddDays(defaultExpiryDays),
                Opozvan = false
            };

            _db.Shares.Add(share);
            await _db.SaveChangesAsync();

            await CacheAsync(share);

            return share;
        }

        public async Task<List<Share>> GetForPlanAsync(Guid planId)
        {
            return await _db.Shares
                .Where(s => s.PlanId == planId && !s.Opozvan)
                .ToListAsync();
        }

        public async Task<Share?> ResolveAsync(string kod)
        {
            var cacheDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<string, ShareCacheEntry>>(CacheDictionaryName);

            ShareCacheEntry? cachedEntry = null;
            using (var tx = _stateManager.CreateTransaction())
            {
                var cached = await cacheDictionary.TryGetValueAsync(tx, kod);
                if (cached.HasValue)
                {
                    cachedEntry = cached.Value;
                }
                await tx.CommitAsync();
            }

            if (cachedEntry != null)
            {
                return new Share
                {
                    Kod = kod,
                    PlanId = cachedEntry.PlanId,
                    Tip = Enum.Parse<ShareTip>(cachedEntry.Tip),
                    DozvoljeniEmails = cachedEntry.DozvoljeniEmails,
                    IstekDatum = cachedEntry.IstekDatum,
                    Opozvan = cachedEntry.Opozvan
                };
            }

            var share = await _db.Shares.FirstOrDefaultAsync(s => s.Kod == kod);
            if (share != null)
            {
                await CacheAsync(share);
            }

            return share;
        }

        public async Task DeleteForPlanAsync(Guid planId)
        {
            var shares = await _db.Shares.Where(s => s.PlanId == planId).ToListAsync();
            if (shares.Count == 0)
            {
                return;
            }

            foreach (var share in shares)
            {
                share.Opozvan = true;
                await CacheAsync(share);
            }

            _db.Shares.RemoveRange(shares);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> RevokeAsync(Guid planId, Guid id)
        {
            var share = await _db.Shares.FirstOrDefaultAsync(s => s.Id == id && s.PlanId == planId);
            if (share == null)
            {
                return false;
            }

            share.Opozvan = true;
            await _db.SaveChangesAsync();

            await CacheAsync(share);

            return true;
        }

        private async Task CacheAsync(Share share)
        {
            var cacheDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<string, ShareCacheEntry>>(CacheDictionaryName);

            using var tx = _stateManager.CreateTransaction();
            await cacheDictionary.SetAsync(tx, share.Kod, new ShareCacheEntry
            {
                PlanId = share.PlanId,
                Tip = share.Tip.ToString(),
                DozvoljeniEmails = share.DozvoljeniEmails,
                IstekDatum = share.IstekDatum,
                Opozvan = share.Opozvan
            });
            await tx.CommitAsync();
        }

        private async Task<string> GenerateUniqueCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            string kod;
            do
            {
                kod = new string(Enumerable.Range(0, 10).Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)]).ToArray());
            }
            while (await _db.Shares.AnyAsync(s => s.Kod == kod));

            return kod;
        }
    }
}
