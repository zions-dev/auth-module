using System;
using System.Threading;
using System.Threading.Tasks;
using GameServer.Caching;
using GameServer.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Database.Repositories
{
    public class PlayerRepository
    {
        private readonly Func<ApplicationDbContext> _dbFactory;
        private readonly IRedisCache _cache;
        private readonly IDbConcurrencyGate _dbGate;

        public PlayerRepository(Func<ApplicationDbContext> dbFactory, IRedisCache cache, IDbConcurrencyGate dbGate)
        {
            _dbFactory = dbFactory;
            _cache = cache;
            _dbGate = dbGate;
        }

        public async Task<PlayerData> FindBySocialClub(ulong socialClubId, CancellationToken ct = default)
        {
            var key = CacheKeys.PlayerBySocialClubId(socialClubId);
            return await _cache.GetOrSetAsync(key, async _ =>
            {
                await using var db = _dbFactory();
                return await db.Players
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.SocialClubId == socialClubId, ct);
            }, TimeSpan.FromMinutes(10), ct);
        }

        public async Task<PlayerData> FindByEmailAsync(string email, CancellationToken ct = default)
        {
            var key = CacheKeys.PlayerByEmail(email);
            return await _cache.GetOrSetAsync(key, async _ =>
            {
                await using var db = _dbFactory();
                return await db.Players
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Email == email, ct);
            }, TimeSpan.FromMinutes(10), ct);
        }

        public async Task<PlayerData> FindByLoginAsync(string login, CancellationToken ct = default)
        {
            var key = CacheKeys.PlayerByLogin(login);
            return await _cache.GetOrSetAsync(key, async _ =>
            {
                await using var db = _dbFactory();
                return await db.Players
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Login == login, ct);
            }, TimeSpan.FromMinutes(10), ct);
        }

        public async Task<PlayerData> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var key = CacheKeys.PlayerById(id);
            return await _cache.GetOrSetAsync(key, async _ =>
            {
                await using var db = _dbFactory();
                return await db.Players
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == id, ct);
            }, TimeSpan.FromMinutes(10), ct);
        }

        public async Task<PlayerData> CreateAsync(PlayerData player, CancellationToken ct = default)
        {
            await using var db = _dbFactory();
            db.Players.Add(player);
            await db.SaveChangesAsync(ct);

            await _cache.RemoveAsync(CacheKeys.PlayerById(player.Id), ct);
            await _cache.RemoveAsync(CacheKeys.PlayerBySocialClubId(player.SocialClubId), ct);
            await _cache.RemoveAsync(CacheKeys.PlayerByEmail(player.Email ?? string.Empty), ct);
            await _cache.RemoveAsync(CacheKeys.PlayerByLogin(player.Login ?? string.Empty), ct);
            await _cache.PublishInvalidationAsync(CacheKeys.PlayerById(player.Id), ct);

            return player;
        }

        public async Task SaveAsync(PlayerData player, CancellationToken ct = default)
        {
            await using var _ = await _dbGate.EnterAsync($"player:{player.Id}", ct);
            await using var db = _dbFactory();
            db.Players.Update(player);
            await db.SaveChangesAsync(ct);

            await _cache.RemoveAsync(CacheKeys.PlayerById(player.Id), ct);
            await _cache.RemoveAsync(CacheKeys.PlayerBySocialClubId(player.SocialClubId), ct);
            await _cache.RemoveAsync(CacheKeys.PlayerByEmail(player.Email ?? string.Empty), ct);
            await _cache.RemoveAsync(CacheKeys.PlayerByLogin(player.Login ?? string.Empty), ct);
            await _cache.PublishInvalidationAsync(CacheKeys.PlayerById(player.Id), ct);
        }

        public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword, CancellationToken ct = default)
        {
            await using var _ = await _dbGate.EnterAsync("player:reset:" + email.ToLowerInvariant(), ct);
            await using var db = _dbFactory();

            var player = await db.Players.FirstOrDefaultAsync(p => p.Email == email, ct);
            if (player == null || player.ResetCode != code)
                return false;
            
            player.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword.NormalizePassword(), workFactor: 12);
            player.ResetCode = null;
            await db.SaveChangesAsync(ct);

            await _cache.RemoveAsync(CacheKeys.PlayerById(player.Id), ct);
            await _cache.RemoveAsync(CacheKeys.PlayerBySocialClubId(player.SocialClubId), ct);
            await _cache.RemoveAsync(CacheKeys.PlayerByEmail(player.Email ?? string.Empty), ct);
            await _cache.RemoveAsync(CacheKeys.PlayerByLogin(player.Login ?? string.Empty), ct);
            return true;
        }

        public Task<IAsyncDisposable> EnterSaveAsync(string emailOrLogin, CancellationToken ct = default)
        {
            return EnterAsync($"player:enter:save:{emailOrLogin}", ct);
        }

        public async Task<IAsyncDisposable> EnterAsync(string key, CancellationToken ct = default)
        {
            return await _dbGate.EnterAsync(key, ct);
        }
    }
}
