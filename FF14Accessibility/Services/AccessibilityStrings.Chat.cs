using System;

namespace FF14Accessibility.Services;

/// <summary>
/// Die Sprachbausteine für die Chat-Puffer und für das Einstellungsmenü.
///
/// Bewusst eine eigene Datei und deshalb <c>partial</c>: die Hauptdatei ist groß und
/// ändert sich in fast jeder Version, und diese Erweiterung soll sich in einem Stück
/// wieder entfernen lassen. In <c>AccessibilityStrings.cs</c> steht dafür nur ein
/// einziges zusätzliches Wort - <c>partial</c>.
///
/// Gleiches Muster wie dort: eine Sprache pro Zeile, umgeschaltet über
/// <see cref="Loc.IsGerman"/>, damit "/acc lang" auch hier greift.
/// </summary>
public static partial class AccessibilityStrings
{
    // ── Menü-Rahmen (SpokenMenu) ──────────────────────────────────

    /// <summary>Kopfzeile beim Öffnen eines Menüs - der Titel und sonst nichts. Die
    /// Anzahl der Einträge steht nicht dabei: die unmittelbar folgende Zeile trägt
    /// ihre eigene Position ("1 von 15"), siehe <see cref="MenuEntry"/>.</summary>
    public static string MenuOpened(string title) => $"{title}.";

    /// <summary>Eine Menüzeile mit ihrer Position.</summary>
    public static string MenuEntry(string label, int index, int count) =>
        RowWithPosition(label, index, count);

    public static string MenuClosed => Pick("Menü geschlossen.", "Menu closed.", "메뉴 닫힘.");

    public static string MenuEmpty => Pick("Keine Einträge.", "No entries.", "항목 없음.");

    // ── Umsortieren: Zeile aufnehmen, schieben, ablegen ───────────
    //
    // Jede dieser vier Ansagen nennt die Position mit, weil sie das Einzige ist,
    // was den Zustand verrät: eine aufgenommene Zeile sieht man nicht, und ob
    // sie sich beim letzten Druck bewegt hat, hört man sonst nirgends.
    // "aufgenommen" und "abgelegt" sind die Klammern darum - solange sie nicht
    // gefallen ist, ist die Zeile noch in der Hand.

    /// <summary>Beim Aufnehmen einer Zeile: "Gegner aufgenommen, 4 von 21".</summary>
    public static string MenuGrabbed(string label, int index, int count) =>
        IsGerman ? $"{label} aufgenommen, {index} von {count}."
                 : $"{label} picked up, {index} of {count}.";

    /// <summary>Nach jedem Schritt: "Gegner, jetzt 3 von 21".</summary>
    public static string MenuMovedTo(string label, int index, int count) =>
        IsGerman ? $"{label}, jetzt {index} von {count}."
                 : $"{label}, now {index} of {count}.";

    /// <summary>Wenn es in diese Richtung nicht weitergeht. Sagt die Position
    /// erneut, damit ein Druck ins Leere nicht wie ein verschluckter klingt.</summary>
    public static string MenuMoveEnd(string label, int index, int count) =>
        IsGerman ? $"{label} bleibt auf {index} von {count}."
                 : $"{label} stays at {index} of {count}.";

    /// <summary>Beim Ablegen: "Gegner abgelegt auf Platz 2".</summary>
    public static string MenuDropped(string label, int index) =>
        IsGerman ? $"{label} abgelegt auf Platz {index}."
                 : $"{label} dropped at position {index}.";

