using System;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Text;

namespace FF14Accessibility.Services;

// [Chat-Puffer] `partial`, damit die Zeichenketten der Chat-Puffer und des
// Einstellungsmenüs in AccessibilityStrings.Chat.cs stehen können: diese Datei ist
// groß und ändert sich in fast jeder Version. Das ist die einzige Änderung an ihr.
public static partial class AccessibilityStrings
{
    // Language is driven by the config-backed Loc provider ("/acc lang"),
    // NOT the OS culture directly. Auto still falls back to the OS culture.
    private static bool IsGerman => Loc.IsGerman;

    // Three-language form. Generated from overlay/ko/ko.json by tools/ko-apply -
    // do not hand-edit the Korean in this file, the catalogue is the original.
    private static string Pick(string de, string en, string? ko = null) =>
        Loc.Pick(de, en, ko);

    public static string TitleScreen => Pick("Titelbildschirm", "Title screen", "타이틀 화면");
    public static string MainMenu => Pick("Hauptmenü", "Main menu", "주 메뉴");
    public static string Back => Pick("Zurück", "Back", "뒤로");
    public static string NoHelpAvailable => Pick("Keine Hilfe verfügbar",
                                                 "No help available",
                                                 "도움말 없음");
    public static string HelpForTitle => Pick("Enter öffnet das Hauptmenü. Strg+F1 sagt diese Hilfe erneut an.",
                                              "Press Enter to open the main menu. Press Ctrl+F1 to hear this help again.",
                                              "엔터를 누르면 주 메뉴가 열린다. 컨트롤 F1로 이 도움말을 다시 듣는다.");
    public static string HelpForTitleMenu => Pick("Pfeil hoch und runter zum Wechseln, Enter zum Bestätigen, Escape zurück, Strg+F1 für Hilfe.",
                                                  "Use up and down arrow keys to move, Enter to confirm, Escape to go back, Ctrl+F1 for help.",
                                                  "위아래 화살표로 항목 사이를 옮겨 다니고, 엔터로 확인하고, 이스케이프로 돌아간다. 컨트롤 F1은 도움말.");

    public static string Confirmed(string item) =>
        Pick($"Auswahl bestätigt: {item}", $"Confirmed: {item}", $"선택함: {item}");

    public static string MenuPosition(string item, int index, int count) =>
        Pick($"{item}, {index} von {count}",
             $"{item}, {index} of {count}",
             $"{item}, {count} 중 {index}");

    /// <summary>GrandCompanyExchange (seal quartermaster) row: item name, seal
    /// price, amount already owned. The generic reader announced the bare
    /// "0, 1.060, name" without labels; this makes the columns explicit.</summary>
    public static string GrandCompanyRow(string name, string price, string owned) =>
        Pick($"{name}, {price} Staatstaler, Besitz {owned}",
             $"{name}, {price} seals, {owned} owned",
             $"{name}, 군표 {price}, 보유 {owned}");

    /// <summary>Announces the active category tab of a shop/window, e.g. the
    /// GrandCompanyExchange tabs (Waffen/Rüstung/...).</summary>
    public static string CategoryLabel(string name) =>
        Pick($"Kategorie {name}.", $"Category {name}.", $"분류 {name}.");

    // ── Reittier-Verzeichnis (MountNoteBook) ─────────────────────────
    /// <summary>Active view tab of the mount guide (Favorites/Normal/Search).</summary>
    public static string MountViewFavorites => Pick("Favoriten.", "Favorites.", "즐겨찾기.");
    public static string MountViewNormal    => Pick("Alle Reittiere.", "All mounts.", "탈것 전체.");
    public static string MountViewSearch    => Pick("Suche.", "Search.", "검색.");

    /// <summary>Page tab of the mount guide (1-based).</summary>
    public static string MountPage(int page) =>
        Pick($"Seite {page}.", $"Page {page}.", $"페이지 {page}.");

    /// <summary>Spoken when the focus lands on the mount guide's search box.</summary>
    public static string MountSearchField => Pick("Reittier suchen, Eingabefeld.",
                                                  "Mount search, text field.",
                                                  "탈것 검색, 입력란.");

    /// <summary>Spoken when the focus lands on the minion guide's search box.</summary>
    public static string MinionSearchField => Pick("Begleiter suchen, Eingabefeld.",
                                                   "Minion search, text field.",
                                                   "꼬마 친구 검색, 입력란.");

    // ── Umschalt-Zustaende (Checkbox / Radiobutton) ──────────────────
    /// <summary>Checkbox is ticked / unticked.</summary>
    public static string StateOn  => Pick("an", "on", "켜짐");
    public static string StateOff => Pick("aus", "off", "꺼짐");
    /// <summary>Radio button is the selected option.</summary>
    public static string RadioSelected => Pick("ausgewählt", "selected", "선택됨");
    /// <summary>Control-type word for a checkbox, so the user knows it is a
    /// toggle they can flip - not just an informational label.</summary>
    public static string SwitchControl => Pick("Schalter", "switch", "스위치");
    /// <summary>Control is greyed out / not currently changeable (NodeFlags.Enabled
    /// cleared) - e.g. a sub-toggle while its master switch is off.</summary>
    public static string StateDisabled => Pick("ausgegraut", "greyed out", "사용 불가");

    // ── Sprachumschaltung (/acc lang) ────────────────────────────────
    public static string LanguageGerman  => Pick("Deutsch", "German", "독일어");
    public static string LanguageEnglish => Pick("Englisch", "English", "영어");
    public static string LanguageKorean  => Pick("Koreanisch", "Korean", "한국어");

    public static string LanguageSet(string language) =>
        Pick($"Sprache auf {language} umgestellt.",
             $"Language set to {language}.",
             $"언어가 {language}로 변경됨.");

    public static string LanguageAuto(string language) =>
        Pick($"Sprache folgt jetzt Windows: {language}.",
             $"Language now follows Windows: {language}.",
             $"언어가 윈도 설정을 따르도록 변경됨. 현재 {language}.");

    public static string LanguageUsage =>
        Pick("Sprache wählen mit: /acc lang de, /acc lang en, /acc lang ko oder /acc lang auto.",
             "Choose a language with: /acc lang de, /acc lang en, /acc lang ko or /acc lang auto.",
             "언어를 고르려면 /acc lang ko, /acc lang en, /acc lang de, /acc lang auto 중 하나를 입력한다.");

    public static string UnknownCommand =>
        Pick("Unbekannter Befehl. Tippe /acc help für Hilfe.",
             "Unknown command. Type /acc help for help.",
             "모르는 명령. 도움말은 /acc help.");

    // ── Keybind-Dump (/acc keys) ─────────────────────────────────────
    /// <summary>
    /// Short conflict notice for the automatic dump at login. The full sentence
    /// below is for the explicit "/acc keys" call - at login it arrived in the
    /// middle of the HUD build-up and was cut off anyway (user 2026-08-06).
    /// Only the conflict count is actionable there: a plugin key is dead.
    /// </summary>
    public static string KeybindConflictsShort(int conflictCount) =>
        Loc.IsKorean ? $"키 충돌 {conflictCount}개."
        : IsGerman
            ? (conflictCount == 1 ? "1 Tastenkonflikt." : $"{conflictCount} Tastenkonflikte.")
            : (conflictCount == 1 ? "1 key conflict." : $"{conflictCount} key conflicts.");

    public static string KeybindDumpSaved(int boundCount, int conflictCount) =>
        Pick($"Tastenbelegung gespeichert: {boundCount} Aktionen mit Taste, {conflictCount} Konflikte mit Plugin-Tasten. Datei auf dem Desktop, Details im Log.",
             $"Keybinds saved: {boundCount} bound actions, {conflictCount} conflicts with plugin keys. File on desktop, details in log.",
             $"단축키 저장됨. 키가 지정된 동작 {boundCount}개, 모드 키와 충돌 {conflictCount}개. 파일은 바탕 화면에, 자세한 것은 로그에.");

    public static string KeybindDumpFailed =>
        Pick("Tastenbelegung konnte nicht gelesen werden. Details im Log.",
             "Could not read keybinds. See log for details.",
             "단축키를 읽지 못함. 자세한 것은 로그에.");

    // ── ConfigSystem ─────────────────────────────────────────────────
    public static string ConfigSystem =>
        Pick("Systemeinstellungen", "System Configuration", "시스템 설정");

    public static string ConfigSystemSaved =>
        Pick("Einstellungen gespeichert", "Settings saved", "설정 저장됨");

    public static string ConfigSystemDiscarded =>
        Pick("Änderungen verworfen", "Changes discarded", "변경 취소됨");

    public static string HelpForConfigSystem => Pick("Pfeile hoch und runter wechseln Option. Links und rechts ändern Wert oder Tab. Enter speichert, Escape verwirft, Strg+F1 für Hilfe.",
                                                     "Up and down arrows move between options. Left and right change value or tab. Enter saves, Escape discards, Ctrl+F1 for help.",
                                                     "위아래 화살표로 다른 항목으로 옮겨 간다. 좌우 화살표로 값이나 탭을 바꾼다. 엔터는 저장, 이스케이프는 취소, 컨트롤 F1은 도움말.");

    public static string CheckboxOn  => Pick("an", "on", "켜짐");
    public static string CheckboxOff => Pick("aus", "off", "꺼짐");

    public static string OptionPosition(string label, string value, int index, int count) =>
        Pick($"{label}, {value}, {index} von {count}",
             $"{label}, {value}, {index} of {count}",
             $"{label}, {value}, {count} 중 {index}");

    public static string TabPosition(string label, int index, int count) =>
        Pick($"{label}, Tab {index} von {count}",
             $"{label}, tab {index} of {count}",
             $"{label}, 탭 {count} 중 {index}");

    // ── Triple Triad (Kartenspiel) ───────────────────────────────────
    // Fields read directly from AddonTripleTriad (Board/BlueDeck/RedDeck,
    // ilspycmd-verified). Numbers are pre-formatted by the service (1-9, 10 -> "A")
    // so the digit/A convention stays language-independent.
    public static string CardGameTitle => Pick("Kartenspiel", "Card game", "카드 대결");

    /// <summary>The four edge numbers of a card, in a fixed clockwise-from-top order.</summary>
    public static string CardSides(string up, string right, string down, string left) =>
        Pick($"oben {up}, rechts {right}, unten {down}, links {left}",
             $"top {up}, right {right}, bottom {down}, left {left}",
             $"위 {up}, 오른쪽 {right}, 아래 {down}, 왼쪽 {left}");

    /// <summary>Owner of a card that sits on the board or in a hand.</summary>
    public static string CardOwnerYours => Pick("deine", "yours", "내 것");
    public static string CardOwnerEnemy => Pick("gegnerische", "enemy", "상대 것");

    /// <summary>One board cell (1-based), either empty or holding a card.</summary>
    public static string BoardCellEmpty(int cell) =>
        Pick($"Feld {cell}: leer", $"Cell {cell}: empty", $"칸 {cell}: 비어 있음");

    public static string BoardCellCard(int cell, string owner, string sides) =>
        Pick($"Feld {cell}: {owner}, {sides}",
             $"Cell {cell}: {owner}, {sides}",
             $"칸 {cell}: {owner}, {sides}");

    /// <summary>One hand card (1-based).</summary>
    public static string HandCard(int index, string sides) =>
        Pick($"Karte {index}: {sides}", $"Card {index}: {sides}", $"카드 {index}: {sides}");

    /// <summary>Focus announcement for a single card (board cell or hand card).</summary>
    public static string FocusBoardCell(int cell, string content) =>
        Pick($"Feld {cell}, {content}", $"Cell {cell}, {content}", $"칸 {cell}, {content}");

    public static string FocusHandCard(int index, int count, string sides) =>
        Pick($"Handkarte {index} von {count}, {sides}",
             $"Hand card {index} of {count}, {sides}",
             $"카드 {count} 중 {index}, {sides}");

    public static string CardGameNotOpen =>
        Pick("Kartenspiel ist nicht offen.", "Card game is not open.", "카드 대결이 열려 있지 않음.");

    public static string BoardIntro(int yours, int enemy) =>
        Pick($"Brett. Deine Karten {yours}, gegnerische {enemy}.",
             $"Board. Your cards {yours}, enemy {enemy}.",
             $"판에 놓인 카드, 내 것 {yours}장, 상대 것 {enemy}장.");

    public static string HandIntro(int count) =>
        Pick($"Deine Hand, {count} Karten.", $"Your hand, {count} cards.", $"손에 든 카드 {count}장.");

    public static string HandEmpty =>
        Pick("Keine Handkarten mehr.", "No hand cards left.", "남은 카드 없음.");

    // HYPOTHESE (in-game zu verifizieren): TurnState NormalMove/MaskedMove = du bist
    // am Zug, Waiting = Gegner/warten. Der Rohwert wird zusaetzlich geloggt.
    public static string YourTurn => Pick("Du bist am Zug.", "Your turn.", "내 차례.");
    public static string WaitingTurn => Pick("Warten.", "Waiting.", "대기.");

    // ── Fenster-Ansage (F2 / /acc win) ───────────────────────────────
    public static string ActiveWindow(string name, int visibleCount) =>
        Pick($"Aktives Fenster: {name}. {visibleCount} Fenster sichtbar, Liste im Log.",
             $"Active window: {name}. {visibleCount} windows visible, list written to log.",
             $"초점 창: {name}. 보이는 창 {visibleCount}개, 전체 목록은 로그에 적힘.");

    public static string NoWindowFocused(int visibleCount) =>
        Pick($"Kein Fenster fokussiert. {visibleCount} Fenster sichtbar, Liste im Log.",
             $"No window focused. {visibleCount} windows visible, list written to log.",
             $"초점이 놓인 창 없음. 보이는 창 {visibleCount}개, 전체 목록은 로그에 적힘.");

    public static string UiManagerUnavailable =>
        Pick("Fenster-Liste nicht verfügbar.", "Window list not available.", "창 목록을 가져올 수 없음.");

    public static string DumpSaved(int addonCount, int nodeCount) =>
        Pick($"UI Dump auf Desktop gespeichert. {addonCount} Fenster, {nodeCount} Nodes.",
             $"UI dump saved to desktop. {addonCount} windows, {nodeCount} nodes.",
             $"UI 덤프를 바탕 화면에 저장함. 창 {addonCount}개, 노드 {nodeCount}개.");

    public static string AddonNotOpen(string names) =>
        Pick($"Addon {names} nicht offen.", $"Addon {names} not open.", $"{names} 창이 열려 있지 않음.");

    // ── Ok-Taste (Enter in Lobby/Charaktererstellung) ────────────────
    public static string OkPressed  => Pick("Ok", "Ok", "확인");
    public static string NoOkButton => Pick("Kein Ok-Knopf gefunden.",
                                            "No Ok button found.",
                                            "확인 버튼을 못 찾음.");

    // ── Charaktererstellung: Volk & Geschlecht ───────────────────────
    public static string GenderMale   => Pick("männlich", "male", "남성");
    public static string GenderFemale => Pick("weiblich", "female", "여성");

    // ── SelectYesno ──────────────────────────────────────────────────
    /// <summary>Fallback button labels, used only when the dialog's own button
    /// nodes carry no text - normally the labels are READ from the game.</summary>
    public static string YesWord => Pick("Ja", "Yes", "예");
    public static string NoWord  => Pick("Nein", "No", "아니오");
    public static string DialogButtons(string confirm, string cancel) =>
        Pick($"{confirm} oder {cancel}? Links und rechts wechseln, Enter wählt aus.",
             $"{confirm} or {cancel}? Left and right to switch, Enter to select.",
             $"{confirm} 또는 {cancel}? 좌우 방향키로 고르고 엔터로 확정한다.");

    // ── Navigation: Himmelsrichtungen, relative Richtung, Distanz ─────
    // Sprachabhängige Kompass-Wörter (0 = Norden .. 7 = Nordwesten). Property,
    // KEIN static readonly Array: "/acc lang" schaltet zur Laufzeit um, ein
    // eingefrorenes Array würde die alte Sprache behalten.
    private static readonly string[] CompassDe =
        { "Norden", "Nordosten", "Osten", "Südosten", "Süden", "Südwesten", "Westen", "Nordwesten" };
    private static readonly string[] CompassEn =
        { "North", "Northeast", "East", "Southeast", "South", "Southwest", "West", "Northwest" };

    // Korean does not inflect a compass word for this position, so the two Korean
    // arrays hold the same words. They stay TWO arrays: merging them would force
    // German and English to share one, and those two do differ.
    private static readonly string[] CompassKo =
        { "북", "북동", "동", "남동", "남", "남서", "서", "북서" };

    public static string[] CompassWords => Loc.IsKorean ? CompassKo : IsGerman ? CompassDe : CompassEn;

    // Adjective/adverb compass forms for "&lt;distance&gt; meters &lt;dir&gt;"
    // spot lines (0 = North .. 7 = Northwest).
    private static readonly string[] CompassAdjDe =
        { "nördlich", "nordöstlich", "östlich", "südöstlich", "südlich", "südwestlich", "westlich", "nordwestlich" };
    private static readonly string[] CompassAdjEn =
        { "north", "northeast", "east", "southeast", "south", "southwest", "west", "northwest" };
    private static readonly string[] CompassAdjKo =
        { "북", "북동", "동", "남동", "남", "남서", "서", "북서" };
    public static string[] CompassAdjectives => Loc.IsKorean ? CompassAdjKo : IsGerman ? CompassAdjDe : CompassAdjEn;

    /// <summary>A spot list line: name, level, distance and compass bearing
    /// (shared by the fishing- and gathering-spot read-outs).</summary>
    public static string SpotListLine(string name, int level, float distance, string compass) =>
        Pick($"{name}, Stufe {level}, {distance:F0} Meter {compass}",
             $"{name}, level {level}, {distance:F0} meters {compass}",
             $"{name}, 레벨 {level}, {compass} 방향 {distance:F0}미터");

    /// <summary>Relative-to-heading direction word for a signed angle in degrees
    /// (negative = left, 0 = ahead). The spoken steering cue.</summary>
    public static string RelativeDirection(double relativeAngle) => relativeAngle switch
    {
        < -135 => Pick("hinter links", "behind to the left", "왼쪽 뒤"),
        < -45  => Pick("links", "left", "왼쪽"),
        < -15  => Pick("leicht links", "slightly left", "약간 왼쪽"),
        <= 15  => Pick("geradeaus", "straight ahead", "정면"),
        <= 45  => Pick("leicht rechts", "slightly right", "약간 오른쪽"),
        <= 135 => Pick("rechts", "right", "오른쪽"),
        _      => Pick("hinter rechts", "behind to the right", "오른쪽 뒤"),
    };

    /// <summary>Spoken distance: very close as a phrase, otherwise metres, then
    /// kilometres. Mirrors the mod's metre convention (not in-game yalms).</summary>
    public static string FormatDistance(float distance) =>
        distance < 2f    ? (Pick("direkt neben dir", "right next to you", "바로 옆")) :
        distance < 100f  ? (Pick($"{distance:F0} Meter",
                                 $"{distance:F0} meters",
                                 $"{distance:F0}미터")) :
                           (Pick($"{distance / 1000:F1} Kilometer",
                                 $"{distance / 1000:F1} kilometers",
                                 $"{distance / 1000:F1}킬로미터"));

    // ── Objekt-Browser: Kategorie-Labels & -Ansagen (NavigationService) ─
    /// <summary>The spoken name of an object-browser category in the active
    /// language. Identity is the NavCategory key; this is display only.</summary>
    internal static string CategoryLabel(NavCategory cat) => cat switch
    {
        NavCategory.All              => Pick("Alles", "Everything", "전체"),
        NavCategory.Npcs             => Pick("NPCs", "NPCs", "NPC"),
        NavCategory.Merchants        => Pick("Händler", "Merchants", "상인"),
        NavCategory.Enemies          => Pick("Gegner", "Enemies", "적"),
        NavCategory.Allies           => Pick("Verbündete", "Allies", "아군"),
        NavCategory.Players          => Pick("Spieler", "Players", "플레이어"),
        NavCategory.Objects          => Pick("Objekte", "Objects", "사물"),
        // NICHT "Dungeons": die Kategorie haelt auch Prüfungs-, Raid- und
        // PvP-Türen, und die Ansage nennt Inhalt und Art ohnehin. Das deutsche Wort
        // ist das des SPIELS, keine nach Gefühl gewählte Übersetzung - der Client
        // nennt die Inhaltssuche "Inhaltssuche", den Zufallsinhalt "Zufallsinhalt",
        // und "There are no duties available" heisst dort "Keine Inhalte vorhanden"
        // (Addon 2500/2509, in beiden Sprachen gedumpt). Ein Duty ist also ein
        // "Inhalt", und der Spieler hört das Wort im Spiel bereits.
        NavCategory.Duties           => Pick("Inhalte", "Duties", "임무"),
        NavCategory.QuestNpcs        => Pick("Quest-NPCs", "Quest NPCs", "퀘스트 NPC"),
        NavCategory.QuestObjects     => Pick("Quest-Objekte", "Quest objects", "퀘스트 사물"),
        NavCategory.QuestEnemies     => Pick("Quest-Gegner", "Quest enemies", "퀘스트 적"),
        NavCategory.GatheringNodes   => Pick("Sammelpunkte", "Gathering nodes", "채집 지점"),
        NavCategory.Fates            => Pick("FATEs", "FATEs", "돌발 임무"),
        NavCategory.HuntingTargets   => Pick("Jagdziele", "Hunting targets", "토벌 대상"),
        NavCategory.FishingSpots     => Pick("Angelplätze", "Fishing spots", "낚시터"),
        NavCategory.Aetherytes       => Pick("Ätheryten", "Aetherytes", "에테라이트"),
        NavCategory.QuestGoals       => Pick("Quest-Ziele", "Quest goals", "퀘스트 목표"),
        NavCategory.AcceptableQuests => Pick("Annehmbare Quests", "Available quests", "받을 수 있는 퀘스트"),
        NavCategory.Levequests       => Pick("Freibriefe", "Levequests", "길드 의뢰"),
        NavCategory.Waypoints        => Pick("Wegpunkte", "Waypoints", "경유지"),
        _                            => cat.ToString(),
    };

    /// <summary>What a merchant deals in, spoken in place of the generic "NPC"
    /// while browsing the merchant category.</summary>
    internal static string ShopKindWord(ShopKind kind) => kind switch
    {
        ShopKind.GilShop  => Pick("Laden", "shop", "상점"),
        ShopKind.Exchange => Pick("Tausch", "exchange", "교환"),
        _                 => Pick("Händler", "merchant", "상인"),
    };

    /// <summary>
    /// Was hinter einer Tür liegt, gesprochen an der Stelle des generischen
    /// "Objekt", während der Spieler die Kategorie Inhalte durchblättert. User:
    /// *"right now they are just called entrance instead of having more useful
    /// names like the name of the dungeon."* Deshalb trägt das hier den NAMEN und
    /// die Stufe, nicht nur ein Kategoriewort.
    ///
    /// Der Objektname wird unmittelbar davor gesprochen, das Ergebnis liest sich
    /// also als *"Eingang, Dungeon: Das Tam-Tara-Grab, Stufe 16, 12 Meter, Norden."*
    ///
    /// NAME UND STUFE GEHÖREN DEM SPIEL - aus ContentFinderCondition, in der
    /// Client-Sprache; der Mod übersetzt und erfindet hier nichts. Nur das
    /// Kategoriewort im Singular ist mod-eigen, und es ist der Singular dessen, was
    /// der deutsche Client selbst für diese Inhaltsart schreibt (ContentType, in
    /// beiden Sprachen gedumpt: 2 'Dungeons', 4 'Prüfungen', 5 'Raids', 6 'PvP').
    /// Jede andere Art behält das Wort des Spiels wörtlich, statt verworfen oder
    /// geraten zu werden.
    /// </summary>
    internal static string DutyEntrance(string dutyName, uint contentType, ushort level, string gameTypeName)
    {
        var category = contentType switch
        {
            2 => Pick("Dungeon", "dungeon", "던전"),
            4 => Pick("Prüfung", "trial", "토벌전"),
            5 => Pick("Raid", "raid", "레이드"),
            6 => "PvP",
            _ => gameTypeName,
        };

        // Ein Inhalt ohne Stufenanforderung sagt nichts über eine Stufe, statt
        // "Stufe 0" zu sagen.
        var withLevel = level > 0
            ? (Pick($"{dutyName}, Stufe {level}",
                    $"{dutyName}, level {level}",
                    $"{dutyName}, 레벨 {level}"))
            : dutyName;

        return category.Length > 0 ? $"{category}: {withLevel}" : withLevel;
    }

