using System.Collections.Generic;
using Dalamud.Interface;
using Umbra.Common;
using Umbra.Widgets;

namespace Umbra.AscianMusicPlayer.Widgets;

[ToolbarWidget(
    "AmpMusicWidget",
    "Ascian Music Player",
    "Displays the currently playing track from Ascian Music Player and provides playback controls."
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

    private readonly AmpIpcClient _ipc = new();

    private AmpMusicWidgetPopup? _popup;

    public override WidgetPopup? Popup => _popup;

    protected override void OnLoad()
    {
        _popup = new AmpMusicWidgetPopup(_ipc);
        SetFontAwesomeIcon(FontAwesomeIcon.Music);
    }

    protected override void OnDraw()
    {
        if (!_ipc.IsAvailable) {
            SetText("AMP");
            return;
        }

        int    state  = _ipc.GetPlaybackState();
        string title  = _ipc.GetTitle();
        string artist = _ipc.GetArtist();

        if (state is 1 or 2 && !string.IsNullOrEmpty(title)) {
            SetText(title);
            SetSubText(string.IsNullOrEmpty(artist) ? null : artist);
        } else {
            SetText("Not playing");
            SetSubText(null);
        }
    }

    protected override void OnUnload()
    {
        _ipc.Dispose();
    }

    protected override IEnumerable<IWidgetConfigVariable> GetConfigVariables()
    {
        return [
            ..base.GetConfigVariables(),
        ];
    }
}