    /// <summary>
    /// Wird an jede Verschiebe-Ansage angehängt und sagt, WOZWISCHEN die
    /// aufgenommene Zeile jetzt steht: " zwischen Händler und Spieler."
    ///
    /// Sortiert wird nicht nach Platznummern, sondern nach Nachbarschaft - "die
    /// Gegner sollen gleich hinter Alles kommen". Die Platznummer allein
    /// beantwortet diese Frage nicht, und ohne die Nachbarn musste der Spieler
    /// die Zeile ablegen, die Liste ablaufen und sie wieder aufnehmen, nur um zu
    /// hören, wo er gelandet war (Wunsch vom 2026-08-26).
    ///
    /// AN DEN ENDEN wird nur genannt, was wirklich da ist. Dass Platz 1 der
    /// erste ist, steht schon in "1 von 21" - ein zusätzliches "ganz vorn" wäre
    /// dieselbe Auskunft zum zweiten Mal. Bleibt eine Liste mit nur einer Zeile,
    /// gibt es keine Nachbarn und der Satz entfällt ganz.
    ///
    /// Führendes Leerzeichen, weil er immer angehängt wird.
    /// </summary>
    /// <param name="before">Die Zeile darüber, oder leer am Anfang der Liste.</param>
    /// <param name="after">Die Zeile darunter, oder leer am Ende der Liste.</param>
    public static string MenuBetween(string before, string after)
    {
        var hasBefore = before.Length > 0;
        var hasAfter  = after.Length > 0;

        if (hasBefore && hasAfter)
            return IsGerman ? $" Zwischen {before} und {after}." : $" Between {before} and {after}.";
        if (hasBefore)
            return IsGerman ? $" Hinter {before}." : $" After {before}.";
        if (hasAfter)
            return IsGerman ? $" Vor {after}." : $" Before {after}.";
        return string.Empty;
    }

    // ── Einstellungsmenü ──────────────────────────────────────────

    public static string OptionsTitle => Pick("Einstellungen", "Settings", "설정");
    public static string OptionsSounds => Pick("Töne", "Sounds", "소리");
    public static string OptionsAnnouncements => Pick("Ansagen", "Announcements", "안내");

    /// <summary>Eine An/Aus-Zeile: "Kartenmarkierung, an".</summary>
    public static string OptionToggle(string name, bool on) =>
        Loc.IsKorean ? $"{name}, {(on ? "켜짐" : "꺼짐")}"
        : IsGerman ? $"{name}, {(on ? "an" : "aus")}" : $"{name}, {(on ? "on" : "off")}";

    /// <summary>Wird im Moment des Umschaltens gesprochen. <c>Rebuild</c> frischt nur
    /// die Beschriftung auf und liest die Zeile bewusst nicht erneut vor - ohne diese
    /// Ansage wäre das Umschalten also stumm.</summary>
    public static string OptionToggled(string name, bool on) =>
        Loc.IsKorean ? $"{name} {(on ? "켜짐" : "꺼짐")}."
        : IsGerman ? $"{name} {(on ? "an" : "aus")}." : $"{name} {(on ? "on" : "off")}.";

    /// <summary>Eine Lautstärke-Zeile: "Beacon, 35 Prozent" oder "Beacon, aus".</summary>
    public static string OptionVolume(string name, float volume) =>
        volume <= 0f
            ? (Pick($"{name}, aus", $"{name}, off", $"{name}, 꺼짐"))
            : (Pick($"{name}, {(int)MathF.Round(volume * 100)} Prozent",
                    $"{name}, {(int)MathF.Round(volume * 100)} percent",
                    $"{name}, {(int)MathF.Round(volume * 100)} 퍼센트"));

    /// <summary>Eine einzelne Stufe im Lautstärke-Untermenü.</summary>
    public static string VolumeStep(float volume) =>
        volume <= 0f
            ? (Pick("Aus", "Off", "꺼짐"))
            : (Pick($"{(int)MathF.Round(volume * 100)} Prozent",
                    $"{(int)MathF.Round(volume * 100)} percent",
                    $"{(int)MathF.Round(volume * 100)} 퍼센트"));

    public static string VolumeSet(string name, float volume) =>
        volume <= 0f
            ? (Pick($"{name} aus.", $"{name} off.", $"{name} 꺼짐으로 변경됨."))
            : (Pick($"{name} auf {(int)MathF.Round(volume * 100)} Prozent.",
                    $"{name} at {(int)MathF.Round(volume * 100)} percent.",
                    $"{name} {(int)MathF.Round(volume * 100)} 퍼센트로 변경됨."));

