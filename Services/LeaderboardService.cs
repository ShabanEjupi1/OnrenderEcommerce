using ProjectTemplate.Data;
using ProjectTemplate.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ProjectTemplate.Services;

public interface ILeaderboardService
{
    Task RecordAsync(GameSession session);
    Task<List<LeaderboardEntry>> GetTopAsync(int count = 20);
}

public class LeaderboardService : ILeaderboardService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public LeaderboardService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task RecordAsync(GameSession session)
    {
        // Only record completed sessions
        if (!session.IsComplete) return;

        // Avoid duplicates for same session
        bool exists = await _db.LeaderboardEntries
            .AnyAsync(e => e.PlayerName == session.PlayerName
                        && e.AchievedAt >= session.StartedAt.AddSeconds(-5));
        if (exists) return;

        _db.LeaderboardEntries.Add(new LeaderboardEntry
        {
            PlayerName     = session.PlayerName,
            Score          = session.Score,
            CorrectAnswers = session.CorrectAnswers,
            Rank           = session.Rank,
            AchievedAt     = session.CompletedAt ?? DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        _cache.Remove($"leaderboard_{20}");
        _cache.Remove($"leaderboard_{10}");
    }

    public Task<List<LeaderboardEntry>> GetTopAsync(int count = 20)
    {
        var cacheKey = $"leaderboard_{count}";
        return _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            return _db.LeaderboardEntries
               .AsNoTracking()
               .OrderByDescending(e => e.Score)
               .ThenByDescending(e => e.AchievedAt)
               .Take(count)
               .ToListAsync();
        });
    }
}