    // The word "Kategorie"/"Category" is deliberately NOT spoken in front of the
    // name (user 2026-08-04): the player just pressed the category key, so the
    // context is already clear - only the name carries information. The chat
    // history has always announced its categories this way; the object browser
    // now matches it.
    public static string CategoryQuestCount(string label, int here, int away) =>
        away > 0
            ? (Pick($"{label}: {here} im Gebiet, {away} in anderen Gebieten.",
                    $"{label}: {here} in this area, {away} in other areas.",
                    $"이 지역에 {label} {here}, 다른 지역에 {away}."))
            : (Pick($"{label}: {here} im Gebiet.",
                    $"{label}: {here} in this area.",
                    $"이 지역에 {label} {here}."));

    public static string CategoryWaypointCount(int count, int exits) =>
        exits > 0
            ? (Pick($"Wegpunkte: {count} im Gebiet, davon {exits} Übergänge.",
                    $"Waypoints: {count} in this area, {exits} of them exits.",
                    $"이 지역에 경유지 {count}곳, 그중 다른 지역으로 넘어가는 통로 {exits}곳."))
            : (Pick($"Wegpunkte: {count} im Gebiet.",
                    $"Waypoints: {count} in this area.",
                    $"이 지역에 경유지 {count}곳."));

    public static string CategoryAetheryteCount(int count) =>
        Pick($"Ätheryten: {count} im Gebiet.",
             $"Aetherytes: {count} in this area.",
             $"이 지역에 에테라이트 {count}곳.");

    // ── FATEs: aktive Welt-Ereignisse der Zone ──
    public static string CategoryFateCount(int active, int preparing) =>
        preparing > 0
            ? (Pick($"FATEs: {active} aktiv, {preparing} starten gleich.",
                    $"FATEs: {active} active, {preparing} starting soon.",
                    $"돌발 임무: 이 지역에 진행 중 {active}개, 곧 시작 {preparing}개."))
            : (Pick($"FATEs: {active} aktiv.",
                    $"FATEs: {active} active.",
                    $"돌발 임무: 이 지역에 진행 중 {active}개."));

    /// <summary>One FATE line: name, level, then either the completion percent or,
    /// for a not-yet-started FATE, a "starting soon" note.</summary>
    public static string FateEntry(string name, int level, byte progress, bool preparing) =>
        Loc.IsKorean ? $"{name}, 레벨 {level}, {(preparing ? "곧 시작" : $"{progress} 퍼센트")}"
        : IsGerman
            ? $"{name}, Stufe {level}, {(preparing ? "startet gleich" : $"{progress} Prozent")}"
            : $"{name}, level {level}, {(preparing ? "starting soon" : $"{progress} percent")}";

    public static string NoFatesInZone =>
        Pick("Keine FATEs in diesem Gebiet.", "No FATEs in this area.", "이 지역에 돌발 임무 없음.");

    // ── Jagdziele: offene Monster des aktuellen Jagdtagebuch-Rangs ──
    public static string CategoryHuntingCount(int total, int here) =>
        here > 0
            ? (Pick($"Jagdziele: {total} offen, {here} in diesem Gebiet.",
                    $"Hunting targets: {total} open, {here} in this area.",
                    $"토벌 대상: 남은 것 {total}종, 그중 이 지역에 {here}종."))
            : (Pick($"Jagdziele: {total} offen, keines in diesem Gebiet.",
                    $"Hunting targets: {total} open, none in this area.",
                    $"토벌 대상: 남은 것 {total}종. 이 지역에는 없음."));

    /// <summary>One hunting log line: the monster and how many kills are still missing.</summary>
    public static string HuntingTargetEntry(string monster, int killed, int required) =>
        Pick($"{monster}, {killed} von {required} erlegt",
             $"{monster}, {killed} of {required} killed",
             $"{monster}, {required} 중 {killed} 처치");

    /// <summary>Said instead of the habitat when a live specimen is in range -
    /// the distance and direction that follow lead to the monster itself.</summary>
    public static string HuntingMonsterNearby =>
        Pick("in der Nähe", "nearby", "근처");

    /// <summary>The area a hunting log monster lives in, as the log names it.</summary>
    public static string HuntingArea(string area) =>
        area.Length == 0 ? string.Empty : (Pick($"lebt in {area}",
                                                $"lives in {area}",
                                                $"{area}에 서식"));

    public static string HuntingNoRoute(string monster, string zone) =>
        Pick($"{monster} lebt in {zone}. Dorthin führt kein Weg über Gebietsübergänge.",
             $"{monster} lives in {zone}. No route there over zone transitions.",
             $"{monster}, {zone}에 서식. 지역 통로로 가는 길이 없음.");

    public static string HuntingAreaUnknown(string monster, string area) =>
        area.Length > 0
            ? (Pick($"{monster} lebt in {area}. Dieses Gebiet ist auf der Karte nicht verzeichnet.",
                    $"{monster} lives in {area}. That area is not marked on the map.",
                    $"{monster}, {area}에 서식. 그 지역은 지도에 표시되지 않음."))
            : (Pick($"Für {monster} ist kein Ort bekannt.",
                    $"No location known for {monster}.",
                    $"{monster}의 위치를 모름."));

    public static string NoHuntingTargets =>
        Pick("Keine offenen Jagdziele in diesem Rang.",
             "No open hunting targets in this rank.",
             "이 등급에 남은 토벌 대상 없음.");

    // ── Freibriefe (Levequests): Geber-NPCs + Ziele ──
    public static string CategoryLevequestCount(int givers, int goals) =>
        Pick($"Freibriefe: {givers} Geber, {goals} Ziele.",
             $"Levequests: {givers} givers, {goals} goals.",
             $"길드 의뢰: 의뢰인 {givers}명, 목표 지점 {goals}곳.");

    /// <summary>Spoken role prefix so the player knows whether a leve destination
    /// is the Levemete (accept/hand in) or the objective (do the task).</summary>
    public static string LeveRolePrefix(QuestMarkerRole role) => role switch
    {
        QuestMarkerRole.LeveGiver     => Pick("Freibrief-Geber: ", "Levequest giver: ", "길드 의뢰인: "),
        QuestMarkerRole.LeveObjective => Pick("Freibrief-Ziel: ", "Levequest goal: ", "길드 의뢰 목표: "),
        _                             => string.Empty,
    };

    public static string NoLevequests =>
        Pick("Keine Freibriefe. Erst bei einem Freibrief-Geber annehmen.",
             "No levequests. Accept one from a levemete first.",
             "길드 의뢰 없음. 먼저 의뢰인에게서 받는다.");

    // Fishing spots (Angelplätze). Type label used when the spot flows through
    // the shared PlaceDestination path; entry adds the required fishing level.
    public static string FishingSpotType => Pick("Angelplatz", "Fishing spot", "낚시터");

    public static string FishingSpotEntry(string name, int level) =>
        Pick($"{name}, Stufe {level}", $"{name}, level {level}", $"{name}, 레벨 {level}");

    public static string CategoryFishingCount(int count) =>
        Pick($"Angelplätze: {count} im Gebiet.",
             $"Fishing spots: {count} in this area.",
             $"이 지역에 낚시터 {count}곳.");

    public static string NoFishingSpots =>
        Pick("Keine Angelplätze in diesem Gebiet.",
             "No fishing spots in this area.",
             "이 지역에 낚시터 없음.");

    /// <summary>Spoken the moment the game reports the player can cast from where
    /// they stand and face - the orientation cue a blind fisher rotates until
    /// they hear (FishingEventHandler.CanFish flips true in the ready stance).</summary>
    public static string FishReady =>
        Pick("Angelbereit.", "Ready to fish.", "낚시 준비됨.");

    /// <summary>Spoken on a bite - strike now (FishingState -> Bite).</summary>
    public static string FishBite =>
        Pick("Biss!", "Bite!", "입질.");

    public static string CategoryObjectCount(string label, int count) =>
        Pick($"{label}: {count} in der Nähe.", $"{label}: {count} nearby.", $"근처에 {label} {count}.");

    public static string NoObjectsInRange(string label, float range) =>
        Pick($"Keine {label} in {range:F0} Metern.",
             $"No {label} within {range:F0} meters.",
             $"{range:F0}미터 안에 {label} 없음.");

    // ── Objekt-/Ziel-Ansagen (NavigationService) ─────────────────────
    // "Unbenannt" removed 2026-08-08: it said nothing about what the thing was,
    // and every caller now uses UnnamedOfKind (or a resolved name) instead.

    /// <summary>Spoken "N of M" position counter for browser cycling (no period).</summary>
    /// <summary>The word between the two numbers of a counter. Exposed because
    /// code that RECOGNISES a counter it printed earlier (see UIReaderService,
    /// IsSpokenProgress) must not hard-code the German "von" - that comparison
    /// silently stops matching the moment the announcement speaks English.</summary>
    public static string CounterConnector => IsGerman ? "von" : "of";

    public static string Counter(int index, int count) =>
        $"{index} {CounterConnector} {count}";

    /// <summary>Same "x of y" form for values that arrive as text (a progress
    /// display read from the UI, e.g. "3/5"), where parsing them to numbers
    /// would only risk losing what the game actually printed.</summary>
    public static string Counter(string index, string count) =>
        $"{index} {CounterConnector} {count}";

    /// <summary>Trailing warning when the game refused to set the target
    /// (leading space, appended to a target announcement).</summary>
    public static string NotTargetedSuffix => Pick(" Achtung, nicht anvisiert.",
                                                   " Warning, not targeted.",
                                                   " 주의, 대상 지정 안 됨.");

    public static string TargetPrefix => Pick("Ziel: ", "Target: ", "대상: ");

    public static string Tracking(string name)      => Pick($"Verfolge {name}.",
                                                            $"Tracking {name}.",
                                                            $"{name} 추적 중.");
    public static string TargetNotFound(string name)=> Pick($"Ziel {name} nicht gefunden.",
                                                            $"Target {name} not found.",
                                                            $"대상 {name}, 못 찾음.");
    public static string TargetReached(string name) => Pick($"Ziel erreicht: {name}.",
                                                            $"Target reached: {name}.",
                                                            $"도착: {name}.");
    public static string TargetDirection(string name, string distance, string direction) =>
        IsGerman ? $"{name}: {distance}, {direction}." : $"{name}: {distance}, {direction}.";

    public static string TrackingStopped => Pick("Zielverfolgung beendet.",
                                                 "Target tracking stopped.",
                                                 "대상 추적 끝냄.");
    public static string WalkTargetLost  => Pick("Gehhilfe: Ziel verloren.",
                                                 "Walk guide: target lost.",
                                                 "길안내 대상을 놓침.");
    public static string NoGameTarget    => Pick("Kein Ziel anvisiert.",
                                                 "No target selected.",
                                                 "지정된 대상 없음.");
    public static string NoNearbyObjects => Pick("Keine Objekte in der Nähe.",
                                                 "No objects nearby.",
                                                 "근처에 사물 없음.");
    public static string NearbyList(string joined) => Pick($"In der Nähe: {joined}",
                                                           $"Nearby: {joined}",
                                                           $"근처: {joined}");

    /// <summary>"No target. Select an object with Page Down first." (object browser hint).
    /// The object browser moved off N onto the Page keys in V5.31, so the hint
    /// names Page Down (KeyNextObject default) now, not the old N.</summary>
    public static string NoTargetSelectN => Pick("Kein Ziel. Erst mit Bild ab ein Objekt wählen.",
                                                 "No target. Select an object with Page Down first.",
                                                 "대상 없음. 먼저 페이지 다운으로 사물을 고른다.");

    /// <summary>"No target set. Select an object with Page Down first." (direction key).</summary>
    public static string NoTargetTracked => Pick("Kein Ziel gesetzt. Erst mit Bild ab ein Objekt wählen.",
                                                 "No target set. Select an object with Page Down first.",
                                                 "대상이 정해지지 않음. 먼저 페이지 다운으로 사물을 고른다.");

    /// <summary>Type name for an object kind, spoken after the object name.</summary>
    public static string ObjectKindName(ObjectKind kind) => kind switch
    {
        ObjectKind.Pc             => Pick("Spieler", "Player", "플레이어"),
        ObjectKind.BattleNpc      => Pick("Kampf-NPC", "Combat NPC", "전투 NPC"),
        ObjectKind.EventNpc       => Pick("NPC", "NPC", "NPC"),
        ObjectKind.Treasure       => Pick("Schatz", "Treasure", "보물상자"),
        ObjectKind.Aetheryte      => Pick("Ätheryt", "Aetheryte", "에테라이트"),
        ObjectKind.GatheringPoint => Pick("Sammelpunkt", "Gathering node", "채집 지점"),
        ObjectKind.EventObj       => Pick("Objekt", "Object", "사물"),
        // The game's own class for usable housing furniture (Chocobo-Stall,
        // Briefkasten, Diarium-Pult). "Einrichtung" rather than "Möbel": it also
        // covers the stable and the mailbox, which are not furniture in the
        // everyday sense but are exactly this kind to the game.
        ObjectKind.HousingEventObject => Pick("Einrichtung", "Furnishing", "가구"),
        ObjectKind.Companion      => Pick("Begleiter", "Companion", "꼬마 친구"),
        ObjectKind.Retainer       => Pick("Gehilfe", "Retainer", "집사"),
        ObjectKind.Mount          => Pick("Reittier", "Mount", "탈것"),
        _                         => kind.ToString(),
    };

    /// <summary>
    /// Stand-in for an object the GAME itself leaves nameless - "Objekt ohne
    /// Namen", "NPC ohne Namen". Says which kind of thing it is and makes clear
    /// that the missing name is the game's, not a failure of the mod (user
    /// decision 2026-08-08). Verified offline the same day: for every nameless
    /// object in the log, the game's own name sheets are empty too.
    /// </summary>
    public static string UnnamedOfKind(ObjectKind kind) => Pick($"{ObjectKindName(kind)} ohne Namen",
                                                                $"{ObjectKindName(kind)} with no name",
                                                                $"이름 없는 {ObjectKindName(kind)}");

    /// <summary>
    /// The two objects the game names with an ICON instead of a word. Both words
    /// are the game's own vocabulary, not an invention of this mod - see
    /// ObjectNameService.IconNamed for where each one is quoted from.
    /// </summary>
    public static string GardenBed => Pick("Beet", "Garden bed", "밭");
    public static string MailBox   => Pick("Postkasten", "Mailbox", "편지함");

    /// <summary>
    /// Appended to an object's name to say which quest it serves: "Zielort für
    /// Narben im Wald". The game calls 1667 different props "Zielort", so the
    /// name alone identifies nothing (user report 2026-08-08).
    /// </summary>
    public static string ForQuest(string quest) => Pick($" für {quest}",
                                                        $" for {quest}",
                                                        $" {quest} 관련");

    /// <summary>
    /// Appended to a zone transition to say where it leads: "Ausgang nach
    /// Neu-Gridania".
    /// </summary>
    public static string LeadsToArea(string area) => Pick($" nach {area}",
                                                          $" to {area}",
                                                          $" {area} 방향");

    /// <summary>
    /// Appended to an object the player has already stood next to: "Truhe 2,
    /// Schatz, schon besucht". In a dungeon several things carry one name, and
    /// which of them one has already dealt with is the thing a sighted player
    /// reads off the room they remember walking through (user wish 2026-08-08).
    /// Leading comma and space, so it slots into the description like the kind.
    /// </summary>
    public static string AlreadyVisited => Pick(", schon besucht", ", already visited", ", 이미 다녀옴");

    /// <summary>Quest hint from a nameplate icon id, or empty for none.</summary>
    public static string QuestMarkerHint(uint iconId) => iconId switch
    {
        0                     => string.Empty,
        >= 71001 and <= 71006 => Pick("Quest verfügbar", "Quest available", "퀘스트 받을 수 있음"),
        >= 71021 and <= 71046 => Pick("Quest aktiv", "Quest active", "퀘스트 진행 중"),
        >= 71000 and <= 71999 => Pick("Quest", "Quest", "퀘스트"),
        _                     => string.Empty,
    };

    /// <summary>Gathering-node description ("Gathering node" / "&lt;type&gt;, level N").
    /// <paramref name="type"/> is the game-provided node type; may be empty.</summary>
    public static string GatheringNodeFallback => Pick("Sammelpunkt", "Gathering node", "채집 지점");
    public static string GatheringNodeDesc(string type, int level) =>
        level > 0
            ? (Pick($"{type}, Stufe {level}", $"{type}, level {level}", $"{type}, 레벨 {level}"))
            : type;

    // ── Quest-Ziel-Ansage (zusammengesetzt) ──────────────────────────
    public static string NoAcceptableQuests => Pick("Keine annehmbaren Quests in der Nähe.",
                                                    "No available quests nearby.",
                                                    "근처에 받을 수 있는 퀘스트 없음.");
    public static string NoQuestGoals       => Pick("Keine Quest-Ziele. Erst eine Quest annehmen.",
                                                    "No quest goals. Accept a quest first.",
                                                    "퀘스트 목표 없음. 먼저 퀘스트를 받는다.");
    public static string StoryPrefix        => Pick("Story: ", "Story: ", "스토리: ");

    /// <summary>
    /// The kind of quest, spoken in front of the quest name. Every known kind is
    /// named, side quests included: silence would leave the player unable to tell
    /// "side quest" from "feature broken" (user 2026-08-06). Only
    /// <see cref="QuestKind.Unknown"/> stays empty - there we have nothing to
    /// back a claim with. Main story keeps the wording players already know from
    /// <see cref="StoryPrefix"/>.
    /// </summary>
    public static string QuestKindPrefix(QuestKind kind) => kind switch
    {
        QuestKind.MainStory  => StoryPrefix,
        QuestKind.Job        => Pick("Job: ", "Job: ", "직업: "),
        QuestKind.BeastTribe => Pick("Freundesvolk: ", "Beast tribe: ", "우호부족: "),
        QuestKind.Chronicle  => Pick("Chronik: ", "Chronicle: ", "연대기: "),
        QuestKind.SideQuest  => Pick("Nebenauftrag: ", "Side quest: ", "서브 퀘스트: "),
        QuestKind.Other      => Pick("Sonstiges: ", "Other: ", "기타: "),
        _                    => string.Empty,
    };
    public static string LevelPrefix(int level) => Pick($"Stufe {level}, ",
                                                        $"Level {level}, ",
                                                        $"레벨 {level}, ");
    public static string InArea(string zone)    => Pick($"im Gebiet {zone}.",
                                                        $"in the area {zone}.",
                                                        $"{zone} 지역.");
    public static string InAnotherArea       => Pick("in einem anderen Gebiet.",
                                                     "in another area.",
                                                     "다른 지역.");
    public static string NumpadWalksToTransition => Pick(" Nummernblock 3 läuft zum Übergang.",
                                                         " Numpad 3 walks to the transition.",
                                                         " 숫자패드 3을 누르면 통로로 간다.");

    /// <summary>The "get there via &lt;transition&gt;" clause of a cross-zone quest
    /// announcement, including the count of remaining transitions.</summary>
    public static string RouteViaHop(string hopName, string distance, string direction, int extraHops) =>
        Loc.IsKorean
            ? $" {hopName} 통해서, {distance}, {direction}" +
              (extraHops > 0 ? $", 그다음 통로 {extraHops}개 더." : ".")
        : IsGerman
            ? $" Dorthin über {hopName}, {distance}, {direction}" +
              (extraHops > 0 ? $", danach noch {extraHops} weitere Übergänge." : ".")
            : $" Get there via {hopName}, {distance}, {direction}" +
              (extraHops > 0 ? $", then {extraHops} more transitions." : ".");

    // ── Wegpunkte / Gehhilfe / Routen-Ansagen ────────────────────────
    /// <summary>Appended to a marker selection when the real object behind it
    /// was taken as the game target - that is what makes it usable, and the
    /// player has no other way to tell.</summary>
    public static string MarkerTargeted => Pick("Angezielt.", "Targeted.", "대상 지정됨.");

    public static string NoAetherytesFound => Pick("Keine Ätheryten in diesem Gebiet gefunden.",
                                                   "No aetherytes found in this area.",
                                                   "이 지역에서 에테라이트를 못 찾음.");
    public static string NoWaypointsFound  => Pick("Keine Wegpunkte in diesem Gebiet gefunden.",
                                                   "No waypoints found in this area.",
                                                   "이 지역에서 경유지를 못 찾음.");
    public static string NoNavmeshStraightLine => Pick("Kein Wegenetz, führe in Luftlinie.",
                                                       "No navmesh, guiding in a straight line.",
                                                       "길 정보 없음. 직선으로 안내함.");
    public static string ComputingRoute    => Pick("Weg wird berechnet.",
                                                   "Computing route.",
                                                   "경로 계산 중.");
    public static string NewRoute(string direction) => Pick($"Neuer Weg: {direction}.",
                                                            $"New route: {direction}.",
                                                            $"새 경로: {direction}.");
    public static string ComputingRouteTo(string name) => Pick($"Berechne Weg zu {name}.",
                                                               $"Computing route to {name}.",
                                                               $"{name}까지 경로 계산 중.");
    public static string NoNavmeshPlugin   => Pick("Kein Wegenetz. Das Plugin vnavmesh fehlt oder lädt noch.",
                                                   "No navmesh. The vnavmesh plugin is missing or still loading.",
                                                   "길 정보 없음. vnavmesh 플러그인이 없거나 아직 불러오는 중.");
    public static string NewFlagMarker(string distance, string compass) =>
        Pick($"Neue Markierung, {distance}, {compass}.",
             $"New flag, {distance}, {compass}.",
             $"새 표식, {distance}, {compass}.");

    /// <summary>", up"/", down" vertical hint appended to a guide step; "" when level.</summary>
    public static string VerticalUp   => Pick(", aufwärts", ", up", ", 위쪽");
    public static string VerticalDown => Pick(", abwärts", ", down", ", 아래쪽");

    // ── Routen-Vorschau (RouteService.DescribeRoute) ─────────────────
    public static string RoutePracticallyThere(string name) =>
        Pick($"Weg zu {name}: praktisch am Ziel.",
             $"Route to {name}: practically there.",
             $"{name}까지 경로: 거의 다 왔음.");
    public static string RouteHeader(string name, float total) =>
        Pick($"Weg zu {name}, {total:F0} Meter: ",
             $"Route to {name}, {total:F0} meters: ",
             $"{name}까지 경로, {total:F0}미터: ");
    public static string RouteSegment(float distance, string compass) =>
        Pick($"{distance:F0} Meter nach {compass}",
             $"{distance:F0} meters {compass}",
             $"{compass} 방향 {distance:F0}미터");
    public static string RouteThen => Pick(", dann ", ", then ", ", 그다음 ");
    public static string RouteAndOn => Pick(", dann weiter", ", then onward", ", 그다음 계속");

    // ── Datenzentrums-Auswahl (TitleDCWorldMap) ──────────────────────
    public static string DCSelected(string dc, IReadOnlyCollection<string> worlds) =>
        worlds.Count > 0
            ? (Loc.IsKorean
                ? $"{dc} 선택됨. 서버: {string.Join(", ", worlds)}. 확인 버튼을 누르면 확정된다."
                : IsGerman
                ? $"{dc} ausgewählt. Welten: {string.Join(", ", worlds)}. Zum Bestätigen den Ok-Knopf drücken."
                : $"{dc} selected. Worlds: {string.Join(", ", worlds)}. Press the Ok button to confirm.")
            : (Pick($"{dc} ausgewählt.", $"{dc} selected.", $"{dc} 선택됨."));

    // ════════════════════════════════════════════════════════════════
    //  UIReaderService - Fenster-, Listen- und Menue-Ansagen
    //  NOTE: Only the mod's OWN announcement frames are translated here.
    //  Strings that MATCH against the game UI (button labels like "Schließen",
    //  "Bestätigen", journal headers) stay in the game-client language and are
    //  handled by the separate client-language-robustness work, NOT via /acc lang.
    // ════════════════════════════════════════════════════════════════

