using ProjectTemplate.Models;

namespace ProjectTemplate.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        var existingChapters = db.Chapters
            .AsEnumerable()
            .GroupBy(c => c.Language + "_" + c.GameType + "_" + c.Concept)
            .ToDictionary(g => g.Key, g => g.First());

        var chapters = new List<Chapter>();

        // EN & SQ helper to create bilingual chapters easily
        void AddChapter(int order, string concept, 
            string enLabel, string enStory, string enCode, string enPrompt, string enOk, string enBad,
            string sqLabel, string sqStory, string sqCode, string sqPrompt, string sqOk, string sqBad,
            (string text, bool ok)[] enChoices,
            (string text, bool ok)[] sqChoices,
            string gameType = "Coding")
        {
            var enKey = "en_" + gameType + "_" + concept;
            if (!existingChapters.TryGetValue(enKey, out var existingEn))
            {
                var cEn = new Chapter { Language = "en", GameType = gameType, OrderIndex = order, Concept = concept, Label = enLabel, StoryHtml = enStory, CodeHtml = enCode, QuizPrompt = enPrompt, OkFeedback = enOk, BadFeedback = enBad };
                for (int i=0; i<enChoices.Length; i++) cEn.Choices.Add(new Choice { Text = enChoices[i].text, IsCorrect = enChoices[i].ok, OrderIndex = i });
                chapters.Add(cEn);
            }
            else
            {
                existingEn.Label = enLabel; existingEn.StoryHtml = enStory; existingEn.CodeHtml = enCode; existingEn.QuizPrompt = enPrompt; existingEn.OkFeedback = enOk; existingEn.BadFeedback = enBad; existingEn.OrderIndex = order;
            }

            var sqKey = "sq_" + gameType + "_" + concept;
            if (!existingChapters.TryGetValue(sqKey, out var existingSq))
            {
                var cSq = new Chapter { Language = "sq", GameType = gameType, OrderIndex = order, Concept = concept, Label = sqLabel, StoryHtml = sqStory, CodeHtml = sqCode, QuizPrompt = sqPrompt, OkFeedback = sqOk, BadFeedback = sqBad };
                for (int i=0; i<sqChoices.Length; i++) cSq.Choices.Add(new Choice { Text = sqChoices[i].text, IsCorrect = sqChoices[i].ok, OrderIndex = i });
                chapters.Add(cSq);
            }
            else
            {
                existingSq.Label = sqLabel; existingSq.StoryHtml = sqStory; existingSq.CodeHtml = sqCode; existingSq.QuizPrompt = sqPrompt; existingSq.OkFeedback = sqOk; existingSq.BadFeedback = sqBad; existingSq.OrderIndex = order;
            }
        }

        // 1. VARIABLES
        AddChapter(0, "Variables",
            "Chapter 1 Â· The POS Terminal",
            "<p>You connect to the <strong>YourBrand Mainframe</strong>...</p><p>A variable is like a <strong>labelled box</strong> â€” you give it a name, and put a value inside it.</p>",
            "<span class='kw'>cashier_name</span> = <span class='str'>\"Arben\"</span>",
            "The POS asks: \"What IS a variable?\"",
            "âœ“ Exactly right! You spoke the name â€” 'cashier_name' â€” and the terminal unlocked.",
            "âœ— A variable is like a labelled box.",
            "Kapitulli 1 Â· Terminali i YourBrand",
            "<p>Lidheni me <strong>Sistemin YourBrand</strong>...</p><p>NjÃ« variabÃ«l Ã«shtÃ« si njÃ« <strong>kuti me etiketÃ«</strong> â€” i jepni njÃ« emÃ«r dhe futni njÃ« vlerÃ« brenda.</p>",
            "<span class='kw'>emri_arkÃ«tarit</span> = <span class='str'>\"Agroni\"</span>",
            "Sistemi i mbrojtjes sÃ« arkÃ«s aktivizohet dhe tÃ« shtron njÃ« pyetje sfiduese: \"A mund tÃ« mÃ« thuash, Ã§farÃ« Ã‹SHTÃ‹ saktÃ«sisht njÃ« variabÃ«l nÃ« botÃ«n e programimit?\"",
            "âœ“ SaktÃ«! PÃ«rdorÃ«t emrin dhearka u zhbllokua.",
            "âœ— NjÃ« variabÃ«l Ã«shtÃ« si njÃ« kuti me etiketÃ«.",
            new[] { ("A labelled container that stores a value", true), ("A type of loop that repeats forever", false), ("A system command", false), ("A constantly changing number", false) },
            new[] { ("NjÃ« enÃ« e emÃ«rtuar qÃ« ruan njÃ« vlerÃ«", true), ("NjÃ« lloj unaze qÃ« pÃ«rsÃ«ritet pÃ«rgjithmonÃ«", false), ("NjÃ« komandÃ« e veÃ§antÃ«", false), ("NjÃ« numÃ«r i pandryshueshÃ«m", false) });

        // 2. DATA TYPES
        AddChapter(1, "Data Types",
            "Chapter 2 Â· The Order Processor",
            "<p>Order details drift on your screen... The main types: int, string, boolean.</p>",
            "<span class='kw'>is_pizza_ready</span> = <span class='kw'>True</span>",
            "A fragment reads: \"is_oven_hot = False\". What data type is this?",
            "âœ“ Correct! Booleans only hold True or False.",
            "âœ— 'False' is a boolean value.",
            "Kapitulli 2 Â· Procesori i Porosive",
            "<p>PorositÃ« shfaqen nÃ« ekran... Llojet kryesore: int, string, boolean.</p>",
            "<span class='kw'>eshte_pica_gati</span> = <span class='kw'>True</span>",
            "NjÃ« nga udhÃ«zimet nÃ« ekran tregon: \"eshte_furra_ngrohte = False\". Duke marrÃ« parasysh atÃ« qÃ« sapo mÃ«sove, cili lloj i tÃ« dhÃ«nave mund tÃ« mbajÃ« kÃ«tÃ« vlerÃ«?",
            "âœ“ E saktÃ«! Boolean mund tÃ« jetÃ« vetÃ«m True ose False.",
            "âœ— 'False' Ã«shtÃ« njÃ« vlerÃ« boolean.",
            new[] { ("int", false), ("string", false), ("boolean", true), ("float", false) },
            new[] { ("int (numÃ«r i plotÃ«)", false), ("string (tekst)", false), ("boolean (E vÃ«rtetÃ«/E gabuar)", true), ("float (numÃ«r me presje)", false) });

        // 3. CONDITIONALS
        AddChapter(2, "Conditionals",
            "Chapter 3 Â· The Logic Bridge",
            "<p>Two paths lead forward... The bridge chooses based on a <strong>condition</strong>. If True, left. Else, right.</p>",
            "<span class='kw'>if</span> has_flashlight:\n    <span class='fn'>print</span>(<span class='str'>\"Walk the lit path.\"</span>)\n<span class='kw'>else</span>:\n    <span class='fn'>print</span>(<span class='str'>\"Darkness.\"</span>)",
            "If has_flashlight is False, which line prints?",
            "âœ“ Right! When condition is False, 'else' block runs.",
            "âœ— When condition is False, only 'else' runs.",
            "Kapitulli 3 Â· Ura e LogjikÃ«s",
            "<p>Dy rrugÃ« hapen... Ura zgjedh bazuar nÃ« njÃ« <strong>kusht (condition)</strong>. NÃ«se Ã«shtÃ« True (e vÃ«rtetÃ«), shko majtas. PÃ«rndryshe (Else), djathtas.</p>",
            "<span class='kw'>if</span> ka_drite:\n    <span class='fn'>print</span>(<span class='str'>\"Rruga e ndriÃ§uar.\"</span>)\n<span class='kw'>else</span>:\n    <span class='fn'>print</span>(<span class='str'>\"ErrÃ«sirÃ«.\"</span>)",
            "Kujdes, kjo Ã«shtÃ« e rÃ«ndÃ«sishme: NÃ«se variabla 'ka_drite' kthehet si False (E gabuar), cilin rresht kodi do tÃ« shohÃ«sh tÃ« printuar nÃ« ekranin tÃ«nd?",
            "âœ“ SaktÃ«! Kur kushti Ã«shtÃ« False, blloku 'else' ekzekutohet.",
            "âœ— Kur kushti Ã«shtÃ« False, ekzekutohet vetÃ«m 'else'.",
            new[] { ("Both lines", false), ("Walk the lit path.", false), ("Darkness.", true), ("Neither", false) },
            new[] { ("TÃ« dy rreshtat", false), ("Rruga e ndriÃ§uar.", false), ("ErrÃ«sirÃ«.", true), ("AsnjÃ«ri", false) });

        // 4. LOOPS
        AddChapter(3, "Loops",
            "Chapter 4 Â· The Repeating Process",
            "<p>There are 100 files to extract. We use a <strong>loop</strong> to repeat an action for every item.</p>",
            "<span class='kw'>for</span> <span class='var'>file</span> <span class='kw'>in</span> files:\n    <span class='fn'>extract</span>(<span class='var'>file</span>)",
            "The list has 4 files. How many times does extract() run?",
            "âœ“ Exactly! The loop runs 4 times.",
            "âœ— It runs once per item.",
            "Kapitulli 4 Â· Procesi PÃ«rsÃ«ritÃ«s",
            "<p>Ka 100 skedarÃ« pÃ«r t'u hapur. Ne pÃ«rdorim njÃ« <strong>UnazÃ« (Loop)</strong> pÃ«r tÃ« pÃ«rsÃ«ritur njÃ« veprim pÃ«r secilin element.</p>",
            "<span class='kw'>for</span> <span class='var'>dosje</span> <span class='kw'>in</span> dosjet:\n    <span class='fn'>hap</span>(<span class='var'>dosje</span>)",
            "SupozojmÃ« qÃ« kemi njÃ« listÃ« me saktÃ«sisht 4 skedarÃ«. Duke pÃ«rdorur kÃ«tÃ« unazÃ« (loop), sa herÃ« do tÃ« thirret funksioni per t'i hapur?",
            "âœ“ PikÃ«risht! Unaza ekzekutohet 4 herÃ«.",
            "âœ— Ajo ekzekutohet njÃ« herÃ« pÃ«r secilin element.",
            new[] { ("1 time", false), ("4 times", true), ("Infinite times", false), ("0 times", false) },
            new[] { ("1 herÃ«", false), ("4 herÃ«", true), ("PafundÃ«sisht", false), ("0 herÃ«", false) });

        // 5. FUNCTIONS
        AddChapter(4, "Functions",
            "Chapter 5 Â· The Core Components",
            "<p>We must repeat a sequence of steps 3 times. We bundle them into a <strong>function</strong>â€”a reusable package of code.</p>",
            "<span class='kw'>def</span> <span class='fn'>process_data</span>(<span class='var'>name</span>): ...",
            "What is the MAIN benefit of wrapping code in a function?",
            "âœ“ Perfect! You define it once and reuse it.",
            "âœ— Reusability is the main point.",
            "Kapitulli 5 Â· KomponentÃ«t Baze",
            "<p>Ne duhet tÃ« pÃ«rsÃ«risim disa hapa 3 herÃ«. Ne i grupojmÃ« ato nÃ« njÃ« <strong>funksion (function)</strong>â€”njÃ« paketÃ« kodi e ripÃ«rdorshme.</p>",
            "<span class='kw'>def</span> <span class='fn'>proceso_te_dhenat</span>(<span class='var'>emri</span>): ...",
            "NdÃ«rsa vazhdon tÃ« eksplorosh, duhet tÃ« kuptosh thelbin: Cila Ã«shtÃ« arsyeja KRYESORE dhe mÃ« e rÃ«ndÃ«sishme pse ne i grupojmÃ« komandat tona brenda njÃ« funksioni?",
            "âœ“ ShkÃ«lqyeshÃ«m! E shkruan njÃ« herÃ« dhe e pÃ«rdor shumÃ« herÃ«.",
            "âœ— RipÃ«rdorimi i kodit Ã«shtÃ« qÃ«llimi kryesor.",
            new[] { ("Makes code run faster", false), ("Define once, reuse many times", true), ("Replaces variables", false), ("Only for numbers", false) },
            new[] { ("E bÃ«n kodin tÃ« ekzekutohet mÃ« shpejt", false), ("E shkruan kodin njÃ« herÃ«, e pÃ«rdor shumÃ« herÃ«", true), ("ZÃ«vendÃ«son variablat", false), ("Punon vetÃ«m me numra", false) });

        // 6. ARRAYS / LISTS (NEW CHAPTER)
        AddChapter(5, "Lists / Arrays",
            "Chapter 6 Â· The Infinite Database",
            "<p>You find a massive system directory. To manage so many records, you need a <strong>List (or Array)</strong>. A List holds multiple items in a specific order within one single variable.</p>",
            "<span class='kw'>records</span> = [<span class='str'>\"User_Data\"</span>, <span class='str'>\"System_Log\"</span>]",
            "How do you organize multiple items together in order?",
            "âœ“ Correct! A list holds them together.",
            "âœ— A list or array manages ordered items.",
            "Kapitulli 6 Â· Baza e tÃ« DhÃ«nave e Pafundme",
            "<p>Gjeni njÃ« direktori tÃ« madhe sistemi. PÃ«r tÃ« menaxhuar gjithÃ« kÃ«to regjistrime, nevojitet njÃ« <strong>ListÃ«/MatricÃ« (List/Array)</strong>. NjÃ« listÃ« mban shumÃ« elementÃ« njÃ«ri pas tjetrit nÃ« njÃ« variabÃ«l tÃ« vetme.</p>",
            "<span class='kw'>regjistrat</span> = [<span class='str'>\"Te_Dhenat\"</span>, <span class='str'>\"Log_Sistemi\"</span>]",
            "Tani qÃ« keni gjithÃ« kÃ«tÃ« sasi informacioni, si mund t'i ruani dhe organizoni tÃ« gjithÃ« kÃ«ta elementÃ« nÃ« njÃ« rregull tÃ« caktuar brenda kompjuterit?",
            "âœ“ E saktÃ«! NjÃ« listÃ« i mban sÃ« bashku.",
            "âœ— NjÃ« listÃ« menaxhon elementÃ« nÃ« seri.",
            new[] { ("With an integer", false), ("Using a Loop", false), ("In a List / Array", true), ("With an if-statement", false) },
            new[] { ("Me anÃ« tÃ« njÃ« numri tÃ« plotÃ«", false), ("Duke pÃ«rdorur njÃ« UnazÃ« (Loop)", false), ("NÃ« njÃ« ListÃ« (List/Array)", true), ("Ato krijohen automatikisht", false) });

        // 7. DICTIONARIES
        AddChapter(6, "Dictionaries",
            "Chapter 7 Â· The Index of Nodes",
            "<p>To find the exact key for each server, the system uses a <strong>Dictionary (Map)</strong>. Instead of numbered positions, Dictionaries link a \"Key\" (like a server name) to a \"Value\" (like its passcode).</p>",
            "<span class='kw'>passcodes</span> = {<span class='str'>\"ServerA\"</span>: <span class='str'>\"1234\"</span>, <span class='str'>\"ServerB\"</span>: <span class='str'>\"5678\"</span>}",
            "What makes a Dictionary different from a simple List?",
            "âœ“ Spot on! Keys and Values link data directly.",
            "âœ— Dictionaries use Key-Value pairs.",
            "Kapitulli 7 Â· Treguesi i Nyjeve",
            "<p>PÃ«r tÃ« gjetur kodin e saktÃ« tÃ« secilit server, sistemi pÃ«rdor njÃ« <strong>Fjalor (Dictionary)</strong>. FjalorÃ«t lidhin njÃ« \"Ã‡elÃ«s\" (si emri i serverit) me njÃ« \"VlerÃ«\" (si fjalÃ«kalimi i saj).</p>",
            "<span class='kw'>koded</span> = {<span class='str'>\"ServeriA\"</span>: <span class='str'>\"1234\"</span>, <span class='str'>\"ServeriB\"</span>: <span class='str'>\"5678\"</span>}",
            "Sistemi kÃ«rkon saktÃ«si absolute: Cila Ã«shtÃ« karakteristika themelore qÃ« e bÃ«n njÃ« Fjalor (Dictionary) tÃ« ndryshÃ«m dhe mÃ« tÃ« pÃ«rshtatshÃ«m sesa njÃ« ListÃ« e thjeshtÃ« pÃ«r raste tÃ« veÃ§anta?",
            "âœ“ Perfekte! Ã‡elÃ«sat dhe vlerat e strukturojnÃ« atÃ«.",
            "âœ— FjalorÃ«t pÃ«rdorin Ã§ifte Ã‡elÃ«s-VlerÃ« (Key-Value).",
            new[] { ("It loops forever", false), ("It pairs Keys and Values", true), ("It only stores numbers", false), ("It automatically compresses data", false) },
            new[] { ("PÃ«rsÃ«rit pÃ«rgjithmonÃ«", false), ("Ã‡ifton njÃ« Ã‡elÃ«s me njÃ« VlerÃ«", true), ("Ruan vetÃ«m numra", false), ("Kompjeshon tÃ« dhÃ«nat", false) });

        // 8. CLASSES
        AddChapter(7, "Classes",
            "Chapter 8 Â· The Blueprints",
            "<p>A Class acts like a blueprint for creating objects. It defines properties and behaviors that its objects will have in the exact same format.</p>",
            "<span class='kw'>class</span> Robot:\n    <span class='kw'>def</span> <span class='fn'>__init__</span>(self, name):\n        self.name = name",
            "What is the primary role of a Class in programming?",
            "âœ“ Exactly! A class is a blueprint for objects.",
            "âœ— A class acts as a blueprint.",
            "Kapitulli 8 Â· Skicat Teknologjike",
            "<p>NjÃ« KlasÃ« (Class) shÃ«rben si njÃ« skicÃ« pÃ«r krijimin e objekteve. Ajo pÃ«rcakton vetitÃ« dhe sjelljet qÃ« do tÃ« kenÃ« objektet e ngjashme.</p>",
            "<span class='kw'>class</span> Roboti:\n    <span class='kw'>def</span> <span class='fn'>__init__</span>(self, emri):\n        self.emri = emri",
            "NdÃ«rsa vazhdojmÃ« tÃ« ndÃ«rtojmÃ« sisteme komplekse, duhet tÃ« kuptosh se cili Ã«shtÃ« roli kryesor i njÃ« Klase nÃ« programim?",
            "âœ“ PÃ«rkryer! Klasa Ã«shtÃ« njÃ« skicÃ« pÃ«r objektet.",
            "âœ— NjÃ« klasÃ« funksionon si skicÃ« pÃ«r objektet.",
            new[] { ("To define a blueprint for real objects", true), ("To style text", false), ("To stop the program", false), ("To multiply variables", false) },
            new[] { ("TÃ« ofrojÃ« njÃ« skicÃ« pÃ«r objektet", true), ("TÃ« formatojÃ« tekstin", false), ("TÃ« ndalojÃ« programin", false), ("TÃ« shumÃ«zojÃ« variablat", false) });

        // 9. EXCEPTIONS
        AddChapter(8, "Exceptions",
            "Chapter 9 Â· Error Handling",
            "<p>When unexpected issues happen, they crash the app. We use <strong>Try-Except (Catch)</strong> mechanisms to gracefully handle exceptions and keep the system online.</p>",
            "<span class='kw'>try</span>:\n    <span class='fn'>connect</span>()\n<span class='kw'>except</span> Exception:\n    <span class='fn'>print</span>(<span class='str'>\"Connection Failed!\"</span>)",
            "What is the main purpose of an Exception handler?",
            "âœ“ Correct! It catches errors safely.",
            "âœ— Exception handlers deal with unexpected errors.",
            "Kapitulli 9 Â· Menaxhimi i Gabimeve",
            "<p>Kur ndodhin probleme tÃ« papritura, programi mbyllet papritur. PÃ«rdorim blloqet <strong>Try-Except (Catch)</strong> pÃ«r tÃ« menaxhuar gabimet pa prishur punÃ«n e sistemit.</p>",
            "<span class='kw'>try</span>:\n    <span class='fn'>lidhu</span>()\n<span class='kw'>except</span> Exception:\n    <span class='fn'>print</span>(<span class='str'>\"Lidhja DÃ«shtoi!\"</span>)",
            "TashmÃ« qÃ« kemi ardhur kaq larg, duhet tÃ« sigurojmÃ« stabilitet: Cili Ã«shtÃ« qÃ«llimi kryesor i njÃ« blloku pÃ«r menaxhimin e gabimeve (Exception handler)?",
            "âœ“ E saktÃ«! Ai kap gabimet pa mbyllur sistemin.",
            "âœ— Menaxhon gabimet dhe ruan programin nga bllokimi.",
            new[] { ("To make the system crash faster", false), ("To catch and manage unpredictable errors", true), ("To delete corrupted files by default", false), ("To ignore correct logic", false) },
            new[] { ("TÃ« shkaktojÃ« kolaps mÃ« shpejt", false), ("TÃ« menaxhojÃ« gabimet e papritura", true), ("TÃ« fshijÃ« dokumentet me problem", false), ("TÃ« injorojÃ« kodin e saktÃ«", false) });

        // 10. ASYNC/AWAIT
        AddChapter(9, "Async/Await",
            "Chapter 10 Â· The Asynchronous Network",
            "<p>When a task takes a long time, like downloading a file, the system shouldn't freeze. <strong>Asynchrony (Async/Await)</strong> lets the program keep running while waiting for the task to finish.</p>",
            "<span class='kw'>async def</span> <span class='fn'>download_data</span>():\n    <span class='kw'>await</span> <span class='fn'>fetch_file</span>()",
            "Why do we use async/await?",
            "âœ“ Great! It prevents the system from freezing while waiting.",
            "âœ— Async helps prevent blocking operations.",
            "Kapitulli 10 Â· Rrjeti Asinkron",
            "<p>Kur njÃ« detyrÃ« zgjat shumÃ«, si shkarkimi i njÃ« skedari, sistemi nuk duhet tÃ« ngrijÃ«. <strong>Ekzekutimi Asinkron (Async/Await)</strong> lejon programin tÃ« vazhdojÃ« ndÃ«rkohÃ« qÃ« pret pÃ«rfundimin e detyrÃ«s.</p>",
            "<span class='kw'>async def</span> <span class='fn'>shkarko_te_dhena</span>():\n    <span class='kw'>await</span> <span class='fn'>merr_skedar</span>()",
            "Pse pÃ«rdorim programimin asinkron?",
            "âœ“ E saktÃ«! Ai parandalon ngrirjen e sistemit.",
            "âœ— Async shmang bllokimin e operacioneve.",
            new[] { ("To make code run slower", false), ("To stop the program", false), ("To prevent freezing while waiting", true), ("To create endless loops", false) },
            new[] { ("TÃ« ngadalÃ«sojÃ« kodin", false), ("TÃ« ndalojÃ« programin", false), ("TÃ« parandalojÃ« ngrirjen gjatÃ« pritjes", true), ("TÃ« krijojÃ« unaza pa fund", false) });

        // 11. APIS
        AddChapter(10, "APIs",
            "Chapter 11 Â· The Communication Port",
            "<p>Systems talk to each other through <strong>APIs (Application Programming Interfaces)</strong>. They allow different programs to send and receive data securely.</p>",
            "<span class='var'>response</span> = <span class='fn'>requests.get</span>(<span class='str'>\"https://api.server.com/status\"</span>)",
            "What is the main role of an API?",
            "âœ“ Spot on! APIs enable communication between different systems.",
            "âœ— APIs are used for communication between systems.",
            "Kapitulli 11 Â· Porta e Komunikimit",
            "<p>Sistemet komunikojnÃ« me njÃ«ri-tjetrin nÃ«pÃ«rmjet <strong>API-ve (Application Programming Interfaces)</strong>. Ato lejojnÃ« programe tÃ« ndryshme tÃ« dÃ«rgojnÃ« dhe marrin tÃ« dhÃ«na tÃ« sigurta.</p>",
            "<span class='var'>pergjegja</span> = <span class='fn'>kerkesa.merr</span>(<span class='str'>\"https://api.server.com/status\"</span>)",
            "Cili Ã«shtÃ« roli kryesor i njÃ« API-je?",
            "âœ“ E saktÃ«! API-tÃ« mundÃ«sojnÃ« komunikimin mes sistemeve.",
            "âœ— API-tÃ« pÃ«rdoren pÃ«r tÃ« komunikuar mes programeve.",
            new[] { ("To design user interfaces", false), ("To allow programs to communicate", true), ("To delete system files", false), ("To draw 3D graphics", false) },
            new[] { ("TÃ« dizenjojÃ« pamjet pÃ«r pÃ«rdoruesit", false), ("TÃ« lejojÃ« programet tÃ« komunikojnÃ«", true), ("TÃ« fshijÃ« dokumentet e sistemit", false), ("TÃ« vizatojÃ« grafika 3D", false) });

        // 12. DATABASES
        AddChapter(11, "Databases",
            "Chapter 12 Â· The Data Vault",
            "<p>To store data permanently, we use <strong>Databases</strong>. Using languages like SQL, we can save, retrieve, update, and delete millions of records efficiently.</p>",
            "<span class='kw'>SELECT</span> * <span class='kw'>FROM</span> Users <span class='kw'>WHERE</span> role = <span class='str'>'admin'</span>;",
            "What is the primary purpose of a Database?",
            "âœ“ Correct! It stores data permanently and efficiently.",
            "âœ— Databases are used for permanent data storage.",
            "Kapitulli 12 Â· Kasaforta e tÃ« DhÃ«nave",
            "<p>PÃ«r tÃ« ruajtur tÃ« dhÃ«nat pÃ«rgjithmonÃ«, ne pÃ«rdorim <strong>Bazat e tÃ« DhÃ«nave (Databases)</strong>. Duke pÃ«rdorur gjuhÃ« si SQL, mund tÃ« ruajmÃ«, gjejmÃ«, pÃ«rditÃ«sojmÃ« dhe fshijmÃ« miliona regjistrime nÃ« mÃ«nyrÃ« efikase.</p>",
            "<span class='kw'>SELECT</span> * <span class='kw'>FROM</span> Perdoruesit <span class='kw'>WHERE</span> roli = <span class='str'>'admin'</span>;",
            "Cili Ã«shtÃ« qÃ«llimi kryesor i njÃ« Baze tÃ« DhÃ«nash?",
            "âœ“ E saktÃ«! Ruan tÃ« dhÃ«nat pÃ«rgjithmonÃ« dhe nÃ« mÃ«nyrÃ« efikase.",
            "âœ— Bazat e tÃ« dhÃ«nave pÃ«rdoren pÃ«r ruajtjen e pÃ«rhershme.",
            new[] { ("To render web pages", false), ("To store and manage data permanently", true), ("To format text", false), ("To style the application", false) },
            new[] { ("TÃ« shfaqÃ« faqet e internetit", false), ("TÃ« ruajÃ« dhe menaxhojÃ« tÃ« dhÃ«nat pÃ«rgjithmonÃ«", true), ("TÃ« formatojÃ« tekstin", false), ("TÃ« stilojÃ« aplikacionin", false) });

        // 13. VERSION CONTROL
        AddChapter(12, "Version Control",
            "Chapter 13 Â· The Time Machine",
            "<p>When building systems, you need a way to track changes. <strong>Version Control (like Git)</strong> saves history, letting you revert mistakes and merge work with others.</p>",
            "<span class='kw'>git</span> commit -m <span class='str'>\"Added new feature\"</span>",
            "Why is Version Control important?",
            "âœ“ Great! It tracks changes and helps collaboration.",
            "âœ— Version control tracks your code's history.",
            "Kapitulli 13 Â· Makina e KohÃ«s",
            "<p>GjatÃ« ndÃ«rtimit tÃ« sistemeve, duhet tÃ« gjurmoni ndryshimet. <strong>Kontrolli i Versioneve (si Git)</strong> ruan historikun, duke ju lejuar tÃ« ktheni gabimet mbrapsht dhe tÃ« bashkoni punÃ«n me tÃ« tjerÃ«t.</p>",
            "<span class='kw'>git</span> commit -m <span class='str'>\"Shtuar tipar i ri\"</span>",
            "Pse Ã«shtÃ« i rÃ«ndÃ«sishÃ«m Kontrolli i Versioneve?",
            "âœ“ SaktÃ«! Gjurmon ndryshimet dhe ndihmon bashkÃ«punimin.",
            "âœ— Kontrolli i versioneve ruan historinÃ« e kodit.",
            new[] { ("To encrypt passwords", false), ("To run code faster", false), ("To track changes and collaborate", true), ("To compile code to machine language", false) },
            new[] { ("TÃ« kriptojÃ« fjalÃ«kalimet", false), ("TÃ« ekzekutojÃ« kodin mÃ« shpejt", false), ("TÃ« gjurmojÃ« ndryshimet dhe tÃ« bashkÃ«punojÃ«", true), ("TÃ« pÃ«rkthejÃ« kodin", false) });

        // 14. INHERITANCE
        AddChapter(13, "Inheritance",
            "Chapter 14 Â· The Parent Protocols",
            "<p>Instead of rewriting code, classes can <strong>Inherit</strong> from parents, gaining all their attributes.</p>",
            "<span class='kw'>class</span> Admin(User):\n    <span class='fn'>pass</span>",
            "What does inheritance allow you to do?",
            "âœ“ Right! It lets you reuse an existing structure.",
            "âœ— Think about reusing parent code.",
            "Kapitulli 14 Â· Protokollet PrindÃ«rore",
            "<p>NÃ« vend qÃ« tÃ« rishkruajnÃ« kod, klasat mund tÃ« <strong>TrashÃ«gojnÃ« (Inherit)</strong> nga prindÃ«rit, duke marrÃ« tÃ« gjitha vetitÃ« e tyre.</p>",
            "<span class='kw'>class</span> Admin(User):\n    <span class='fn'>pass</span>",
            "Ã‡farÃ« ju lejon tÃ« bÃ«ni trashÃ«gimia?",
            "âœ“ SaktÃ«! TÃ« ripÃ«rdorni njÃ« strukturÃ« ekzistuese.",
            "âœ— Mendoni pÃ«r ripÃ«rdorimin e kodit prindÃ«r.",
            new[] { ("Run programs concurrently", false), ("Inherit and reuse code from another class", true), ("Delete old code automatically", false), ("Prevent variables from changing", false) },
            new[] { ("Ekzekutimin e programeve njÃ«kohÃ«sisht", false), ("TrashÃ«gimin dhe ripÃ«rdorimin e kodit nga njÃ« klasÃ« tjetÃ«r", true), ("Fshirjen e kodit tÃ« vjetÃ«r automatikisht", false), ("Parandalimin e ndryshimit tÃ« variablave", false) });

        // 15. UNIT TESTING
        AddChapter(14, "Unit Testing",
            "Chapter 15 Â· The Safety Checks",
            "<p>To ensure our functions work, we write <strong>Unit Tests</strong>. These are separate pieces of code that run our actual code with test data.</p>",
            "<span class='kw'>def</span> <span class='fn'>test_add</span>():\n    <span class='kw'>assert</span> <span class='fn'>add</span>(2, 3) == 5",
            "Why do we write Unit Tests?",
            "âœ“ Excellent! Tests verify individual parts of the code.",
            "âœ— Unit tests check if code works as expected.",
            "Kapitulli 15 Â· Kontrollet e SigurisÃ«",
            "<p>PÃ«r t'u siguruar qÃ« funksionet tona punojnÃ«, shkruajmÃ« <strong>Testime NjÃ«sie (Unit Tests)</strong>. KÃ«to testojnÃ« kodin tonÃ« me tÃ« dhÃ«na provÃ«.</p>",
            "<span class='kw'>def</span> <span class='fn'>test_mbledhja</span>():\n    <span class='kw'>assert</span> <span class='fn'>mbledhja</span>(2, 3) == 5",
            "Pse shkruajmÃ« Testime NjÃ«sie?",
            "âœ“ E shkÃ«lqyer! Testet verifikojnÃ« pjesÃ«t e veÃ§anta tÃ« kodit.",
            "âœ— Testimet e njÃ«sisÃ« kontrollojnÃ« nÃ«se kodi punon siÃ§ pritet.",
            new[] { ("To test individual components of the code", true), ("To increase application size", false), ("To translate code to English", false), ("To style text on a webpage", false) },
            new[] { ("PÃ«r tÃ« testuar komponentÃ« tÃ« veÃ§antÃ« tÃ« kodit", true), ("PÃ«r tÃ« rritur madhÃ«sinÃ« e aplikacionit", false), ("PÃ«r tÃ« pÃ«rkthyer kodin nÃ« anglisht", false), ("PÃ«r tÃ« stiluar tekstin", false) });

        // 16. LINQ
        AddChapter(15, "LINQ",
            "Chapter 16 Â· The Data Query",
            "<p>To filter and transform lists of data easily, developers use <strong>LINQ (Language Integrated Query)</strong> in C#.</p>",
            "<span class='kw'>var</span> admins = users.Where(u => u.Role == <span class='str'>\"admin\"</span>).ToList();",
            "What is the primary benefit of using LINQ?",
            "âœ“ Correct! It provides a powerful and readable way to query collections.",
            "âœ— Think about querying data collections.",
            "Kapitulli 16 Â· KÃ«rkimi i tÃ« DhÃ«nave",
            "<p>PÃ«r tÃ« filtruar dhe transformuar lista me tÃ« dhÃ«na lehtÃ«sisht, zhvilluesit pÃ«rdorin <strong>LINQ (Language Integrated Query)</strong> nÃ« C#.</p>",
            "<span class='kw'>var</span> adminet = perdoruesit.Where(p => p.Roli == <span class='str'>\"admin\"</span>).ToList();",
            "Cili Ã«shtÃ« pÃ«rfitimi kryesor i pÃ«rdorimit tÃ« LINQ?",
            "âœ“ E saktÃ«! Ofron njÃ« mÃ«nyrÃ« tÃ« fuqishme dhe tÃ« lexueshme pÃ«r tÃ« kÃ«rkuar nÃ« koleksione.",
            "âœ— Mendoni pÃ«r kÃ«rkimin brenda koleksioneve.",
            new[] { ("To make web pages load faster", false), ("To provide a readable way to query collections", true), ("To style user interfaces", false), ("To connect to physical printers", false) },
            new[] { ("PÃ«r tÃ« ngarkuar faqet mÃ« shpejt", false), ("PÃ«r tÃ« ofruar njÃ« mÃ«nyrÃ« tÃ« lexueshme kÃ«rkimi nÃ« koleksione", true), ("PÃ«r tÃ« dizajnuar ndÃ«rfaqet e pÃ«rdoruesit", false), ("PÃ«r lidhjen me printerÃ«t", false) });

        // 17. DEPENDENCY INJECTION
        AddChapter(16, "Dependency Injection",
            "Chapter 17 Â· The Service Provider",
            "<p>Modern applications use <strong>Dependency Injection (DI)</strong> to provide objects with the services they need, making the code more modular and testable.</p>",
            "<span class='kw'>public</span> <span class='fn'>UserService</span>(IDatabase db) { _db = db; }",
            "Why do we use Dependency Injection?",
            "âœ“ Spot on! It creates loosely coupled code that is easier to test and maintain.",
            "âœ— DI is used to decouple components.",
            "Kapitulli 17 Â· Ofrimi i ShÃ«rbimeve",
            "<p>Aplikacionet moderne pÃ«rdorin <strong>Injektimin e VarÃ«sisÃ« (DI)</strong> pÃ«r t'u ofruar objekteve shÃ«rbimet qÃ« u nevojiten, duke e bÃ«rÃ« kodin mÃ« modular dhe mÃ« tÃ« testueshÃ«m.</p>",
            "<span class='kw'>public</span> <span class='fn'>SherbimiPerdoruesit</span>(IBazaTeDhenave btd) { _btd = btd; }",
            "Pse e pÃ«rdorim Injektimin e VarÃ«sisÃ«?",
            "âœ“ E saktÃ«! Krijon kod pak tÃ« varur (loosely coupled) qÃ« testohet e mirÃ«mbahet lehtÃ«.",
            "âœ— DI pÃ«rdoret pÃ«r tÃ« shkÃ«putur komponentÃ«t.",
            new[] { ("To tightly couple classes together", false), ("To create modular and testable code", true), ("To prevent objects from being created", false), ("To execute SQL commands directly", false) },
            new[] { ("PÃ«r t'i lidhur ngushtÃ« klasat", false), ("PÃ«r tÃ« krijuar kod modular dhe tÃ« testueshÃ«m", true), ("PÃ«r tÃ« ndaluar krijimin e objekteve", false), ("PÃ«r tÃ« ekzekutuar komanda SQL direkt", false) });

        // 18. DESIGN PATTERNS
        AddChapter(17, "Design Patterns",
            "Chapter 18 Â· The Architectural Blueprints",
            "<p>A <strong>Design Pattern</strong> is a proven solution to a common software design problem. For example, the Singleton pattern ensures only one instance of a class exists.</p>",
            "<span class='kw'>public static</span> Settings Instance { <span class='kw'>get</span>; }",
            "What is a Design Pattern in software engineering?",
            "âœ“ Exactly! It's a reusable solution to a commonly occurring problem.",
            "âœ— Look for the definition as a reusable template.",
            "Kapitulli 18 Â· Skicat Arkitekturore",
            "<p>NjÃ« <strong>Model Dizajni (Design Pattern)</strong> Ã«shtÃ« njÃ« zgjidhje e provuar pÃ«r njÃ« problem tÃ« zakonshÃ«m nÃ« dizajn tÃ« softuerit. PÃ«r shembull, modeli Singleton siguron qÃ« vetÃ«m njÃ« instancÃ« e klasÃ«s tÃ« vijojÃ«.</p>",
            "<span class='kw'>public static</span> Rregullimet Instanca { <span class='kw'>get</span>; }",
            "Ã‡farÃ« Ã«shtÃ« njÃ« Model Dizajni nÃ« inxhinierinÃ« softuerike?",
            "âœ“ E saktÃ«! Ã‹shtÃ« njÃ« zgjidhje e ripÃ«rdorshme pÃ«r probleme tÃ« njohura.",
            "âœ— KÃ«rkoni pÃ«rkufizimin e njÃ« shembulli tÃ« ripÃ«rdorshÃ«m.",
            new[] { ("A specific coding language", false), ("A reusable solution to a common design problem", true), ("A tool that draws diagrams", false), ("A function that returns a pattern of numbers", false) },
            new[] { ("NjÃ« gjuhÃ« e veÃ§antÃ« kodimi", false), ("NjÃ« zgjidhje e ripÃ«rdorshme pÃ«r probleme dizajni", true), ("NjÃ« vegÃ«l qÃ« vizaton diagrame", false), ("NjÃ« funksion qÃ« kthen numra", false) });

        // 19. RESTful APIs
        AddChapter(18, "RESTful APIs",
            "Chapter 19 Â· The Architecture of Web",
            "<p>Modern web services often use <strong>REST (Representational State Transfer)</strong>. It relies on standard HTTP methods like GET, POST, PUT, and DELETE to manage resources.</p>",
            "<span class='kw'>[HttpGet]</span>\n<span class='kw'>public</span> <span class='fn'>IActionResult</span> GetUser(<span class='kw'>int</span> id)",
            "Which HTTP method is typically used to create a new resource in REST?",
            "âœ“ Correct! POST is used to create new resources.",
            "âœ— Think about the standard method for submitting new data.",
            "Kapitulli 19 Â· Arkitektura e Uebit",
            "<p>ShÃ«rbimet moderne tÃ« uebit shpesh pÃ«rdorin <strong>REST (Representational State Transfer)</strong>. Ai mbÃ«shtetet nÃ« metodat standarde HTTP si GET, POST, PUT dhe DELETE pÃ«r menaxhimin e burimeve.</p>",
            "<span class='kw'>[HttpGet]</span>\n<span class='kw'>public</span> <span class='fn'>IActionResult</span> MerrPerdoruesin(<span class='kw'>int</span> id)",
            "Cila metodÃ« HTTP pÃ«rdoret zakonisht pÃ«r tÃ« krijuar njÃ« burim tÃ« ri nÃ« REST?",
            "âœ“ E saktÃ«! POST pÃ«rdoret pÃ«r tÃ« krijuar burime tÃ« reja.",
            "âœ— Mendoni pÃ«r metodÃ«n standarde tÃ« dÃ«rgimit tÃ« tÃ« dhÃ«nave tÃ« reja.",
            new[] { ("GET", false), ("PUT", false), ("POST", true), ("DELETE", false) },
            new[] { ("GET", false), ("PUT", false), ("POST", true), ("DELETE", false) });

        // 20. CI/CD
        AddChapter(19, "CI/CD",
            "Chapter 20 Â· The Automated Pipeline",
            "<p><strong>CI/CD (Continuous Integration / Continuous Deployment)</strong> automates the testing and deployment of code, ensuring that new features reach users quickly and safely.</p>",
            "<span class='kw'>steps</span>:\n  - <span class='fn'>run</span>: dotnet build\n  - <span class='fn'>run</span>: dotnet test",
            "What is the main goal of CI/CD?",
            "âœ“ Great! It automates building, testing, and deploying over and over.",
            "âœ— CI/CD revolves around automation of the release process.",
            "Kapitulli 20 Â· Rrjedha e Automatizuar",
            "<p><strong>CI/CD (Integrimi i VazhdueshÃ«m / Daudzimi i VazhdueshÃ«m)</strong> automatizon testimin dhe shpÃ«rndarjen e kodit, duke u siguruar qÃ« tiparet e reja arrijnÃ« te pÃ«rdoruesit shpejt dhe sigurt.</p>",
            "<span class='kw'>hapat</span>:\n  - <span class='fn'>run</span>: dotnet build\n  - <span class='fn'>run</span>: dotnet test",
            "Cili Ã«shtÃ« qÃ«llimi kryesor i CI/CD?",
            "âœ“ E shkÃ«lqyer! Automatizon ndÃ«rtimin, testimin dhe shpÃ«rndarjen e kodit vazhdimisht.",
            "âœ— CI/CD pÃ«rqendrohet tek automatizimi i procesit tÃ« lÃ«shimit.",
            new[] { ("To manually review all code changes", false), ("To automate the building, testing, and deployment processes", true), ("To prevent users from accessing the app", false), ("To design database schemas", false) },
            new[] { ("TÃ« rishikojÃ« manualisht Ã§do ndryshim", false), ("TÃ« automatizojÃ« proceset e ndÃ«rtimit, testimit dhe shpÃ«rndarjes", true), ("TÃ« ndalojÃ« pÃ«rdoruesit nga aplikacioni", false), ("TÃ« dizajnojÃ« skema bazash tÃ« dhÃ«nash", false) });

        // 21. MICROSERVICES
        AddChapter(20, "Microservices",
            "Chapter 21 Â· The Decentralized System",
            "<p>In a <strong>Microservices Architecture</strong>, an application is built as a suite of small, independent services communicating over a network, rather than a single monolith.</p>",
            "<span class='kw'>OrderService</span> -> HTTP/gRPC -> <span class='kw'>PaymentService</span>",
            "Why would a team choose a Microservices architecture over a Monolith?",
            "âœ“ Exactly! It allows independent deployment and scaling of different parts.",
            "âœ— Consider how large applications are split into smaller, independent parts.",
            "Kapitulli 21 Â· Sistemi i Decentralizuar",
            "<p>NÃ« njÃ« <strong>ArkitekturÃ« MikroshÃ«rbimesh</strong>, njÃ« aplikacion ndÃ«rtohet si njÃ« grup shÃ«rbimesh tÃ« vogla, tÃ« pavarura qÃ« komunikojnÃ« pÃ«rmes njÃ« rrjeti, nÃ« vend tÃ« njÃ« monoliti tÃ« vetÃ«m.</p>",
            "<span class='kw'>SherbimiPorosive</span> -> HTTP/gRPC -> <span class='kw'>SherbimiPagesave</span>",
            "Pse njÃ« ekip do tÃ« zgjidhte arkitekturÃ«n e MikroshÃ«rbimeve mbi njÃ« Monolit?",
            "âœ“ E saktÃ«! Ai lejon dÃ«rgimin dhe rritjen e pavarur tÃ« pjesÃ«ve tÃ« ndryshme.",
            "âœ— Mendoni se si aplikacionet e mÃ«dha ndahen nÃ« pjesÃ« mÃ« tÃ« vogla.",
            new[] { ("Because it forces everyone to use the same language", false), ("To allow independent deployment and scaling of services", true), ("Because it automatically fixes code bugs", false), ("To ensure all code lives in one giant file", false) },
            new[] { ("Sepse i detyron tÃ« gjithÃ« tÃ« pÃ«rdorin tÃ« njÃ«jtÃ«n gjuhÃ«", false), ("PÃ«r tÃ« lejuar shpÃ«rndarjen dhe shkallÃ«zimin e pavarur tÃ« shÃ«rbimeve", true), ("Sepse rregullon automatikisht gabimet nÃ« kod", false), ("PÃ«r tÃ« mbajtur kodin nÃ« njÃ« dokument tÃ« vetÃ«m", false) });

        // 22. SECURITY
        AddChapter(21, "Security",
            "Chapter 22 Â· The Firewall Guard",
            "<p>Good developers must practice <strong>Cybersecurity</strong> principles, like input validation and encryption, to protect user data from SQL injection and cross-site scripting (XSS).</p>",
            "<span class='kw'>string</span> hash = <span class='fn'>BCrypt</span>.HashPassword(<span class='str'>\"user_pass\"</span>);",
            "What is a primary defense against SQL Injection?",
            "âœ“ Spot on! Using parameterized queries or ORMs prevents malicious SQL execution.",
            "âœ— Think about how user input is passed to the database safely.",
            "Kapitulli 22 Â· Roja e FireÃ«all-it",
            "<p>Zhvilluesit e mirÃ« duhet tÃ« zbatojnÃ« parimet e <strong>SigurisÃ« Kibernetike</strong>, si validimi i tÃ« dhÃ«nave hyrÃ«se dhe kriptimi, pÃ«r tÃ« mbrojtur tÃ« dhÃ«nat nga injektimi i SQL-it dhe sulmet XSS.</p>",
            "<span class='kw'>string</span> hash = <span class='fn'>BCrypt</span>.KriptoFjalekalimin(<span class='str'>\"pasw_perdoruesi\"</span>);",
            "Cila Ã«shtÃ« njÃ« mbrojtje kryesore kundÃ«r Injektimit tÃ« SQL-it (SQL Injection)?",
            "âœ“ PÃ«rkryer! PÃ«rdorimi i kÃ«rkesave tÃ« parametrizuara parandalon ekzekutimin e dÃ«mshÃ«m.",
            "âœ— Mendoni se si kontrollohen tÃ« dhÃ«nat qÃ« i jepen databazÃ«s.",
            new[] { ("Storing passwords in plain text", false), ("Using parameterized queries", true), ("Making APIs public", false), ("Writing shorter queries", false) },
            new[] { ("Ruajtja e fjalÃ«kalimeve si tekst i thjeshtÃ«", false), ("PÃ«rdorimi i kÃ«rkesave me parametra (parameterized queries)", true), ("Afrimi i API-ve publikÃ«", false), ("Shkrimi i kÃ«rkesave mÃ« tÃ« shkurtra", false) });

        // 23. ENUMS
        AddChapter(22, "Enums",
            "Chapter 23 Â· The State Machine",
            "<p>To restrict a variable's value to a predefined set of named constants, we use <strong>Enums (Enumerations)</strong>.</p>",
            "<span class='kw'>enum</span> Status { Active, Inactive, Pending }",
            "What is the main purpose of an Enum?",
            "âœ“ Correct! It defines a set of named constants.",
            "âœ— Enums restrict variables to specific values.",
            "Kapitulli 23 Â· Gjendja e Sistemit",
            "<p>PÃ«r tÃ« kufizuar vlerÃ«n e njÃ« variable nÃ« njÃ« grup tÃ« paracaktuar konstantÃ«sh tÃ« emÃ«rtuar, pÃ«rdorim <strong>Enums (Enumeracione)</strong>.</p>",
            "<span class='kw'>enum</span> Statusi { Aktiv, Joaktiv, NePritje }",
            "Cili Ã«shtÃ« qÃ«llimi kryesor i njÃ« Enum-i?",
            "âœ“ E saktÃ«! Ai pÃ«rcakton njÃ« set konstantÃ«sh tÃ« emÃ«rtuar.",
            "âœ— Enums kufizojnÃ« variablat nÃ« vlera tÃ« caktuara.",
            new[] { ("To loop indefinitely", false), ("To define a set of named constants", true), ("To declare variables", false), ("To calculate integers", false) },
            new[] { ("PÃ«r tÃ« pÃ«rsÃ«ritur pafundÃ«sisht", false), ("PÃ«r tÃ« pÃ«rcaktuar njÃ« set konstantÃ«sh tÃ« emÃ«rtuar", true), ("PÃ«r tÃ« deklaruar variabla", false), ("PÃ«r tÃ« llogaritur numra tÃ« plotÃ«", false) });

        // 24. POLYMORPHISM
        AddChapter(23, "Polymorphism",
            "Chapter 24 Â· The Shape Shifter",
            "<p><strong>Polymorphism</strong> allows objects of different classes to be treated as objects of a common superclass. This means a single function can handle different types of objects.</p>",
            "<span class='kw'>public virtual void</span> <span class='fn'>Draw</span>() { }",
            "What does Polymorphism in OOP enable you to do?",
            "âœ“ Exactly! It lets you use a single interface to represent different underlying forms.",
            "âœ— Think about multiple forms for a single action.",
            "Kapitulli 24 Â· Ndryshuesi i FormÃ«s",
            "<p><strong>Polimorfizmi</strong> lejon qÃ« objektet e klasave tÃ« ndryshme tÃ« trajtohen si objekte tÃ« njÃ« mbitipi tÃ« pÃ«rbashkÃ«t. Kjo do tÃ« thotÃ« se njÃ« funksion mund tÃ« trajtojÃ« lloje tÃ« ndryshme objektesh.</p>",
            "<span class='kw'>public virtual void</span> <span class='fn'>Vizato</span>() { }",
            "Ã‡farÃ« ju lejon Polimorfizmi nÃ« OOP tÃ« bÃ«ni?",
            "âœ“ SaktÃ«! TÃ« pÃ«rdorni njÃ« ndÃ«rfaqe tÃ« vetme pÃ«r tÃ« pÃ«rfaqÃ«suar forma tÃ« ndryshme.",
            "âœ— Mendoni pÃ«r forma tÃ« shumta pÃ«r njÃ« veprim tÃ« vetÃ«m.",
            new[] { ("To hide data from the user", false), ("To process objects differently based on their data type or class", true), ("To prevent classes from inheriting", false), ("To query a database", false) },
            new[] { ("TÃ« fshehÃ« tÃ« dhÃ«nat nga pÃ«rdoruesi", false), ("TÃ« pÃ«rpunojÃ« objektet ndryshe bazuar nÃ« llojin e tyre tÃ« tÃ« dhÃ«nave", true), ("TÃ« parandalojÃ« trashÃ«gimin e klasave", false), ("TÃ« kÃ«rkojÃ« nÃ« databazÃ«", false) });

        // 25. INTERFACES
        AddChapter(24, "Interfaces",
            "Chapter 25 Â· The Blueprint Contracts",
            "<p>An <strong>Interface</strong> defines a contract. Any class that implements the interface must provide the specific methods and properties defined inside it.</p>",
            "<span class='kw'>public interface</span> ILogger:\n    <span class='kw'>void</span> Log(<span class='kw'>string</span> message);",
            "What is the role of an Interface?",
            "âœ“ Correct! It forces classes to implement specific methods, creating a contract.",
            "âœ— Think about contracts in system design.",
            "Kapitulli 25 Â· Kontratat e Projektimit",
            "<p>NjÃ« <strong>NdÃ«rfaqe (Interface)</strong> pÃ«rcakton njÃ« kontratÃ«. Ã‡do klasÃ« qÃ« e zbaton ndÃ«rfaqen duhet tÃ« ofrojÃ« metodat dhe vetitÃ« specifike tÃ« pÃ«rcaktuara brenda saj.</p>",
            "<span class='kw'>public interface</span> ILogger:\n    <span class='kw'>void</span> Logo(<span class='kw'>string</span> mesazhi);",
            "Cili Ã«shtÃ« roli i njÃ« NdÃ«rfaqeje?",
            "âœ“ E saktÃ«! Ai detyron klasat tÃ« zbatojnÃ« metoda specifike, duke formuar njÃ« kontratÃ«.",
            "âœ— Mendoni pÃ«r kontratat nÃ« dizajnin e sistemit.",
            new[] { ("To store user credentials safely", false), ("To define a contract that classes must follow", true), ("To manage server memory", false), ("To deploy the app faster", false) },
            new[] { ("TÃ« ruajÃ« kredencialet nÃ« mÃ«nyrÃ« tÃ« sigurt", false), ("TÃ« pÃ«rcaktojÃ« njÃ« kontratÃ« qÃ« klasat duhet tÃ« ndjekin", true), ("TÃ« menaxhojÃ« kujtesÃ«n e serverit", false), ("TÃ« publikojÃ« aplikacionin mÃ« shpejt", false) });

        // 26. GENERICS
        AddChapter(25, "Generics",
            "Chapter 26 Â· The Universal Box",
            "<p>To write flexible, reusable code, we use <strong>Generics</strong>. They let you define classes and methods with placeholders for data types.</p>",
            "<span class='kw'>public class</span> Box&lt;T&gt; {\n    <span class='kw'>public</span> T Content;\n}",
            "What is the main benefit of Generics?",
            "âœ“ Correct! They provide type safety and reusability.",
            "âœ— Think about writing a class that works with any type safely.",
            "Kapitulli 26 Â· Kutia Universale",
            "<p>PÃ«r tÃ« shkruar kod fleksibÃ«l dhe tÃ« ripÃ«rdorshÃ«m, pÃ«rdorim <strong>Generics (Tipet Gjenerike)</strong>. Ato ju lejojnÃ« tÃ« pÃ«rcaktoni klasa dhe metoda me argumente pÃ«r tipet e tÃ« dhÃ«nave.</p>",
            "<span class='kw'>public class</span> Kutia&lt;T&gt; {\n    <span class='kw'>public</span> T Permbajtja;\n}",
            "Cili Ã«shtÃ« pÃ«rfitimi kryesor i tipeve gjenerike?",
            "âœ“ E saktÃ«! OfrojnÃ« siguri tipi dhe ripÃ«rdorim.",
            "âœ— Mendoni pÃ«r shkrimin e njÃ« klase qÃ« funksionon me Ã§do tip kompjuterik nÃ« mÃ«nyrÃ« tÃ« sigurt.",
            new[] { ("To make the program run twice as fast", false), ("To provide type safety and code reusability", true), ("To declare variables without a specific type automatically", false), ("To connect to the internet", false) },
            new[] { ("PÃ«r tÃ« rritur shpejtÃ«sinÃ« e programit", false), ("PÃ«r tÃ« ofruar siguri tipi dhe ripÃ«rdorim tÃ« kodit", true), ("PÃ«r tÃ« deklaruar variabla automatikisht pa tip", false), ("PÃ«r t'u lidhur me internetin", false) });

        // 27. DELEGATES & EVENTS
        AddChapter(26, "Delegates & Events",
            "Chapter 27 Â· The Function Pointers",
            "<p>A <strong>Delegate</strong> is a type that safely encapsulates a method, similar to a function pointer in C or C++. This is heavily used for events.</p>",
            "<span class='kw'>public delegate void</span> <span class='fn'>LogHandler</span>(<span class='kw'>string</span> msg);",
            "What does a delegate point to?",
            "âœ“ Exactly! A delegate holds a reference to a method.",
            "âœ— Think of a reference to a function.",
            "Kapitulli 27 Â· Treguesit e Funksioneve",
            "<p>NjÃ« <strong>Delegat (Delegate)</strong> Ã«shtÃ« njÃ« tip qÃ« kapsulon nÃ« mÃ«nyrÃ« tÃ« sigurt njÃ« metodÃ«, i ngjashÃ«m me treguesit e funksioneve nÃ« C. Kjo pÃ«rdoret masivisht pÃ«r ngjarjet (events).</p>",
            "<span class='kw'>public delegate void</span> <span class='fn'>TrajtuesITeDhenave</span>(<span class='kw'>string</span> msg);",
            "Ã‡farÃ« tregon saktÃ«sisht njÃ« delegat?",
            "âœ“ SaktÃ«! NjÃ« delegat ruan njÃ« referencÃ« drejt njÃ« metode.",
            "âœ— Mendoni pÃ«r referencÃ«n e njÃ« funksioni.",
            new[] { ("It points to a variable in memory", false), ("It holds a reference to a method", true), ("It points to an array of objects", false), ("It opens a database connection", false) },
            new[] { ("Tregon njÃ« variabÃ«l nÃ« memorie", false), ("Ruan njÃ« referencÃ« drejt njÃ« metode", true), ("Tregon njÃ« varg objektesh", false), ("Hap njÃ« lidhje databaze", false) });

        // 28. MEMORY MANAGEMENT
        AddChapter(27, "Memory Management",
            "Chapter 28 Â· The Garbage Collector",
            "<p>In modern .NET, the <strong>Garbage Collector (GC)</strong> automatically manages memory, freeing up unused objects so you don't have to call delete manually.</p>",
            "<span class='kw'>object</span> x = <span class='kw'>new</span> <span class='fn'>object</span>();\n<span class='fn'>x</span> = null; <span class='cmt'>// GC can claim it</span>",
            "What is the role of the Garbage Collector?",
            "âœ“ Spot on! It automatically reclaims memory by deleting unused objects.",
            "âœ— The GC cleans up unneeded objects in memory.",
            "Kapitulli 28 Â· Menaxhuesi i KujtesÃ«s",
            "<p>NÃ« .NET, <strong>MbledhÃ«si i Mbeturinave (Garbage Collector)</strong> menaxhon automatikisht memorien, duke pastruar objektet e papÃ«rdorura nÃ« mÃ«nyrÃ« qÃ« tÃ« mos keni nevojÃ« t'i fshini manualisht.</p>",
            "<span class='kw'>object</span> x = <span class='kw'>new</span> <span class='fn'>object</span>();\n<span class='fn'>x</span> = null; <span class='cmt'>// GC mund ta pastrojÃ«</span>",
            "Cili Ã«shtÃ« roli i MbledhÃ«sit tÃ« Mbeturinave (GC)?",
            "âœ“ E saktÃ«! Ai rikthen memorien duke fshirÃ« objektet e papÃ«rdorura.",
            "âœ— MbledhÃ«si pastron objektet e panevojshme nga memoria.",
            new[] { ("To compress files to save disk space", false), ("To automatically reclaim memory by deleting unused objects", true), ("To prevent memory leaks 100% of the time", false), ("To encrypt data in RAM", false) },
            new[] { ("TÃ« kompresojÃ« skedarÃ«t pÃ«r kursim hapsire", false), ("TÃ« rikthejÃ« automatikisht memorien duke fshirÃ« objektet e papÃ«rdorura", true), ("TÃ« parandalojÃ« humbjet e pÃ«rhershme tÃ« memories plotÃ«sisht", false), ("TÃ« kriptojÃ« tÃ« dhÃ«nat nÃ« RAM", false) });

        // 29. MULTITHREADING
        AddChapter(28, "Multithreading",
            "Chapter 29 Â· Parallel processing",
            "<p>For processor-intensive operations, we use <strong>Multithreading</strong> to execute multiple threads simultaneously, utilizing multicore processors.</p>",
            "<span class='kw'>Task</span>.Run(() => <span class='fn'>HeavyWork</span>());",
            "Why do developers use multithreading?",
            "âœ“ Great! It enables executing multiple processes concurrently.",
            "âœ— Think about doing more than one thing at exactly the same time.",
            "Kapitulli 29 Â· Procesim Paralel",
            "<p>PÃ«r operacione qÃ« kÃ«rkojnÃ« shumÃ« punÃ« nga procesori, pÃ«rdorim <strong>Ekzekutimin nÃ« ShumÃ« Fije (Multithreading)</strong> pÃ«r tÃ« ekzekutuar disa fije njÃ«herÃ«sh, duke pÃ«rdorur procesorÃ«t me shumÃ« bÃ«rthama.</p>",
            "<span class='kw'>Task</span>.Run(() => <span class='fn'>PuneERende</span>());",
            "Pse pÃ«rdorin zhvilluesit shumÃ« fije (multithreading)?",
            "âœ“ PÃ«rkryer! MundÃ«son ekzekutimin e disa proceseve njÃ«kohÃ«sisht.",
            "âœ— Mendoni pÃ«r tÃ« bÃ«rÃ« mÃ« shumÃ« se njÃ« gjÃ« nÃ« tÃ« njÃ«jtÃ«n kohÃ«.",
            new[] { ("To convert programs to web apps", false), ("To execute multiple operations concurrently", true), ("To debug the program faster", false), ("To lock the UI thread for safety", false) },
            new[] { ("PÃ«r tÃ« kthyer programet nÃ« ueb aplikacione", false), ("PÃ«r tÃ« ekzekutuar shumÃ« operacione njÃ«kohÃ«sisht", true), ("PÃ«r tÃ« gjetur gabimet mÃ« shpejt", false), ("PÃ«r tÃ« bllokuar fijen kryesore pÃ«r siguri", false) });

        // 30. ENTITY FRAMEWORK
        AddChapter(29, "Entity Framework",
            "Chapter 30 Â· Order out of Chaos",
            "<p><strong>Entity Framework (EF Core)</strong> is an Object-Relational Mapper (ORM) that lets you interact with a database using .NET objects rather than writing raw SQL commands.</p>",
            "<span class='kw'>var</span> users = db.Users.Where(u => u.IsActive).ToList();",
            "What is Entity Framework?",
            "âœ“ Correct! It is an Object-Relational Mapper (ORM).",
            "âœ— EF Maps objects to databases.",
            "Kapitulli 30 Â· Rregull nga Kaosi",
            "<p><strong>Entity Framework (EF Core)</strong> Ã«shtÃ« njÃ« NdÃ«rtues Objektesh-Relacionesh (ORM) qÃ« ju lejon tÃ« ndÃ«rveproni me databazÃ«n duke pÃ«rdorur objekte .NET nÃ« vend tÃ« komandave rresht SQL.</p>",
            "<span class='kw'>var</span> perdoruesit = db.Perdoruesit.Where(p => p.EshteAktiv).ToList();",
            "Ã‡farÃ« Ã«shtÃ« Entity Framework?",
            "âœ“ E saktÃ«! Ã‹shtÃ« njÃ« ORM (Object-Relational Mapper).",
            "âœ— EF lidh objektet me databazat.",
            new[] { ("A graphical interface framework for desktop apps", false), ("An Object-Relational Mapper to access the database with objects", true), ("A tool to test API endpoints", false), ("A cloud hosting platform", false) },
            new[] { ("NjÃ« framework pÃ«r ndÃ«rfaqe grafike", false), ("NjÃ« ORM pÃ«r t'iu qasur ndÃ«rfaqeve tÃ« databazave pÃ«rmes objekteve", true), ("NjÃ« mjet pÃ«r tÃ« testuar API-tÃ«", false), ("NjÃ« platformÃ« pÃ«r cloud hosting", false) });

        // POS Chapters
        AddChapter(0, "IntroPOS",
            "Chapter 1 Â· The POS Interface",
            "<p>Welcome to <strong>YourBrand</strong>! You are starting your shift. First, what does POS stand for and why is it essential for any retail business?</p><img src='https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?auto=format&fit=crop&q=80&w=800' alt='POS Intro' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "--- Starting POS System ---",
            "What does POS stand for?",
            "âœ“ Correct! It is the Point of Sale.",
            "âœ— Think about where a sale happens.",
            "Kapitulli 1 Â· NdÃ«rfaqja e POS",
            "<p>MirÃ«sevini nÃ« <strong>YourBrand</strong>! Po fillon ndÃ«rrimin tuaj. SÃ« pari, Ã§farÃ« do tÃ« thotÃ« POS dhe pse Ã«shtÃ« thelbÃ«sor?</p><img src='https://images.unsplash.com/photo-1556742049-0cfed4f6a45d?auto=format&fit=crop&q=80&w=800' alt='POS Intro' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "--- Nisja e POS ---",
            "Ã‡farÃ« do tÃ« thotÃ« POS?",
            "âœ“ SaktÃ«! Do tÃ« thotÃ« Pika e Shitjes.",
            "âœ— Mendo ku ndodh shitja.",
            new[] { ("Point of Sale", true), ("Proof of System", false), ("Part of Store", false), ("Point of Service", false) },
            new[] { ("Pika e Shitjes (Point of Sale)", true), ("ProvÃ« e Sistemit", false), ("PjesÃ« e Dyqanit", false), ("PikÃ« ShÃ«rbimi", false) }, "POS");

        AddChapter(1, "FiscalPrinter",
            "Chapter 2 Â· The Fiscal Printer",
            "<p>In the Republic of Kosova, every sale must go through a <strong>Fiscal Printer</strong> (Arka Fiskale) certified by ATK (Administrata Tatimore e KosovÃ«s).</p><img src='https://images.unsplash.com/photo-1518458028785-8fbcd101ebb9?auto=format&fit=crop&q=80&w=800' alt='Fiscal Printer' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "--- Fiscal Printer Connection: OK ---",
            "Why do we need a Fiscal Printer?",
            "âœ“ Yes! It ensures sales are reported and taxes are paid.",
            "âœ— It's required by law for tax recording.",
            "Kapitulli 2 Â· Arka Fiskale",
            "<p>NÃ« RepublikÃ«n e KosovÃ«s, Ã§do shitje duhet tÃ« kalojÃ« pÃ«rmes njÃ« <strong>Arke Fiskale</strong> tÃ« certifikuar nga ATK-ja.</p><img src='https://images.unsplash.com/photo-1518458028785-8fbcd101ebb9?auto=format&fit=crop&q=80&w=800' alt='Arka Fiskale' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "--- Lidhja me ArkÃ«n Fiskale: OK ---",
            "Pse pÃ«rdorim ArkÃ«n Fiskale?",
            "âœ“ SaktÃ«! Raporton shitjet tek ATK-ja.",
            "âœ— Ã‹shtÃ« e domosdoshme ligjÃ«risht.",
            new[] { ("To record sales for the Tax Administration (ATK)", true), ("To print nicer receipts", false), ("To run the internet", false), ("To hold cash safely", false) },
            new[] { ("PÃ«r tÃ« raportuar shitjet tek Administrata Tatimore (ATK)", true), ("PÃ«r tÃ« printuar kuponÃ« mÃ« tÃ« bukur", false), ("PÃ«r internet", false), ("PÃ«r tÃ« mbajtur paratÃ«", false) }, "POS");

        AddChapter(2, "Barcode",
            "Chapter 3 Â· Barcodes & Products",
            "<p>Let's scan our first product! A barcode scanner reads the EAN/UPC code to find the exact item in our YourBrand database.</p><img src='https://images.unsplash.com/photo-1607344645866-009c520b61c9?auto=format&fit=crop&q=80&w=800' alt='Scanner' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "SCAN: 3830001234567 -> Found: 'UjÃ« Mineral 0.5L'",
            "What does the scanner actually scan?",
            "âœ“ Correct, the barcode translates into a unique ID.",
            "âœ— It reads the barcode.",
            "Kapitulli 3 Â· Barkodet",
            "<p>Le tÃ« skanojmÃ« produktin e parÃ«! Skaneri lexon kodin EAN/UPC pÃ«r tÃ« gjetur artikullin thelbÃ«sor.</p><img src='https://images.unsplash.com/photo-1607344645866-009c520b61c9?auto=format&fit=crop&q=80&w=800' alt='Scanner' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "SKANIM: 3830001234567 -> Gjetur: 'UjÃ« Mineral 0.5L'",
            "Ã‡farÃ« lexon saktÃ«sisht skaneri?",
            "âœ“ SaktÃ«, barkodi pÃ«rfaqÃ«son njÃ« ID unike.",
            "âœ— Lexon barkodin e produktit.",
            new[] { ("The unique barcode (EAN/UPC)", true), ("The ingredients", false), ("The expiration date", false), ("The business tax number", false) },
            new[] { ("Barkodin unik (EAN/UPC)", true), ("PÃ«rbÃ«rÃ«sit", false), ("DatÃ«n e skadencÃ«s", false), ("Numrin fiskal tÃ« biznesit", false) }, "POS");

        AddChapter(3, "VAT",
            "Chapter 4 Â· Value Added Tax (VAT / TVSH)",
            "<p>In Kosovo, standard VAT (TVSH) is 18%, while for essential items and utilities it might be 8%. The POS calculates this automatically before printing the fiscal receipt.</p>",
            "Price: 10.00 EUR | TVSH (18%): 1.53 EUR",
            "What is the standard VAT (TVSH) rate in Kosovo for most items?",
            "âœ“ Spot on! 18% is the standard rate.",
            "âœ— The standard rate is 18%.",
            "Kapitulli 4 Â· TVSH",
            "<p>NÃ« KosovÃ«, TVSH-ja standarde Ã«shtÃ« 18%, ndÃ«rsa pÃ«r produkte esenciale 8%. Sistemi i llogarit kÃ«tÃ« automatikisht.</p>",
            "Ã‡mimi: 10.00 EUR | TVSH (18%): 1.53 EUR",
            "Sa Ã«shtÃ« norma standarde e TVSH-sÃ« nÃ« KosovÃ«?",
            "âœ“ SaktÃ«! 18% Ã«shtÃ« norma standarde.",
            "âœ— 18% Ã«shtÃ« pÃ«rgjigja korrekte.",
            new[] { ("18%", true), ("8%", false), ("20%", false), ("10%", false) },
            new[] { ("18%", true), ("8%", false), ("20%", false), ("10%", false) }, "POS");

        AddChapter(4, "PaymentMethods",
            "Chapter 5 Â· Payment Methods",
            "<p>A customer wants to pay for their order. You can accept Cash, Credit/Debit Card, or Mobile Payments. In the fiscal receipt, the payment method must be clearly marked.</p><img src='https://images.unsplash.com/photo-1556742044-fbbd63327d53?auto=format&fit=crop&q=80&w=800' alt='Payment' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "Total: 15.50 EUR. Pay with: [CASH] [CARD]",
            "Why does the payment type matter for the fiscal printer?",
            "âœ“ Yes, ATK requires tracking of cash vs. electronic payments.",
            "âœ— It matters for accounting and tax declaring.",
            "Kapitulli 5 Â· Kthimi i Kusurit",
            "<p>Klienti po paguan. Mund tÃ« pranoni Para tÃ« Gatshme (Cash) ose KartelÃ«. Lloji i pagesÃ«s printohet theksueshÃ«m nÃ« kuponin fiskal.</p><img src='https://images.unsplash.com/photo-1556742044-fbbd63327d53?auto=format&fit=crop&q=80&w=800' alt='Payment' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "Total: 15.50 EUR. Paguaj me: [CASH] [KARTELÃ‹]",
            "Pse duhet specifikuar metoda e pagesÃ«s nÃ« arkÃ« fiskale?",
            "âœ“ KÃ«shtu Administrata Tatimore e di saktÃ« si janÃ« marrÃ« paratÃ«.",
            "âœ— Ã‹shtÃ« pÃ«r raportim tÃ« saktÃ«.",
            new[] { ("It must match the daily closing statement for taxes (Z-Report)", true), ("To change the product price", false), ("So the cash drawer opens slower", false), ("It does not matter", false) },
            new[] { ("Duhet tÃ« pÃ«rputhet me mbylljen ditore pÃ«r ATK-nÃ« (Raporti Z)", true), ("PÃ«r tÃ« ndryshuar Ã§mimin", false), ("QÃ« tÃ« hapet arka mÃ« ngadalÃ«", false), ("Nuk ka rÃ«ndÃ«si", false) }, "POS");

        AddChapter(5, "ZReport",
            "Chapter 6 Â· The Z-Report",
            "<p>At the end of your shift, you must close the register. This prints a <strong>Z-Report</strong> (Raporti Z) from the fiscal printer, summarizing all sales, taxes, and money received.</p>",
            "--- PRINTING Z-REPORT ---",
            "What is a Z-Report?",
            "âœ“ Correct! It is the mandatory daily closure report.",
            "âœ— The Z-Report is for daily closing.",
            "Kapitulli 6 Â· Raporti Z",
            "<p>NÃ« fund tÃ« ndÃ«rrimit tuaj pÃ«r ditÃ«n e punÃ«s, duhet mbyllur arka. Kjo printon njÃ« <strong>Raport Z</strong> nga arka fiskale, qÃ« tregon tÃ« gjitha shitjet, TVSH-tÃ« dhe paratÃ« e gatshme.</p>",
            "--- PRINTIMI I RAPORTIT Z ---",
            "Ã‡farÃ« Ã«shtÃ« Raporti Z?",
            "âœ“ Saktesisht. Ã‹shtÃ« mbyllja obligative ditore.",
            "âœ— Ã‹shtÃ« raporti i mbylljes ditore.",
            new[] { ("A daily financial summary required by ATK to close the registry", true), ("A report of only returned items", false), ("Inventory breakdown", false), ("A maintenance ticket", false) },
            new[] { ("NjÃ« pÃ«rmbledhje ditore financiare qÃ« kÃ«rkohet nga ATK", true), ("NjÃ« raport vetÃ«m pÃ«r kthimet", false), ("Lista e inventarit", false), ("NjÃ« biletÃ« mirÃ«mbajtjeje", false) }, "POS");

        AddChapter(6, "Returns",
            "Chapter 7 Â· Handling Returns (Storno)",
            "<p>Sometimes customers return products. In POS, you perform a Return or Storno. For fiscal reasons, the original receipt must be matched.</p>",
            "ACTION: Storno | Receipt #1045",
            "What must you typically do when a customer returns a purchased item in Kosovo?",
            "âœ“ Yes! You issue a specific return fiscal receipt or record the storno officially.",
            "âœ— You must register it in the POS to balance the books.",
            "Kapitulli 7 Â· Kthimet (Storno)",
            "<p>NdonjÃ«herÃ« klientÃ«t kthejnÃ« produkte. NÃ« sistemin POS, bÃ«het Storno. PÃ«r rregulla fiskale, duhet tÃ« keni kuponin e origjinÃ«s.</p>",
            "VEPRIM: Storno | Kuponi #1045",
            "Ã‡farÃ« bÃ«het gjatÃ« kthimit tÃ« produktit me para?",
            "âœ“ SaktÃ«! Duhet tÃ« lÃ«shohet kupon kthimi dhe tÃ« pÃ«rditÃ«sohet stoku.",
            "âœ— Duhet regjistruar kthimin zyrtarisht nÃ« YourBrand.",
            new[] { ("Process it through POS to print a return fiscal receipt & update stock", true), ("Trash the original receipt and give hidden cash back", false), ("Nothing, returns aren't allowed", false), ("Ignore VAT changes", false) },
            new[] { ("Kalohet nÃ« POS pÃ«r kupon tÃ« kthimit & pÃ«rditÃ«sim tÃ« depose/stokut", true), ("Hidhni kuponin dhe kthe paratÃ« fshehurazi", false), ("AsgjÃ«, nuk lejohen", false), ("Injorohenndryshimet e TVSH-sÃ«", false) }, "POS");

        AddChapter(7, "Inventory",
            "Chapter 8 Â· Inventory Management",
            "<p>Stock levels must be accurate. Every time a sale is completed, the items are mechanically deducted from the inventory. When a truck arrives, you add stock.</p><img src='https://images.unsplash.com/photo-1586528116311-ad8ed74509b5?auto=format&fit=crop&q=80&w=800' alt='Inventory' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "INVENTORY UPDATE LOG",
            "What happens to the inventory when a sale is finalized?",
            "âœ“ Correct! Quantities are updated automatically.",
            "âœ— Think about stock accuracy.",
            "Kapitulli 8 Â· Menaxhimi i Stokut",
            "<p>Nivelet e stokut duhet tÃ« jenÃ« tÃ« sakta. Sa herÃ« njÃ« shitje mbyllet, artikujt zbriten nga inventari. Kur vjen njÃ« dÃ«rgesÃ«, shtohet stoku.</p><img src='https://images.unsplash.com/photo-1586528116311-ad8ed74509b5?auto=format&fit=crop&q=80&w=800' alt='Inventory' style='max-width:100%; object-fit:cover; border-radius:10px; margin: 15px 0;' />",
            "LOG I PÃ‹RDITÃ‹SIMIT TÃ‹ STOKUT",
            "Ã‡farÃ« i ndodh inventarit sapo kryhet njÃ« shitje?",
            "âœ“ SaktÃ«! SasitÃ« pÃ«rditÃ«sohen automatikisht.",
            "âœ— Mendo pÃ«r saktÃ«sinÃ« e stokut.",
            new[] { ("Inventory levels are automatically deducted", true), ("Nothing happens", false), ("Inventory is manually written on paper", false), ("The shift ends", false) },
            new[] { ("Nivelet e inventarit zbriten automatikisht", true), ("AsgjÃ« nuk ndodh", false), ("Inventari shkruhet manualisht nÃ« letÃ«r", false), ("NdÃ«rrimi mbaron", false) }, "POS");

        AddChapter(8, "Discounts",
            "Chapter 9 Â· Discounts & Loyalty",
            "<p>Offering discounts or a Loyalty Card gives special perks to returning customers. Applying it in the POS adjusts the final price while calculating taxes correctly.</p>",
            "CUSTOMER: VIP | DISCOUNT: 10%",
            "Why is the discount processed through the POS system directly?",
            "âœ“ Exactly, to ensure proper tax calculation and transparent pricing.",
            "âœ— Taxes must be accurate based on final price.",
            "Kapitulli 9 Â· Zbritjet & BesnikÃ«ria",
            "<p>Ofrimi i zbritjeve ose njÃ« KartelÃ« BesnikÃ«rie i jep pÃ«rfitime tÃ« veÃ§anta klientÃ«ve. Aplikimi nÃ« POS rregullon Ã§mimin final duke llogaritur TVSH-nÃ« saktÃ«.</p>",
            "KLIENT: VIP | ZBRITJE: 10%",
            "Pse zbritja duhet kaluar gjithmonÃ« pÃ«rmes sistemit POS?",
            "âœ“ Ashtu Ã«shtÃ«, pÃ«r tÃ« llogaritur TVSH-nÃ« saktÃ« nÃ« Ã§mimin final.",
            "âœ— Taksat kÃ«rkojnÃ« vlerÃ«n pas zbritjes.",
            new[] { ("To recalculate taxes on the new discounted price", true), ("To just make the receipt look longer", false), ("It doesn't have to be, you can give cash back", false), ("Only to collect emails", false) },
            new[] { ("TÃ« rillogariten taksat nÃ« Ã§mimin e ri me zbritje", true), ("VetÃ«m pÃ«r tÃ« bÃ«rÃ« kuponin tÃ« gjatÃ«", false), ("S'ka nevojÃ«, ktheni cash dore", false), ("PÃ«r tÃ« marrÃ« emailin e klientit", false) }, "POS");

        AddChapter(9, "CashDrawer",
            "Chapter 10 Â· The Cash Drawer",
            "<p>The cash drawer contains the day's physical money and is secured. It pops open ONLY when an authorized sale is recorded or via a secure 'Open Drawer' command by managers.</p>",
            ">> DRAWER KICK SIGNAL SENT",
            "When should the cash drawer electronically open?",
            "âœ“ Correct, usually only upon closing a transaction.",
            "âœ— Mostly it opens on finalized transactions.",
            "Kapitulli 10 Â· Sirtari i Parave",
            "<p>Sirtari mban paratÃ« fizike dhe Ã«shtÃ« i siguruar. Ai hapet VETÃ‹M kur njÃ« shitje e autorizuar regjistrohet ose nga njÃ« komandÃ« e sigurt nga menaxherÃ«t.</p>",
            ">> KOMANDA PÃ‹R HAPJE E DÃ‹RGUAR",
            "Kur duhet tÃ« hapet elektronikisht sirtari i parave?",
            "âœ“ SaktÃ«, sapo tÃ« mbyllet njÃ« transaksion valid.",
            "âœ— Zakonisht hapet pas printimit tÃ« kuponit.",
            new[] { ("Only when a valid transaction is finalized or by manager key", true), ("Whenever a customer walks in", false), ("Every 5 minutes", false), ("It stays unlocked all day", false) },
            new[] { ("VetÃ«m pasi mbyllet transaksioni ose me Ã§elÃ«s menaxheri", true), ("Sa herÃ« hyn njÃ« klient", false), ("Ã‡do 5 minuta", false), ("QÃ«ndron hapur gjithÃ« ditÃ«n", false) }, "POS");

        if (chapters.Any())
        {
            db.Chapters.AddRange(chapters);
        }
        db.SaveChanges();

        // â”€â”€ Seed YourBrand Business Data â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        if (!db.Businesses.Any())
        {
            var b1 = new Business { Name = "Supermarket Meridian", BusinessType = "Supermarket", Address = "Dardania, PrishtinÃ«", Phone = "044111222", TaxNumber = "810123456" };
            var p1 = new PosSystem { Business = b1, Version = "3.1", SystemType = "Supermarket POS", FiscalPrinterEnabled = true, Theme = "Dark" };
            db.Businesses.Add(b1);
            db.PosSystems.Add(p1);

            var b2 = new Business { Name = "Pizzeria Proper", BusinessType = "Pizzeria", Address = "Ulpiana, PrishtinÃ«", Phone = "045333444", TaxNumber = "810654321" };
            var p2 = new PosSystem { Business = b2, Version = "2.9", SystemType = "Pizzeria POS", FiscalPrinterEnabled = true, Theme = "Light" };
            db.Businesses.Add(b2);
            db.PosSystems.Add(p2);

            db.SaveChanges();
        }
    }
}