    // Namen der einzelnen Einstellungen. Jede Zeile hier hat ein Feld in
    // Configuration und einen Dienst dahinter, der es liest - eine Beschriftung ohne
    // Funktion dahinter ist genau der Weg, auf dem eine tote Einstellung eine
    // Überarbeitung überlebt.
    // "beim Laufen" steht bewusst in der Beschriftung: der Schalter hiess bis
    // v5.89 "Peil-Ton auf das Ziel", und genau so hat er sich auch verhalten -
    // er lief bei jedem anvisierten Ziel, im Kampf also ununterbrochen. Seit
    // 2026-08-23 laeuft er nur waehrend eines Laufs, und die Beschriftung muss
    // das sagen, sonst sucht der Spieler den Fehler beim Ton statt beim Lauf.
    /// <summary>Schalter für den Peil-Ton, der während eines Laufs die Richtung hält.</summary>
    public static string OptTargetBeacon => Pick("Peil-Ton beim Laufen",
                                                 "Navigation beacon",
                                                 "길안내 신호음");

    // Heisst weiterhin so, weil es die LAUTSTAERKE desselben Tons ist.
    public static string OptBeacon => Pick("Lautstärke Peil-Ton",
                                           "Navigation beacon volume",
                                           "길안내 신호음 음량");

    /// <summary>Schalter für die HP- und MP-Töne (jede 10-Prozent-Stufe).</summary>
    public static string OptVitalCues => Pick("Töne für Leben und Mana",
                                              "Health and mana tones",
                                              "HP MP 신호음");

    /// <summary>Lautstärke der HP- und MP-Töne.</summary>
    public static string OptVitalCueVolume =>
        Pick("Lautstärke Leben und Mana", "Health and mana tone volume", "HP MP 신호음 음량");
    public static string OptRouteCues => Pick("Wegpunkt- und Ankunftston",
                                              "Waypoint and arrival cues",
                                              "경유지와 도착 알림음");

    // AoE-Warnung: die Lautstärke gab es als Konfigurationswert schon lange, aber
    // in keinem Menü - sie war damit nicht erreichbar. Der Klang kam 2026-08-21
    // auf Wunsch des Spielers dazu.
    public static string OptAoeWarnVolume => Pick("Lautstärke AoE-Warnung",
                                                  "AoE warning volume",
                                                  "장판 경고음 음량");

    public static string OptAoeWarnTone => Pick("Klang AoE-Warnung", "AoE warning sound", "장판 경고음");

    /// <summary>Name einer Warnton-Stimme, wie ihn die Auswahl vorliest.
    /// Beschreibend statt technisch: "300 Hertz mit Obertönen" sagt niemandem,
    /// wie etwas klingt.</summary>
    public static string AoeToneName(AoeWarnTone tone) => tone switch
    {
        AoeWarnTone.Bright => Pick("Hell (bisheriger Klang)",
                                   "Bright (previous sound)",
                                   "맑은 소리 (이전 소리)"),
        AoeWarnTone.Soft   => Pick("Weich", "Soft", "부드러운 소리"),
        AoeWarnTone.Deep   => Pick("Tiefes Brummen", "Deep hum", "낮은 울림"),
        AoeWarnTone.Wave   => Pick("An- und abschwellend", "Swelling", "물결"),
        _                  => Pick("Unbekannt", "Unknown", "알 수 없음"),
    };

    /// <summary>Die Zeile, die das Untermenü öffnet: "Klang AoE-Warnung, Weich".</summary>
    public static string OptionChoice(string name, string value) =>
        Pick($"{name}, {value}", $"{name}, {value}", $"{name}, {value}");

    /// <summary>Bestätigung nach der Wahl eines Klangs.</summary>
    public static string AoeToneSet(string value) =>
        Pick($"Warnton {value}.", $"Warning sound {value}.", $"경고음 {value}.");

    // ── Warnstimme (zweiter Sprachkanal) ──────────────────────────────────────
    // Sie heißt im Menü nicht "SAPI": für den Spieler zählt, WAS sie tut - dass
    // die Kampfwarnungen an der Sprachausgabe vorbeigehen und dort nicht mehr
    // abgeschnitten werden können.
    public static string OptWarningVoice => Pick("Eigene Stimme für Kampfwarnungen",
                                                 "Separate voice for combat warnings",
                                                 "전투 경고 전용 음성");