    // ── Listen / Menue (Social, generische Auswahl) ──────────────────
    /// <summary>List summary: "&lt;selection&gt;, N entries" or "Menu, N entries"
    /// when nothing is selected.</summary>
    public static string ListSummary(string selection, int count) =>
        selection.Length > 0
            ? (Pick($"{selection}, {count} Einträge",
                    $"{selection}, {count} entries",
                    $"{selection}, 항목 {count}개"))
            : (Pick($"Menü, {count} Einträge", $"Menu, {count} entries", $"메뉴, 항목 {count}개"));

    public static string NoEntries       => Pick("Keine Einträge", "No entries", "항목 없음");
    public static string NoEntriesSuffix => Pick(", keine Einträge", ", no entries", ", 항목 없음");

    /// <summary>", N entries" plus optional ": &lt;selection&gt;", appended to a tab line.</summary>
    public static string ListEntriesSuffix(int count, string selection) =>
        Loc.IsKorean ? $", 항목 {count}개{(selection.Length > 0 ? $": {selection}" : string.Empty)}"
        : IsGerman
            ? $", {count} Einträge{(selection.Length > 0 ? $": {selection}" : string.Empty)}"
            : $", {count} entries{(selection.Length > 0 ? $": {selection}" : string.Empty)}";

    public static string SocialTabHeader(string label, int index, int total) =>
        Pick($"{label}, Registerkarte {index} von {total}",
             $"{label}, tab {index} of {total}",
             $"{label}, 탭 {total} 중 {index}");

    public static string OnlineWindowPrefix(string rest) =>
        Pick($"Online-Fenster. {rest}", $"Online window. {rest}", $"온라인 창. {rest}");

    // ── Text-Eingabe-Echo (beim Tippen) ──────────────────────────────
    public static string InputEmpty => Pick("leer", "empty", "비어 있음");
    public static string Deleted(string removed) => Pick($"{removed} gelöscht",
                                                         $"{removed} deleted",
                                                         $"{removed} 지움");

    // ── Benachrichtigung (ActivateNotification) ──────────────────────
    public static string NoOpenNotification => Pick("Keine offene Benachrichtigung.",
                                                    "No open notification.",
                                                    "열린 알림 없음.");
    public static string NotificationNotResponding => Pick("Benachrichtigung reagiert nicht.",
                                                           "Notification not responding.",
                                                           "알림이 반응하지 않음.");

    // ── ContentsTutorial-Popup (Freischaltungen) ─────────────────────
    // NOTE: The actual close-button match ("Schließen") lives in the service and
    // stays in the game-client language (Teil 2), these are the spoken frames.
    public static string PageOf(int current, int total) =>
        Pick($" Seite {current} von {total}.",
             $" Page {current} of {total}.",
             $" 페이지 {total} 중 {current}.");
    public static string EnterCloses    => Pick(" Enter schließt.",
                                                " Press Enter to close.",
                                                " 엔터로 닫는다.");
    public static string EnterPagesOn   => Pick(" Enter blättert weiter.",
                                                " Press Enter to continue.",
                                                " 엔터를 누르면 다음 쪽으로 넘어간다.");
    public static string Closed         => Pick("Geschlossen.", "Closed.", "닫힘.");
    public static string CloseButtonNotResponding => Pick("Schließen-Knopf reagiert nicht.",
                                                          "Close button not responding.",
                                                          "닫기 버튼이 반응하지 않음.");
    public static string NextButtonNotResponding  => Pick("Weiter-Knopf reagiert nicht.",
                                                          "Next button not responding.",
                                                          "다음 버튼이 반응하지 않음.");

    // ── Bestiarium (MonsterNote) ─────────────────────────────────────
    public static string BestiaryNotOpen   => Pick("Bestiarium ist nicht geöffnet.",
                                                   "The bestiary is not open.",
                                                   "토벌수첩이 열려 있지 않음.");
    public static string BestiaryListNotFound => Pick("Bestiarium-Liste nicht gefunden.",
                                                      "Bestiary list not found.",
                                                      "토벌수첩 목록을 못 찾음.");
    public static string NoMonstersInList  => Pick("Keine Monster in dieser Liste.",
                                                   "No monsters in this list.",
                                                   "이 목록에 마물 없음.");
    public static string LivesIn(string habitat) => Pick($", lebt in {habitat}",
                                                         $", lives in {habitat}",
                                                         $", {habitat}에 서식");
    public static string BestiaryOverview(int count, string rows) =>
        Pick($"Bestiarium, {count} Monster. {rows}",
             $"Bestiary, {count} monsters. {rows}",
             $"토벌수첩, 마물 {count}종. {rows}");
    /// <summary>A rank picker row: which rank, and how many of its ten entries are done.</summary>
    public static string BestiaryRankRow(string rank, int done, int total) => done >= total
        ? (Pick($"Rang {rank}, alle {total} Einträge erledigt",
                $"Rank {rank}, all {total} entries complete",
                $"{rank}단계, 항목 {total} 전부 완료"))
        : (Pick($"Rang {rank}, {done} von {total} Einträgen erledigt",
                $"Rank {rank}, {done} of {total} entries done",
                $"{rank}단계, 항목 {total} 중 {done} 완료"));

    // ── Gegenstand abliefern (Request / delivery) ────────────────────
    // "Hand Over" is the EN client's button; verify against an EN dump in Teil 2.
    public static string DeliveryOpen => Pick("Gegenstand abliefern. Drücke Strg F3 für die passenden Gegenstände, dann auswählen und Übergeben.",
                                              "Hand over item. Press Ctrl F3 for the matching items, then select and Hand Over.",
                                              "아이템 건네주기. 컨트롤 F3을 누르면 맞는 아이템이 나온다. 고른 다음 건네주기를 누른다.");
    public static string DeliveryItems(IReadOnlyList<string> items) => items.Count switch
    {
        0 => Pick("Keine passenden Gegenstände im Beutel gefunden.",
                  "No matching items found in your bag.",
                  "소지품에 맞는 아이템 없음."),
        1 => Pick($"Ein passender Gegenstand: {items[0]}. Auswählen und dann Übergeben drücken.",
                  $"One matching item: {items[0]}. Select it, then press Hand Over.",
                  $"맞는 아이템 하나: {items[0]}. 고른 다음 건네주기를 누른다."),
        _ => Loc.IsKorean
                ? $"맞는 아이템 {items.Count}개: {string.Join(", ", items)}. 하나 고른 다음 건네주기를 누른다."
                : IsGerman
                ? $"{items.Count} passende Gegenstände: {string.Join(", ", items)}. Auswählen und dann Übergeben drücken."
                : $"{items.Count} matching items: {string.Join(", ", items)}. Select one, then press Hand Over.",
    };

    // ── Zufaelliges Aussehen (CharaMake RandomLook) ──────────────────
    public static string NoAppearanceWindow => Pick("Kein Aussehen-Fenster offen. Nur im Schritt Aussehen der Charaktererschaffung.",
                                                    "No appearance window open. Only during the Appearance step of character creation.",
                                                    "외모 창이 열려 있지 않음. 캐릭터 생성의 외모 단계에서만 쓸 수 있음.");
    public static string RandomAppearanceNotFound => Pick("Knopf Zufälliges Aussehen nicht gefunden.",
                                                          "Random appearance button not found.",
                                                          "무작위 외모 버튼을 못 찾음.");
    public static string RandomAppearanceNotResponding => Pick("Knopf Zufälliges Aussehen reagiert nicht.",
                                                               "Random appearance button not responding.",
                                                               "무작위 외모 버튼이 반응하지 않음.");
    public static string RandomAppearancePressed => Pick("Zufälliges Aussehen gedrückt.",
                                                         "Random appearance pressed.",
                                                         "무작위 외모 누름.");

    // ── Seitenwechsel / Reiter (generisch) ───────────────────────────
    /// <summary>Konfigurationsseite mit der Anzahl ihrer Einstellungen. Die Zahl
    /// ist die Antwort auf eine echte Frage des Users (2026-08-18): die Seite
    /// meldete sich nur mit ihrer ersten Ueberschrift, die zufaellig genauso
    /// heisst wie die erste Einstellung darunter — "ich frage mich obs noch
    /// andere Menuepunkte ausser Grafik-Voreinstellungen gibt". Ein sehender
    /// Spieler sieht die ganze Seite auf einen Blick; die Zahl ist das
    /// Gegenstueck dazu.</summary>
    public static string ConfigPageWithCount(string heading, int count) =>
        Loc.IsKorean
            ? $"{heading}, 설정 {count}개"
            : IsGerman
                ? $"{heading}, {count} {(count == 1 ? "Einstellung" : "Einstellungen")}"
                : $"{heading}, {count} {(count == 1 ? "setting" : "settings")}";

    public static string TabPressedNoPageChange => Pick("Reiter gedrückt, aber kein Seitenwechsel erkannt.",
                                                        "Tab pressed, but no page change detected.",
                                                        "탭을 눌렀지만 페이지가 바뀌지 않음.");
    public static string TabNotResponding => Pick("Reiter reagiert nicht.",
                                                  "Tab not responding.",
                                                  "탭이 반응하지 않음.");

    // ── Datenzentrum / Gamepad / Uebung / Menue ──────────────────────
    public static string ChooseDataCenter => Pick("Datenzentrum wählen.",
                                                  "Choose a data center.",
                                                  "데이터 센터 선택.");
    public static string GamepadCalibration => Pick("Gamepad-Kalibrierung. Escape zum Schließen.",
                                                    "Gamepad calibration. Press Escape to close.",
                                                    "게임패드 보정. 이스케이프로 닫는다.");
    public static string ExerciseStarted => Pick("Übung gestartet.", "Exercise started.", "훈련 시작됨.");
    public static string BeginButtonNotResponding => Pick("Beginnen-Knopf reagiert nicht.",
                                                          "Begin button not responding.",
                                                          "시작 버튼이 반응하지 않음.");
    public static string NoActiveMenu => Pick("Kein aktives Menü.", "No active menu.", "활성 메뉴 없음.");

    // ── Dump (/acc dump) ─────────────────────────────────────────────
    public static string NoActiveAddonToDump => Pick("Kein aktives Addon für Dump gefunden.",
                                                     "No active addon found to dump.",
                                                     "덤프할 창을 못 찾음.");
    public static string NoAddonName => Pick("Kein Addon-Name. Beispiel: /acc dump TitleDCWorldMap",
                                             "No addon name. Example: /acc dump TitleDCWorldMap",
                                             "애드온 이름 없음. 예: /acc dump TitleDCWorldMap");
    public static string DumpFileError => Pick("Dump nur im Dalamud-Log. Datei-Fehler.",
                                               "Dump only in the Dalamud log. File error.",
                                               "덤프는 Dalamud 로그에만 남음. 파일 저장 실패.");
    public static string UnknownWindowDumped(int count) =>
        Pick($"Kein bekanntes Fenster. {count} sichtbare Fenster gedumpt, Liste im Log.",
             $"No known window. Dumped {count} visible windows, list in the log.",
             $"모드가 아는 창이 아님. 보이는 창 {count}개를 덤프해서 목록을 로그에 적음.");

    // ── Zusammengesetzte Ansagen (UIReader Etappe 2) ─────────────────
    /// <summary>" item: " / " items: " count label for a gathered/read item list.</summary>
    public static string ItemsCountLabel(int count) =>
        Loc.IsKorean ? " 아이템: "
        : IsGerman ? (count == 1 ? " Gegenstand: " : " Gegenstände: ")
                 : (count == 1 ? " item: " : " items: ");

    /// <summary>The word "Level" / "Stufe" - used both standalone and to expand
    /// the game's abbreviated level label.</summary>
    public static string LevelWord => Pick("Stufe", "Level", "레벨");
    public static string LevelSuffix(int level) => Pick($", Stufe {level}",
                                                        $", level {level}",
                                                        $", 레벨 {level}");
    public static string NameWithLevel(string name, int level) =>
        Pick($"{name}, Stufe {level}", $"{name}, level {level}", $"{name}, 레벨 {level}");
    public static string AmountLabel(string yield) => Pick($"Menge {yield}",
                                                           $"Amount {yield}",
                                                           $"수량 {yield}");
    public static string UnknownItem(uint iconId) =>
        Pick($"Unbekannter Gegenstand, Icon {iconId}",
             $"Unknown item, icon {iconId}",
             $"모르는 아이템, 아이콘 {iconId}");

    // ── Konfig-Steuerelemente (Slider / Dropdown / Eingabefeld) ──────
    public static string SliderDesc(string label, string value, int min, int max) =>
        Pick($"{label}, Regler, {value}, von {min} bis {max}.",
             $"{label}, slider, {value}, from {min} to {max}.",
             $"{label}, 조절바, {value}, {min}부터 {max}까지.");
    // Short form for 0..100 percentage sliders (volumes): the "%" already implies
    // the range, so drop "slider" and "from 0 to 100" - the long form got cut off
    // by the next control while navigating quickly (user report 2026-07-27).
    public static string SliderPercent(string label, string value) =>
        Pick($"{label}, {value} %", $"{label}, {value}%", $"{label}, {value} 퍼센트");
    public static string DropdownDesc(string label, string value) =>
        Pick($"{label}, Auswahlliste, {value}.",
             $"{label}, dropdown, {value}.",
             $"{label}, 선택 목록, {value}.");
    /// <summary>One option while stepping through an OPENED drop-down. Names the
    /// stored choice, which a sighted player sees highlighted in the list.</summary>
    public static string DropdownOption(string option, int index, int count, bool selected) =>
        Loc.IsKorean ? $"{option}, {count} 중 {index}{(selected ? ", 선택됨" : "")}"
        : IsGerman
            ? $"{option}, {index} von {count}{(selected ? ", ausgewählt" : "")}"
            : $"{option}, {index} of {count}{(selected ? ", selected" : "")}";
    // ── Zur Wegrichtung drehen (Numpad5) ─────────────────────────────
    /// <summary>Spoken after the player was turned towards the guide point.</summary>
    public static string FaceAligned(string distance) =>
        Pick($"Ausgerichtet. {distance} geradeaus.",
             $"Aligned. {distance} straight ahead.",
             $"길안내 방향으로 돌아섬. 정면으로 {distance}.");

    /// <summary>The key was pressed without a walk guide running.</summary>
    public static string FaceNoRoute =>
        Pick("Kein Weg aktiv. Erst ein Ziel wählen.",
             "No route active. Pick a destination first.",
             "진행 중인 경로 없음. 먼저 목적지를 고른다.");

    /// <summary>Guide point and player are on the same spot - no direction to turn to.</summary>
    public static string FaceAlreadyThere =>
        Pick("Du stehst schon am Wegpunkt.", "You are already at the waypoint.", "이미 경유지에 서 있음.");

    /// <summary>Stand-in when no label text can be found next to a control.</summary>
    public static string NoLabel => Pick("Ohne Beschriftung", "Unlabelled", "이름 없음");

    /// <summary>The browsed history category is a real chat channel, but its
    /// internal number has not been measured yet, so the mod will not switch to
    /// it rather than risk sending into the wrong channel.</summary>
    public static string ChannelNotAvailable(string channel) =>
        Pick($"Kanal {channel} kann noch nicht gesetzt werden.",
             $"Channel {channel} cannot be set yet.",
             $"채널 {channel}, 아직 지정할 수 없음.");

    /// <summary>Browsing the tell history, but no message carries a player the
    /// mod could answer.</summary>
    public static string NoTellPartner =>
        Pick("Kein Flüster-Partner zum Antworten.", "No tell partner to answer.", "답장할 귓속말 상대 없음.");

    /// <summary>The game refused the tell target - said out loud, because a
    /// silent failure would look like the message is on its way.</summary>
    public static string TellTargetFailed(string target) =>
        Pick($"Flüstern an {target} nicht möglich.",
             $"Cannot set tell target {target}.",
             $"{target}에게 귓속말을 걸 수 없음.");
    public static string InputFieldValue(string typed) =>
        typed.Length > 0
            ? (Pick($"Eingabefeld: {typed}", $"Input field: {typed}", $"입력란: {typed}"))
            : (Pick("Eingabefeld, leer", "Input field, empty", "입력란, 비어 있음"));

    // ── Zahl mit ihrer Beschriftung ──────────────────────────────────
    /// <summary>
    /// A number followed by what it counts: "49.457 Gil",
    /// "1.652/10.000 Legionstaler", "350 Errungenschaftspunkte". Used wherever
    /// the game shows a bare figure next to an icon and the word comes from the
    /// game itself (currency rows, the achievement window header).
    ///
    /// The order is the user's decision (2026-08-16), not a default - the name
    /// goes BEHIND the number. Both halves are the game's own words in the
    /// CLIENT language, so this format adds no words of its own and reads the
    /// same in both mod languages.
    /// </summary>
    public static string AmountWithLabel(string amount, string label) => $"{amount} {label}";

    // ── Belohnungs-Zeile (JournalResult) ─────────────────────────────
    // Currency type is only a UI image, so the mod labels amounts by position.
    public static string[] RewardCurrencyLabels =>
        Loc.IsKorean ? new[] { "경험치", "길" }
        : IsGerman ? new[] { "Erfahrung", "Gil" } : new[] { "EXP", "Gil" };
    public static string MoreReward => Pick("weitere Vergütung", "further reward", "추가 보상");
    /// <summary>Prefix spoken in front of the whole quest-completion reward summary.</summary>
    public static string RewardPrefix => Pick("Belohnung: ", "Reward: ", "보상: ");
    /// <summary>A reward item with a quantity: German "&lt;qty&gt; mal &lt;name&gt;",
    /// English just "&lt;qty&gt; &lt;name&gt;" (no "times").</summary>
    public static string RewardItemQuantity(string qty, string name) =>
        Pick($"{qty} mal {name}", $"{qty} {name}", $"{name} {qty}개");
    /// <summary>A reward item followed by its description - name first, then the
    /// description, like the ability tooltips (period so the reader pauses).</summary>
    public static string RewardItemWithDescription(string label, string description) =>
        $"{label}. {description}";

    /// <summary>The tooltip description spoken on its own after the focus has
    /// dwelled on an inventory item (the name was already announced when the
    /// focus landed) - prefixed so the user knows what is being read.</summary>
    public static string ItemDescription(string description) =>
        Pick($"Beschreibung: {description}", $"Description: {description}", $"설명: {description}");

    /// <summary>
    /// What a row in an exchange window costs, appended to the item name. The
    /// currency is already inflected by the caller from the game's own Singular /
    /// Plural sheet columns, so it is inserted verbatim.
    ///
    /// Without a resolved currency only the bare number is spoken: an invented
    /// unit ("2 Marken") would be worse than none, because the same window trades
    /// certificates, seals, tokens and coins.
    /// </summary>
    public static string ShopPrice(uint count, string currency) =>
        currency.Length > 0
            ? (Pick($", für {count} {currency}",
                    $", for {count} {currency}",
                    $", 가격 {count} {currency}"))
            : (Pick($", Preis {count}", $", price {count}", $", 가격 {count}"));

    /// <summary>How many of the currency the player holds, appended after the
    /// price. Only spoken when the number actually changed - see the caller.</summary>
    public static string ShopOwned(int owned) =>
        Pick($", du hast {owned}", $", you have {owned}", $", 보유 화폐 {owned}");

    // ── Inventar-Reiter (Inventory) ──────────────────────────────────
    /// <summary>The active inventory bag tab, announced on switch. The label is
    /// the game's own tab number ("1".."4").</summary>
    public static string InventoryTab(string label) =>
        Pick($"Tasche {label}", $"Bag {label}", $"소지품 {label}");
    /// <summary>Fallback for an inventory tab the game leaves unlabeled - so the
    /// user still hears that focus reached a tab, without inventing a number.</summary>
    public static string InventoryTabOther =>
        Pick("Inventar, weiterer Reiter", "Inventory, other tab", "소지품, 다른 탭");

    // ── Keybind-Zeile (Config) ───────────────────────────────────────
    public static string KeyBindingLine(string label, IReadOnlyList<string> keys) =>
        keys.Count > 0
            ? (Loc.IsKorean ? $"{label}, {string.Join(", ", keys)} 키"
               : IsGerman ? $"{label}, Taste {string.Join(", ", keys)}" : $"{label}, key {string.Join(", ", keys)}")
            : (Pick($"{label}, keine Taste", $"{label}, no key", $"{label}, 키 없음"));

    // ── Anfaenger-Arena (BeginnersMansionProblem) ────────────────────
    // "Beginner's Arena" is the EN content name; verify against an EN dump (Teil 2).
    public static string ArenaTitle => Pick("Anfänger-Arena", "Beginner's Arena", "초보자의 집");
    public static string ArenaExercise(string exercise) =>
        Pick($". Übung: {exercise}", $". Exercise: {exercise}", $". 훈련: {exercise}");
    public static string ArenaEnterBegins => Pick(". Enter beginnt.",
                                                  ". Press Enter to begin.",
                                                  ". 엔터로 시작한다.");

    // ── Benachrichtigung aktivieren ──────────────────────────────────
    public static string Activating(string text) => Pick($"Aktiviere: {text}",
                                                         $"Activating: {text}",
                                                         $"실행: {text}");
    public static string NotificationActivated => Pick("Benachrichtigung aktiviert",
                                                       "Notification activated",
                                                       "알림 수락됨");

    // ════════════════════════════════════════════════════════════════
    //  CombatService / VitalsService - Kampf, Vitalwerte, Level
    // ════════════════════════════════════════════════════════════════
    public static string NotLoggedIn => Pick("Nicht eingeloggt.", "Not logged in.", "접속 안 됨.");
    public static string CombatStart => Pick("Kampf.", "Combat.", "전투.");
    public static string CombatEnd => Pick("Kampf vorbei.", "Combat over.", "전투 끝.");
    public static string AoeWarningOn  => Pick("Flächenwarnung an.",
                                               "Area warning on.",
                                               "범위 경고 켜짐.");
    public static string AoeWarningOff => Pick("Flächenwarnung aus.",
                                               "Area warning off.",
                                               "범위 경고 꺼짐.");

    /// <summary>
    /// Bar fill as a whole percent - the same reading a sighted player takes off
    /// the bar, which is why HP/MP/GP are announced this way and not as raw
    /// numbers (user decision 2026-08-07).
    /// <para>
    /// Floored, so "50 Prozent" never means "a hair under half". The one
    /// exception is the bottom: 5 of 5000 HP floors to 0, and "HP 0 Prozent"
    /// would sound like death - anything above zero therefore reports at
    /// least 1 percent. Zero is reserved for an empty bar.
    /// </para></summary>
    private static int Percent(uint cur, uint max)
    {
        if (max == 0) return 0;
        var percent = (int)(cur * 100u / max);
        return percent == 0 && cur > 0 ? 1 : percent;
    }

    /// <summary>Eigene HP: als ZAHL, weil das Spiel sie als Zahl anzeigt.
    /// Das Ziel behaelt den Prozentwert (<see cref="TargetHpSentence"/>) - dort
    /// zeigt das Spiel nie eine Zahl an.</summary>
    public static string HpSentence(uint cur, uint max) =>
        Pick($"HP: {cur} von {max}.", $"HP: {cur} of {max}.", $"HP: {max} 중 {cur}.");
    public static string TargetHpSentence(uint cur, uint max) =>
        Pick($"Ziel HP: {Percent(cur, max)} Prozent.",
             $"Target HP: {Percent(cur, max)} percent.",
             $"대상 HP: {Percent(cur, max)} 퍼센트.");

    // ── Aktions-Form (ActionShapeService) ───────────────────────────
    //  Der Tooltip nennt die Zahl ("Radius, 5y"), die FORM zeichnet das Spiel nur.
    //  Diese Woerter sind der Text-Ersatz dafuer.

    /// <summary>Kreis. Beim Kreis stimmt das Wort "Radius" des Tooltips.</summary>
    public static string ShapeCircle => Pick("Kreis", "circle", "원");

    /// <summary>Kegel, dessen Telegraph-Grafik keinen Winkel nennt. Sagt KEINEN
    /// Winkel statt eines geratenen.</summary>
    public static string ShapeCone => Pick("Kegel", "cone", "부채꼴");

