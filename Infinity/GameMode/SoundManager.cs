using System.Linq;
using Godot;
using R3;

namespace fms;

public partial class SoundManager : Node
{
    [Export]
    private AudioStreamPlayer _bgmAPlayer = null!;

    [Export]
    private AudioStreamPlayer _bgmBPlayer = null!;

    [Export]
    private AudioStreamPlayer _bgmCPlayer = null!;

    [Export]
    private AudioStreamOggVorbis _seFanfale = null!;

    [Export]
    private AudioStreamPlayer _effectPlayer = null!;

    [Export]
    private AudioStream _buttonClickSound = null!;

    private static SoundManager? _instance;

    public SoundManager()
    {
        _instance = this;
    }

    public override void _Ready()
    {
        var tree = GetTree();
        var nodes = tree.Root.FindChildren("*", nameof(BaseButton), true, false);

        _bgmBPlayer.Play();

        foreach (var button in from n in nodes
                 where n is BaseButton
                 select n)
        {
            this.DebugLog($"Name: {button.Name} Binded Sound");
            ((BaseButton)button).MouseEntered += PlayButtonClickSound;
        }

        Main.WaveState.Phase.Subscribe(x =>
        {
            if (x == WavePhase.BATTLE)
            {
                EffectBgmShopToBattle();
            }
            else if (x == WavePhase.SHOP)
            {
                EffectBgmBattleToShop();
            }
            else if (x == WavePhase.BATTLERESULT)
            {
                EnterBattleResult();
            }
        });
    }

    public override void _ExitTree()
    {
        _instance = null;
    }

    public static async void PlaySoundEffect(AudioStream stream)
    {
        if (_instance is not null)
        {
            if (_instance._effectPlayer.Playing)
            {
                return;
            }

            _instance._effectPlayer.Stream = stream;
            _instance._effectPlayer.Play();
            await _instance.ToSignal(_instance._effectPlayer, AudioStreamPlayer.SignalName.Finished);
            _instance._effectPlayer.Stop();
        }
    }

    private async void EffectBgmBattleToShop()
    {
        // acess bass
        var busIndex = AudioServer.GetBusIndex("BGM");
        var lowPass = (AudioEffectLowPassFilter)AudioServer.GetBusEffect(busIndex, 0);

        // 140Hz -> 2000Hz
        var p = AudioEffectFilter.PropertyName.CutoffHz.ToString();
        var tween = CreateTween();
        tween.TweenProperty(lowPass, p, 140f, 0.5f);
        tween.Play();
    }

    private async void EffectBgmShopToBattle()
    {
        // acess bass
        var busIndex = AudioServer.GetBusIndex("BGM");
        var lowPass = (AudioEffectLowPassFilter)AudioServer.GetBusEffect(busIndex, 0);

        // 140Hz -> 2000Hz
        var p = AudioEffectFilter.PropertyName.CutoffHz.ToString();
        var tween = CreateTween();
        tween.TweenProperty(lowPass, p, 2000f, 2f).SetEase(Tween.EaseType.InOut);
        tween.Play();

        // Add BGM
        var nextStartTimeLeft = 6f - GetPlaybackTime(_bgmBPlayer);
        var delay = AudioServer.GetTimeToNextMix() + AudioServer.GetOutputLatency();
        await this.WaitForSecondsAsync(nextStartTimeLeft - delay);
        _bgmAPlayer.Play();
    }

    private async void EnterBattleResult()
    {
        // Add BGM
        var nextStartTimeLeft = (2f - GetPlaybackTime(_bgmBPlayer)) % 0.5f;
        var delay = AudioServer.GetTimeToNextMix() + AudioServer.GetOutputLatency();
        await this.WaitForSecondsAsync(nextStartTimeLeft - delay);

        _bgmAPlayer.Stop();
        _bgmCPlayer.Stream = _seFanfale;
        _bgmCPlayer.Play();
        await ToSignal(_bgmCPlayer, AudioStreamPlayer.SignalName.Finished);
        _bgmCPlayer.Stop();
    }

    private static double GetPlaybackTime(AudioStreamPlayer player)
    {
        var time = player.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix();
        return time;
    }

    private async void PlayButtonClickSound()
    {
        if (_effectPlayer.Playing)
        {
            return;
        }

        _effectPlayer.Stream = _buttonClickSound;
        _effectPlayer.Play();
        await ToSignal(_effectPlayer, AudioStreamPlayer.SignalName.Finished);
        _effectPlayer.Stop();
    }
}