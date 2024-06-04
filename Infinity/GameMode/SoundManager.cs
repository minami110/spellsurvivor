using System.Linq;
using fms;
using Godot;
using R3;

public partial class SoundManager : Node
{
    [Export]
    private AudioStreamPlayer _bgmAPlayer = null!;

    [Export]
    private AudioStreamPlayer _bgmBPlayer = null!;

    [Export]
    private AudioStreamPlayer _effectPlayer = null!;

    [Export]
    private AudioStream _buttonClickSound = null!;

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
        });
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

        // Remove BGM
        var nextStartTimeLeft = 2d - GetPlaybackTime(_bgmBPlayer);
        var delay = AudioServer.GetTimeToNextMix() + AudioServer.GetOutputLatency();
        await this.WaitForSecondsAsync(nextStartTimeLeft - delay);
        _bgmAPlayer.Stop();
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