    /// <summary>Kegel mit dem vollen Winkel aus dem Grafiknamen (gl_fan090 = 90).</summary>
    public static string ShapeConeWithAngle(float fullAngleDeg) =>
        Pick($"Kegel, {fullAngleDeg:0.#} Grad",
             $"cone, {fullAngleDeg:0.#} degrees",
             $"부채꼴, {fullAngleDeg:0.#}도");

    /// <summary>Linie beziehungsweise Rechteck. Die halbe Breite (XAxisModifier)
    /// ist unbestaetigt und wird deshalb nicht gesprochen.</summary>
    public static string ShapeLine => Pick("Linie", "line", "직선");

    /// <summary>Wie die Form an die Tooltip-Ansage angehaengt wird.</summary>
    public static string ShapeSuffix(string shape) =>
        Pick($"Form: {shape}", $"Shape: {shape}", $"모양: {shape}");

    /// <summary>", HP X percent" fragment appended to a target announcement.</summary>
    public static string TargetHpFragment(uint cur, uint max) =>
        Pick($", HP {Percent(cur, max)} Prozent",
             $", HP {Percent(cur, max)} percent",
             $", HP {Percent(cur, max)} 퍼센트");

    /// <summary>", Stufe 12, HP 40 Prozent" - der Anhang fuer eine Zielansage.
    /// Die Stufe kommt aus ICharacter.Level, die HP bleiben prozentual.
    /// Fehlt eines von beidem, faellt genau dieser Teil weg.</summary>
    public static string TargetLevelHpFragment(byte level, uint cur, uint max)
    {
        var lvl = level > 0
            ? (Pick($", Stufe {level}", $", level {level}", $", 레벨 {level}"))
            : string.Empty;
        return max > 0 ? lvl + TargetHpFragment(cur, max) : lvl;
    }

    /// <summary>HP als Zahl, MP als Prozentwert und nur wenn die Klasse Mana hat.
    /// MP bleibt prozentual, weil MaxMp seit Patch 5.0 fuer JEDE Klasse auf JEDEM
    /// Level 10000 ist - "MP 10000 von 10000" ist keine Zahl, die ein Spieler
    /// irgendwo sieht; das Partyfenster zeichnet daraus "100.00%".</summary>
    public static string VitalStatus(uint hp, uint hpMax, uint mp, uint mpMax, bool hasMp) =>
        hasMp
            ? (Pick($"HP {hp} von {hpMax}, MP {Percent(mp, mpMax)} Prozent.",
                    $"HP {hp} of {hpMax}, MP {Percent(mp, mpMax)} percent.",
                    $"HP {hpMax} 중 {hp}, MP {Percent(mp, mpMax)} 퍼센트."))
            : (Pick($"HP {hp} von {hpMax}.", $"HP {hp} of {hpMax}.", $"HP {hpMax} 중 {hp}."));

    /// <summary>" &lt;name&gt;, HP X percent." target clause appended to the status readout.</summary>
    public static string TargetStatusClause(string name, uint cur, uint max) =>
        Pick($" {name}, HP {Percent(cur, max)} Prozent.",
             $" {name}, HP {Percent(cur, max)} percent.",
             $" {name}, HP {Percent(cur, max)} 퍼센트.");

    public static string TargetFallbackName => Pick("Ziel", "Target", "대상");

    // GP (Sammelpunkte) - the DE client says "SP", the EN client "GP".
    public static string NoGatheringPoints => Pick("Keine Sammelpunkte. SP gibt es nur als Sammler.",
                                                   "No gathering points. GP only exists for gatherers.",
                                                   "지금 직업에는 GP가 없음. GP는 채집가에게만 있음.");
    public static string GpValue(uint cur, uint max) =>
        Pick($"SP {Percent(cur, max)} Prozent.",
             $"GP {Percent(cur, max)} percent.",
             $"GP {Percent(cur, max)} 퍼센트.");

    public static string EnemyCasts(string action) => Pick($"Gegner wirkt {action}.",
                                                           $"Enemy casts {action}.",
                                                           $"적이 {action} 시전.");

    /// <summary>Cast warning naming the caster - used when the casting enemy is
    /// NOT the player's current target, so it is clear the danger comes from
    /// somewhere else.</summary>
    public static string NamedEnemyCasts(string enemy, string action) =>
        Pick($"{enemy} wirkt {action}.", $"{enemy} casts {action}.", $"{enemy}, {action} 시전.");
    public static string AnAbility => Pick("eine Fähigkeit", "an ability", "기술");

    // ── Level / Erfahrung ────────────────────────────────────────────
    public static string LevelReached(int level) => Pick($"Stufe {level} erreicht.",
                                                         $"Reached level {level}.",
                                                         $"레벨 {level} 달성.");
    public static string LevelNotAvailable => Pick("Stufe nicht verfügbar.",
                                                   "Level not available.",
                                                   "레벨을 알 수 없음.");
    public static string LevelMax(int level) => Pick($"Stufe {level}, Maximalstufe erreicht.",
                                                     $"Level {level}, maximum level reached.",
                                                     $"레벨 {level}, 최고 레벨 도달.");
    public static string LevelExpLeft(int level, int left) =>
        Pick($"Stufe {level}. Noch {left} Erfahrungspunkte bis zur nächsten Stufe.",
             $"Level {level}. {left} experience points to the next level.",
             $"레벨 {level}. 다음 레벨까지 경험치 {left}.");
    // Live-Ansage bei jedem XP-Gewinn (kurz gehalten, laeuft im Kampf oft).
    public static string XpGained(int amount) =>
        Pick($"{amount} Erfahrung.", $"{amount} experience.", $"경험치 {amount}.");

    // Ruhebereich (Sichelmond an der EP-Leiste): dort sammelt sich der
    // Erholungsbonus an, auch offline.
    public static string RestedAreaEntered =>
        Pick("Ruhebereich. Erholungsbonus sammelt sich.",
             "Rested area. Rested bonus is accumulating.",
             "휴식 지역. 휴식 보너스 쌓이는 중.");
    public static string RestedAreaLeft =>
        Pick("Ruhebereich verlassen.", "Left the rested area.", "휴식 지역을 벗어남.");
    // Zusatz zur Stufen-Ansage (Strg+L). Fuehrendes Leerzeichen, weil die Teile
    // an den Stufen-Satz angehaengt werden.
    public static string RestedAreaNow =>
        Pick(" Im Ruhebereich.", " In a rested area.", " 휴식 지역 안.");
    public static string RestedAreaNot =>
        Pick(" Kein Ruhebereich.", " Not in a rested area.", " 휴식 지역 아님.");
    public static string RestedBonusPercent(int percent) =>
        Pick($" Erholungsbonus für {percent} Prozent einer Stufe.",
             $" Rested bonus for {percent} percent of a level.",
             $" 휴식 보너스, 한 레벨의 {percent} 퍼센트.");
    public static string RestedBonusEmpty =>
        Pick(" Kein Erholungsbonus.", " No rested bonus.", " 휴식 보너스 없음.");
    public static string RestedNotAvailable =>
        Pick("Erholungsbonus nicht verfügbar.", "Rested bonus not available.", "휴식 보너스를 알 수 없음.");

    // ── Ausruestungsset-Markierung ───────────────────────────────────
    // Das Symbol, das dem sehenden Spieler sagt "steckt in einem gespeicherten
    // Set" - also NICHT verkaufen. Wortwahl wie im Spiel (Addon 756/11993).
    // Kurzform fuer Listen, in denen sie hinter jedem Gegenstand stehen kann.
    public static string InGearsetShort =>
        Pick(", im Ausrüstungsset", ", in a gear set", ", 장비세트에 있음");
    // Welche EIGENEN Klassen das Teil tragen koennen - die Frage vorm Verkaufen.
    // Ein- und Mehrzahl getrennt, sonst stolpert die Ansage bei einer Klasse.
    public static string ForYourClasses(string classes, int count) =>
        Loc.IsKorean ? $", 내 클래스: {classes}"
        : IsGerman
            ? (count == 1 ? $", für deine Klasse {classes}" : $", für deine Klassen {classes}")
            : (count == 1 ? $", for your {classes}" : $", for your classes {classes}");
    // Langform fuer den einzelnen Gegenstand, wo Platz fuer die Warnung ist.
    public static string InGearsetWarning =>
        Pick(" Achtung: in einem Ausrüstungsset gespeichert, nicht verkaufen.",
             " Careful: saved in a gear set, do not sell.",
             " 주의: 장비세트에 저장됨. 팔지 마라.");

    // ════════════════════════════════════════════════════════════════
    //  EquipmentService - Ausruestung
    // ════════════════════════════════════════════════════════════════
    public static string HighQuality => Pick(" Hoch-Qualität", " high quality", " 고품질");
    public static string NoEquipmentWorn => Pick("Keine Ausrüstung angelegt.",
                                                 "No equipment worn.",
                                                 "착용한 장비 없음.");
    public static string SlotsFree(int empty) => Pick($" {empty} Plätze frei.",
                                                      $" {empty} slots free.",
                                                      $" 빈 칸 {empty}개.");
    public static string EquipmentList(string parts, string emptyNote) =>
        Pick($"Ausrüstung: {parts}.{emptyNote}",
             $"Equipment: {parts}.{emptyNote}",
             $"장비: {parts}.{emptyNote}");
    public static string ItemFallback(uint id) => Pick($"Gegenstand {id}",
                                                       $"Item {id}",
                                                       $"아이템 {id}");

    public static string EquipChangeInProgress => Pick("Ausrüstungswechsel läuft schon.",
                                                       "Equipment change already in progress.",
                                                       "장비 변경이 이미 진행 중.");
    public static string EquipModuleUnavailable => Pick("Ausrüstungsmodul nicht verfügbar.",
                                                        "Equipment module not available.",
                                                        "장비 모듈을 쓸 수 없음.");
    public static string ApplyingRecommendedGear => Pick("Lege empfohlene Ausrüstung an.",
                                                         "Applying recommended equipment.",
                                                         "추천 장비 착용 중.");
    public static string EquipChangeFailed => Pick("Ausrüstungswechsel fehlgeschlagen.",
                                                   "Equipment change failed.",
                                                   "장비 변경 실패.");
    public static string EquipChangeDidntWork => Pick("Ausrüstungswechsel hat nicht geklappt.",
                                                      "Equipment change did not work.",
                                                      "장비 변경이 되지 않음.");
    public static string EquipResult(int changed) =>
        changed > 0
            ? (Pick($"Empfohlene Ausrüstung angelegt, {changed} Teile gewechselt.",
                    $"Recommended equipment applied, {changed} pieces changed.",
                    $"추천 장비 착용됨. {changed}개 바뀜."))
            : (Pick("Ausrüstung unverändert. Entweder schon optimal, oder Wechsel gerade nicht möglich.",
                    "Equipment unchanged. Either already optimal, or a change is not possible right now.",
                    "장비 그대로. 이미 최적이거나 지금은 바꿀 수 없음."));

    /// <summary>Spoken equipment-slot label (mod wording, not the game's).</summary>
    public static string SlotEquipment  => Pick("Ausrüstung", "Equipment", "장비");
    public static string SlotWeapon     => Pick("Waffe", "Weapon", "주 무기");
    public static string SlotOffHand    => Pick("Nebenhand", "Off hand", "보조 무기");
    public static string SlotHead       => Pick("Kopf", "Head", "머리");
    public static string SlotBody       => Pick("Rumpf", "Body", "몸통");
    public static string SlotHands      => Pick("Hände", "Hands", "손");
    public static string SlotWaist      => Pick("Gürtel", "Waist", "허리");
    public static string SlotLegs       => Pick("Beine", "Legs", "다리");
    public static string SlotFeet       => Pick("Füße", "Feet", "발");
    public static string SlotEars       => Pick("Ohren", "Ears", "귀");
    public static string SlotNeck       => Pick("Hals", "Neck", "목");
    public static string SlotWrists     => Pick("Handgelenke", "Wrists", "손목");
    public static string SlotRing       => Pick("Ring", "Ring", "반지");
    public static string SlotSoulCrystal=> Pick("Jobkristall", "Soul Crystal", "소울 크리스탈");

    // ════════════════════════════════════════════════════════════════
    //  GearInfoService - Stufe & Tragbarkeit
    // ════════════════════════════════════════════════════════════════
    public static string GearLevel(uint level) => Pick($"Stufe {level}",
                                                       $"Level {level}",
                                                       $"레벨 {level}");
    public static string Wearable(string level) => Pick($"{level}, tragbar",
                                                        $"{level}, wearable",
                                                        $"{level}, 착용 가능");
    public static string NotWearable(string level, string reason) =>
        Pick($"{level}, nicht tragbar, {reason}",
             $"{level}, not wearable, {reason}",
             $"{level}, 착용 불가, {reason}");
    public static string FromLevel(uint level) => Pick($"ab Stufe {level}",
                                                       $"from level {level}",
                                                       $"레벨 {level}부터");
    public static string OnlyForClass(string forWho) => Pick($"nur für {forWho}",
                                                             $"only for {forWho}",
                                                             $"{forWho} 전용");
    public static string DifferentClassNeeded => Pick("andere Klasse nötig",
                                                      "different class required",
                                                      "다른 클래스가 필요");
    public static string NotForYourRace => Pick("nicht für dein Volk",
                                                "not for your race",
                                                "내 종족용이 아님");

    // ── Werte eines Ausrüstungsteils (zum Vergleichen) ──
    // Die Attributnamen selbst kommen aus dem BaseParam-Sheet in Spielsprache
    // und werden NICHT hier übersetzt - sie werden gelesen, nicht erfunden.
    public static string ItemLevelValue(uint level) =>
        Pick($"Gegenstandsstufe {level}", $"item level {level}", $"아이템 레벨 {level}");
    public static string DefensePhysValue(int v) =>
        Pick($"Verteidigung {v}", $"defence {v}", $"물리 방어력 {v}");
    public static string DefenseMagValue(int v) =>
        Pick($"Magieabwehr {v}", $"magic defence {v}", $"마법 방어력 {v}");
    public static string DamagePhysValue(int v) =>
        Pick($"Angriff {v}", $"physical damage {v}", $"물리 공격력 {v}");
    public static string DamageMagValue(int v) =>
        Pick($"Magieschaden {v}", $"magic damage {v}", $"마법 공격력 {v}");
    /// <summary>Weapon delay, given in seconds (the game stores milliseconds).</summary>
    public static string DelayValue(double seconds) =>
        Pick($"Verzögerung {seconds:0.0} Sekunden",
             $"delay {seconds:0.0} seconds",
             $"공격 주기 {seconds:0.0}초");
    /// <summary>One attribute bonus, e.g. "Stärke plus 4" - name from the sheet.</summary>
    public static string AttributeValue(string name, int v) =>
        Loc.IsKorean ? $"{name} {(v < 0 ? "마이너스" : "플러스")} {Math.Abs(v)}"
        : IsGerman
            ? $"{name} {(v < 0 ? "minus" : "plus")} {Math.Abs(v)}"
            : $"{name} {(v < 0 ? "minus" : "plus")} {Math.Abs(v)}";
    public static string MateriaSlots(int n) =>
        Loc.IsKorean ? $"마테리아 {n}칸"
        : IsGerman
        ? (n == 1 ? "1 Materia-Slot" : $"{n} Materia-Slots")
        : (n == 1 ? "1 materia slot" : $"{n} materia slots");

    // ════════════════════════════════════════════════════════════════
    //  Plugin.cs - Start, Koordinaten-Lauf, Himmelsrichtung, Hilfe
    // ════════════════════════════════════════════════════════════════
    /// <summary>Startup greeting. <paramref name="version"/> is the raw "5.58"
    /// string; the dots are spoken out per language so the screen reader does
    /// not run the digits together.</summary>
    public static string VersionReady(string version) =>
        Loc.IsKorean ? $"FF14 Accessibility 버전 {version.Replace(".", " 점 ")} 준비됨."
        : IsGerman
            ? $"FF14 Accessibility Version {version.Replace(".", " Punkt ")} bereit."
            : $"FF14 Accessibility version {version.Replace(".", " point ")} ready.";

    // Koordinaten-Lauf (Goto/Copy clipboard coords)
    public static string ClipboardUnreadable =>
        Pick("Zwischenablage konnte nicht gelesen werden.",
             "Could not read the clipboard.",
             "클립보드를 읽지 못함.");
    public static string NoCoordsInClipboard =>
        Pick("Keine Koordinaten in der Zwischenablage gefunden. Erst die Zahlen kopieren, dann die Taste drücken.",
             "No coordinates found on the clipboard. Copy the numbers first, then press the key.",
             "클립보드에 좌표가 없음. 먼저 숫자를 복사한 다음 키를 누른다.");
    public static string MapUnknownConvert =>
        Pick("Aktuelle Karte unbekannt, kann nicht umrechnen.",
             "Current map unknown, cannot convert.",
             "현재 지도를 알 수 없어 변환할 수 없음.");
    /// <summary>Walk-target name for a clipboard coordinate (feeds the later
    /// "walking to / arrived at &lt;name&gt;" announcements).</summary>
    public static string CoordsName(float mapX, float mapY) =>
        Pick($"Koordinaten {mapX:0.0}, {mapY:0.0}",
             $"Coordinates {mapX:0.0}, {mapY:0.0}",
             $"좌표 {mapX:0.0}, {mapY:0.0}");
    public static string WalkingToCoords(float mapX, float mapY) =>
        Pick($"Laufe zu Koordinaten {mapX:0.0}, {mapY:0.0}.",
             $"Walking to coordinates {mapX:0.0}, {mapY:0.0}.",
             $"{mapX:0.0}, {mapY:0.0} 좌표로 이동 중.");
    public static string PositionUnknown =>
        Pick("Position unbekannt.", "Position unknown.", "위치를 알 수 없음.");
    public static string MapUnknownCoords =>
        Pick("Aktuelle Karte unbekannt, kann Koordinaten nicht bestimmen.",
             "Current map unknown, cannot determine coordinates.",
             "현재 지도를 알 수 없어 좌표를 정할 수 없음.");
    public static string ClipboardNotWritable =>
        Pick("Zwischenablage konnte nicht beschrieben werden.",
             "Could not write to the clipboard.",
             "클립보드에 쓰지 못함.");
    public static string CoordsCopied(float mapX, float mapY) =>
        Pick($"Koordinaten {mapX:0.0}, {mapY:0.0} kopiert.",
             $"Coordinates {mapX:0.0}, {mapY:0.0} copied.",
             $"좌표 {mapX:0.0}, {mapY:0.0} 복사됨.");

    // Gathering walk-to (shared by /acc gathergo and GatheringService)
    public static string NoGatheringSpotsJob =>
        Pick("Keine Sammelstellen für deinen Beruf in dieser Zone.",
             "No gathering spots for your job in this area.",
             "이 지역에 지금 직업으로 채집할 곳 없음.");
    public static string GatheringSpotName(int level) =>
        Pick($"Sammelstelle, Stufe {level}", $"Gathering spot, level {level}", $"채집 지점, 레벨 {level}");

    // Himmelsrichtung (compass heading toggle)
    public static string HeadingOn(string direction) =>
        direction.Length > 0
            ? (Pick($"Himmelsrichtung an. {direction}.",
                    $"Compass heading on. {direction}.",
                    $"방향 안내 켜짐. {direction}."))
            : (Pick("Himmelsrichtung an.", "Compass heading on.", "방향 안내 켜짐."));
    public static string HeadingOff =>
        Pick("Himmelsrichtung aus.", "Compass heading off.", "방향 안내 꺼짐.");

    /// <summary>Spoken at the start of "/acc soundtest" (audition the cue sounds).</summary>
    public static string SoundTestRunning =>
        Pick("Klangtest: Navigations-Ton von vorn, rechts, hinten, dann Wegpunkt und Ankunft, dann HP- und Mana-Töne.",
             "Sound test: navigation tone from ahead, right, behind, then waypoint and arrival, then HP and mana tones.",
             "소리 확인. 길안내 알림음이 앞, 오른쪽, 뒤에서 차례로 난다. 그다음 경유지 알림음과 도착 알림음, 마지막으로 HP 알림음과 MP 알림음이 난다.");

    // Labels spoken before each HP/MP tone in the sound test, so the audition is
    // self-explaining.
    public static string SoundTestHpHeal    => Pick("HP, Heilung", "HP, healing", "HP, 회복");
    public static string SoundTestHpDamage  => Pick("HP, Schaden", "HP, damage", "HP, 피해");
    public static string SoundTestHpCritical=> Pick("HP, kritisch", "HP, critical", "HP, 위험");
    public static string SoundTestMpGain    => Pick("Mana, Aufladung", "Mana, restored", "MP, 회복");
    public static string SoundTestMpSpend   => Pick("Mana, Verbrauch", "Mana, spent", "MP, 소모");

    // Quest-/Marker-Ziel nicht auflösbar
    public static string QuestInAnotherZoneNoHop(string quest) =>
        Pick($"{quest} ist in einem anderen Gebiet und ich finde keinen Übergang dorthin.",
             $"{quest} is in another area and I can't find a transition there.",
             $"{quest}, 다른 지역에 있고 그리로 가는 통로를 못 찾음.");
    public static string NoWalkablePointAt(string name) =>
        Pick($"Kein begehbarer Punkt am {name} gefunden.",
             $"No walkable point found at {name}.",
             $"{name}에서 걸어갈 수 있는 지점을 못 찾음.");
    public static string NoWalkablePointNear(string name) =>
        Pick($"Kein begehbarer Punkt bei {name} gefunden.",
             $"No walkable point found near {name}.",
             $"{name} 근처에서 걸어갈 수 있는 지점을 못 찾음.");

    // Bestiarium: nächstes lebendes Exemplar / Lebensraum
    public static string NoMonsterNearby(string monster) =>
        Pick($"Kein {monster} in der Nähe.", $"No {monster} nearby.", $"근처에 {monster} 없음.");
    public static string NoMonsterNearbyHabitat(string monster, string habitat) =>
        Pick($"Kein {monster} in der Nähe. Lebt in {habitat}.",
             $"No {monster} nearby. Lives in {habitat}.",
             $"근처에 {monster} 없음. {habitat}에 서식.");

    /// <summary>Standalone "not targeted" warning (Bestiary walk); the leading-space
    /// variant is <see cref="NotTargetedSuffix"/>.</summary>
    public static string NotTargetedWarning =>
        Pick("Achtung, nicht anvisiert.", "Warning, not targeted.", "주의, 대상 지정 안 됨.");

