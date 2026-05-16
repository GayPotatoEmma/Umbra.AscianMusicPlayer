using System.Collections.Generic;
using Dalamud.Interface;
using Umbra.Common;
using Umbra.Game;
using Umbra.Widgets;
using Una.Drawing;

namespace Umbra.AscianMusicPlayer.Widgets;

[ToolbarWidget(
    "AmpMusicWidget",
    "Music Player",
    "Displays the currently playing track from a music player plugin and provides playback controls."
)]
public class AmpMusicWidget(
    WidgetInfo                  info,
    string?                     guid         = null,
    Dictionary<string, object>? configValues = null
) : StandardToolbarWidget(info, guid, configValues)
{
    protected override StandardWidgetFeatures Features =>
        StandardWidgetFeatures.Text |
        StandardWidgetFeatures.SubText |
        StandardWidgetFeatures.Icon;

    private AmpIpcClient _ipc = new();

    private AmpMusicWidgetPopup? _popup;

    private string _currentProvider = "AscianMusicPlayer";

    public override WidgetPopup? Popup => _popup;

    protected override void OnLoad()
    {
        _popup = new AmpMusicWidgetPopup(_ipc);
        SetFontAwesomeIcon(FontAwesomeIcon.Music);

        Node.OnRightClick += OnRightClicked;
    }

    protected override void OnDraw()
    {
        string provider = GetConfigValue<string>("MusicProvider");

        if (provider != _currentProvider) {
            _currentProvider = provider;
            _ipc.Dispose();
            _ipc    = new AmpIpcClient(provider);
            _popup  = new AmpMusicWidgetPopup(_ipc);
        }

        if (!_ipc.IsAvailable) {
            SetText("Music");
            return;
        }

        int    state  = _ipc.GetPlaybackState();
        string title  = _ipc.GetTitle();
        string artist = _ipc.GetArtist();

        bool hideWhenPaused = GetConfigValue<bool>("HideTextWhenPaused")
            && GetConfigValue<string>("DisplayMode") == "TextAndIcon";

        if (state != 1 && hideWhenPaused) {
            SetText(null);
            SetSubText(null);
        } else if (state is 1 or 2 && !string.IsNullOrEmpty(title)) {
            SetText(title);
            SetSubText(string.IsNullOrEmpty(artist) ? null : artist);
        } else {
            SetText("Not playing");
            SetSubText(null);
        }
    }

    protected override void OnUnload()
    {
        Node.OnRightClick -= OnRightClicked;
        _ipc.Dispose();
    }

    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        return [
            ..base.GetConfigVariables(),
            new SelectWidgetConfigVariable(
                "MusicProvider",
                "Music provider",
                "The music player plugin to use as the data source.",
                "AscianMusicPlayer",
                new() {
                    { "AscianMusicPlayer", ProviderLabel("AscianMusicPlayer", "Ascian Music Player") },
                    { "FantasyPlayer",     ProviderLabel("FantasyPlayer",     "FantasyPlayer") },
                }
            ),
            new BooleanWidgetConfigVariable(
                "HideTextWhenPaused",
                "Hide text when paused",
                "Hides the track title and artist text when playback is paused.",
                false
            ) { DisplayIf = () => GetConfigValue<string>("DisplayMode") == "TextAndIcon" },
            new SelectWidgetConfigVariable(
                "RightClickBehavior",
                "Right-click behavior",
                "What happens when you right-click the widget.",
                "OpenAmpWindow",
                new() {
                    { "OpenAmpWindow", "Open music player window" },
                    { "PlayPause",     "Play / Pause" },
                }
            ),
        ];
    }

    private static string ProviderLabel(string pluginName, string displayName)
    {
        using var probe = new AmpIpcClient(pluginName);
        return probe.IsAvailable ? displayName : $"{displayName} (Not installed)";
    }

    private void OnRightClicked(Node _)
    {
        switch (GetConfigValue<string>("RightClickBehavior")) {
            case "PlayPause":
                if (_ipc.GetPlaybackState() == 1) {
                    _ipc.Pause();
                } else {
                    _ipc.Play();
                }
                break;
            default:
                string command = GetConfigValue<string>("MusicProvider") == "FantasyPlayer"
                    ? "/pfp settings"
                    : "/amp";
                Framework.Service<IChatSender>().Send(command);
                break;
        }
    }
}