    public static string OptWarningVoiceVolume => Pick("Lautstärke Warnstimme",
                                                       "Warning voice volume",
                                                       "경고 음성 음량");

    public static string OptWarningVoiceRate => Pick("Tempo Warnstimme",
                                                     "Warning voice speed",
                                                     "경고 음성 속도");

    public static string OptWarningVoiceName => Pick("Stimme für Kampfwarnungen",
                                                     "Voice for combat warnings",
                                                     "전투 경고 음성");

    /// <summary>Der Satz, den eine Stimme zur Probe spricht. Eine echte Warnung
    /// und kein "Test eins zwei": beurteilt werden soll, ob man SIE im Kampf
    /// versteht.</summary>
    public static string WarningVoiceSample => Pick("Kegel von vorne. Nach rechts ausweichen, sieben Meter.",
                                                    "Cone from the front. Dodge right, seven metres.",
                                                    "앞쪽에서 부채꼴. 오른쪽으로 7미터 회피.");

    /// <summary>Steht in der Stimmenauswahl, wenn keine eigene gewählt ist.</summary>
    public static string WarningVoiceAutomatic => Pick("Automatisch", "Automatic", "자동");

    /// <summary>Bestätigung nach der Wahl einer Stimme - nur nötig, wenn die
    /// Probe stumm blieb.</summary>
    public static string WarningVoiceSet(string value) =>
        Pick($"Warnstimme {value}.", $"Warning voice {value}.", $"경고 음성 {value}.");

    /// <summary>Steht statt der Auswahl, wenn das System gar keine Sprachausgabe
    /// anbietet. Eine leere Liste ohne Erklärung wäre von einem Fehler des
    /// Plugins nicht zu unterscheiden.</summary>
    public static string WarningVoiceUnavailable => Pick("Keine Sprachausgabe des Systems verfügbar. Die Kampfwarnungen kommen über den Screenreader.",
                                                         "No system speech available. Combat warnings go through the screen reader.",
                                                         "시스템 음성 없음. 전투 경고는 스크린 리더로 출력.");

    /// <summary>
    /// Die wählbaren Tempostufen der Warnstimme. SAPI kennt -10 bis 10; die
    /// Enden sind bewusst NICHT dabei - bei -10 dauert eine Warnung länger als
    /// der Zauber, den sie ankündigt, und bei 10 ist der Satz auch für ein
    /// geübtes Ohr nicht mehr sicher zu verstehen. Sieben Stufen reichen, um
    /// die eigene Grenze zu finden, ohne sich durch zwanzig zu hören.
    /// </summary>
    public static readonly int[] VoiceRateSteps = { -4, -2, 0, 2, 4, 6, 8 };

    /// <summary>Name einer Tempostufe. Beschreibend statt technisch: "Tempo 6"
    /// sagt niemandem, wie schnell das ist.</summary>
    public static string VoiceRateName(int rate) => rate switch
    {
        <= -4 => Pick("Sehr langsam", "Very slow", "매우 느림"),
        -3 or -2 => Pick("Langsam", "Slow", "느림"),
        -1 or 0 => Pick("Normal", "Normal", "보통"),
        1 or 2 => Pick("Etwas schneller", "Slightly faster", "조금 빠름"),
        3 or 4 => Pick("Schnell", "Fast", "빠름"),
        5 or 6 => Pick("Sehr schnell", "Very fast", "매우 빠름"),
        _ => Pick("Am schnellsten", "Fastest", "가장 빠름"),
    };

    /// <summary>Bestätigung nach der Wahl einer Tempostufe - nur nötig, wenn die
    /// Probe stumm blieb.</summary>
    public static string VoiceRateSet(string value) =>
        Pick($"Tempo {value}.", $"Speed {value}.", $"속도 {value}.");