    /// <summary>The full "/acc help" readout: every plugin hotkey and command.
    /// Keys are the current defaults (Page keys, Numpad 3, Plus - kept in sync
    /// with <see cref="Configuration"/>).</summary>
    public static string HelpFull => Loc.IsKorean
        ? "단축키: " +
          "페이지 다운, 다음 사물을 말하고 대상으로 지정. " +
          "페이지 업, 이전 사물. " +
          "컨트롤 페이지 다운, 다음 분류. " +
          "컨트롤 페이지 업, 이전 분류. " +
          "컨트롤 숫자패드 3, 길안내 켜기 또는 끄기. 길 정보를 따라 장애물을 돌아간다. " +
          "숫자패드 3, 목적지까지 자동으로 이동. " +
          "플러스, 지정한 대상 따라가기 켜기 또는 끄기. " +
          "컨트롤 숫자패드 5, 걷지 않고 목적지까지의 경로만 말하기. " +
          "F, 목적지 쪽으로 돌기. W, 앞으로 이동. " +
          "컨트롤 F1, 이 도움말. " +
          "컨트롤 F2, 활성 창. " +
          "컨트롤 F10, 현재 메뉴 읽기. " +
          "컨트롤 F11, 말하기 멈춤. " +
          "컨트롤 딜리트, HP와 MP 말하기. " +
          "컨트롤 F9, 고른 단축바 읽기. " +
          "컨트롤 F6, 착용한 장비 읽기. " +
          "컨트롤 F7, 추천 장비 착용. " +
          "컨트롤 F8, 캐릭터 생성에서 무작위 외모. " +
          "컨트롤 숫자패드 0, 기술 메뉴 열기. 숫자패드 8과 2로 넘기고, 숫자패드 0으로 고르고, 숫자패드 마침표로 돌아간다. " +
          "컨트롤 시프트 F6, 발자취 기록 켜기 또는 끄기. 길 정보가 모르는 구간을 한 번 직접 걸어간다. " +
          "명령어: " +
          "/acc nav, 목적지 방향. " +
          "/acc set, 지금 지정한 대상을 추적. " +
          "/acc clear, 대상 해제. " +
          "/acc near, 근처 사물. " +
          "/acc status, HP와 MP 말하기. " +
          "/acc ui, 현재 메뉴 읽기. " +
          "/acc win, 활성 창 말하기. " +
          "/acc keys, 게임 단축키를 바탕 화면에 저장. " +
          "/acc cooldowns, 기술 준비됨 안내 켜기 또는 끄기. " +
          "/acc trails, 이 지역에 기록된 발자취 목록. " +
          "/acc trail del과 번호, 발자취 하나 지우기. " +
          "/acc stop, 말하기 멈춤."
        : IsGerman
        ? "Tasten: " +
          "Bild ab, nächstes Objekt ansagen und anvisieren. " +
          "Bild auf, vorheriges Objekt. " +
          "Strg+Bild ab, Kategorie vorwärts. " +
          "Strg+Bild auf, Kategorie zurück. " +
          "Strg+Nummernblock 3, Gehhilfe an oder aus, folgt dem Wegenetz um Hindernisse. " +
          "Nummernblock 3, automatisch zum Ziel laufen. " +
          "Plus, dem anvisierten Ziel folgen an oder aus. " +
          "Strg+Nummernblock 5, Weg zum Ziel ansagen ohne zu laufen. " +
          "F, zum Ziel hindrehen. W, laufen. " +
          "Strg+F1, diese Hilfe. " +
          "Strg+F2, aktives Fenster. " +
          "Strg+F10, Menü vorlesen. " +
          "Strg+F11, Sprache stoppen. " +
          "Strg+Entfernen, HP und MP ansagen. " +
          "Strg+F9, gewählte Aktionsleiste vorlesen. " +
          "Strg+F6, angelegte Ausrüstung vorlesen. " +
          "Strg+F7, empfohlene Ausrüstung anlegen. " +
          "Strg+F8, zufälliges Aussehen in der Charaktererschaffung. " +
          "Strg+Nummernblock 0, Skill-Menü öffnen: Nummernblock 8 und 2 blättern, Nummernblock 0 wählt, Nummernblock Komma zurück. " +
          "Strg+Umschalt+F6, Spur aufzeichnen an oder aus: eine Stelle, die das Wegenetz nicht kennt, einmal selbst ablaufen. " +
          "Befehle: " +
          "/acc nav, Richtung zum Ziel. " +
          "/acc set, Aktuelles Ziel verfolgen. " +
          "/acc clear, Ziel aufheben. " +
          "/acc near, Objekte in der Nähe. " +
          "/acc status, HP und MP ansagen. " +
          "/acc ui, Menü vorlesen. " +
          "/acc win, Aktives Fenster ansagen. " +
          "/acc keys, Spiel-Tastenbelegung auf den Desktop speichern. " +
          "/acc cooldowns, Fähigkeit-bereit-Ansage an oder aus. " +
          "/acc trails, aufgezeichnete Spuren in diesem Gebiet auflisten. " +
          "/acc trail del und die Nummer, eine Spur löschen. " +
          "/acc stop, Sprache stoppen."
        : "Keys: " +
          "Page Down, announce and target the next object. " +
          "Page Up, previous object. " +
          "Ctrl+Page Down, next category. " +
          "Ctrl+Page Up, previous category. " +
          "Ctrl+Numpad 3, walk guide on or off, follows the navmesh around obstacles. " +
          "Numpad 3, walk to the target automatically. " +
          "Plus, follow the current target on or off. " +
          "Ctrl+Numpad 5, describe the route to the target without walking. " +
          "F, turn toward the target. W, move forward. " +
          "Ctrl+F1, this help. " +
          "Ctrl+F2, active window. " +
          "Ctrl+F10, read the current menu. " +
          "Ctrl+F11, stop speech. " +
          "Ctrl+Delete, announce HP and MP. " +
          "Ctrl+F9, read the selected hotbar. " +
          "Ctrl+F6, read worn equipment. " +
          "Ctrl+F7, apply recommended equipment. " +
          "Ctrl+F8, random appearance in character creation. " +
          "Ctrl+Numpad 0, open the skill menu: Numpad 8 and 2 to browse, Numpad 0 selects, Numpad decimal to go back. " +
          "Ctrl+Shift+F6, record a trail on or off: walk a stretch the navmesh does not know once yourself. " +
          "Commands: " +
          "/acc nav, direction to the target. " +
          "/acc set, track the current target. " +
          "/acc clear, clear the target. " +
          "/acc near, nearby objects. " +
          "/acc status, announce HP and MP. " +
          "/acc ui, read the current menu. " +
          "/acc win, announce the active window. " +
          "/acc keys, save the game's key bindings to the desktop. " +
          "/acc cooldowns, ability-ready announcements on or off. " +
          "/acc trails, list the trails recorded in this area. " +
          "/acc trail del and the number, delete a trail. " +
          "/acc stop, stop speech.";

    // ════════════════════════════════════════════════════════════════
    //  AutoWalkService - Auto-Lauf, Ziel folgen, Wegenetz-Aufbau
    // ════════════════════════════════════════════════════════════════
    public static string FollowNoTarget =>
        Pick("Kein Ziel zum Folgen. Erst ein Ziel anwählen.",
             "No target to follow. Select a target first.",
             "따라갈 대상 없음. 먼저 대상을 지정한다.");
    public static string FollowSelf =>
        Pick("Das bist du selbst.", "That is you.", "대상이 나 자신이라 따라갈 수 없음.");
    public static string Following(string name) =>
        Pick($"Folge {name}.", $"Following {name}.", $"{name} 따라가는 중.");
    public static string FollowStopped =>
        Pick("Folgen beendet.", "Follow stopped.", "따라가기 끝냄.");
    public static string FollowStoppedZone =>
        Pick("Folgen beendet, Gebiet gewechselt.",
             "Follow stopped, zone changed.",
             "지역이 바뀌어 따라가기 끝냄.");
    public static string FollowTargetGone(string name) =>
        Pick($"{name} ist weg. Folgen beendet.",
             $"{name} is gone. Follow stopped.",
             $"{name} 사라짐. 따라가기 끝냄.");
    public static string FollowAbortedNoResponse =>
        Pick("Folgen abgebrochen, vnavmesh antwortet nicht.",
             "Follow aborted, vnavmesh not responding.",
             "따라가기 중단. vnavmesh가 응답하지 않음.");
    public static string FollowAbortedUnavailable =>
        Pick("Folgen abgebrochen, vnavmesh nicht verfügbar.",
             "Follow aborted, vnavmesh not available.",
             "따라가기 중단. vnavmesh를 쓸 수 없음.");

    public static string MeshLoading =>
        Pick("Wegenetz wird geladen.", "Loading navmesh.", "길 정보 불러오는 중.");
    public static string MeshPercent(int percent) =>
        Pick($"Wegenetz {percent} Prozent.", $"Navmesh {percent} percent.", $"길 정보 {percent} 퍼센트.");
    public static string MeshReady =>
        Pick("Wegenetz fertig geladen.", "Navmesh loaded.", "길 정보 준비 완료.");
    public static string MeshAborted =>
        Pick("Wegenetz-Aufbau abgebrochen.", "Navmesh build aborted.", "길 정보 생성 중단됨.");
    public static string MeshStillLoading(float percent) =>
        Pick($"Wegenetz lädt noch, {percent:F0} Prozent. Gleich nochmal versuchen.",
             $"Navmesh still loading, {percent:F0} percent. Try again shortly.",
             $"길 정보 불러오는 중, {percent:F0} 퍼센트. 잠시 뒤 다시 해라.");
    public static string MeshNotReady =>
        Pick("Wegenetz ist noch nicht bereit. Gleich nochmal versuchen.",
             "Navmesh is not ready yet. Try again shortly.",
             "길 정보가 아직 준비 안 됨. 잠시 뒤 다시 해라.");
    public static string PathfindBusy =>
        Pick("Wegfindung läuft schon. Gleich nochmal versuchen.",
             "Pathfinding is already running. Try again shortly.",
             "경로 찾기가 이미 진행 중. 잠시 뒤 다시 해라.");
    public static string AutoWalkUnavailable =>
        Pick("Auto-Lauf nicht verfügbar. Das Plugin vnavmesh fehlt oder ist nicht geladen.",
             "Auto-walk not available. The vnavmesh plugin is missing or not loaded.",
             "자동 이동을 쓸 수 없음. vnavmesh 플러그인이 없거나 불러오지 못함.");

    public static string WalkingTo(string name) =>
        Pick($"Laufe zu {name}.", $"Walking to {name}.", $"{name}까지 이동 중.");
    public static string AutoWalkStopped =>
        Pick("Auto-Lauf gestoppt.", "Auto-walk stopped.", "자동 이동 멈춤.");
    public static string ArrivedNewZone =>
        Pick("Angekommen, neues Gebiet erreicht.", "Arrived, reached a new area.", "도착. 새 지역에 들어옴.");
    public static string AutoWalkAbortedNoResponse =>
        Pick("Auto-Lauf abgebrochen, vnavmesh antwortet nicht.",
             "Auto-walk aborted, vnavmesh not responding.",
             "자동 이동 중단. vnavmesh가 응답하지 않음.");

    /// <summary>Distance-remaining fragment: metres, or an "unknown" phrase for NaN.</summary>
    public static string MetersRemaining(float distance) =>
        float.IsNaN(distance)
            ? (Pick("Ziel unbekannt", "target unknown", "목적지 모름"))
            : (Pick($"{distance:F0} Meter", $"{distance:F0} meters", $"{distance:F0}미터"));
    public static string StillToGo(float distance) =>
        Pick($"Noch {MetersRemaining(distance)}.",
             $"{MetersRemaining(distance)} remaining.",
             $"{MetersRemaining(distance)} 남음.");
    public static string AutoWalkEndedRemaining(float distance) =>
        Pick($"Auto-Lauf beendet, noch {MetersRemaining(distance)}.",
             $"Auto-walk ended, {MetersRemaining(distance)} remaining.",
             $"자동 이동 끝. {MetersRemaining(distance)} 남음.");
    public static string StuckRemaining(float distance) =>
        Pick($"Ich stecke fest, noch {MetersRemaining(distance)}. Auto-Lauf beendet.",
             $"I'm stuck, {MetersRemaining(distance)} remaining. Auto-walk ended.",
             $"길이 막혀 더 못 감. 목적지까지 {MetersRemaining(distance)} 남기고 자동 이동 끝.");
    public static string NoPathTo(string name, string hint) =>
        Pick($"Kein Weg zu {name} gefunden.{hint}",
             $"No path to {name} found.{hint}",
             $"{name}까지 가는 길을 못 찾음.{hint}");
    /// <summary>
    /// Appended to the stuck message in a housing ward. Names the cause AND the
    /// one-line remedy, because neither is the player's doing: the mesh vnavmesh
    /// built on entering the zone predates the houses (see
    /// AutoWalkService.TrailHint for the measurement), and rebuilding it fixes
    /// the walk outright.
    /// </summary>
    /// <summary>
    /// Spoken once on entering a housing ward, when the mesh gets rebuilt so it
    /// knows the houses. Says WHY the wait happens - a build starting by itself
    /// would otherwise be an unexplained ten seconds of progress numbers.
    /// </summary>
    public static string HousingMeshRebuilding => Pick("Wohngebiet. Wegenetz wird neu gebaut, damit die Häuser darin stehen.",
                                                       "Housing ward. Rebuilding the navigation mesh so it includes the houses.",
                                                       "하우징 구역. 집이 포함되도록 길 정보를 다시 만드는 중.");

    public static string HousingFenceHint => Pick(" Das Wegenetz ist hier älter als die Häuser. Mit dem Befehl vnav rebuild neu bauen lassen.",
                                                  " The navigation mesh here is older than the houses. Rebuild it with the vnav rebuild command.",
                                                  " 여기 길 정보가 집보다 오래돼서 지금 지형과 맞지 않는다. vnav rebuild 명령을 쳐서 다시 만들어라.");
    /// <summary>The walk ran as far as the walkable mesh goes. Says the direction
    /// too, because "still 454 metres" without a bearing leaves the player with
    /// nothing to do next.</summary>
    public static string WalkMeshEndsHere(float distance, string direction) =>
        Pick($"Weiter komme ich nicht, hier endet der begehbare Weg. Noch {MetersRemaining(distance)} nach {direction}.",
             $"This is as far as the walkable path goes. {MetersRemaining(distance)} to the {direction}.",
             $"더 못 간다. 걸어갈 수 있는 길이 여기서 끝난다. {direction} 방향으로 {MetersRemaining(distance)} 남음.");
    /// <summary>Refuses a walk that would not move the character at all.</summary>
    public static string AlreadyAtTarget(string name) =>
        Pick($"Du bist schon bei {name}.", $"You are already at {name}.", $"이미 {name}에 있음.");

    /// <summary>The "no path, near &lt;aetheryte&gt;" hint appended to a no-path
    /// announcement (empty when no aetheryte is close). The aetheryte name is
    /// game text; only the frame is translated.</summary>
    public static string NoPathAetheryteHint(string aetheryteName) =>
        Pick($" Das Ziel liegt nahe dem Ätheryt {aetheryteName}. Reise per Aethernet dorthin.",
             $" The destination is near the aetheryte {aetheryteName}. Travel there via the aethernet.",
             $" 목적지가 에테라이트 {aetheryteName} 근처다. 전송망으로 이동해라.");

    // ── Orts-Namen (PlacesService) - der gesprochene Name, NICHT der interne
    //    TypeLabel (der bleibt als Identität deutsch, siehe PlacesService). ──
    /// <summary>Spoken name of the map flag waypoint.</summary>
    public static string FlagName => Pick("Markierung", "Flag", "표식");
    /// <summary>Spoken name of a zone transition to a named map.</summary>
    public static string TransitionToName(string name) =>
        Pick($"Übergang nach {name}", $"Transition to {name}", $"{name} 방향 통로");
    /// <summary>Fallback spoken name for an unnamed aetheryte.</summary>
    public static string AetheryteFallbackName => Pick("Ätheryt", "Aetheryte", "에테라이트");

    /// <summary>Spoken form of <c>PlaceDestination.TypeLabel</c>. The label itself
    /// stays German: PlacesService matches on it as an identity string
    /// (IsAetherytePlace, NearestAetheryteTo), so only the spoken form is
    /// localised here.
    ///
    /// Empty means "say nothing". For zone transitions and the map flag the
    /// spoken NAME already carries the type (TransitionToName, FlagName), so
    /// announcing the type as well says the same word twice - the player hears
    /// "Übergang nach X, Übergang". That doubling is present in every language.</summary>
    public static string SpokenPlaceType(string typeLabel) => typeLabel switch
    {
        "Übergang"   => string.Empty,
        "Markierung" => string.Empty,
        "Ätheryt"    => AetheryteFallbackName,
        "Aethernet"  => Pick("Aethernet", "Aethernet", "전송망"),
        "Ort"        => Pick("Ort", "Place", "장소"),
        _            => typeLabel,
    };

    // ════════════════════════════════════════════════════════════════
    //  NavigationService - Gehhilfe (walk guide)
    // ════════════════════════════════════════════════════════════════
    public static string WalkGuideEnded =>
        Pick("Gehhilfe beendet.", "Walk guide ended.", "길안내 끝냄.");
    public static string WalkGuideOff =>
        Pick("Gehhilfe aus.", "Walk guide off.", "길안내 꺼짐.");
    public static string WalkGuideOn(string name) =>
        Pick($"Gehhilfe an: {name}.", $"Walk guide on: {name}.", $"길안내 켜짐. 목적지 {name}.");
    public static string NoPathStraightLine(string hint) =>
        Pick($"Kein Weg gefunden, führe in Luftlinie.{hint}",
             $"No path found, guiding in a straight line.{hint}",
             $"길을 못 찾음. 직선으로 안내함.{hint}");
    // ════════════════════════════════════════════════════════════════
    //  TrailService - selbst abgelaufene Spuren ueber Netzluecken
    // ════════════════════════════════════════════════════════════════
    public static string TrailRecordingStarted => Pick("Spur wird aufgezeichnet. Lauf die Stelle jetzt ab und drueck die Taste am Ende noch einmal.",
                                                       "Recording a trail. Walk the stretch now and press the key again at the end.",
                                                       "발자취 기록 시작. 지금 그 구간을 걸어가고, 끝에서 같은 키를 다시 누른다.");
    public static string TrailRecordingCancelledZone => Pick("Spur verworfen, du hast das Gebiet verlassen.",
                                                             "Trail discarded, you left the area.",
                                                             "지역을 벗어나서 발자취를 버림.");
    public static string TrailTooShort => Pick("Zu kurz, keine Spur gespeichert.",
                                               "Too short, no trail saved.",
                                               "너무 짧아 발자취를 저장하지 않음.");
    public static string TrailSaved(string name, float length) => Pick($"Spur gespeichert: {name}, {MetersRemaining(length)}.",
                                                                       $"Trail saved: {name}, {MetersRemaining(length)}.",
                                                                       $"발자취 저장됨: {name}, {MetersRemaining(length)}.");
    /// <summary>Said out loud, not just logged: a trail that only works downhill
    /// is a promise the plugin cannot keep in reverse, and being stranded on the
    /// far side is exactly what happened in-game on 2026-08-09.</summary>
    public static string TrailOneWayOnly(float drop) => Pick($"Achtung, diese Spur ueberwindet {MetersRemaining(drop)} Hoehe und gilt deshalb nur in Laufrichtung. Fuer den Rueckweg zeichne bitte eine eigene Spur auf.",
                                                             $"Careful: this trail covers {MetersRemaining(drop)} of height, so it only counts in the direction you walked it. Record a separate trail for the way back.",
                                                             $"주의: 이 발자취는 높이 {MetersRemaining(drop)}를 내려가므로 걸어간 방향으로만 쓸 수 있다. 돌아오는 길은 따로 기록해라.");
    public static string TrailDefaultName(int number) => Pick($"Verbindung {number}",
                                                              $"Crossing {number}",
                                                              $"연결 {number}");
    public static string TrailNoneHere => Pick("Keine Spuren in diesem Gebiet.",
                                               "No trails in this area.",
                                               "이 지역에 발자취 없음.");
    public static string TrailCount(int count) => Pick($"{count} Spuren in diesem Gebiet.",
                                                       $"{count} trails in this area.",
                                                       $"이 지역에 발자취 {count}개.");
    public static string TrailListEntry(int number, string name, float length, bool bothWays) =>
        Loc.IsKorean ? $"{number}: {name}, {MetersRemaining(length)}, {(bothWays ? "양방향" : "걸어간 방향만")}."
        : IsGerman
        ? $"{number}: {name}, {MetersRemaining(length)}, {(bothWays ? "in beide Richtungen" : "nur in Laufrichtung")}."
        : $"{number}: {name}, {MetersRemaining(length)}, {(bothWays ? "both ways" : "one way only")}.";
    public static string TrailUnknownNumber => Pick("Diese Nummer gibt es hier nicht.",
                                                    "No trail with that number here.",
                                                    "그 번호의 발자취는 여기 없음.");
    public static string TrailDeleted(string name) => Pick($"Spur geloescht: {name}.",
                                                           $"Trail deleted: {name}.",
                                                           $"발자취 지움: {name}.");
    public static string TrailCommandHelp => Pick("Sag Schrägstrich acc trails zum Auflisten, oder Schrägstrich acc trail del und die Nummer zum Löschen.",
                                                  "Use slash acc trails to list them, or slash acc trail del and the number to delete one.",
                                                  "슬래시 acc trails로 목록을 보고, 슬래시 acc trail del과 번호로 지운다.");
    /// <summary>The auto-walk ran out of mesh and is taking a recorded trail.</summary>
    public static string TrailTaking(string name) => Pick($"Hier endet das Wegenetz, ich nehme {name}.",
                                                          $"The navmesh ends here; taking {name}.",
                                                          $"여기서 길 정보가 끝난다. 이제 발자취 {name} 따라간다.");
    public static string TrailFinished => Pick("Spur zu Ende, ich laufe normal weiter.",
                                               "End of the trail, continuing normally.",
                                               "발자취 끝. 이제부터는 평소대로 이동한다.");
    /// <summary>vnavmesh threw our fixed point list away and started routing on
    /// its own (OnStuck + RetryOnStuck) - from here on nothing is under our
    /// control, so the walk ends honestly instead of drifting off.</summary>
    public static string TrailLost => Pick("Ich komme auf der Spur nicht durch, Lauf beendet.",
                                           "I cannot get through on the trail; walk ended.",
                                           "발자취를 따라가다 막힘. 자동 이동 끝.");

    /// <summary>The walk guide ran out of walkable mesh. Unlike the auto-walk
    /// nothing is stopped - the player does the walking - so the line says what
    /// actually changes: guidance continues as the crow flies.</summary>
    public static string GuideMeshEndsHere(float distance, string direction) =>
        Pick($"Hier endet der begehbare Weg. Noch {MetersRemaining(distance)} nach {direction}, ich führe ab jetzt in Luftlinie.",
             $"This is where the walkable path ends. {MetersRemaining(distance)} to the {direction}; guiding in a straight line from here.",
             $"걸어갈 수 있는 길이 여기서 끝난다. {direction} 방향으로 {MetersRemaining(distance)} 남았고, 여기부터는 직선으로 안내한다.");

    // ════════════════════════════════════════════════════════════════
    //  HotbarService - Aktionsleiste & Skill-Browser
    // ════════════════════════════════════════════════════════════════
    public static string HotbarUnavailable =>
        Pick("Aktionsleiste nicht verfügbar.", "Hotbar not available.", "단축바를 쓸 수 없음.");
    public static string HotbarEmpty(int bar) =>
        Pick($"Aktionsleiste {bar} ist leer.", $"Hotbar {bar} is empty.", $"{bar}번 단축바, 비어 있음.");
    public static string HotbarPrefix(int bar) =>
        Pick($"Aktionsleiste {bar}. ", $"Hotbar {bar}. ", $"{bar}번 단축바. ");
    /// <summary>Slot label: main bar is "key X", other bars name bar+slot/key.</summary>
    public static string SlotMainKey(string key) =>
        Pick($"Taste {key}", $"key {key}", $"{key} 키");
    public static string SlotBarKey(int bar, string key) =>
        Pick($"Leiste {bar}, Taste {key}", $"bar {bar}, key {key}", $"{bar}번 단축바, {key} 키");
    public static string SlotBarSlot(int bar, int slot) =>
        Pick($"Leiste {bar}, Slot {slot}", $"bar {bar}, slot {slot}", $"{bar}번 단축바, {slot}번 칸");
    public static string TargetSlotCurrent(string slotLabel, string current) =>
        Pick($"Ziel-{slotLabel}: {current}",
             $"Target {slotLabel}: {current}",
             $"대상 {slotLabel}: {current}");
    public static string NoSkillSelected =>
        Pick("Kein Skill gewählt. Erst mit dem Skill-Browser blättern.",
             "No skill selected. Browse with the skill browser first.",
             "고른 기술 없음. 먼저 기술 목록에서 고른다.");
    public static string NoTargetSlot =>
        Pick("Keine Ziel-Taste gewählt. Erst die Ziel-Taste wählen.",
             "No target slot selected. Select the target slot first.",
             "배정할 키를 안 골랐음. 먼저 배정할 키를 고른다.");
    public static string AssignFailed =>
        Pick("Belegen fehlgeschlagen.", "Assignment failed.", "단축바에 올리지 못했음.");
    public static string SkillAssigned(string name, string slotLabel) =>
        Pick($"{name} liegt jetzt auf {slotLabel}.",
             $"{name} is now on {slotLabel}.",
             $"{name}, 이제 {slotLabel}에 있음.");
    public static string AssignFailedNoChange =>
        Pick("Belegen fehlgeschlagen, die Taste hat sich nicht geändert.",
             "Assignment failed, the key did not change.",
             "단축바에 올리지 못했음. 키가 그대로임.");
    public static string PlayerDataNotReady =>
        Pick("Spielerdaten noch nicht bereit.", "Player data not ready yet.", "플레이어 정보가 아직 준비 안 됨.");
    public static string NoSkillsFound =>
        Pick("Keine Skills gefunden.", "No skills found.", "기술을 못 찾음.");
    /// <summary>Bare "slot N" label (no bar), used in the hotbar read-out.</summary>
    public static string SlotNumberWord(int slot) =>
        Pick($"Slot {slot}", $"slot {slot}", $"{slot}번 칸");
    /// <summary>Target-bar summary: how many slots are filled, plus a warning
    /// when the bar has no keys bound.</summary>
    public static string TargetBarSummary(int bar, int filled, int total, bool anyKey) =>
        Loc.IsKorean ? $"{bar}번 대상 단축바, {total}칸 중 {filled}칸 채움{(anyKey ? "" : ", 키가 배정되지 않음")}."
        : IsGerman
            ? $"Ziel-Leiste {bar}, {filled} von {total} belegt{(anyKey ? "" : ", keine Tasten zugewiesen")}."
            : $"Target bar {bar}, {filled} of {total} filled{(anyKey ? "" : ", no keys assigned")}.";
    /// <summary>One browsed skill: name, level, where it currently sits (optional)
    /// and its position in the list.</summary>
    public static string SkillBrowseEntry(string name, int level, string? location, int index, int count) =>
        Loc.IsKorean ? $"{name}, 레벨 {level}{(location != null ? $", {location}에 있음" : "")}, {count} 중 {index}"
        : IsGerman
            ? $"{name}, Stufe {level}{(location != null ? $", liegt auf {location}" : "")}, {index} von {count}"
            : $"{name}, level {level}{(location != null ? $", on {location}" : "")}, {index} of {count}";

