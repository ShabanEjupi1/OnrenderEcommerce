using ProjectTemplate.Data;
using ProjectTemplate.Models;
using ProjectTemplate.Models.Ecommerce;
using ProjectTemplate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace ProjectTemplate.Controllers;

public class HomeController : Controller
{
    private readonly ILeaderboardService _leaderboard;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _db;

    public HomeController(ILeaderboardService leaderboard, IEmailService emailService, AppDbContext db)
    {
        _leaderboard = leaderboard;
        _emailService = emailService;
        _db = db;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> Training()
    {
        var top = await _leaderboard.GetTopAsync(10);
        return View(top);
    }

    public IActionResult Services() => View();
    public IActionResult Features() => View();
    public IActionResult Careers() => View();
    public IActionResult About() => View();
    public IActionResult Faq() => View();
    public IActionResult Pricing() => View();
    public IActionResult Contact() => View();

    // ── Language switch ──────────────────────────────────────────────────

    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(
                new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );
        return LocalRedirect(returnUrl ?? "/");
    }

    // ── Contact form ─────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> SendContactMessage(string Name, string Email, string Message)
    {
        var isSq = Thread.CurrentThread.CurrentUICulture.Name == "sq";

        // Forward message to ourselves
        await _emailService.SendContactMessageAsync(Name, Email, Message, isSq);

        // Send auto-reply to the customer
        string subject = "Faleminderit që kontaktuat Enisi Center";
        string body = $"Përshëndetje <b>{System.Net.WebUtility.HtmlEncode(Name)}</b>," +
                      "<br/><br/>Kemi pranuar mesazhin tuaj dhe do t'ju përgjigjemi sa më shpejt." +
                      "<br/><br/>Të falat,<br/><b>Ekipi i Enisi Center</b>";

        await _emailService.SendNoReplyEmailAsync(Email, subject, body, isSq);

        TempData["SuccessMessage"] = isSq
            ? "Mesazhi juaj u dërgua me sukses! Do ju kontaktojmë së shpejti."
            : "Your message was sent successfully! We will contact you shortly.";
        return RedirectToAction("Contact");
    }

    // ── Newsletter subscribe ─────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Subscribe(string email, string? name)
    {
        var isSq = Thread.CurrentThread.CurrentUICulture.Name != "en";

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            TempData["SubscribeError"] = "Ju lutem vendosni një email të vlefshëm.";
            return RedirectToAction("Index");
        }

        var existing = await _db.Subscribers
            .FirstOrDefaultAsync(s => s.Email.ToLower() == email.ToLower().Trim());

        if (existing != null)
        {
            if (existing.IsActive)
            {
                TempData["SubscribeSuccess"] = "Ky email është tashmë i abonuar. Faleminderit! 🎉";
            }
            else
            {
                // Re-activate
                existing.IsActive = true;
                existing.UnsubscribedAt = null;
                existing.UnsubscribeToken = Guid.NewGuid().ToString("N");
                existing.SubscribedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                await _emailService.SendSubscriptionConfirmationAsync(
                    existing.Email, name, existing.UnsubscribeToken, isSq);

                TempData["SubscribeSuccess"] = "Jeni abonuar sërisht me sukses! Faleminderit 🎉";
            }
        }
        else
        {
            var sub = new Subscriber
            {
                Email = email.Trim().ToLower(),
                Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim(),
                IsActive = true,
                SubscribedAt = DateTime.UtcNow
            };
            _db.Subscribers.Add(sub);
            await _db.SaveChangesAsync();

            await _emailService.SendSubscriptionConfirmationAsync(
                sub.Email, sub.Name, sub.UnsubscribeToken, isSq);

            TempData["SubscribeSuccess"] = "Abonuar me sukses! Kontrolloni emailin tuaj. 🎉";
        }

        return RedirectToAction("Index");
    }

    // ── Newsletter unsubscribe (from email link) ──────────────────────────

    [HttpGet]
    public async Task<IActionResult> Unsubscribe(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return View("UnsubscribeResult", (bool?)false);

        var sub = await _db.Subscribers
            .FirstOrDefaultAsync(s => s.UnsubscribeToken == token && s.IsActive);

        if (sub == null)
            return View("UnsubscribeResult", (bool?)null); // already unsub or not found

        sub.IsActive = false;
        sub.UnsubscribedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return View("UnsubscribeResult", (bool?)true);
    }

    // ── Game actions ─────────────────────────────────────────────────────

    [HttpPost]
    public IActionResult Start(StartGameViewModel vm)
    {
        if (string.IsNullOrWhiteSpace(vm.PlayerName))
            return RedirectToAction("Training");

        TempData["PlayerName"] = vm.PlayerName.Trim();
        TempData["Language"] = vm.Language;
        TempData["GameType"] = vm.GameType;
        return RedirectToAction("Begin", "Game");
    }

    public IActionResult Error() => View();
}