    public static string OptSkillReady => Pick("Fähigkeit bereit", "Ability ready", "기술 준비됨");
    public static string OptSkillReadyVolume => Pick("Fähigkeit bereit Lautstärke",
                                                     "Ability ready volume",
                                                     "기술 준비됨 음량");
    public static string OptHeading => Pick("Himmelsrichtung", "Compass heading", "방향 안내");
    public static string OptTargetChanges => Pick("Zielwechsel", "Target changes", "대상 변경");
    public static string OptTargetHp => Pick("Ziel-Lebenspunkte", "Target health", "대상 HP");
    public static string OptEnemyCast => Pick("Gegner wirkt Aktion", "Enemy casting", "적 기술 시전");
    public static string OptFineHpDuringLeve => Pick("Feine Ziel-Lebenspunkte im Freibrief",
                                                     "Fine target health during levequests",
                                                     "길드 의뢰 중 대상 HP 자세히");
    public static string OptMapFlag => Pick("Kartenmarkierung", "Map flag", "지도 표식");
    public static string OptErrorToasts => Pick("Fehlermeldungen", "Error messages", "오류 메시지");
    public static string OptInfoToasts => Pick("Hinweismeldungen", "Notice messages", "알림 메시지");

    // ── Namen der Puffer ──────────────────────────────────────────
    //
    // Ein Kanal-Puffer und ein Register-Puffer bekommen ihren Namen vom SPIEL - aus
    // der LogFilter-Zeile beziehungsweise aus dem, was der Spieler selbst als
    // Registernamen eingetippt hat. Übersetzt wird davon nichts. Nur die drei Puffer,
    // die keine Register sind, tragen einen Namen vom Plugin, und jeder sagt, was er
    // ist, nicht was ihn füllt.

    /// <summary>Der Puffer für Dialogfenster. Der eine Puffer, der kein Chat-Register
    /// ist und keines werden kann: der Chat bekommt die Zeile eines NPC erst, wenn der
    /// Spieler weitergeklickt hat, ein aus dem Chat gefüllter Dialogpuffer hinkte dem
    /// Bildschirm also immer einen Schritt hinterher. Gefüllt wird er statt dessen von
    /// den Talk- und _BattleTalk-Lesern.</summary>
    public static string BufferDialogue => Pick("Dialoge", "Dialogue", "대화");

    /// <summary>Die eigenen Meldungen des Plugins - Toasts, Abmelde-Countdown,
    /// Fensteransagen. Die liefen nie über den Chatlog, also hält sie kein
    /// Register.</summary>
    public static string BufferSystem => Pick("Meldungen", "Notices", "알림");

    /// <summary>Der einzelne Sammelpuffer, der nur benutzt wird, solange die
    /// Chatfilter des Spiels nicht lesbar sind. Siehe
    /// <see cref="ChatFiltersUnavailable"/>.</summary>
    public static string BufferChat => Pick("Chat", "Chat", "대화");

    /// <summary>
    /// Ein ganzes Chat-Register in Ankunftsreihenfolge - das, was ein sehender Spieler
    /// sieht, wenn er auf dieses Register schaut.
    ///
    /// DAS IST DER EINZIGE SELBST VERGEBENE NAME IN DER PUFFERLISTE, und das ist
    /// Absicht. Jeder andere Puffer wird vom Spiel benannt. Für "das ganze Register"
    /// hat das Spiel kein Wort, weil es keine Pufferliste hat - es zeichnet das
    /// Register, und ein Auge überfliegt es. Der Addon-Sheet-Block der
    /// Chat-Einstellungen wurde daraufhin durchgesehen (Zeilen 1205-1290): dort stehen
    /// "Alle auswählen" und "Alle abwählen" für die Voreinstellungsknöpfe, aber nichts,
    /// was eine Ansicht benennt.
    ///
    /// Dass der Name vom Plugin kommt, ist der sichtbare Hinweis darauf, dass auch die
    /// GRUPPIERUNG vom Plugin kommt. Der INHALT nicht: eine Zeile liegt genau dann
    /// hier, wenn die Filterdaten des Spiels sagen, dass dieses Register sie zeigt.
    /// </summary>
    public static string BufferTabAll => Pick("Alles", "All", "전체");

