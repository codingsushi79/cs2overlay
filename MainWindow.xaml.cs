using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cs2Overlay;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly GameStateListener _listener;
    private readonly DispatcherTimer _processTimer;

    public ObservableCollection<PlayerHud> TeamLeftPlayers { get; } = new();
    public ObservableCollection<PlayerHud> TeamRightPlayers { get; } = new();

    private string _teamLeftName = "CT";
    private string _teamRightName = "T";
    private int _teamLeftScore;
    private int _teamRightScore;
    private string _mapName = "de_mirage";
    private string _roundInfo = "Round 1";
    private string _roundTimer = "1:55";
    private string _teamLeftRole = "CT";
    private string _teamRightRole = "T";
    private Brush _teamLeftBarBrush = new SolidColorBrush(Color.FromRgb(0, 90, 200));
    private Brush _teamRightBarBrush = new SolidColorBrush(Color.FromRgb(200, 140, 0));
    private string _cs2StatusText = "Waiting for CS2";
    private Brush _cs2StatusBrush = Brushes.Orange;
    private string _focusPlayerName = "No POV";
    private string _focusPlayerAvatar = "assets/players/default.png";

    public string TeamLeftName
    {
        get => _teamLeftName;
        set { _teamLeftName = value; OnPropertyChanged(); }
    }

    public string TeamRightName
    {
        get => _teamRightName;
        set { _teamRightName = value; OnPropertyChanged(); }
    }

    public int TeamLeftScore
    {
        get => _teamLeftScore;
        set { _teamLeftScore = value; OnPropertyChanged(); }
    }

    public int TeamRightScore
    {
        get => _teamRightScore;
        set { _teamRightScore = value; OnPropertyChanged(); }
    }

    public string MapName
    {
        get => _mapName;
        set { _mapName = value; OnPropertyChanged(); }
    }

    public string RoundInfo
    {
        get => _roundInfo;
        set { _roundInfo = value; OnPropertyChanged(); }
    }

    public string RoundTimer
    {
        get => _roundTimer;
        set { _roundTimer = value; OnPropertyChanged(); }
    }

    public string TeamLeftRole
    {
        get => _teamLeftRole;
        set { _teamLeftRole = value; OnPropertyChanged(); }
    }

    public string TeamRightRole
    {
        get => _teamRightRole;
        set { _teamRightRole = value; OnPropertyChanged(); }
    }

    public Brush TeamLeftBarBrush
    {
        get => _teamLeftBarBrush;
        set { _teamLeftBarBrush = value; OnPropertyChanged(); }
    }

    public Brush TeamRightBarBrush
    {
        get => _teamRightBarBrush;
        set { _teamRightBarBrush = value; OnPropertyChanged(); }
    }

    public string Cs2StatusText
    {
        get => _cs2StatusText;
        set { _cs2StatusText = value; OnPropertyChanged(); }
    }

    public Brush Cs2StatusBrush
    {
        get => _cs2StatusBrush;
        set { _cs2StatusBrush = value; OnPropertyChanged(); }
    }

    public string FocusPlayerName
    {
        get => _focusPlayerName;
        set { _focusPlayerName = value; OnPropertyChanged(); }
    }

    public string FocusPlayerAvatar
    {
        get => _focusPlayerAvatar;
        set { _focusPlayerAvatar = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        _listener = new GameStateListener(3000);
        _listener.GameStateUpdated += OnGameStateUpdated;
        _listener.Start();

        _processTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        _processTimer.Tick += (_, _) => UpdateCs2ProcessStatus();
        _processTimer.Start();
        UpdateCs2ProcessStatus();
    }

    private void OnGameStateUpdated(object? sender, GameState e)
    {
        Dispatcher.Invoke(() =>
        {
            MapName = e.Map?.Name ?? MapName;

            if (e.Map != null)
            {
                if (e.Map.TeamCT != null)
                {
                    TeamLeftName = string.IsNullOrWhiteSpace(e.Map.TeamCT.Name) ? "CT" : e.Map.TeamCT.Name!;
                    TeamLeftScore = e.Map.TeamCT.Score;
                    TeamLeftRole = "CT";
                    TeamLeftBarBrush = new SolidColorBrush(Color.FromRgb(0, 120, 255));
                }

                if (e.Map.TeamT != null)
                {
                    TeamRightName = string.IsNullOrWhiteSpace(e.Map.TeamT.Name) ? "T" : e.Map.TeamT.Name!;
                    TeamRightScore = e.Map.TeamT.Score;
                    TeamRightRole = "T";
                    TeamRightBarBrush = new SolidColorBrush(Color.FromRgb(230, 160, 0));
                }
            }

            if (e.Round != null && e.PhaseCountdowns != null)
            {
                RoundInfo = $"{e.Round.Phase} - {e.Round.WinTeam ?? "live"}";
                RoundTimer = FormatTime(e.PhaseCountdowns.PhaseEndsIn);
            }

            TeamLeftPlayers.Clear();
            TeamRightPlayers.Clear();
            if (e.AllPlayers != null)
            {
                var ct = new List<PlayerHud>();
                var t = new List<PlayerHud>();

                foreach (var kv in e.AllPlayers)
                {
                    var p = kv.Value;
                    if (p.State == null) continue;
                    var isAlive = p.State.Health > 0;

                    var hud = new PlayerHud
                    {
                        Name = p.Name ?? "Player",
                        Health = p.State.Health,
                        WeaponName = p.Weapons?.GetActiveWeaponName() ?? "Knife",
                        WeaponIcon = p.Weapons?.GetActiveWeaponIconPath() ?? "assets/weapons/ak47.svg",
                        WeaponShortName = (p.Weapons?.GetActiveWeaponName() ?? "Knife").Replace("weapon_", "", StringComparison.OrdinalIgnoreCase),
                        MoneyText = $"$ {Math.Max(0, p.State.Money)}",
                        IsAlive = isAlive,
                        HudBackground = GetTeamBackgroundBrush(p.Team),
                        TeamDamageFillBrush = GetDamageFillBrush(p.Team),
                        DamageFillHeight = Math.Round(Math.Clamp(p.State.Health, 0, 100) * 0.9, 1),
                        DamageFillOpacity = isAlive ? 0.9 : 0.25,
                        CardOpacity = isAlive ? 1.0 : 0.45,
                        NameBrush = isAlive ? Brushes.White : new SolidColorBrush(Color.FromRgb(190, 190, 190)),
                        DeadOverlayVisibility = isAlive ? Visibility.Collapsed : Visibility.Visible
                    };

                    if (string.Equals(p.Team, "CT", StringComparison.OrdinalIgnoreCase))
                    {
                        ct.Add(hud);
                    }
                    else if (string.Equals(p.Team, "T", StringComparison.OrdinalIgnoreCase))
                    {
                        t.Add(hud);
                    }
                }

                foreach (var item in t.OrderByDescending(p => p.IsAlive).ThenBy(p => p.Name).Take(5))
                    TeamRightPlayers.Add(item);
                foreach (var item in ct.OrderByDescending(p => p.IsAlive).ThenBy(p => p.Name).Take(5))
                    TeamLeftPlayers.Add(item);

                var focus = TeamLeftPlayers.Concat(TeamRightPlayers).FirstOrDefault(p => p.IsAlive)
                            ?? TeamLeftPlayers.Concat(TeamRightPlayers).FirstOrDefault();
                if (focus != null)
                {
                    FocusPlayerName = focus.Name;
                    FocusPlayerAvatar = $"assets/players/{SanitizeAssetName(focus.Name)}.png";
                }
            }
        });
    }

    private static Brush GetTeamBackgroundBrush(string? team)
    {
        return team switch
        {
            "CT" => new SolidColorBrush(Color.FromArgb(220, 0, 136, 255)),
            "T" => new SolidColorBrush(Color.FromArgb(220, 255, 153, 0)),
            _ => new SolidColorBrush(Color.FromArgb(180, 80, 80, 80))
        };
    }

    private static Brush GetDamageFillBrush(string? team)
    {
        return string.Equals(team, "CT", StringComparison.OrdinalIgnoreCase)
            ? new SolidColorBrush(Color.FromRgb(26, 128, 255))
            : new SolidColorBrush(Color.FromRgb(241, 74, 143));
    }

    private static string SanitizeAssetName(string raw)
    {
        var chars = raw
            .Trim()
            .ToLowerInvariant()
            .Select(c => char.IsLetterOrDigit(c) ? c : '_')
            .ToArray();
        return new string(chars);
    }

    private void UpdateCs2ProcessStatus()
    {
        var running = Process.GetProcessesByName("cs2").Length > 0 || Process.GetProcessesByName("csgo").Length > 0;
        Cs2StatusText = running ? "CS2 detected - GSI live when in match" : "CS2 not detected";
        Cs2StatusBrush = running ? Brushes.LawnGreen : Brushes.OrangeRed;
    }

    private static string FormatTime(double seconds)
    {
        if (seconds < 0) seconds = 0;
        var ts = TimeSpan.FromSeconds(seconds);
        return $"{(int)ts.Minutes}:{ts.Seconds:00}";
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _processTimer.Stop();
        _listener.Dispose();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class PlayerHud
{
    public string Name { get; set; } = "";
    public int Health { get; set; }
    public string WeaponName { get; set; } = "";
    public string WeaponShortName { get; set; } = "";
    public string WeaponIcon { get; set; } = "";
    public string MoneyText { get; set; } = "";
    public bool IsAlive { get; set; }
    public double DamageFillHeight { get; set; }
    public double DamageFillOpacity { get; set; }
    public double CardOpacity { get; set; } = 1.0;
    public Brush TeamDamageFillBrush { get; set; } = Brushes.DodgerBlue;
    public Brush NameBrush { get; set; } = Brushes.White;
    public Visibility DeadOverlayVisibility { get; set; } = Visibility.Collapsed;
    public Brush HudBackground { get; set; } = Brushes.Gray;
}