    /// <summary>One browsed item: name, stack size, quality, where it currently
    /// sits (optional) and its position in the list. The count is spoken because
    /// a stack of one is a different decision than a stack of twenty.</summary>
    public static string ItemBrowseEntry(string name, int quantity, bool isHq, string? location, int index, int count) =>
        Loc.IsKorean ? $"{name}{(isHq ? HighQuality : "")}, {quantity}개{(location != null ? $", {location}에 있음" : "")}, {count} 중 {index}"
        : IsGerman
            ? $"{name}{(isHq ? HighQuality : "")}, {quantity} Stück{(location != null ? $", liegt auf {location}" : "")}, {index} von {count}"
            : $"{name}{(isHq ? HighQuality : "")}, {quantity}{(location != null ? $", on {location}" : "")}, {index} of {count}";

    // ── Skill-Zuweisungs-Menü (modal, Nummernblock) ──
    /// <summary>Spoken when the modal skill menu opens, with the browse hint.</summary>
    public static string SkillMenuOpened(int count) =>
        Pick($"Skill-Zuweisung, {count} Skills. Nummernblock 8 und 2 blättern, 4 oder 6 wechselt die Liste, Nummernblock 0 wählt, Nummernblock Komma zurück.",
             $"Skill assignment, {count} skills. Numpad 8 and 2 to browse, 4 or 6 switches the list, Numpad 0 selects, Numpad decimal to go back.",
             $"기술 배정, 기술 {count}개. 숫자패드 8과 2로 넘기고, 4 또는 6으로 목록을 바꾸고, 숫자패드 0으로 고르고, 숫자패드 마침표로 돌아간다.");

    /// <summary>Spoken when the menu switches to the carried-item list.</summary>
    public static string ItemMenuOpened(int count) =>
        Pick($"Gegenstände, {count} Einträge. Nummernblock 8 und 2 blättern, 4 oder 6 wechselt die Liste, Nummernblock 0 wählt, Nummernblock Komma zurück.",
             $"Items, {count} entries. Numpad 8 and 2 to browse, 4 or 6 switches the list, Numpad 0 selects, Numpad decimal to go back.",
             $"아이템, 항목 {count}개. 숫자패드 8과 2로 넘기고, 4 또는 6으로 목록을 바꾸고, 숫자패드 0으로 고르고, 숫자패드 마침표로 돌아간다.");

    /// <summary>Spoken when the menu switches to the general-action list
    /// (Absteigen, Reittier-Roulette, Sprint, Teleport ...).</summary>
    public static string GeneralActionMenuOpened(int count) =>
        Pick($"Allgemeine Aktionen, {count} Einträge. Nummernblock 8 und 2 blättern, 4 oder 6 wechselt die Liste, Nummernblock 0 wählt, Nummernblock Komma zurück.",
             $"General actions, {count} entries. Numpad 8 and 2 to browse, 4 or 6 switches the list, Numpad 0 selects, Numpad decimal to go back.",
             $"일반 기술, 항목 {count}개. 숫자패드 8과 2로 넘기고, 4 또는 6으로 목록을 바꾸고, 숫자패드 0으로 고르고, 숫자패드 마침표로 돌아간다.");

    /// <summary>Spoken when the menu switches to the mount list.</summary>
    public static string MountMenuOpened(int count) =>
        Pick($"Reittiere, {count} Einträge. Nummernblock 8 und 2 blättern, 4 oder 6 wechselt die Liste, Nummernblock 0 wählt, Nummernblock Komma zurück.",
             $"Mounts, {count} entries. Numpad 8 and 2 to browse, 4 or 6 switches the list, Numpad 0 selects, Numpad decimal to go back.",
             $"탈것, 항목 {count}개. 숫자패드 8과 2로 넘기고, 4 또는 6으로 목록을 바꾸고, 숫자패드 0으로 고르고, 숫자패드 마침표로 돌아간다.");

    /// <summary>One browsed entry that has nothing but a name: general actions
    /// and mounts. Same shape as the other browse entries so the menu sounds
    /// consistent no matter which list is open.</summary>
    public static string PlainBrowseEntry(string name, string? location, int index, int count) =>
        Loc.IsKorean ? $"{name}{(location != null ? $", {location}에 있음" : "")}, {count} 중 {index}"
        : IsGerman
            ? $"{name}{(location != null ? $", liegt auf {location}" : "")}, {index} von {count}"
            : $"{name}{(location != null ? $", on {location}" : "")}, {index} of {count}";

    /// <summary>Spoken when the menu switches to the quest-item list.</summary>
    public static string QuestItemMenuOpened(int count) =>
        Pick($"Quest-Gegenstände, {count} Einträge. Nummernblock 8 und 2 blättern, 4 oder 6 wechselt die Liste, Nummernblock 0 wählt, Nummernblock Komma zurück.",
             $"Quest items, {count} entries. Numpad 8 and 2 to browse, 4 or 6 switches the list, Numpad 0 selects, Numpad decimal to go back.",
             $"퀘스트 아이템, 항목 {count}개. 숫자패드 8과 2로 넘기고, 4 또는 6으로 목록을 바꾸고, 숫자패드 0으로 고르고, 숫자패드 마침표로 돌아간다.");

    /// <summary>One browsed quest item: name, how many are left, its cast time
    /// and where it already sits. The cast time matters in a fight - three
    /// seconds of standing still is a decision.</summary>
    public static string QuestItemBrowseEntry(string name, int quantity, byte castTime, string? location, int index, int count) =>
        Loc.IsKorean ? $"{name}, {quantity}개{(castTime > 0 ? $", 시전 시간 {castTime}초" : "")}{(location != null ? $", {location}에 있음" : "")}, {count} 중 {index}"
        : IsGerman
            ? $"{name}, {quantity} Stück{(castTime > 0 ? $", Wirkzeit {castTime} Sekunden" : "")}{(location != null ? $", liegt auf {location}" : "")}, {index} von {count}"
            : $"{name}, {quantity}{(castTime > 0 ? $", cast time {castTime} seconds" : "")}{(location != null ? $", on {location}" : "")}, {index} of {count}";

    /// <summary>Spoken when stepping the source list finds nothing else with
    /// entries - the player stays where they are.</summary>
    public static string SkillMenuNoOtherSource =>
        Pick("Keine andere Liste verfügbar.", "No other list available.", "다른 목록 없음.");

    /// <summary>Announced when usable quest items arrive. Says what the loot
    /// channel does not: that they DO something, and how to reach them.</summary>
    public static string QuestItemReceived(string joined) =>
        Pick($"Quest-Gegenstand zum Benutzen: {joined}. Mit Strg und Nummernblock 0 auf die Leiste legen.",
             $"Usable quest item: {joined}. Put it on a bar with Ctrl and Numpad 0.",
             $"쓸 수 있는 퀘스트 아이템: {joined}. 컨트롤과 숫자패드 0으로 단축바에 올린다.");

    // ── Zugang zum Ziel (Aufgangs-Erkennung) ─────────────────────────
    // Wenn das Ziel auf einer Fläche liegt, die im Wegenetz nicht an unserer
    // hängt (Schiffsdeck, Balkon, Empore), läuft der Auto-Lauf sonst stumm
    // gegen nichts. Diese Meldungen sagen stattdessen, WIE NAH man herankommt.

    /// <summary>Approach search: nothing selected to check.</summary>
    public static string ApproachNoTarget =>
        Pick("Kein Ziel gewählt. Erst ein Ziel anvisieren oder im Objekt-Browser auswählen.",
             "No destination selected. Target something first, or pick it in the object browser.",
             "고른 목적지 없음. 먼저 대상을 지정하거나 사물 목록에서 고른다.");

    /// <summary>Approach search: started (it takes a moment, so say so).</summary>
    public static string ApproachChecking(string target) =>
        Pick($"Prüfe den Weg zu {target}.",
             $"Checking the route to {target}.",
             $"{target}까지 가는 길을 확인 중.");

    /// <summary>Approach search: a continuous route exists.</summary>
    public static string ApproachReachable(string target, float distance) =>
        Pick($"Zu {target} führt ein durchgehender Weg, {distance:F0} Meter.",
             $"There is a continuous route to {target}, {distance:F0} meters.",
             $"{target}까지 이어진 길이 있음. {distance:F0}미터.");

    /// <summary>Approach search: no route, and no reachable spot nearby either.</summary>
    public static string ApproachNone(string target) =>
        Pick($"Zu {target} führt kein Weg, und in der Nähe gibt es keinen erreichbaren Punkt. Der Zugang liegt weiter weg.",
             $"No route to {target}, and no reachable spot nearby either. The way in is further off.",
             $"{target}까지 가는 길이 없고, 근처에 닿을 수 있는 지점도 없음. 입구가 더 멀리 있음.");

    /// <summary>Approach search: names the closest reachable spot, how to get
    /// there and how the destination sits relative to it.</summary>
    public static string ApproachFound(string target, float walkDistance, string compass,
                                       float gapDistance, float heightDiff)
    {
        var hoehe = heightDiff switch
        {
            >= 1f => Pick($", {heightDiff:F0} Meter über dir",
                          $", {heightDiff:F0} meters above you",
                          $", 나보다 {heightDiff:F0}미터 위"),
            <= -1f => Pick($", {-heightDiff:F0} Meter unter dir",
                           $", {-heightDiff:F0} meters below you",
                           $", 나보다 {-heightDiff:F0}미터 아래"),
            _ => string.Empty,
        };
        return Loc.IsKorean
            ? $"{target}까지 이어진 길이 없음. 가장 가까운 지점까지 간다. " +
              $"{compass} 방향 {walkDistance:F0}미터. 거기서 목적지까지 " +
              $"{gapDistance:F0}미터 남음{hoehe}."
            : IsGerman
            ? $"Kein durchgehender Weg zu {target}. Ich laufe zum nächstmöglichen Punkt, " +
              $"{walkDistance:F0} Meter nach {compass}. Von dort ist das Ziel noch " +
              $"{gapDistance:F0} Meter entfernt{hoehe}."
            : $"No continuous route to {target}. Walking to the closest spot instead, " +
              $"{walkDistance:F0} meters {compass}. From there the destination is " +
              $"{gapDistance:F0} meters away{hoehe}.";
    }

    /// <summary>Destination name for the walk to the near side of a gap.</summary>
    public static string GapCrossSpotName =>
        Pick("Übergangsstelle", "crossing point", "건너는 지점");

    /// <summary>Now crossing a gap the navigation mesh does not cover.</summary>
    public static string GapCrossing =>
        Pick("Übergangsstelle erreicht. Überquere die Lücke.",
             "Crossing point reached. Crossing the gap now.",
             "건너는 지점 도착. 틈을 건너는 중.");

    /// <summary>The game's collision module could not be reached.</summary>
    public static string GroundProbeUnavailable =>
        Pick("Die Kollisionsabfrage des Spiels ist nicht erreichbar.",
             "The game's collision query is unavailable.",
             "게임의 충돌 판정을 쓸 수 없음.");

    /// <summary>Result of the ground probe: how much floor was found and how
    /// much of it the navigation mesh does not cover.</summary>
    public static string GroundProbeResult(int hits, int withoutMesh) =>
        Pick($"Bodenmessung fertig. {hits} Treffer, davon {withoutMesh} ohne Wegenetz.",
             $"Ground probe done. {hits} hits, {withoutMesh} of them without navigation mesh.",
             $"바닥 측정 완료. 바닥이 {hits}군데 잡혔고, 그중 {withoutMesh}군데는 길 정보 없음.");

    /// <summary>The crossing was surveyed for one zone only and we are elsewhere.</summary>
    public static string GapCrossWrongZone =>
        Pick("Diesen Übergang gibt es nur auf den Unteren Decks.",
             "This crossing only exists on the Lower Decks.",
             "건너기는 하층 갑판에서만 할 수 있음.");

    /// <summary>Neither side of the gap can be walked to from where we stand.</summary>
    public static string GapCrossNoSide =>
        Pick("Von hier aus führt kein Weg zur Übergangsstelle.",
             "No route to the crossing point from here.",
             "여기서 건너는 지점까지 가는 길이 없음.");

    /// <summary>The walk to the crossing point did not arrive, so no crossing.</summary>
    public static string GapCrossTooFar =>
        Pick("Übergang abgebrochen - die Übergangsstelle wurde nicht erreicht.",
             "Crossing cancelled - the crossing point was not reached.",
             "건너기 중단. 건너는 지점에 닿지 못함.");

    /// <summary>Name for the walk to an approach spot - the walk announcements
    /// must not claim we are heading for the destination itself.</summary>
    public static string ApproachSpotName(string target) =>
        Pick($"Zugang zu {target}", $"way in to {target}", $"{target} 입구");

    /// <summary>Name for the walk to the near side of a crossing. Like
    /// <see cref="ApproachSpotName"/> this only ever surfaces in a failure
    /// announcement - a crossing that works stays silent, the same way the
    /// near-miss redirect does.</summary>
    public static string CrossingSpotName(string target) =>
        Pick($"Übergang zu {target}", $"crossing to {target}", $"{target} 방향 건너는 지점");

    /// <summary>Auto-walk refused to start: the destination hangs on a separate
    /// patch of the navigation mesh, so walking there is impossible.</summary>
    public static string TargetUnreachable(string target) =>
        Pick($"{target} ist nicht erreichbar - dorthin führt kein Weg.",
             $"{target} cannot be reached - no route leads there.",
             $"{target}에 닿을 수 없음. 그리로 가는 길이 없음.");

    // Es gab hier drei Ansagen rund um den Fall "Weg endet kurz vorm Ziel"
    // (Umleitung, Restfahrt, Restweg). Der User hat sie am 2026-08-07 direkt
    // nach dem Bau abgelehnt: "das ist evtl zu viel info, ich werd ja sehen wie
    // weit er vom ziel weg ist". Der Ablauf laeuft jetzt still durch; endet er
    // ohne Ankunft, greift AutoWalkEndedRemaining wie bei jedem anderen Lauf.

    /// <summary>Debug probe: the slot it wants to test is not free.</summary>
    public static string ProbeSlotOccupied =>
        Pick("Sonde braucht Taste 12 der ersten Leiste frei.",
             "Probe needs key 12 on the first bar to be free.",
             "측정하려면 첫 단축바 12번 키가 비어 있어야 함.");

    /// <summary>Debug probe: finished, results are in the log.</summary>
    public static string ProbeDone =>
        Pick("Sonde fertig, Ergebnis im Log.",
             "Probe finished, results in the log.",
             "측정 완료. 결과는 로그에.");

    /// <summary>Spoken when the player carries nothing that can go on a bar.</summary>
    public static string NoUsableItems =>
        Pick("Keine benutzbaren Gegenstände in der Tasche.",
             "No usable items in your bag.",
             "소지품에 쓸 수 있는 아이템 없음.");
    /// <summary>Spoken after a skill is chosen: now pick the target key.</summary>
    public static string SkillMenuPickTarget(string skillName, int count) =>
        Pick($"{skillName} gewählt. Ziel-Taste wählen, {count} verfügbar. Nummernblock 8 und 2 blättern, Nummernblock 0 belegt, Nummernblock Komma zurück.",
             $"{skillName} selected. Choose a target key, {count} available. Numpad 8 and 2 to browse, Numpad 0 assigns, Numpad decimal to go back.",
             $"{skillName} 선택됨. 대상 키를 고른다. 고를 수 있는 키 {count}개. 숫자패드 8과 2로 넘기고, 숫자패드 0으로 배정하고, 숫자패드 마침표로 돌아간다.");
    /// <summary>One browsed target key: its label, what is on it now, position in list.</summary>
    public static string SkillMenuTargetEntry(string slotLabel, string current, int index, int count) =>
        Pick($"{slotLabel}, aktuell {current}, {index} von {count}",
             $"{slotLabel}, currently {current}, {index} of {count}",
             $"{slotLabel}, 현재 {current}, {count} 중 {index}");
    public static string SkillMenuClosed =>
        Pick("Skill-Menü geschlossen.", "Skill menu closed.", "기술 메뉴 닫힘.");
    public static string SkillMenuNoTargets =>
        Pick("Keine belegbaren Tasten gefunden.", "No assignable keys found.", "배정할 수 있는 키를 못 찾음.");

    // ── CooldownService: Fähigkeit wieder bereit ──
    public static string SkillReady(string name) =>
        Pick($"{name} bereit.", $"{name} ready.", $"{name} 준비됨.");
    public static string SkillChargeReady(string name, uint charges, ushort maxCharges) =>
        Pick($"{name} bereit, {charges} von {maxCharges} Ladungen.",
             $"{name} ready, {charges} of {maxCharges} charges.",
             $"{name} 준비됨, 충전 {maxCharges}회 중 {charges}회.");
    public static string SkillReadyAnnounceOn =>
        Pick("Fähigkeit-bereit-Ansage an.", "Ability-ready announcements on.", "기술 준비됨 안내 켜짐.");
    public static string SkillReadyAnnounceOff =>
        Pick("Fähigkeit-bereit-Ansage aus.", "Ability-ready announcements off.", "기술 준비됨 안내 꺼짐.");

    // ════════════════════════════════════════════════════════════════
    //  EmoteService
    // ════════════════════════════════════════════════════════════════
    public static string NoEmoteSelected =>
        Pick("Kein Emote gewählt. Erst durchblättern.",
             "No emote selected. Browse first.",
             "고른 감정 표현 없음. 먼저 넘겨서 고른다.");
    public static string EmoteUnavailable =>
        Pick("Emote nicht verfügbar.", "Emote not available.", "감정 표현을 쓸 수 없음.");
    public static string EmoteFailed =>
        Pick("Emote fehlgeschlagen.", "Emote failed.", "감정 표현 실패.");
    public static string EmotesNotReady =>
        Pick("Emotes noch nicht bereit.", "Emotes not ready yet.", "감정 표현이 아직 준비 안 됨.");
    public static string NoEmotesAvailable =>
        Pick("Keine Emotes verfügbar.", "No emotes available.", "쓸 수 있는 감정 표현 없음.");
    /// <summary>One browsed emote: name, chat command (optional), list position.</summary>
    public static string EmoteBrowseEntry(string name, string command, int index, int count) =>
        Loc.IsKorean ? $"{name}{(command.Length > 0 ? $", 명령어 {command}" : "")}, {count} 중 {index}"
        : IsGerman
            ? $"{name}{(command.Length > 0 ? $", Befehl {command}" : "")}, {index} von {count}"
            : $"{name}{(command.Length > 0 ? $", command {command}" : "")}, {index} of {count}";

    // ════════════════════════════════════════════════════════════════
    //  DalamudPluginsService - Plugin-Liste
    // ════════════════════════════════════════════════════════════════
    public static string NoPluginSelected =>
        Pick("Kein Plugin gewählt. Erst durchblättern.",
             "No plugin selected. Browse first.",
             "고른 플러그인 없음. 먼저 넘겨서 고른다.");
    public static string PluginNoSettings(string name) =>
        Pick($"{name} hat keine Einstellungen.", $"{name} has no settings.", $"{name}, 설정 없음.");
    public static string PluginSettingsOpened(string name) =>
        Pick($"Einstellungen von {name} geöffnet. Das Fenster ist nicht vorlesbar.",
             $"Opened settings of {name}. The window cannot be read aloud.",
             $"{name} 설정을 열었음. 그 창은 읽어 줄 수 없음.");
    public static string PluginSettingsCantOpen(string name) =>
        Pick($"Einstellungen von {name} lassen sich nicht öffnen.",
             $"Cannot open settings of {name}.",
             $"{name} 설정을 열 수 없음.");
    public static string PluginListUnavailable =>
        Pick("Plugin-Liste nicht verfügbar.", "Plugin list not available.", "플러그인 목록을 불러오지 못함.");
    public static string NoPluginsInstalled =>
        Pick("Keine Plugins installiert.", "No plugins installed.", "설치된 플러그인 없음.");
    // Plugin-Zustandswörter (Describe / BuildOverview)
    public static string PluginVersionLabel(string version) =>
        Pick($"Version {version}", $"version {version}", $"버전 {version}");
    public static string PluginLoaded    => Pick("geladen", "loaded", "불러옴");
    public static string PluginNotLoaded => Pick("nicht geladen", "not loaded", "불러오지 않음");
    public static string PluginOutdated  => Pick("veraltet", "outdated", "구버전");
    public static string PluginBanned    => Pick("gesperrt", "banned", "차단됨");
    public static string PluginDev       => Pick("Entwickler-Plugin", "dev plugin", "개발용 플러그인");
    public static string PluginHasConfig => Pick("hat Einstellungen", "has settings", "설정 있음");
    public static string PluginAllLoaded => Pick("alle geladen", "all loaded", "전부 불러옴");
    public static string PluginCountNotLoaded(int n) => Pick($"{n} nicht geladen",
                                                             $"{n} not loaded",
                                                             $"{n}개 불러오지 않음");
    public static string PluginCountOutdated(int n)  => Pick($"{n} veraltet",
                                                             $"{n} outdated",
                                                             $"{n}개 구버전");
    public static string PluginCountBanned(int n)    => Pick($"{n} gesperrt",
                                                             $"{n} banned",
                                                             $"{n}개 차단됨");
    public static string PluginOverview(int total, string state) =>
        Pick($"{total} Plugins, {state}.", $"{total} plugins, {state}.", $"플러그인 {total}개, {state}.");

    // ════════════════════════════════════════════════════════════════
    //  FishingService (spoken parts; the /acc fishobj probe stays German)
    // ════════════════════════════════════════════════════════════════
    public static string FishingSpotsList(int count, string joined) =>
        Pick($"{count} Angelplätze: {joined}.",
             $"{count} fishing spots: {joined}.",
             $"낚시터 {count}곳: {joined}.");
    public static string NoFishingSpotNearEnough(string name, float distance) =>
        Pick($"Kein Angelplatz nah genug. Nächster: {name}, {distance:F0} Meter. Stell dich an die Angelstelle und drück erneut.",
             $"No fishing spot close enough. Nearest: {name}, {distance:F0} meters. Stand at the fishing spot and press again.",
             $"충분히 가까운 낚시터 없음. 가장 가까운 곳: {name}, {distance:F0}미터. 낚시 자리에 서서 다시 누른다.");
    public static string MapUnknownCantRemember =>
        Pick("Aktuelle Karte unbekannt, kann die Stelle nicht merken.",
             "Current map unknown, cannot remember this spot.",
             "현재 지도를 알 수 없어 이 자리를 기억할 수 없음.");
    public static string FishingSpotRemembered(string name, float mapX, float mapY) =>
        Pick($"Angelplatz {name} hier gemerkt: Karte {mapX:F1}, {mapY:F1}.",
             $"Fishing spot {name} remembered here: map {mapX:F1}, {mapY:F1}.",
             $"낚시터 {name}, 지금 서 있는 자리로 기억함: 지도 {mapX:F1}, {mapY:F1}.");

