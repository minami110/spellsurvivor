using Godot;
using R3;

namespace fms;

public partial class SoundManager : Node
{
    [ExportGroup("Player Reference")]
    [Export]
    private AudioStreamPlayer _bgmBasePlayer = null!;

    [Export]
    private AudioStreamPlayer _bgmOptionalAPlayer = null!;

    [Export]
    private AudioStreamPlayer _bgmOptionalBPlayer = null!;

    [Export]
    private AudioStreamPlayer _effectPlayer = null!;

    private static readonly StringName BusNameMaster = new("Master");
    private static readonly StringName BusNameBgm = new("BGM");

    private static SoundManager? _instance;

    public SoundManager()
    {
        _instance = this;
    }

    public override void _Ready()
    {
        // Subscribe configs
        GameConfig.Singleton.AudioMasterVolume.Subscribe(SetMasterBusVolume).AddTo(this);
    }

    public override void _ExitTree()
    {
        _instance = null;
    }

    public static void PlayBgm(AudioStream stream)
    {
        if (_instance is not null)
        {
            _instance._bgmBasePlayer.Stream = stream;
            _instance._bgmBasePlayer.Play();
        }
    }

    public static async void PlayBgmOnOptionalTrackA(AudioStream stream, bool syncBeat = true)
    {
        if (_instance is not null)
        {
            if (syncBeat)
            {
                // Add BGM
                var nextStartTimeLeft = GetPlaybackTime(_instance._bgmBasePlayer);
                var delay = AudioServer.GetTimeToNextMix() + AudioServer.GetOutputLatency();
                await _instance.WaitForSecondsAsync(nextStartTimeLeft - delay);
                _instance._bgmOptionalAPlayer.Stream = stream;
                _instance._bgmOptionalAPlayer.Play();
            }
            else
            {
                _instance._bgmOptionalAPlayer.Stream = stream;
                _instance._bgmOptionalAPlayer.Play();
            }
        }
    }

    public static void PlaySoundEffect(AudioStream stream)
    {
        if (_instance is null)
        {
            return;
        }

        if (_instance._effectPlayer.Playing)
        {
            return;
        }

        _instance._effectPlayer.Stream = stream;
        _instance._effectPlayer.Play();
    }

    public static void SetBgmBusLowPassFilterCutOff(float hz)
    {
        if (_instance is not null)
        {
            // acess bass
            var busIndex = AudioServer.GetBusIndex(BusNameBgm);
            var lowPass = (AudioEffectLowPassFilter)AudioServer.GetBusEffect(busIndex, 0);
            lowPass.CutoffHz = hz;
        }
    }

    /// <summary>
    ///     Set Master Bus Volume
    /// </summary>
    /// <param name="volume">Linear volume [0, 1]</param>
    public static void SetMasterBusVolume(float volume)
    {
        var busIndex = AudioServer.GetBusIndex(BusNameMaster);
        AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb(volume));
        GD.Print($"Master Bus Volume => {Mathf.LinearToDb(volume)} db");
    }

    private static double GetPlaybackTime(AudioStreamPlayer player)
    {
        var time = player.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix();
        return time;
    }
}