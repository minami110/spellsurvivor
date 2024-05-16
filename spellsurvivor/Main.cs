using System;
using System.Runtime.CompilerServices;
using Godot;
using R3;

namespace spellsurvivor;

public partial class Main : Node
{
    private static Main? _instance;
    private readonly Subject<Unit> _waveEndedSub = new();

    private readonly ReactiveProperty<int> _waveRp = new();

    private readonly Subject<Unit> _waveStartedSub = new();

    private Player _player = null!;

    public static Main GameMode
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _instance ?? throw new ApplicationException("Main instance is null");
    }

    public ReadOnlyReactiveProperty<int> Wave => _waveRp;

    public Observable<Unit> WaveStarted => _waveStartedSub;
    public Observable<Unit> WaveEnded => _waveEndedSub;

    public override void _EnterTree()
    {
        if (_instance is null)
        {
            _instance = this;
        }
        else
        {
            throw new AggregateException("Main instance already exists");
        }
    }

    public override async void _Ready()
    {
        _player = GetNode<Player>("Player");

        await this.WaitForSeconds(1f);

        StartGame();
    }

    public override void _ExitTree()
    {
        _waveStartedSub.Dispose();
        _waveRp.Dispose();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    public static Vector2 GetPlayerGlobalPosition()
    {
        if (_instance is not null)
        {
            return _instance._player.GlobalPosition;
        }

        throw new ApplicationException("Main instance is null");
    }

    public void StartGame()
    {
        EnterWave();
    }

    public void EnterWave()
    {
        _waveStartedSub.OnNext(Unit.Default);
        _waveRp.Value++;
        DebugGUI.CommitText("Wave", _waveRp.Value.ToString());
    }

    public void ExitShop()
    {
        EnterWave();
    }

    public void CompleteWave()
    {
        _waveEndedSub.OnNext(Unit.Default);

        // ToDO: Open Shop Time
    }
}