    // ════════════════════════════════════════════════════════════════
    //  GatheringService
    // ════════════════════════════════════════════════════════════════
    public static string GatheringSpotsList(int count, string joined) =>
        Pick($"{count} Sammelstellen: {joined}.",
             $"{count} gathering spots: {joined}.",
             $"채집 지점 {count}곳: {joined}.");

    // ════════════════════════════════════════════════════════════════
    //  InventoryService
    // ════════════════════════════════════════════════════════════════
    public static string InventoryEmpty =>
        Pick("Inventar ist leer.", "Inventory is empty.", "소지품이 비어 있음.");
    public static string GilUnavailable =>
        Pick("Gil-Stand nicht verfügbar.", "Gil amount not available.", "소지금을 알 수 없음.");
    public static string KeyItemsLabel(string joined) =>
        Pick($"Schlüsselgegenstände: {joined}", $"Key items: {joined}", $"중요 아이템: {joined}");
    public static string BagLabel(int count, string joined) =>
        Pick($"Tasche, {count} Gegenstände: {joined}",
             $"Bag, {count} items: {joined}",
             $"소지품, 아이템 {count}개: {joined}");
    /// <summary>A stacked item: "&lt;name&gt; times &lt;count&gt;" plus an optional
    /// HQ suffix. Single items are announced by the caller without this frame.</summary>
    public static string ItemStack(string name, int quantity, string hqSuffix) =>
        Pick($"{name} mal {quantity}{hqSuffix}",
             $"{name} times {quantity}{hqSuffix}",
             $"{name} {quantity}개{hqSuffix}");
    public static string KeyItemFallback(uint id) =>
        Pick($"Schlüsselgegenstand {id}", $"Key item {id}", $"중요 아이템 {id}");

    // ════════════════════════════════════════════════════════════════
    //  LootRollService - Beute auswuerfeln (Bedarf / Gier / Passen)
    // ════════════════════════════════════════════════════════════════
    /// <summary>Announced the moment a roll opens.</summary>
    public static string LootRollStarted(string name, int count, string options) =>
        Loc.IsKorean ? $"입찰: {name}{(count > 1 ? $" {count}개" : "")}. {options}"
        : IsGerman
            ? $"Verlosung: {name}{(count > 1 ? $" mal {count}" : "")}. {options}"
            : $"Loot roll: {name}{(count > 1 ? $" times {count}" : "")}. {options}";

    /// <summary>Spoken after the roll window was handed the keyboard focus.</summary>
    public static string LootRollFocused =>
        Pick("Verlosungs-Fenster im Fokus. Mit dem Nummernblock auswählen.",
             "Loot roll window focused. Use the numpad to choose.",
             "전리품 입찰 창에 초점. 숫자패드로 고른다.");

    /// <summary>Spoken when the focus key is pressed without a roll window up.</summary>
    public static string LootRollNoWindow =>
        Pick("Kein Verlosungs-Fenster offen.", "No loot roll window open.", "전리품 입찰 창이 열려 있지 않음.");

    /// <summary>Spoken when the player asks and nothing is being rolled for.</summary>
    public static string LootRollNone =>
        Pick("Zurzeit wird nichts verlost.", "Nothing is being rolled for.", "지금 입찰 중인 것 없음.");

    /// <summary>Header of the on-demand readout.</summary>
    public static string LootRollList(int count, string joined) =>
        Pick($"{count} Verlosungen. {joined}",
             $"{count} loot rolls. {joined}",
             $"입찰 {count}건. {joined}");

    /// <summary>One entry of the on-demand readout.</summary>
    public static string LootRollEntry(string name, int count, string options, string ownRoll) =>
        Loc.IsKorean ? $"{name}{(count > 1 ? $" {count}개" : "")}, {options}{(ownRoll.Length > 0 ? $", {ownRoll}" : "")}"
        : IsGerman
            ? $"{name}{(count > 1 ? $" mal {count}" : "")}, {options}{(ownRoll.Length > 0 ? $", {ownRoll}" : "")}"
            : $"{name}{(count > 1 ? $" times {count}" : "")}, {options}{(ownRoll.Length > 0 ? $", {ownRoll}" : "")}";

    /// <summary>
    /// One row of the roll window while stepping through the list. The gear
    /// block ("Stufe 15, tragbar, Gegenstandsstufe 20, Verteidigung 31") sits
    /// right behind the name, where the game's own tooltip puts it, and is ""
    /// for everything that is not equipment.
    /// </summary>
    public static string LootRollRow(string name, int count, string gear, string options, string remaining) =>
        Loc.IsKorean
            ? $"{name}{(count > 1 ? $" {count}개" : "")}" +
              $"{(gear.Length      > 0 ? $", {gear}"      : "")}" +
              $"{(options.Length  > 0 ? $", {options}"  : "")}" +
              $"{(remaining.Length > 0 ? $", {remaining}" : "")}"
        : IsGerman
            ? $"{name}{(count > 1 ? $" mal {count}" : "")}" +
              $"{(gear.Length      > 0 ? $", {gear}"      : "")}" +
              $"{(options.Length  > 0 ? $", {options}"  : "")}" +
              $"{(remaining.Length > 0 ? $", {remaining}" : "")}"
            : $"{name}{(count > 1 ? $" times {count}" : "")}" +
              $"{(gear.Length      > 0 ? $", {gear}"      : "")}" +
              $"{(options.Length  > 0 ? $", {options}"  : "")}" +
              $"{(remaining.Length > 0 ? $", {remaining}" : "")}";

    /// <summary>Seconds left before the roll expires.</summary>
    public static string LootRollRemaining(int seconds) =>
        Pick($"noch {seconds} Sekunden", $"{seconds} seconds left", $"{seconds}초 남음");

    /// <summary>What the player may still do - the game's RollState in words.</summary>
    public static string LootOptionsNeedGreedPass =>
        Pick("Bedarf, Gier oder Passen möglich", "need, greed or pass", "선입찰, 입찰, 포기 가능");
    public static string LootOptionsGreedPass =>
        Pick("nur Gier oder Passen möglich", "greed or pass only", "입찰 또는 포기만 가능");
    public static string LootOptionsPassOnly =>
        Pick("nur Passen möglich", "pass only", "포기만 가능");
    public static string LootOptionsDone =>
        Pick("schon gewürfelt", "already rolled", "이미 입찰함");
    public static string LootOptionsUnavailable =>
        Pick("nicht verfügbar", "unavailable", "입찰 불가");

    /// <summary>What the player already did, with the rolled number.</summary>
    public static string LootRolledNeed(byte value) =>
        Pick($"du hast Bedarf gewürfelt, {value}", $"you rolled need, {value}", $"선입찰함, {value}");
    public static string LootRolledGreed(byte value) =>
        Pick($"du hast Gier gewürfelt, {value}", $"you rolled greed, {value}", $"입찰함, {value}");
    public static string LootRolledPass =>
        Pick("du hast gepasst", "you passed", "포기함");
    public static string LootRolledWon =>
        Pick("du hast den Gegenstand erhalten", "you were awarded the item", "아이템을 받음");

    // ════════════════════════════════════════════════════════════════
    //  MessageHistoryService - Nachlese-Kanäle
    // ════════════════════════════════════════════════════════════════
    // [Chat-Puffer] Fuer das NEUE Chatsystem ist ChatCategoryName entfallen. Dessen
    // Puffer sind keine feste Aufzaehlung des Plugins mehr, sondern die Kanaele und
    // Register des SPIELS, und die tragen ihre Namen selbst: eine LogFilter-Zeile
    // ihren Zeilennamen, ein Register das, was der Spieler dort eingetippt hat. Eine
    // uebersetzte Liste daneben wuerde Dinge umbenennen, die dem Spieler gehoeren.
    // Die drei Puffer, die keine Register sind, stehen in AccessibilityStrings.Chat.cs.
    //
    // Das ALTE Chatsystem hat seine feste Kategorienliste weiterhin, und die braucht
    // ihre Namen - daher LegacyChatCategoryName. Wortgleich zum frueheren
    // ChatCategoryName, damit der Spieler beim Umschalten dieselben Woerter hoert
    // wie vorher.
    public static string LegacyChatCategoryName(LegacyChatHistoryService.Category category) => category switch
    {
        LegacyChatHistoryService.Category.Dialogue     => Pick("Dialoge", "Dialogue", "대화"),
        LegacyChatHistoryService.Category.Say          => Pick("Sagen", "Say", "말하기"),
        LegacyChatHistoryService.Category.Shout        => Pick("Rufen", "Shout", "외치기"),
        LegacyChatHistoryService.Category.Party        => Pick("Gruppe", "Party", "파티"),
        LegacyChatHistoryService.Category.Alliance     => Pick("Allianz", "Alliance", "연합 파티"),
        LegacyChatHistoryService.Category.Tell         => Pick("Flüstern", "Tell", "귓속말"),
        LegacyChatHistoryService.Category.FreeCompany  => Pick("Freie Gesellschaft",
                                                               "Free Company",
                                                               "자유부대"),
        LegacyChatHistoryService.Category.System       => Pick("System", "System", "시스템"),
        LegacyChatHistoryService.Category.Loot         => Pick("Beute", "Loot", "전리품"),
        _                                              => category.ToString(),
    };

    // ── Umschalter zwischen altem und neuem Chatsystem ──────────────

    /// <summary>Menu row that switches between the two chat systems.</summary>
    public static string OptChatSystem =>
        Pick("Chatsystem", "Chat system", "로그 시스템");

    /// <summary>The old system's name, as the player knows it from v5.83.</summary>
    public static string ChatSystemLegacyName =>
        Pick("gewohnt, feste Kanäle", "classic, fixed channels", "기존, 고정 채널");

    /// <summary>The PR #5 system's name: buffers follow the game's own tabs.</summary>
    public static string ChatSystemNewName =>
        Pick("neu, Register des Spiels", "new, the game's tabs", "신규, 게임 탭");

    /// <summary>The menu row's label, naming the system in force.</summary>
    public static string OptChatSystemRow(bool legacy) =>
        Pick($"Chatsystem: {(legacy ? ChatSystemLegacyName : ChatSystemNewName)}",
             $"Chat system: {(legacy ? ChatSystemLegacyName : ChatSystemNewName)}",
             $"로그 시스템: {(legacy ? ChatSystemLegacyName : ChatSystemNewName)}");

    /// <summary>Spoken the moment the switch is flipped. Says that nothing was
    /// lost, because that is the first thing a player wonders about a buffer
    /// they cannot see.</summary>
    public static string ChatSystemSwitched(bool legacy) =>
        Pick($"Chatsystem {(legacy ? ChatSystemLegacyName : ChatSystemNewName)}. Beide Nachlesen laufen mit, es ist nichts verloren.",
             $"Chat system {(legacy ? ChatSystemLegacyName : ChatSystemNewName)}. Both histories keep recording, nothing was lost.",
             $"로그 시스템 {(legacy ? ChatSystemLegacyName : ChatSystemNewName)}. 두 기록 다 계속 쌓이고, 잃은 것 없음.");

    /// <summary>Spoken when a key belonging to the OTHER system is pressed.
    /// Silence would read as a broken key - the player cannot see that the key
    /// simply has no counterpart in the system they switched to.</summary>
    public static string ChatKeyOnlyInNewSystem =>
        Pick("Diese Taste gehört zum neuen Chatsystem.",
             "That key belongs to the new chat system.",
             "이 키는 신규 로그 시스템 전용이라 지금은 쓸 수 없음.");

    public static string CategoryEmpty(string category) =>
        Pick($"{category}, leer", $"{category}, empty", $"{category}, 비어 있음");
    public static string CategorySummary(string category, int count) =>
        count == 0
            ? (Pick($"{category}, leer", $"{category}, empty", $"{category}, 비어 있음"))
            : (Loc.IsKorean ? $"{category}, 메시지 {count}개"
               : IsGerman ? $"{category}, {count} {(count == 1 ? "Nachricht" : "Nachrichten")}"
                        : $"{category}, {count} {(count == 1 ? "message" : "messages")}");
    public static string HistoryStart =>
        Pick("Anfang des Verlaufs.", "Start of history.", "기록 처음.");
    public static string HistoryEnd =>
        Pick("Ende des Verlaufs.", "End of history.", "기록 끝.");

    // ════════════════════════════════════════════════════════════════
    //  ChatReaderService - gesprochene Kanal-Präfixe
    //  (spoken BEFORE a chat line, e.g. "Says from X: ...")
    // ════════════════════════════════════════════════════════════════
    /// <summary>Channel prefix for an incoming chat line ("" = no prefix).</summary>
    public static string ChatPrefix(XivChatType type) => type switch
    {
        XivChatType.Say           => Pick("Sagt", "Says", "말하기"),
        XivChatType.Shout         => Pick("Ruft", "Shouts", "외치기"),
        XivChatType.Party         => Pick("Gruppe", "Party", "파티"),
        XivChatType.Alliance      => Pick("Allianz", "Alliance", "연합 파티"),
        XivChatType.TellIncoming  => Pick("Flüstert", "Tells", "귓속말"),
        XivChatType.FreeCompany   => Pick("FC", "FC", "자유부대"),
        XivChatType.SystemMessage => Pick("System", "System", "시스템"),
        XivChatType.ErrorMessage  => Pick("Fehler", "Error", "오류"),
        XivChatType.TellOutgoing  => Pick("Flüstert an", "Tells", "귓속말"),
        XivChatType.Yell          => Pick("Brüllt", "Yells", "떠들기"),
        XivChatType.CrossParty    => Pick("Gruppe", "Party", "파티"),
        XivChatType.Echo          => Pick("Echo", "Echo", "혼잣말"),
        XivChatType.Gathering     => "",   // full sentence, no channel prefix
        XivChatType.LootNotice    => "",   // full sentence, no channel prefix
        // An NPC speaking needs no channel word - the name in front of the line
        // says everything "Chat von ..." would have said, only shorter.
        XivChatType.NPCDialogue   => "",
        XivChatType.NPCDialogueAnnouncements => "",
        _                         => Pick("Chat", "Chat", "로그"),
    };

    /// <summary>Prefix for the player's OWN messages ("You say: ...").</summary>
    public static string OwnChatPrefix(XivChatType type) => type switch
    {
        XivChatType.Say          => Pick("Du sagst", "You say", "내가 말하기"),
        XivChatType.Shout        => Pick("Du rufst", "You shout", "내가 외치기"),
        XivChatType.Yell         => Pick("Du brüllst", "You yell", "내가 떠들기"),
        XivChatType.Party        => Pick("Du zur Gruppe", "You to party", "내가 파티에"),
        XivChatType.CrossParty   => Pick("Du zur Gruppe", "You to party", "내가 파티에"),
        XivChatType.Alliance     => Pick("Du zur Allianz", "You to alliance", "내가 연합 파티에"),
        XivChatType.FreeCompany  => Pick("Du zur FC", "You to FC", "내가 자유부대에"),
        XivChatType.TellOutgoing => Pick("Du flüsterst", "You tell", "내가 귓속말"),
        _                        => Pick("Du", "You", "내가"),
    };

    /// <summary>Outgoing-tell addressee clause (" to X"), appended after the prefix.</summary>
    public static string ChatAddressee(string name) =>
        Pick($" an {name}", $" to {name}", $" {name}에게");

    /// <summary>A chat line with a named sender: "&lt;prefix&gt; from &lt;sender&gt;: &lt;message&gt;".</summary>
    public static string ChatFromLine(string prefix, string sender, string message) =>
        Pick($"{prefix} von {sender}: {message}",
             $"{prefix} from {sender}: {message}",
             $"{sender} {prefix}: {message}");

    // ════════════════════════════════════════════════════════════════
    //  BeaconService
    // ════════════════════════════════════════════════════════════════
    public static string BeaconUnavailable =>
        Pick("Ton-Beacon nicht verfügbar.", "Audio beacon not available.", "알림음을 쓸 수 없음.");

    // ════════════════════════════════════════════════════════════════
    //  UIReaderService - Restpunkte (Benachrichtigung, Countdown)
    // ════════════════════════════════════════════════════════════════
    /// <summary>Notification popup hint; <paramref name="key"/> is the configured
    /// accept hotkey so it stays correct after a rebind.</summary>
    public static string NotificationAccept(string key) =>
        Pick($"Benachrichtigung. Mit {key} annehmen.",
             $"Notification. Press {key} to accept.",
             $"알림. {key} 눌러 수락한다.");
    public static string SecondsToJoin(int seconds) =>
        Pick($"Noch {seconds} Sekunden zum Beitreten.",
             $"{seconds} seconds left to join.",
             $"참가까지 {seconds}초 남음.");

    // ════════════════════════════════════════════════════════════════
    //  Nachzuegler aus dem Sprach-Audit 2026-08-03
    //  Alles hier war noch hart deutsch mitten im Service-Code und wurde
    //  gesprochen. Die englischen Fassungen benennen die Sache, sie sind
    //  KEINE gelesenen Client-Begriffe - wo der englische Client ein
    //  anderes Wort fuehrt, gewinnt spaeter das gelesene Wort.
    // ════════════════════════════════════════════════════════════════

    // ── Sammel-Fenster (Gathering) ──────────────────────────────────
    public static string GatherChance(string percent) =>
        Pick($"Chance {percent} Prozent", $"Chance {percent} percent", $"확률 {percent} 퍼센트");
    public static string GatherBonus(string percent) =>
        Pick($"Bonus {percent} Prozent", $"Bonus {percent} percent", $"보너스 {percent} 퍼센트");
    public static string GatherRare   => Pick("rar", "rare", "희귀");
    public static string GatherHidden => Pick("verborgen", "hidden", "숨겨짐");
    /// <summary>Remaining uses of a gathering node ("Belastbarkeit 4 von 4").</summary>
    public static string GatherIntegrity(string current, string max) =>
        Pick($"Belastbarkeit {current} von {max}",
             $"Integrity {current} of {max}",
             $"채집 횟수 {max} 중 {current}");

    // ── Handwerker-Notizbuch (RecipeNote) ───────────────────────────
    //  Die Werte selbst (Klasse, "Stufe 5", Zahlen) sind GELESENER Client-Text
    //  und werden unveraendert durchgereicht - hier stehen nur die Bindewoerter.
    /// <summary>Spoken once when the crafting log opens: window plus the class
    /// whose recipes are shown ("Handwerker-Notizbuch, Alchemist, Stufe 5").</summary>
    public static string RecipeNoteOpened(string jobAndLevel) =>
        Pick($"Handwerker-Notizbuch, {jobAndLevel}",
             $"Crafting log, {jobAndLevel}",
             $"제작수첩, {jobAndLevel}");
    /// <summary>A list row with its position ("Destilliertes Wasser, Stufe 1, 3 von 12").</summary>
    public static string RowWithPosition(string row, int index, int total) =>
        Pick($"{row}, {index} von {total}",
             $"{row}, {index} of {total}",
             $"{row}, {total} 중 {index}");
    /// <summary>Progress needed to finish the craft (client label "Fertig mit").</summary>
    public static string RecipeDifficulty(string value) =>
        Pick($"Fertig mit {value}", $"Progress needed {value}", $"필요 작업량 {value}");
    /// <summary>Durability the craft starts with (client label "Belastbar bis").</summary>
    public static string RecipeDurability(string value) =>
        Pick($"Belastbar bis {value}", $"Durability {value}", $"내구도 {value}");
    public static string RecipeMaxQuality(string value) =>
        Pick($"Qualität maximal {value}", $"Maximum quality {value}", $"최대 품질 {value}");
    /// <summary>Starting quality granted by HQ materials - only said when it is not zero.</summary>
    public static string RecipeStartQuality(string value) =>
        Pick($"Startqualität {value}", $"Starting quality {value}", $"시작 품질 {value}");
    /// <summary>How many can be made from what the player carries.</summary>
    public static string RecipeCraftable(string value) =>
        Pick($"Herstellbar {value}", $"Craftable {value}", $"제작 가능 {value}");
    /// <summary>How many of the RESULT item the player already owns.</summary>
    public static string RecipeInBag(string value) =>
        Pick($"Im Beutel {value}", $"In bag {value}", $"소지품에 {value}");
    /// <summary>One material line. NQ and HQ are always both named (user decision
    /// 2026-08-08): HQ material raises starting quality, so a silent zero would
    /// hide a real choice.</summary>
    public static string RecipeMaterial(string name, string needed, string nq, string hq) =>
        Pick($"{name}, {needed} benötigt, {nq} NQ, {hq} HQ",
             $"{name}, {needed} needed, {nq} NQ, {hq} HQ",
             $"{name}, {needed} 필요, NQ {nq}, HQ {hq}");
    /// <summary>A crystal row. The window shows crystals as icons only - it
    /// carries no name node (ilspycmd 2026-08-08: CrystalNodes has Image but no
    /// Name), so the element stays unnamed rather than guessed.</summary>
    public static string RecipeCrystal(string needed, string owned) =>
        Pick($"Kristall, {needed} benötigt, {owned} im Beutel",
             $"Crystal, {needed} needed, {owned} in bag",
             $"크리스탈, {needed}개 필요, 소지품에 {owned}개");
    /// <summary>Said instead of the values when no recipe is selected yet.</summary>
    public static string RecipeNoSelection =>
        Pick("Kein Rezept ausgewählt.", "No recipe selected.", "고른 제작법 없음.");

    // ── Inventar / Gegenstands-Slots ────────────────────────────────
    /// <summary>An item with its stack count. German needs the "mal" connector,
    /// English just puts the number first.</summary>
    public static string ItemQuantity(string qty, string name) =>
        Pick($"{qty} mal {name}", $"{qty} {name}", $"{name} {qty}개");
    /// <summary>A visible but empty inventory/equipment slot.</summary>
    public static string EmptySlot => Pick("Leer", "Empty", "비어 있음");

    // ── Listen / Reiter ohne eigene Beschriftung ────────────────────
    /// <summary>Icon-only tab: position alone, no label to announce.</summary>
    public static string TabPositionOnly(int index, int count) =>
        Pick($"Reiter {index} von {count}.", $"Tab {index} of {count}.", $"탭 {count} 중 {index}.");
    public static string EmptyList => Pick("Leere Liste.", "Empty list.", "빈 목록.");
    public static string DialogWord => Pick("Dialog.", "Dialog.", "대화 상자.");

    // ── Weltenwahl (TitleDCWorldMap) ────────────────────────────────
    public static string DataCenterRegions(string regions) =>
        Pick($"Datenzentrum wählen. Regionen: {regions}",
             $"Choose a data center. Regions: {regions}",
             $"데이터 센터 선택. 지역: {regions}");

    // ── Gil-Depot (Bank / Gehilfen-Truhe) ───────────────────────────
    public static string BankTitle    => Pick("Gil-Depot", "Gil storage", "길 보관함");
    public static string BankDeposit  => Pick("Hinterlegen", "Deposit", "맡기기");
    public static string BankWithdraw => Pick("Entnehmen", "Withdraw", "찾기");
    public static string BankAmount(string amount) =>
        Pick($"Betrag {amount}.", $"Amount {amount}.", $"금액 {amount}.");
    /// <summary>One balance line: who, the balance now, the balance afterwards.</summary>
    public static string BankBalance(string owner, string now, string after) =>
        Pick($"{owner}: derzeit {now}, danach {after}.",
             $"{owner}: currently {now}, then {after}.",
             $"{owner}: 현재 {now}, 그 뒤 {after}.");
    /// <summary>Label of the storage side of the window (the retainer's chest).</summary>
    public static string BankChestOwner(string name) =>
        Pick($"Truhe {name}", $"Chest {name}", $"{name} 보관함");
    /// <summary>Typing echo: the amount plus the balance it would leave behind.</summary>
    public static string BankAmountWithBalance(string amount, string owner, string after) =>
        Pick($"Betrag {amount}, {owner} danach {after}.",
             $"Amount {amount}, {owner} then {after}.",
             $"금액 {amount}, 그러면 {owner} 잔액 {after}.");

