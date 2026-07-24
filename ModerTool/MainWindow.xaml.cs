using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ModerTool;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    public ObservableCollection<SteamAccount> SteamAccounts { get; } = [];
    public ObservableCollection<SteamGame> SteamGames { get; } = [];
    public string GameOpenButtonText => LangManager.Get("GameOpenBtn");

    static readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(10) };
    AppConfig cfg = new();

    public MainWindow()
    {
        InitializeComponent();
        SteamAccountsGrid.ItemsSource = SteamAccounts;
        GameListView.ItemsSource = SteamGames;
        LoadConfig();
        LangManager.LanguageChanged += UpdateUI;
        UpdateUI();
        Log(LangManager.Get("LogStarted"));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new(n));

    void UpdateUI()
    {
        BtnLangSwitch.Content = LangManager.IsRussian ? "RU" : "US";
        TxtNavHome.Text = LangManager.Get("NavHome"); TxtNavProgramm.Text = LangManager.Get("NavProgramm");
        TxtNavAccounts.Text = LangManager.Get("NavAccounts"); TxtNavLogs.Text = LangManager.Get("NavLogs");
        TxtNavGameFolder.Text = LangManager.Get("NavGameFolder"); TxtNavLinks.Text = LangManager.Get("NavLinks");
        TxtNavExit.Text = LangManager.Get("NavExit");

        TxtHomeTitle.Text = LangManager.Get("HomeTitle"); TxtHomeDesc.Text = LangManager.Get("HomeDesc");
        TxtHomeBtn.Text = LangManager.Get("HomeBtn"); TxtProgSysFolders.Text = LangManager.Get("ProgSysFolders");
        TxtProgUtils.Text = LangManager.Get("ProgUtils"); TxtProgStore.Text = LangManager.Get("ProgStore");
        TxtProgAppSwitched.Text = LangManager.Get("ProgAppSwitched"); TxtProgHistory.Text = LangManager.Get("ProgHistory");
        TxtProgPrefetch.Text = LangManager.Get("ProgPrefetch"); TxtProgTemp.Text = LangManager.Get("ProgTemp");
        TxtProgRecent.Text = LangManager.Get("ProgRecent"); TxtProgRecentAD.Text = LangManager.Get("ProgRecentAD");
        TxtProgLastActivity.Text = LangManager.Get("ProgLastActivity"); TxtProgShellBag.Text = LangManager.Get("ProgShellBag");
        TxtProgEverything.Text = LangManager.Get("ProgEverything"); TxtProgUSBDeview.Text = LangManager.Get("ProgUSBDeview");
        TxtProgProcessHacker.Text = LangManager.Get("ProgProcessHacker"); TxtAccScan.Text = LangManager.Get("AccScan");
        TxtAccExport.Text = LangManager.Get("AccExport"); ColSteamID.Header = LangManager.Get("AccSteamID");
        ColName.Header = LangManager.Get("AccName"); ColLevel.Header = LangManager.Get("AccLevel");
        ColVAC.Header = LangManager.Get("AccVAC"); TxtLogsTitle.Text = LangManager.Get("LogsTitle");
        TxtGameScanBtn.Text = LangManager.Get("GameScanBtn"); OnPropertyChanged(nameof(GameOpenButtonText));
    }

    void UpdateHeader() => HeaderTitle.Text = LangManager.Get(
        PageHome.Visibility == Visibility.Visible ? "HeaderHome" :
        PageProgramm.Visibility == Visibility.Visible ? "HeaderProgramm" :
        PageAccounts.Visibility == Visibility.Visible ? "HeaderAccounts" :
        PageLogs.Visibility == Visibility.Visible ? "HeaderLogs" :
        PageGameFolder.Visibility == Visibility.Visible ? "HeaderGameFolder" : "HeaderLinks");

    void BtnLangSwitch_Click(object s, RoutedEventArgs e) => LangManager.ToggleLanguage();

    void LoadConfig()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "settings.json");
        if (!File.Exists(path))
        {
            File.WriteAllText(path, JsonSerializer.Serialize(new AppConfig(), new JsonSerializerOptions { WriteIndented = true }));
            Log(LangManager.Get("LogConfigCreated")); return;
        }
        try
        {
            cfg = JsonSerializer.Deserialize<AppConfig>(File.ReadAllText(path)) ?? new();
            Log(string.IsNullOrWhiteSpace(cfg.SteamApiKey) || cfg.SteamApiKey == "INSERT_YOUR_API_KEY_HERE"
                ? LangManager.Get("LogConfigNoKey") : LangManager.Get("LogConfigLoaded"));
        }
        catch (Exception ex) { Log(LangManager.Get("LogConfigReadError") + ex.Message); }
    }

    void NavButton_Click(object s, RoutedEventArgs e)
    {
        if (s is not RadioButton rb) return;
        PageHome.Visibility = PageProgramm.Visibility = PageAccounts.Visibility =
        PageLogs.Visibility = PageGameFolder.Visibility = PageLinks.Visibility = Visibility.Collapsed;

        (rb.Name switch
        {
            "NavHome" => PageHome,
            "NavProgramm" => PageProgramm,
            "NavAccounts" => PageAccounts,
            "NavLogs" => PageLogs,
            "NavGameFolder" => PageGameFolder,
            _ => PageLinks
        }).Visibility = Visibility.Visible;
        UpdateHeader();
    }

    void Border_MouseLeftButtonDown(object s, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    void BtnExit_Click(object s, RoutedEventArgs e) => Application.Current.Shutdown();

    void BtnLaunchAnyDesk_Click(object s, RoutedEventArgs e)
    {
        var p = Path.Combine(AppContext.BaseDirectory, "Tools", "AnyDesk.exe");
        if (File.Exists(p)) { Process.Start(new ProcessStartInfo(p) { UseShellExecute = true }); Log(LangManager.Get("LogAnyDeskStarted")); }
        else Log(LangManager.Get("LogAnyDeskNotFound"));
    }

    void BtnOpenFolder_Click(object s, RoutedEventArgs e)
    {
        if (s is not Button btn) return;
        OpenFolder(btn.Tag?.ToString() switch
        {
            "History" => Environment.GetFolderPath(Environment.SpecialFolder.History),
            "Prefetch" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch"),
            "Temp" => Path.GetTempPath(),
            "Recent" => Environment.GetFolderPath(Environment.SpecialFolder.Recent),
            "RecentAD" => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AnyDesk"),
            _ => null
        });
    }

    async void BtnOpenReg_Click(object s, RoutedEventArgs e)
    {
        if (s is not Button btn) return;
        var path = btn.Tag?.ToString() switch
        {
            "Store" => @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Compatibility Assistant\Store",
            "AppSwitched" => @"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FeatureUsage\AppSwitched",
            _ => ""
        };
        if (string.IsNullOrEmpty(path)) return;
        using var key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Applets\Regedit");
        key.SetValue("LastKey", path);
        await Task.Delay(500);
        Process.Start("regedit.exe");
        Log(LangManager.Get("LogRegOpened") + path);
    }

    void BtnRunTool_Click(object s, RoutedEventArgs e)
    {
        if (s is not Button btn || string.IsNullOrEmpty(btn.Tag?.ToString())) return;
        var tool = btn.Tag.ToString()!;
        var p = Path.Combine(AppContext.BaseDirectory, "Tools", tool);
        if (File.Exists(p)) { Process.Start(new ProcessStartInfo(p) { UseShellExecute = true }); Log(LangManager.Get("LogToolStarted") + tool); }
        else Log(LangManager.Get("LogToolNotFound") + tool);
    }

    void BtnOpenLink_Click(object s, RoutedEventArgs e)
    {
        if (s is not Button btn) return;
        var url = btn.Tag?.ToString() switch
        {
            "Discord" => cfg.Links.Discord,
            "Website" => cfg.Links.Website,
            "Twitch" => cfg.Links.Twitch,
            "Rules" => cfg.Links.Rules,
            "GitHub" => "https://github.com/m4Loyyyy",
            _ => ""
        };
        if (!string.IsNullOrEmpty(url)) Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    void OpenFolder(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path)) { Log(LangManager.Get("LogFolderNotFound") + path); return; }
        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true, Verb = "open" });
        Log(LangManager.Get("LogFolderOpened") + path);
    }

    async void BtnScanSteam_Click(object s, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(cfg.SteamApiKey) || cfg.SteamApiKey == "INSERT_YOUR_API_KEY_HERE")
        { Log(LangManager.Get("LogSteamNoKey")); return; }

        SteamAccounts.Clear();
        var steamPath = GetSteamPath();
        if (steamPath == null) { Log(LangManager.Get("LogSteamNotFound")); return; }
        var vdf = Path.Combine(steamPath, "config", "loginusers.vdf");
        if (!File.Exists(vdf)) { Log(LangManager.Get("LogVdfNotFound")); return; }

        foreach (Match m in Regex.Matches(File.ReadAllText(vdf), @"""(7656119\d{10})"""))
            if (!SteamAccounts.Any(a => a.Steam64ID == m.Groups[1].Value))
                SteamAccounts.Add(new() { Steam64ID = m.Groups[1].Value, Name = "...", Level = "...", VAC = "..." });

        Log(LangManager.Get("LogScanFound") + SteamAccounts.Count + LangManager.Get("LogScanGetData"));
        await FetchSteamData();
    }

    async Task FetchSteamData()
    {
        foreach (var acc in SteamAccounts)
        {
            try
            {
                var tasks = Task.WhenAll(
                    http.GetStringAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={cfg.SteamApiKey}&steamids={acc.Steam64ID}"),
                    http.GetStringAsync($"http://api.steampowered.com/IPlayerService/GetSteamLevel/v1/?key={cfg.SteamApiKey}&steamid={acc.Steam64ID}"),
                    http.GetStringAsync($"http://api.steampowered.com/ISteamUser/GetPlayerBans/v1/?key={cfg.SteamApiKey}&steamids={acc.Steam64ID}")
                );
                await tasks;
                var summary = JsonDocument.Parse(tasks.Result[0]).RootElement.GetProperty("response").GetProperty("players")[0];
                acc.Name = summary.TryGetProperty("personaname", out var n) ? n.GetString() : "Unknown";
                var level = JsonDocument.Parse(tasks.Result[1]).RootElement.GetProperty("response");
                acc.Level = level.TryGetProperty("player_level", out var lvl) ? lvl.GetInt32().ToString() : "Hidden";
                var ban = JsonDocument.Parse(tasks.Result[2]).RootElement.GetProperty("players")[0];
                bool vac = ban.GetProperty("VACBanned").GetBoolean(), game = ban.GetProperty("NumberOfGameBans").GetInt32() > 0;
                acc.VAC = vac ? "VAC BANNED" : game ? "GAME BANNED" : "Clean";
                Log(LangManager.Get("LogAccChecked") + acc.Name);
            }
            catch { acc.Name = acc.VAC = "Error"; }
        }
        Log(LangManager.Get("LogScanComplete"));
    }

    void BtnExportSteam_Click(object s, RoutedEventArgs e)
    {
        if (SteamAccounts.Count == 0) { Log(LangManager.Get("LogNoExportData")); return; }
        var sfd = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = "Steam_Accounts" };
        if (sfd.ShowDialog() != true) return;
        var sb = new StringBuilder("Steam64ID,Name,Level,VAC\n");
        foreach (var a in SteamAccounts) sb.AppendLine($"{a.Steam64ID},{a.Name},{a.Level},{a.VAC}");
        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        Log(LangManager.Get("LogExportSaved") + sfd.FileName);
    }

    string? GetSteamPath()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        return (key?.GetValue("SteamPath") as string)?.Replace('/', '\\') ??
               (Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Valve\Steam")?.GetValue("InstallPath") as string);
    }

    void BtnScanGames_Click(object s, RoutedEventArgs e)
    {
        SteamGames.Clear();
        var steamPath = GetSteamPath();
        if (steamPath == null) { Log(LangManager.Get("LogSteamNotFound")); return; }
        var libFile = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(libFile)) { Log(LangManager.Get("LogLibraryFoldersNotFound")); return; }

        var libs = Regex.Matches(File.ReadAllText(libFile), @"""path""\s+""([^""]+)""").Select(m => m.Groups[1].Value.Replace("\\\\", "\\")).ToList();
        libs.Add(steamPath);

        foreach (var lib in libs.Distinct())
        {
            var apps = Path.Combine(lib, "steamapps");
            if (!Directory.Exists(apps)) continue;
            foreach (var manifest in Directory.GetFiles(apps, "appmanifest_*.acf"))
            {
                var content = File.ReadAllText(manifest);
                var name = Regex.Match(content, @"""name""\s+""([^""]+)""").Groups[1].Value;
                var dir = Regex.Match(content, @"""installdir""\s+""([^""]+)""").Groups[1].Value;
                var fullPath = Path.Combine(apps, "common", dir);
                if (!string.IsNullOrEmpty(name) && Directory.Exists(fullPath))
                    SteamGames.Add(new(name, fullPath));
            }
        }
        Log(LangManager.Get("LogGameScanComplete") + SteamGames.Count);
    }

    void BtnOpenGameFolder_Click(object s, RoutedEventArgs e)
    { if (s is Button btn && btn.DataContext is SteamGame game) OpenFolder(game.FullPath); }

    void Log(string msg) => Dispatcher.Invoke(() => { TxtLogs.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n"); TxtLogs.ScrollToEnd(); });
}

public class SteamAccount : INotifyPropertyChanged
{
    string? _id, _name, _level, _vac;
    public string? Steam64ID { get => _id; set { _id = value; OnPropertyChanged(); } }
    public string? Name { get => _name; set { _name = value; OnPropertyChanged(); } }
    public string? Level { get => _level; set { _level = value; OnPropertyChanged(); } }
    public string? VAC { get => _vac; set { _vac = value; OnPropertyChanged(); } }
    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? n = null) => PropertyChanged?.Invoke(this, new(n));
}

public record SteamGame(string Name, string FullPath);

public record AppConfig
{
    public string SteamApiKey { get; init; } = "INSERT_YOUR_API_KEY_HERE";
    public AppLinks Links { get; init; } = new();
}

public record AppLinks
{
    public string Discord { get; init; } = "https://google.com";
    public string Website { get; init; } = "https://google.com";
    public string Twitch { get; init; } = "https://google.com";
    public string Rules { get; init; } = "https://google.com";
}