using ProjectTemplate.Models;
using ProjectTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace ProjectTemplate.Controllers;

public class GameController : Controller
{
    private readonly IChapterService      _chapters;
    private readonly IGameSessionService  _sessions;
    private readonly ILeaderboardService  _leaderboard;

    private const string SessionKeyName = "CQ_SessionKey";

    public GameController(
        IChapterService chapters,
        IGameSessionService sessions,
        ILeaderboardService leaderboard)
    {
        _chapters    = chapters;
        _sessions    = sessions;
        _leaderboard = leaderboard;
    }

    // ── GET /Game/Begin ───────────────────────────────────────────────
    public async Task<IActionResult> Begin()
    {
        var playerName = TempData["PlayerName"]?.ToString();
        var language = TempData["Language"]?.ToString() ?? "en";
        var gameType = TempData["GameType"]?.ToString() ?? "Coding";

        if (string.IsNullOrWhiteSpace(playerName))
            return RedirectToAction("Index", "Home");

        // Generate a unique session key
        var sessionKey = Guid.NewGuid().ToString();
        HttpContext.Session.SetString(SessionKeyName, sessionKey);

        await _sessions.CreateAsync(sessionKey, playerName, language, gameType);
        return RedirectToAction("Play");
    }

    // ── GET /Game/Play ────────────────────────────────────────────────
    public async Task<IActionResult> Play()
    {
        var sessionKey = HttpContext.Session.GetString(SessionKeyName);
        if (string.IsNullOrEmpty(sessionKey))
            return RedirectToAction("Index", "Home");

        var session = await _sessions.GetActiveAsync(sessionKey);
        if (session == null)
            return RedirectToAction("Result");

        var total   = await _chapters.GetTotalCountAsync(session.Language, session.GameType);
        var chapter = await _chapters.GetByIndexAsync(session.CurrentChapterIndex, session.Language, session.GameType);

        if (chapter == null)
            return RedirectToAction("Result");

        var vm = new ChapterViewModel
        {
            Chapter         = chapter,
            TotalChapters   = total,
            CurrentIndex    = session.CurrentChapterIndex,
            Score           = session.Score,
            CorrectAnswers  = session.CorrectAnswers
        };

        return View(vm);
    }

    // ── POST /Game/Answer ─────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Answer(int choiceId)
    {
        var sessionKey = HttpContext.Session.GetString(SessionKeyName);
        if (string.IsNullOrEmpty(sessionKey))
            return RedirectToAction("Index", "Home");

        var session = await _sessions.GetActiveAsync(sessionKey);
        if (session == null)
             return Json(new { error = "no session" });

        var total  = await _chapters.GetTotalCountAsync(session.Language, session.GameType);
        var result = await _sessions.SubmitAnswerAsync(sessionKey, choiceId, total);

        return Json(result);
    }

    // ── GET /Game/Next ────────────────────────────────────────────────
    public IActionResult Next() => RedirectToAction("Play");

    // ── GET /Game/Result ──────────────────────────────────────────────
    public async Task<IActionResult> Result()
    {
        var sessionKey = HttpContext.Session.GetString(SessionKeyName);
        if (string.IsNullOrEmpty(sessionKey))
            return RedirectToAction("Index", "Home");

        var session = await _sessions.GetCompletedAsync(sessionKey);
        if (session == null)
            return RedirectToAction("Index", "Home");

        // Record to leaderboard
        await _leaderboard.RecordAsync(session);

        var allChapters = await _chapters.GetAllAsync(session.Language, session.GameType);
        var details     = new List<ChapterResultDetail>();

        foreach (var ch in allChapters)
        {
            var answer = session.Answers.FirstOrDefault(a => a.ChapterId == ch.Id);
            var chosen = answer != null
                ? ch.Choices.FirstOrDefault(c => c.Id == answer.ChosenChoiceId)
                : null;
            var correct = ch.Choices.FirstOrDefault(c => c.IsCorrect);

            details.Add(new ChapterResultDetail
            {
                ChapterLabel   = ch.Label,
                Concept        = ch.Concept,
                WasCorrect     = answer?.WasCorrect ?? false,
                ChosenAnswer   = chosen?.Text ?? "—",
                CorrectAnswer  = correct?.Text ?? "—"
            });
        }

        var vm = new ResultViewModel
        {
            Session       = session,
            TotalChapters = allChapters.Count,
            Details       = details
        };

        return View(vm);
    }

    // ── GET /Game/Leaderboard ─────────────────────────────────────────
    public async Task<IActionResult> Leaderboard()
    {
        var entries = await _leaderboard.GetTopAsync(20);
        return View(new LeaderboardViewModel { Entries = entries });
    }
}