    // ── Chat-Eingabezeile ───────────────────────────────────────────
    public static string ChatInput => Pick("Chat-Eingabe", "Chat input", "대화 입력란");
    public static string ChatInputWithChannel(string channel) =>
        Pick($"Chat-Eingabe, {channel}", $"Chat input, {channel}", $"대화 입력란, {channel}");

    // ── Quest-Detailfenster ─────────────────────────────────────────
    public static string QuestObjectiveText(string objectives) =>
        Pick($"Ziel: {objectives}. ", $"Objective: {objectives}. ", $"목표: {objectives}. ");

    // ── Bestiarium: Lebensraum ──────────────────────────────────────
    // The habitat clause itself is LivesIn (further up) - one wording for both
    // the list overview and the single-row announcement.
    /// <summary>Connector between the spawn areas of one monster.</summary>
    public static string HabitatJoin => Pick(", oder ", ", or ", ", 또는 ");

    // ── Plugin-Liste ────────────────────────────────────────────────
    public static string UnnamedPlugin => Pick("Unbenanntes Plugin", "Unnamed plugin", "이름 없는 플러그인");

    // -- Charaktererstellung: Schritt "Aussehen" ---------------------
    // Die Menue-Namen und die Namen der einzelnen Eintraege kommen aus dem
    // spieleigenen Lobby-Sheet, also in der Client-Sprache. Uebersetzt sind hier
    // nur die Bindewoerter.

    /// <summary>Ein Menuepunkt des Aussehen-Schritts. Ein LEERES Label heisst
    /// "dasselbe Menue wie eben" - "Hautfarbe" auf jedem Pfeiltastendruck zu
    /// wiederholen, waehrend man ueber 192 Farbfelder streicht, ist unbenutzbar.
    /// <paramref name="shape"/> ist die mod-eigene Beschreibung des BILDES auf
    /// einem Icon-Eintrag, oder null wo keine geschrieben ist. Sie steht ganz
    /// hinten, hinter der Position: die Position ist das, wonach der Spieler
    /// steuert, und die Beschreibung ist der Teil, den der naechste
    /// Pfeiltastendruck gefahrlos abschneiden darf.</summary>
    public static string CharaMakeOption(string label, string name, int index, int count, string? shape = null)
    {
        var head = string.IsNullOrEmpty(label) ? string.Empty : label + ", ";
        var body = string.IsNullOrEmpty(name) ? string.Empty : name + ", ";
        if (index <= 0)
            return Pick($"{head}{body}Auswahl unbekannt",
                        $"{head}{body}selection unknown",
                        $"{head}{body}선택을 알 수 없음");
        var text = $"{head}{body}" + Counter(index, count);
        return string.IsNullOrEmpty(shape) ? text : $"{text}, {shape}";
    }

    /// <summary>
    /// EINMAL am Ende der Aussehen-Zusammenfassung gesagt, und nur dann, wenn diese
    /// Zusammenfassung wirklich eine der mod-eigenen Bildbeschreibungen enthielt.
    /// Die Icon-Gitter haben in den Spieldaten weder Namen noch Beschreibung, diese
    /// Worte hat also der Mod geschrieben - und ein blinder Spieler kann Mod-Text
    /// nicht von Spiel-Text unterscheiden, ausser man sagt es ihm. Nicht bei jedem
    /// Pfeiltastendruck: einmal pro Zusammenfassung genuegt, oefter kostet mehr als
    /// es informiert.
    /// </summary>
    public static string CharaMakeAuthoredNote =>
        Pick("Die Bildbeschreibungen stammen vom Mod, nicht vom Spiel.",
             "The picture descriptions come from the mod, not from the game.",
             "그림 설명은 게임이 아니라 모드가 쓴 것이다.");

    /// <summary>
    /// Was Eintrag 1 eines Typ-0-Form-Menues IST. User: *"every type 1 ... had no
    /// description ... I'm assuming this means unmodified, but the mod needs to say
    /// that it's basically the base value for the face."*
    ///
    /// Genau richtig gelesen, und die Daten sagen dasselbe: ein Typ-0-Eintrag ist ein
    /// Morph-Target auf dem Gesichtsmodell, Eintrag 1 ist das unveraenderte Mesh und
    /// die Eintraege 2..N sind die Formen a..N-1. Eintrag 1 fehlt in der Messtabelle
    /// also KONSTRUKTIONSBEDINGT - es gibt keine Verschiebung zu messen, weil er das
    /// ist, wogegen alles andere gemessen wird.
    ///
    /// Schweigen war der falsche Weg, das zu sagen: jeder andere Eintrag bekommt eine
    /// Beschreibung, ein Eintrag ohne liest sich also als "noch nicht geschrieben"
    /// statt als "das ist die Ausgangsform". Das ist KEINE mod-eigene
    /// Bildbeschreibung, sondern eine Aussage ueber die Daten des Spiels, und deshalb
    /// bewusst nicht von <see cref="CharaMakeAuthoredNote"/> abgedeckt.
    /// </summary>
    public static string CharaMakeShapeBase =>
        Pick("unverändert", "unmodified", "기본값");

    /// <summary>Welches Auge eine Farbe betrifft - nur gesprochen, wenn die beiden
    /// sich unterscheiden. Das Spiel hat dafuer keinen eigenen Text: das Lobby-Sheet
    /// benennt den Schalter "Odd Eyes" (Zeile 2125), aber nie die beiden Haelften des
    /// Fensters. Die Worte sind also mod-eigen, und das ist richtig - einem sehenden
    /// Spieler wird die Trennung ausschliesslich dadurch vermittelt, in welche
    /// Haelfte des Fensters er schaut.</summary>
    public static string EyeLeft  => Pick("linkes Auge", "left eye", "왼쪽 눈");

    /// <summary>Siehe <see cref="EyeLeft"/>.</summary>
    public static string EyeRight => Pick("rechtes Auge", "right eye", "오른쪽 눈");

    /// <summary>Eine Farbe OHNE Label und OHNE Position, fuer den Fall dass der
    /// Fokus-Leser die Position schon gesagt hat und dies dahinter eingereiht wird.
    /// Das Gegenstueck zur reinen Bildbeschreibung bei den Icon-Gittern.</summary>
    public static string CharaMakeColourOnly(string colour, int group, int shade, string eye)
    {
        var head = string.IsNullOrEmpty(eye) ? string.Empty : eye + ", ";
        return Pick($"{head}{colour}, Gruppe {group} Ton {shade}",
                    $"{head}{colour}, group {group} shade {shade}",
                    $"{head}{colour}, {group}번 묶음 {shade}번 색조");
    }

    /// <summary>Ein Farbmenue, dessen SCHALTER AUS ist - die Lippenfarbe ohne
    /// aufgetragenen Lippenstift. Keine Farbe, keine Position: auf dem Gesicht ist
    /// nichts zu beschreiben, und eine Feldnummer wuerde den Spieler glauben lassen,
    /// dass doch etwas da ist. <paramref name="state"/> ist das WORT DES SPIELS
    /// dafuer (eine Lobby-Zeile, an der Aufrufstelle gelesen), dieser Satz braucht
    /// also keine eigene Uebersetzung.</summary>
    public static string CharaMakeColourOff(string label, string state)
        => string.IsNullOrEmpty(label) ? state : $"{label}, {state}";

    /// <summary>Ein Farbfeld. Das Farbwort steht mit Absicht vorne: es ist der Teil,
    /// der "wie sieht mein Charakter aus" beantwortet, und der Teil, den ein Spieler
    /// beim Durchstreichen des Gitters braucht, bevor die naechste Ansage ihn
    /// unterbricht. Gruppe und Ton beschreiben den Aufbau der Palette selbst - jede
    /// Palette in human.cmp besteht aus Rampen zu acht Toenen.</summary>
    public static string CharaMakeColour(string label, string? colour, int index, int count, int group, int shade)
    {
        var head = string.IsNullOrEmpty(label) ? string.Empty : label + ", ";
        // Kein Farbwort heisst: die Position ist alles, was die Zeile hat.
        if (colour == null)
            return head + Counter(index, count);
        var pos = $"{Counter(index, count)}, ";
        return Pick($"{head}{colour}, {pos}Gruppe {group} Ton {shade}",
                    $"{head}{colour}, {pos}group {group} shade {shade}",
                    $"{head}{colour}, {pos}{group}번 묶음 {shade}번 색조");
    }

    /// <summary>Ein 0-100-Schieberegler. Die beiden Endbezeichnungen sind die Worte
    /// des Spiels fuer die Extreme ("Klein"/"Gross"), und die sind es, die einer
    /// nackten Zahl ueberhaupt Bedeutung geben.</summary>
    public static string CharaMakeSlider(string label, int value, string low, string high)
    {
        // Die Endbezeichnungen fallen weg, sobald der Spieler in EINEM Regler
        // arbeitet: "Klein bis Gross" braucht man einmal, nicht bei jedem Schritt.
        if (string.IsNullOrEmpty(low) || string.IsNullOrEmpty(high))
            return Pick($"{label}, {value} von 100",
                        $"{label}, {value} of 100",
                        $"{label}, 100 중 {value}");
        return Pick($"{label}, {value} von 100, {low} bis {high}",
                    $"{label}, {value} of 100, {low} to {high}",
                    $"{label}, 100 중 {value}, {low}부터 {high}까지");
    }

    /// <summary>Ein Schalter der Gesichtsmerkmals-Bitmaske. Die Zahl ist das BIT,
    /// nicht eine Menueposition: das Sheet sagt nicht, welches Bit zu welchem
    /// Typ-4-Menue gehoert, also wird nichts zugeschrieben, was sich nicht belegen
    /// laesst.</summary>
    public static string CharaMakeFeatureBit(string label, int number, bool on) =>
        Loc.IsKorean ? $"{label} {number}, {(on ? "켜짐" : "꺼짐")}"
        : IsGerman ? $"{label} {number}, {(on ? "an" : "aus")}"
                 : $"{label} {number}, {(on ? "on" : "off")}";

    /// <summary>Derselbe Schalter, aber mit NAMEN statt Nummer. User: *"facial
    /// features have no descriptions, and neither do tattoos. those need descriptions
    /// so the player knows what they are toggling off and on."* Erreichbar, seit die
    /// Zuordnung Reihe-zu-Bit im Spiel gemessen wurde: das 5-Eintraege-Menue
    /// Gesichtsmerkmale, Eintrag "1 von 5", kippte Bit 0 - also Reihe i = Bit
    /// i-1.</summary>
    public static string CharaMakeFeatureNamed(string label, int number, string what, bool on) =>
        Loc.IsKorean ? $"{label} {number}, {what}, {(on ? "켜짐" : "꺼짐")}"
        : IsGerman ? $"{label} {number}, {what}, {(on ? "an" : "aus")}"
                 : $"{label} {number}, {what}, {(on ? "on" : "off")}";

    /// <summary>Eine Typ-4-Reihe beim MARKIEREN: was sie ist und ob sie gerade an
    /// ist. Die Position spricht der Fokus-Leser bereits, hier stehen nur die beiden
    /// Teile, die er nicht wissen kann.</summary>
    public static string CharaMakeFeatureRow(string what, bool on) =>
        Loc.IsKorean ? $"{what}, {(on ? "켜짐" : "꺼짐")}"
        : IsGerman ? $"{what}, {(on ? "an" : "aus")}" : $"{what}, {(on ? "on" : "off")}";

    /// <summary>Nur der Zustand, fuer ein Merkmal ohne geschriebene Beschreibung -
    /// "aus" ist auch dann die Ansage wert, wenn sich die Sache nicht benennen
    /// laesst.</summary>
    public static string CharaMakeFeatureState(bool on) =>
        Loc.IsKorean ? (on ? "켜짐" : "꺼짐")
        : IsGerman ? (on ? "an" : "aus") : (on ? "on" : "off");

    public static string CharaMakeFeatureLabel => Pick("Merkmal", "Feature", "특징");

    /// <summary>Eine Aussehen-Kategorie, deren aktueller Wert sich nicht als EINE
    /// Position benennen laesst - die Typ-4-Bitmasken-Menues, bei denen das Sheet
    /// nicht sagt, welches Bit zu welchem Menue gehoert. Die Anzahl ist das, was sich
    /// ehrlich sagen laesst, und der User hat genau danach gefragt: *"the total number
    /// of selections per value is useful information to have"*.</summary>
    public static string CharaMakeCategory(string label, int count) =>
        Pick($"{label}, {count} Einträge", $"{label}, {count} entries", $"{label}, 항목 {count}개");

    /// <summary>
    /// Die ZWEITE Achse des Stimmen-Waehlers - die Hoerprobe, die das Spiel abspielt,
    /// damit Stimmen vergleichbar sind (User: *"there are categories like laugh,
    /// grunt, thinking etc"*). NUR POSITION, solange das Spiel keinen Namen liefert:
    /// die sieben Knoepfe sind reine Icon-Radiobuttons ohne Textknoten irgendwo im
    /// Fenster, und die beiden Sheets, die nach den Namen aussahen, enthalten sie
    /// nicht. Eine erfundene Liste waere eine selbstbewusste Luege darueber, was der
    /// Spieler gerade hoert.
    ///
    /// Die Position ist hier NICHT entbehrlich, auch wenn das Kopfwort "Hoerprobe"
    /// die Art der Zeile schon nennt: am User gemessen sagten mit abgeschalteten
    /// Positionen alle sieben Zeilen nur das eine Wort, und der Wechsel zwischen
    /// ihnen war nicht hoerbar.
    /// </summary>
    public static string CharaMakeVoiceSample(string name, int index, int count)
    {
        var head = Pick("Hörprobe", "Sample", "미리 듣기");
        var lead = string.IsNullOrEmpty(name) ? head : $"{head}, {name}";
        return $"{lead}, {Counter(index, count)}";
    }

    /// <summary>Die Klasse, die die Erstellungs-Vorschau gerade zeigt. Nur der Name -
    /// die BESCHREIBUNG der Klasse liegt wie bei jedem anderen Erstellungsschritt auf
    /// der Vorlese-Taste.</summary>
    public static string CharaMakeClass(string name) => name;

    /// <summary>Ersatzbezeichnung fuer den Stimmen-Waehler. Normal wird das
    /// Lobby-Label des Spiels benutzt; das hier deckt eine Zeile ohne Stimmen-Menue
    /// ab.</summary>
    public static string CharaMakeVoiceLabel => Pick("Stimme", "Voice", "목소리");

    /// <summary>
    /// Markiert in der Aussehen-Zusammenfassung einen Wert, den das SPIEL selbst
    /// geaendert hat, nicht der Spieler. Die Charaktererstellung bildet Werte wirklich
    /// menueuebergreifend um - Hrothgar-Gesicht 1 zu waehlen verschiebt das
    /// Frisur-Byte mit - und genau so etwas kann ein blinder Spieler sonst nicht
    /// bemerken. Bewusst NICHT in dem Moment gesprochen, in dem es passiert: das
    /// wuerde die Position unterbrechen, nach der der Spieler gerade steuert, und
    /// zwar ueber ein Menue, in dem er gar nicht ist.
    /// </summary>
    public static string CharaMakeChangedByGame =>
        Pick("vom Spiel geändert", "changed by the game", "게임이 바꿈");

    /// <summary>Gesagt, wenn das Aussehen nicht gelesen werden kann, weil das
    /// Vorschau-Modell nicht eindeutig ist. Niemals Schweigen: der Spieler koennte
    /// das nicht von "nichts zu melden" unterscheiden.</summary>
    public static string CharaMakeNoPreview =>
        Pick("Vorschau-Modell nicht eindeutig, Aussehen nicht lesbar.",
             "Preview model not identifiable, appearance cannot be read.",
             "미리보기 모델을 특정할 수 없어 외모를 읽을 수 없음.");

    // ── Tiefes Gewoelbe ─────────────────────────────────────────────
    //
    // Jeder NAME und jede BESCHREIBUNG, die im Gewoelbe gesprochen wird, ist die des
    // Spiels und kommt aus dessen eigenen Sheets. Die Woerter hier sind nur der Rahmen
    // darum: welche Art von Wirkung eine Zeile ist, wo ein Platz sitzt, ob er leer ist.

    /// <summary>Ein ebenenweiter Zustand (DeepDungeonStatus).</summary>
    public static string DeepKindFloor => Pick("Ebene", "Floor", "층");

    /// <summary>Ein ebenenweites Verbot (DeepDungeonBan).</summary>
    public static string DeepKindBan => Pick("Verbot", "Restriction", "제한");

    /// <summary>Eine ebenenweite Gefahr (DeepDungeonDanger).</summary>
    public static string DeepKindDanger => Pick("Gefahr", "Hazard", "위험");

    /// <summary>Pilgerpfad: die auf dieser Ebene laufende Besonderheit.</summary>
    public static string DeepKindGimmick => Pick("Diese Ebene", "This floor", "이 층");

    /// <summary>Pilgerpfad: die fuer die naechste Ebene vorgemerkte Besonderheit.</summary>
    public static string DeepKindGimmickNext => Pick("Nächste Ebene", "Next floor", "다음 층");

    /// <summary>Eine laufende Pomander-Wirkung.</summary>
    public static string DeepKindItemEffect => Pick("Gegenstand", "Item effect", "아이템 효과");

    /// <summary>
    /// Welches Gewoelbe und welche Ebene davon. Beide Hauptwoerter gehoeren dem SPIEL -
    /// der Name des Gewoelbes und das Wort, mit dem der Ergebnisschirm die Zahl
    /// beschriftet - hier kommt nur die Wortstellung dazu.
    /// </summary>
    public static string DeepFloorLine(string dungeon, string floorWord, int floor)
    {
        var number = floorWord.Length > 0 ? $"{floorWord} {floor}" : floor.ToString();
        return dungeon.Length > 0 ? $"{dungeon}, {number}" : number;
    }

    /// <summary>
    /// Gesagt, wenn die Ebenen-Taste ausserhalb eines Tiefen Gewoelbes gedrueckt wird.
    ///
    /// Sie ANTWORTET, statt still zu bleiben: Stille ist die eine Reaktion, die ein
    /// blinder Spieler nicht von einer kaputten Taste unterscheiden kann, und diese
    /// Taste wird aus Gewohnheit gedrueckt, sobald ein Lauf vorbei ist.
    /// </summary>
    public static string DeepFloorOutside =>
        Pick("Kein Tiefes Gewölbe.", "Not in a deep dungeon.", "딥 던전이 아님.");

    /// <summary>Kategorie-Bezeichnung fuer die Raumliste im Tiefen Gewoelbe.</summary>
    public static string DeepCategoryRooms => Pick("Räume", "Rooms", "방");

    /// <summary>Kategorie-Bezeichnung fuer die Truhen einer Ebene.</summary>
    public static string DeepCategoryTreasure => Pick("Schätze", "Treasure", "보물상자");

    /// <summary>Kategorie-Bezeichnung fuer die beiden Leuchten.</summary>
    public static string DeepCategoryCairns => Pick("Leuchten", "Cairns", "석탑");

    /// <summary>Ein Raum, nach dem spieleigenen Index dafuer.</summary>
    public static string DeepRoomName(int index) => Pick($"Raum {index}",
                                                         $"Room {index}",
                                                         $"{index}번 방");

    /// <summary>Markiert den Raum, in dem der Spieler steht.</summary>
    public static string DeepRoomYouAreHere => Pick("hier", "you are here", "현재 위치");

    /// <summary>Markiert den Startraum der Ebene (RoomFlags.Home).</summary>
    public static string DeepRoomStart => Pick("Startraum", "starting room", "시작 방");

    /// <summary>Wie viele Truhen der Director in einen Raum legt, im spieleigenen Wort
    /// fuer eine Truhe.</summary>
    public static string DeepRoomCoffers(int count, string cofferWord) =>
        count == 1 ? $"1 {cofferWord}" : $"{count} {cofferWord}";

    /// <summary>Wohin ein Raum sich oeffnet, aus seinen eigenen Verbindungs-Flags.</summary>
    public static string DeepRoomExits(System.Collections.Generic.IEnumerable<string> directions) =>
        (Pick("Ausgänge ", "exits ", "출구 ")) + string.Join(", ", directions);

    public static string DirNorth => Pick("Norden", "north", "북");
    public static string DirEast  => Pick("Osten", "east", "동");
    public static string DirSouth => Pick("Süden", "south", "남");
    public static string DirWest  => Pick("Westen", "west", "서");

    /// <summary>
    /// Ein aufgedeckter Raum, fuer den es keinen begehbaren Punkt gibt.
    ///
    /// DIE FORMULIERUNG IST DIE KORREKTUR. Hier stand "noch nicht betreten", und das ist
    /// eine Behauptung ueber den SPIELER, die falsch sein kann - er kann laengst
    /// durchgelaufen sein, waehrend das Plugin neu geladen wurde. Was das Plugin
    /// tatsaechlich weiss, ist enger und betrifft es SELBST: der Director gibt Raeumen
    /// keine Koordinaten, der einzige begehbare Punkt ist also einer, auf dem es den
    /// Spieler stehen gesehen hat.
    /// </summary>
    public static string DeepRoomNoRoute => Pick("kein Weg bekannt", "no route known", "아는 길 없음");

    /// <summary>Ein Teil der Ebene, den das Spiel nicht als aufgedeckt fuehrt. Traegt ein
    /// Ziel und NICHTS darueber, was darin ist.</summary>
    public static string DeepRoomUnexplored(int index) =>
        Pick($"Unerforscht {index}", $"Unexplored {index}", $"미탐색 {index}번");

    /// <summary>Gesagt, wenn die Raumliste abgefragt wird und die Ebene keine hergibt -
    /// ausserhalb eines Gewoelbes, oder solange den Raumdaten nicht zu trauen ist.</summary>
    public static string DeepNoRooms =>
        Pick("Keine Raumdaten.", "No room data.", "방 정보 없음.");

    /// <summary>
    /// Wo ein Platz in seinem Abschnitt sitzt ("3 von 16"), oder "", wenn der Spieler
    /// Listenpositionen abgeschaltet hat - der Aufrufer muss dann auch das Trennzeichen
    /// weglassen, sonst endet die Zeile in einem haengenden Komma.
    /// </summary>
    public static string DeepSlotPosition(int index, int count) => Counter(index, count);

    /// <summary>
    /// Eine Aetherpool-Zeile mit der Staerke, die das Fenster nur im Symbol zeichnet.
    /// Der NAME ist der des Spiels, und die Form "+N" ebenfalls - dessen eigene
    /// Chat-Zeile lautet *"Deine Aetherpool-Waffe flackert. Ihre Stärke ist jetzt +5."*
    /// </summary>
    public static string DeepGearStrength(string name, int strength) => $"{name} +{strength}";

    /// <summary>Ein Platz, den dieses Gewoelbe gar nicht benutzt.</summary>
    public static string DeepSlotEmpty => Pick("leerer Platz", "empty slot", "빈 칸");

    /// <summary>
    /// Ein Platz, den das Gewoelbe SEHR WOHL benutzt, mit der Anzahl im Besitz -
    /// einschliesslich keiner. Null ist hier eine echte Antwort und kein Grund zu
    /// schweigen: das Fenster zeichnet das Symbol ausgegraut, ein sehender Spieler sieht
    /// also, WOFUER der Platz ist, bevor er einen besitzt.
    /// </summary>
    public static string DeepSlotCount(string name, int count) =>
        Pick($"{name} mal {count}", $"{name} times {count}", $"{name} {count}개");

    /// <summary>Ein Pomander, dessen Wirkung gerade laeuft.</summary>
    public static string DeepEffectActive => Pick("aktiv", "active", "적용 중");

    /// <summary>Ein Pomander, dessen Wirkung nicht laeuft.</summary>
    public static string DeepEffectInactive => Pick("nicht aktiv", "not active", "적용 안 됨");

    /// <summary>Ein Pomander, den das Spiel gerade verweigert (Items[i].IsUsable false).</summary>
    public static string DeepItemUnusable => Pick("nicht verwendbar", "not usable", "쓸 수 없음");
}