    /// <summary>
    /// Derselbe Puffer, aber mit dem Register davor: "Allgemein, alles".
    ///
    /// Beim Nachlesen reicht "Alles", weil das Register der Kontext ist, in dem
    /// man gerade steht. In der Sortierliste des Einstellungsmenüs stehen die
    /// Register aller Reiter untereinander - dort wären es mehrere Zeilen, die
    /// gleich heißen und Verschiedenes bedeuten.
    /// </summary>
    public static string BufferTabAllOf(string tabName) =>
        IsGerman ? $"{tabName}, alles" : $"{tabName}, all";

    /// <summary>
    /// Wird EINMAL gesagt, wenn der Filterzustand des Spiels nicht gelesen werden kann.
    /// Die Alternative wäre, dass die Pufferliste ohne Angabe eines Grundes falsch
    /// aussieht. Es landet weiterhin alles in einem Puffer und alles außer dem
    /// Kampflog wird weiterhin gesprochen - das Plugin fällt hörbar zurück, es wird
    /// nicht still.
    /// </summary>
    public static string ChatFiltersUnavailable =>
        Pick("Die Chat-Einstellungen des Spiels sind nicht lesbar. Der Chat läuft in einem Puffer.",
             "The game's chat settings cannot be read. Chat is going to one buffer.",
             "게임의 대화창 설정을 읽을 수 없음. 대화가 한 곳으로만 모임.");

    // ── Register wechseln, und was im neuen Register liegt ────────

    /// <summary>
    /// Wird gesagt, nachdem das Plugin das Chat-Register des Spiels gewechselt hat:
    /// welches Register es jetzt ist, wie viele seiner Puffer etwas enthalten, und der
    /// erste davon mit seiner Anzahl - ein Tastendruck beantwortet also "wo bin ich"
    /// und "was liegt hier" zusammen.
    ///
    /// Gezählt wird, worauf die Blättertaste tatsächlich stehenbleibt, nicht, wie viele
    /// Schalter das Register eingeschaltet hat. Ein Register mit vierzig eingeschalteten
    /// Kanälen, von denen zwei gesprochen haben, ist für den Spieler ein Register mit
    /// zwei Puffern; "vierzig" würde eine Filterliste beschreiben und keinen Verlauf.
    /// </summary>
    public static string ChatTabEntered(string tab, int buffers, string first, int count) =>
        Pick($"{tab}, {buffers} Puffer. {first}, {count}.",
             $"{tab}, {buffers} buffers. {first}, {count}.",
             $"{tab}, 읽을 기록 {buffers}개. {first}, {count}.");

    /// <summary>Wird gesagt, wenn die Registertaste den Chatlog des Spiels gar nicht
    /// erreicht. Nie Stille: der Spieler hätte sonst keine Möglichkeit, ein fehlendes
    /// Fenster von einer kaputten Taste zu unterscheiden.</summary>
    public static string ChatTabUnavailable =>
        Pick("Das Chatfenster ist nicht erreichbar.",
             "The chat window cannot be reached.",
             "대화창을 사용할 수 없음.");

    // ── Einstellungen: eine Sprachschaltung je Chat-Register ──────

    /// <summary>Benannt nach dem, was das Spiel hat, denn genau das sind die Zeilen
    /// darunter: eine je Chat-Register, unter dem Namen des Registers selbst.</summary>
    public static string OptionsChatTabs => Pick("Chat-Register", "Chat tabs", "대화 탭");

    /// <summary>
    /// Die oberste Zeile im Untermenü eines Registers: wird dieses Register vorgelesen.
    ///
    /// JEDE ZEILE IN DIESEM ABSCHNITT SAGT "VORLESEN", und zwar mit Absicht. Die
    /// Schaltung des SPIELS entscheidet, ob eine Zeile überhaupt existiert - aus, heißt
    /// nicht angezeigt, nicht archiviert, nicht gesprochen. Die Schaltung HIER
    /// entscheidet nur, ob eine ohnehin vorhandene Zeile laut vorgelesen wird; aus,
    /// heißt archiviert und blätterbar, aber still. Beide sitzen im Kopf des Spielers
    /// nebeneinander, also muss die Zeile des Plugins das eine benennen, was sie
    /// anfasst. Ein Wort wie "stummschalten" beschreibt einen Zustand, ohne zu sagen,
    /// was verstummt, und das ist genau die Zweideutigkeit, die hier zu vermeiden ist.
    /// </summary>
    public static string OptChatTabMaster =>
        Pick("Register vorlesen", "Read tab aloud", "탭 읽어 주기");

