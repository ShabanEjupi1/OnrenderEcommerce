using ProjectTemplate.Data;
using ProjectTemplate.Models;
using Microsoft.EntityFrameworkCore;

namespace ProjectTemplate.Services;

public interface IGameSessionService
{
    Task<GameSession> CreateAsync(string sessionKey, string playerName, string language, string gameType);
    Task<GameSession?> GetActiveAsync(string sessionKey);
    Task<AnswerResultViewModel> SubmitAnswerAsync(string sessionKey, int choiceId, int totalChapters);
    Task<GameSession?> GetCompletedAsync(string sessionKey);
}

public class GameSessionService : IGameSessionService
{
    private readonly AppDbContext _db;

    public GameSessionService(AppDbContext db) => _db = db;

    public async Task<GameSession> CreateAsync(string sessionKey, string playerName, string language, string gameType)
    {
        // Remove any old session for this key using ExecuteDeleteAsync for performance
        await _db.GameSessions
            .Where(s => s.SessionKey == sessionKey)
            .ExecuteDeleteAsync();

        var session = new GameSession
        {
            SessionKey   = sessionKey,
            PlayerName   = playerName,
            Language     = language,
            GameType     = gameType,
            StartedAt    = DateTime.UtcNow
        };
        _db.GameSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public Task<GameSession?> GetActiveAsync(string sessionKey) =>
        _db.GameSessions
           .Include(s => s.Answers)
           .FirstOrDefaultAsync(s => s.SessionKey == sessionKey && !s.IsComplete);

    public async Task<AnswerResultViewModel> SubmitAnswerAsync(
        string sessionKey, int choiceId, int totalChapters)
    {
        var session = await GetActiveAsync(sessionKey)
            ?? throw new InvalidOperationException("No active session found.");

        var choice = await _db.Choices
            .Include(c => c.Chapter)
            .FirstOrDefaultAsync(c => c.Id == choiceId)
            ?? throw new InvalidOperationException("Choice not found.");

        bool correct = choice.IsCorrect;

        // Record
        _db.AnswerRecords.Add(new AnswerRecord
        {
            GameSessionId  = session.Id,
            ChapterId      = choice.ChapterId,
            ChosenChoiceId = choiceId,
            WasCorrect     = correct
        });

        if (correct)
        {
            session.Score += 20;
            session.CorrectAnswers++;
        }

        session.CurrentChapterIndex++;
        bool isLast = session.CurrentChapterIndex >= totalChapters;

        if (isLast)
        {
            session.IsComplete    = true;
            session.CompletedAt   = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        return new AnswerResultViewModel
        {
            WasCorrect      = correct,
            Feedback        = correct ? choice.Chapter.OkFeedback : choice.Chapter.BadFeedback,
            CorrectChoiceId = await _db.Choices
                .Where(c => c.ChapterId == choice.ChapterId && c.IsCorrect)
                .Select(c => c.Id)
                .FirstOrDefaultAsync(),
            Score           = session.Score,
            CorrectAnswers  = session.CorrectAnswers,
            IsLastChapter   = isLast
        };
    }

    public async Task<GameSession?> GetCompletedAsync(string sessionKey)
    {
        var session = await _db.GameSessions
            .AsNoTracking()
            .Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.SessionKey == sessionKey && s.IsComplete);

        return session;
    }
}

