namespace ProjectTemplate.Models;

// ── Chapter & Quiz ─────────────────────────────────────────────────────

public class Chapter
{
    public int Id { get; set; }
    public int OrderIndex { get; set; }
    public string Language { get; set; } = "en";
    public string Label { get; set; } = "";
    public string Concept { get; set; } = "";
    public string GameType { get; set; } = "Coding"; // "Coding" or "POS"
    public string StoryHtml { get; set; } = "";
    public string CodeHtml { get; set; } = "";
    public string QuizPrompt { get; set; } = "";
    public string OkFeedback { get; set; } = "";
    public string BadFeedback { get; set; } = "";

    public ICollection<Choice> Choices { get; set; } = new List<Choice>();
}

public class Choice
{
    public int Id { get; set; }
    public int ChapterId { get; set; }
    public Chapter Chapter { get; set; } = null!;
    public string Text { get; set; } = "";
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
}

// ── Game Session ───────────────────────────────────────────────────────

public class GameSession
{
    public int Id { get; set; }
    public string SessionKey { get; set; } = "";       // ties to ASP.NET session
    public string PlayerName { get; set; } = "";
    public string Language { get; set; } = "en";
    public string GameType { get; set; } = "Coding"; // "Coding" or "POS"
    public int CurrentChapterIndex { get; set; } = 0;
    public int Score { get; set; } = 0;
    public int CorrectAnswers { get; set; } = 0;
    public bool IsComplete { get; set; } = false;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public ICollection<AnswerRecord> Answers { get; set; } = new List<AnswerRecord>();

    // Derived
    public string Rank => CorrectAnswers switch
    {
        >= 7 => "Master Operator",
        >= 4 => "Apprentice Operator",
        _ => "Junior Operator"
    };
}

public class AnswerRecord
{
    public int Id { get; set; }
    public int GameSessionId { get; set; }
    public GameSession GameSession { get; set; } = null!;
    public int ChapterId { get; set; }
    public int ChosenChoiceId { get; set; }
    public bool WasCorrect { get; set; }
    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;
}

// ── Leaderboard ────────────────────────────────────────────────────────

public class LeaderboardEntry
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = "";
    public int Score { get; set; }
    public int CorrectAnswers { get; set; }
    public string Rank { get; set; } = "";
    public DateTime AchievedAt { get; set; } = DateTime.UtcNow;
}

// ── View Models ────────────────────────────────────────────────────────

public class StartGameViewModel
{
    public string PlayerName { get; set; } = "";
    public string Language { get; set; } = "en";
    public string GameType { get; set; } = "Coding"; // "Coding" or "POS"
}

public class ChapterViewModel
{
    public Chapter Chapter { get; set; } = null!;
    public int TotalChapters { get; set; }
    public int CurrentIndex { get; set; }           // 0-based
    public int Score { get; set; }
    public int CorrectAnswers { get; set; }
    public double ProgressPercent => TotalChapters == 0 ? 0
        : (double)CurrentIndex / TotalChapters * 100;
}

public class AnswerResultViewModel
{
    public bool WasCorrect { get; set; }
    public string Feedback { get; set; } = "";
    public int CorrectChoiceId { get; set; }
    public int Score { get; set; }
    public int CorrectAnswers { get; set; }
    public bool IsLastChapter { get; set; }
}

public class ResultViewModel
{
    public GameSession Session { get; set; } = null!;
    public int TotalChapters { get; set; }
    public List<ChapterResultDetail> Details { get; set; } = new();
}

public class ChapterResultDetail
{
    public string ChapterLabel { get; set; } = "";
    public string Concept { get; set; } = "";
    public bool WasCorrect { get; set; }
    public string ChosenAnswer { get; set; } = "";
    public string CorrectAnswer { get; set; } = "";
}

public class LeaderboardViewModel
{
    public List<LeaderboardEntry> Entries { get; set; } = new();
    public LeaderboardEntry? PlayerEntry { get; set; }
}

// ── YourBrand Integration ──────────────────────────────────────────────

public class Business
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BusinessType { get; set; } = string.Empty; // Pizzeria, Supermarket, Clothing Store
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;

    public ICollection<PosSystem> PosSystems { get; set; } = new List<PosSystem>();
}

public class PosSystem
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public string Version { get; set; } = "3.0";
    public string SystemType { get; set; } = "Pizzeria POS"; // Can be dynamic
    public bool FiscalPrinterEnabled { get; set; }
    public string Theme { get; set; } = "Light";
    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
}

public class ProductItem
{
    public int Id { get; set; }
    public string Barcode { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public int PosSystemId { get; set; }
    public PosSystem PosSystem { get; set; } = null!;
    public string Category { get; set; } = "General"; // e.g. Pizza, Beverage, Clothing
}

public class POSOrder
{
    public int Id { get; set; }
    public int PosSystemId { get; set; }
    public PosSystem PosSystem { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Completed";
}

public class PosStaff
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public Business Business { get; set; } = null!;
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "Cashier"; // e.g., Cashier, Manager, Waiter
    public string PinCode { get; set; } = "";
}

public class InventoryMovement
{
    public int Id { get; set; }
    public int ProductItemId { get; set; }
    public ProductItem ProductItem { get; set; } = null!;
    public int QuantityChanged { get; set; }
    public string Reason { get; set; } = "Sale"; // e.g., Sale, Restock, Return
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;
}

public class Vendor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class Customer 
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public int LoyaltyPoints { get; set; }
}

public class PurchaseOrder 
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
}