    /// <summary>Die Gruppenzeile im Untermenü eines Kanals - die ganze Akteursgruppe
    /// auf einmal, über den Kästchen, in die das Spiel sie aufteilt. Gleiche
    /// Wortregel wie bei <see cref="OptChatTabMaster"/>.</summary>
    public static string OptChatChannelAll =>
        Pick("Ganze Gruppe vorlesen", "Read whole group aloud", "채널 묶음 전체 읽어 주기");

    /// <summary>
    /// Die eine Schaltung für Zeilen, für die die Filterliste des Spiels gar kein
    /// Kästchen hat - die Anmeldehinweise, die Phishing-Warnung, ein eingehendes
    /// Tell, ein GM.
    ///
    /// Benannt nach dem, was sie abdeckt, und nicht nach einem Kanal, denn ein Kanal
    /// ist es nicht: es ist alles, was das Spiel nicht filterbar gemacht hat. Sie steht
    /// unten in der Registerliste, weil sie zu keinem Register gehört.
    /// </summary>
    public static string OptChatUnfiltered =>
        Pick("Meldungen ohne Spielfilter vorlesen",
             "Read lines the game cannot filter",
             "게임 로그 필터로 거를 수 없는 메시지 읽어 주기");

    /// <summary>Die Zeile, wenn die Register nicht lesbar sind - eine Schaltung für den
    /// einen Sammelpuffer. Ein Abschnitt, der seinen Namen nennt und dann nichts
    /// anbietet, liest sich wie ein Fehler; also sagt er statt dessen, in welchem
    /// Zustand er ist.</summary>
    public static string OptChatFallback =>
        Pick("Chat vorlesen (Register nicht lesbar)",
             "Read chat aloud (tabs unreadable)",
             "대화 읽어 주기 (탭을 읽을 수 없음)");

    // ── Einstellungen: eigene Reihenfolge der Kategorien ──────────
    //
    // ZWEI GETRENNTE UNTERMENÜS für Reihenfolge und An/Aus, obwohl beide
    // dieselbe Liste zeigen. Der Grund ist die Bestätigungstaste: sie kann pro
    // Ebene nur eines von beidem bedeuten, und eine zweite Taste dafür zu
    // erfinden hieße, dem Spieler eine Sondertaste beizubringen, die es sonst
    // in keinem Menü der Mod gibt. So bleibt Numpad0 überall das, was es überall
    // ist - in der einen Ebene nimmt es auf, in der anderen schaltet es um.

    public static string OptionsOrder => IsGerman ? "Reihenfolge" : "Order";

    /// <summary>Der Abschnitt für die Objekt-Browser-Kategorien. "Objekte
    /// durchblättern" und nicht "Kategorien", weil der Spieler die Liste über die
    /// Bild-Tasten kennt und nicht über ihren internen Namen.</summary>
    public static string OptionsOrderObjects =>
        IsGerman ? "Reihenfolge beim Objekte-Durchblättern" : "Order when browsing objects";

    /// <summary>Derselbe Abschnitt für die Nachlese-Kategorien.</summary>
    public static string OptionsOrderChat =>
        IsGerman ? "Reihenfolge der Nachlese-Kategorien" : "Order of chat history categories";

    public static string OptionsShowObjects =>
        IsGerman ? "Objekt-Kategorien ein- und ausschalten" : "Switch object categories on and off";

    public static string OptionsShowChat =>
        IsGerman ? "Nachlese-Kategorien ein- und ausschalten" : "Switch chat history categories on and off";

    /// <summary>
    /// Der Titel der Sortier-Ebene. Sagt den Kategoriensatz mit, weil es zwei
    /// davon gibt und der Spieler sonst nicht wüsste, welchen er gerade
    /// umsortiert: in einem Tiefen Gewölbe gilt ein eigener, kürzerer Satz.
    /// </summary>
    public static string OrderTitle(string set) =>
        IsGerman ? $"Reihenfolge: {set}" : $"Order: {set}";

