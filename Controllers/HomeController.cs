using ProjectTemplate.Models;
using ProjectTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace ProjectTemplate.Controllers;

public class HomeController : Controller
{
    private readonly ILeaderboardService _leaderboard;
    private readonly IEmailService _emailService;

    public HomeController(ILeaderboardService leaderboard, IEmailService emailService)
    {
        _leaderboard = leaderboard;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return View();
    }

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

    [HttpPost]
    public IActionResult SetLanguage(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.DefaultCookieName,
            Microsoft.AspNetCore.Localization.CookieRequestCultureProvider.MakeCookieValue(new Microsoft.AspNetCore.Localization.RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
        );
        return LocalRedirect(returnUrl ?? "/");
    }

    [HttpPost]
    public async Task<IActionResult> SendContactMessage(string Name, string Email, string Message)
    {
        var isSq = System.Threading.Thread.CurrentThread.CurrentUICulture.Name == "sq";

        await _emailService.SendContactMessageAsync(Name, Email, Message, isSq);

        string subject = isSq ? "Faleminderit që kontaktuat YourBrand" : "Thank you for contacting YourBrand";
        string body = isSq 
            ? $"Përshëndetje {Name},<br/><br/>Kemi pranuar mesazhin tuaj dhe do t'ju përgjigjemi së shpejti.<br/><br/>Të falat,<br/>Ekipi i YourBrand" 
            : $"Hi {Name},<br/><br/>We have received your message and will get back to you shortly.<br/><br/>Best regards,<br/>YourBrand Team";

        await _emailService.SendNoReplyEmailAsync(Email, subject, body, isSq);

        TempData["SuccessMessage"] = isSq ? "Mesazhi juaj u dërgua me sukses!" : "Your message has been sent successfully!";
        return RedirectToAction("Contact");
    }

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