    public static string OrderSetWorld => IsGerman ? "Welt" : "World";
    public static string OrderSetDeepDungeon => IsGerman ? "Tiefes Gewölbe" : "Deep dungeon";
    public static string OrderSetChat => IsGerman ? "Nachlese" : "Chat history";

    /// <summary>
    /// Eine Zeile in der Sortier-Ebene. Sagt "aus" mit, wenn die Kategorie
    /// abgeschaltet ist - man soll beim Sortieren sehen, was man da einsortiert,
    /// ohne zwischen zwei Untermenüs hin und her zu wechseln. Eingeschaltete
    /// Zeilen sagen NICHTS dazu: das ist der Normalfall, und ein "an" hinter
    /// zwanzig von einundzwanzig Zeilen ist nur Lärm.
    /// </summary>
    public static string OrderRow(string name, bool visible) =>
        visible ? name : (Pick($"{name}, aus", $"{name}, off", $"{name}, 꺼짐"));

    /// <summary>
    /// Die Ansage beim Öffnen einer Sortier-Ebene: sie muss die Bedienung
    /// erklären, weil sie die einzige Ebene der Mod ist, in der die
    /// Bestätigungstaste etwas anderes tut als sonst.
    /// </summary>
    public static string OrderHint =>
        IsGerman
            ? "Bestätigen nimmt eine Zeile auf, hoch und runter verschiebt sie, Bestätigen legt sie wieder ab."
            : "Confirm picks a row up, up and down move it, confirm puts it down again.";

    /// <summary>
    /// Wenn der Spieler die letzte eingeschaltete Kategorie abschalten will.
    ///
    /// Verweigert statt zugelassen: ein Browser ohne jede Kategorie antwortet auf
    /// die Bild-Tasten mit gar nichts, und "gar nichts" ist für einen blinden
    /// Spieler von einer kaputten Mod nicht zu unterscheiden. Die Ansage sagt
    /// deshalb den Grund und nicht nur, dass es nicht ging.
    /// </summary>
    public static string OrderLastOneStays(string name) =>
        IsGerman
            ? $"{name} bleibt an - es ist die letzte eingeschaltete Kategorie."
            : $"{name} stays on - it is the last category left switched on.";

    // ── Einstellungen: die Kanäle des GEWOHNTEN Chatsystems ───────

    /// <summary>Der Abschnitt, der im gewohnten Chatsystem an der Stelle steht, an
    /// der im neuen <see cref="OptionsChatTabs"/> steht. "Kanäle" und nicht
    /// "Register", weil das alte System keine Register kennt: seine Einteilung ist
    /// die feste Kategorienliste, die der Spieler auch beim Nachlesen hört.</summary>
    public static string OptionsChatChannels => Pick("Chat-Kanäle", "Chat channels", "대화 채널");

    /// <summary>Die Sammel-Rückmeldungen beim Abbauen (XivChatType.Gathering). Sie
    /// haben einen eigenen Schalter, landen in der Nachlese aber unter "System" -
    /// deshalb ist dies der einzige Kanalname dieses Abschnitts, der nicht aus
    /// <see cref="AccessibilityStrings.LegacyChatCategoryName"/> kommen kann.</summary>
    public static string OptChatGathering => Pick("Sammeln", "Gathering", "채집");

    /// <summary>
    /// Hängt sich an die Bestätigung, wenn ein Kanal ABGESCHALTET wird ("Gruppe aus.
    /// Steht weiter zum Nachlesen bereit.").
    ///
    /// Nur beim Abschalten, und nur dort: das ist der Moment, in dem sich die Frage
    /// stellt, ob die Nachrichten jetzt weg sind. Beim Einschalten ist der Satz
    /// überflüssig, und in der Zeilenbeschriftung stünde er bei jedem Durchblättern
    /// im Weg.
    /// </summary>
    public static string ChatChannelStillArchived =>
        Pick("Steht weiter zum Nachlesen bereit.",
             "Still available in the history.",
             "대화 기록에는 그대로 남음.");
}
