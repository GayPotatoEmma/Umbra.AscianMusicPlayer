using System;
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Una.Drawing;
using Umbra.Widgets;

namespace Umbra.AscianMusicPlayer.Widgets;

internal sealed class AmpMusicWidgetPopup : WidgetPopup
{
    // FontAwesome font ID registered in Umbra (FontId.FontAwesome = 2)
    private const uint FontAwesomeFont = 2u;

    private const int PopupWidth = 280;

    private readonly AmpIpcClient _ipc;

    // Track info nodes
    private readonly Node _artNode;
    private readonly Node _titleNode;
    private readonly Node _artistNode;
    private readonly Node _albumNode;

    private const int ArtSize  = 64;
    private const int TextWidth = PopupWidth - ArtSize - 8; // 8px gap

    private string _currentArtKey = string.Empty;

    // Control button nodes
    private readonly Node _prevButton;
    private readonly Node _playPauseButton;
    private readonly Node _nextButton;

    // Progress nodes
    private readonly Node _progressTrack;
    private readonly Node _progressFill;
    private readonly Node _timeNode;

    protected override Node Node { get; }

    public AmpMusicWidgetPopup(AmpIpcClient ipc)
    {
        _ipc = ipc;

        _artNode = new() {
            Id    = "AlbumArt",
            Style = new() {
                Size            = new(ArtSize, ArtSize),
                BackgroundColor = new("Widget.PopupBorder"),
                ImageRounding   = 4f,
                ImageScaleMode  = ImageScaleMode.Adapt,
                Anchor          = Anchor.TopLeft,
            },
        };

        _titleNode = new() {
            Id    = "TrackTitle",
            Style = new() {
                Flow         = Flow.Horizontal,
                Size         = new(TextWidth, 0),
                FontSize     = 14,
                Color        = new("Widget.PopupMenuText"),
                OutlineColor = new("Widget.PopupMenuTextOutline"),
                OutlineSize  = 1,
                TextOverflow = false,
                WordWrap     = false,
            },
        };

        _artistNode = new() {
            Id    = "TrackArtist",
            Style = new() {
                Flow         = Flow.Horizontal,
                Size         = new(TextWidth, 0),
                FontSize     = 12,
                Color        = new("Widget.PopupMenuText"),
                OutlineColor = new("Widget.PopupMenuTextOutline"),
                OutlineSize  = 1,
                TextOverflow = false,
                WordWrap     = false,
            },
        };

        _albumNode = new() {
            Id    = "TrackAlbum",
            Style = new() {
                Flow         = Flow.Horizontal,
                Size         = new(TextWidth, 0),
                FontSize     = 11,
                Color        = new("Widget.PopupMenuTextMuted"),
                OutlineColor = new("Widget.PopupMenuTextOutline"),
                OutlineSize  = 1,
                TextOverflow = false,
                WordWrap     = false,
            },
        };

        _prevButton = CreateControlButton("PrevButton", FontAwesomeIcon.StepBackward);
        _playPauseButton = CreateControlButton("PlayPauseButton", FontAwesomeIcon.Play);
        _nextButton = CreateControlButton("NextButton", FontAwesomeIcon.StepForward);

        _prevButton.OnClick      += _ => _ipc.Previous();
        _playPauseButton.OnClick += _ => OnPlayPauseClicked();
        _nextButton.OnClick      += _ => _ipc.Next();

        _progressFill = new() {
            Id    = "ProgressFill",
            Style = new() {
                Size            = new(0, 4),
                BackgroundColor = new("Widget.PopupMenuText"),
                Anchor          = Anchor.TopLeft,
            },
        };

        _progressTrack = new() {
            Id         = "ProgressTrack",
            Style      = new() {
                Size            = new(PopupWidth, 4),
                BackgroundColor = new("Widget.PopupBorder"),
            },
            ChildNodes = [_progressFill],
        };

        _progressTrack.OnMouseUp += OnProgressClicked;

        _timeNode = new() {
            Id    = "TimeDisplay",
            Style = new() {
                Size         = new(PopupWidth, 0),
                FontSize     = 11,
                Color        = new("Widget.PopupMenuTextMuted"),
                OutlineColor = new("Widget.PopupMenuTextOutline"),
                OutlineSize  = 1,
                TextAlign    = Anchor.MiddleCenter,
            },
        };

        Node controlRow = new() {
            Id         = "ControlRow",
            Style      = new() {
                Flow = Flow.Horizontal,
                Size = new(PopupWidth, 0),
                Gap  = 8,
            },
            ChildNodes = [_prevButton, _playPauseButton, _nextButton],
        };

        Node separator1 = CreateSeparator();
        Node separator2 = CreateSeparator();

        Node textColumn = new() {
            Id         = "TextColumn",
            Style      = new() {
                Flow = Flow.Vertical,
                Gap  = 2,
            },
            ChildNodes = [_titleNode, _artistNode, _albumNode],
        };

        Node trackInfoSection = new() {
            Id         = "TrackInfo",
            Style      = new() {
                Flow    = Flow.Horizontal,
                Padding = new() { Bottom = 4 },
                Gap     = 8,
            },
            ChildNodes = [_artNode, textColumn],
        };

        Node progressSection = new() {
            Id         = "ProgressSection",
            Style      = new() {
                Flow = Flow.Vertical,
                Gap  = 4,
            },
            ChildNodes = [_progressTrack, _timeNode],
        };

        Node = new() {
            Id         = "AmpMusicPopup",
            Style      = new() {
                Flow    = Flow.Vertical,
                Padding = new(8, 8, 4, 8),
                Gap     = 2,
                Size    = new(PopupWidth + 16, 0),
            },
            ChildNodes = [
                trackInfoSection,
                separator1,
                controlRow,
                separator2,
                progressSection,
            ],
        };
    }

    protected override bool CanOpen() => true;

    protected override void OnUpdate()
    {
        bool available = _ipc.IsAvailable;

        string title  = available ? _ipc.GetTitle()  : string.Empty;
        string artist = available ? _ipc.GetArtist() : string.Empty;
        string album  = available ? _ipc.GetAlbum()  : string.Empty;
        float  dur    = available ? _ipc.GetDuration()  : 0f;
        float  pos    = available ? _ipc.GetPosition()  : 0f;
        int    state  = available ? _ipc.GetPlaybackState() : 0;

        _titleNode.NodeValue  = string.IsNullOrEmpty(title)  ? "No track loaded" : title;
        _artistNode.NodeValue = string.IsNullOrEmpty(artist) ? string.Empty      : artist;
        _albumNode.NodeValue  = string.IsNullOrEmpty(album)  ? string.Empty      : album;

        _artistNode.Style.IsVisible = !string.IsNullOrEmpty(artist);
        _albumNode.Style.IsVisible  = !string.IsNullOrEmpty(album);

        // Album art
        string artKey = $"{artist}|{title}|{album}";
        if (artKey != _currentArtKey) {
            _currentArtKey            = artKey;
            _artNode.Style.ImageBytes = null;
            // Flip ImageRounding to force a snapshot change so the renderer
            // clears the old art immediately (PaintStyleSnapshot tracks ImageRounding
            // but not ImageBytes, so without this the stale image persists).
            _artNode.Style.ImageRounding = 4f;
        }

        if (!string.IsNullOrEmpty(title) && _artNode.Style.ImageBytes == null) {
            AmpAlbumArtFetcher.Request(title, artist, album);
            byte[]? artBytes = AmpAlbumArtFetcher.GetCached(artKey);
            if (artBytes != null) {
                _artNode.Style.ImageBytes    = artBytes;
                _artNode.Style.ImageRounding = 4.001f; // force snapshot change to trigger render
            }
        }

        // Play / Pause icon
        _playPauseButton.NodeValue = state == 1
            ? FontAwesomeIcon.Pause.ToIconString()
            : FontAwesomeIcon.Play.ToIconString();

        // Progress bar fill
        float progress = dur > 0f ? Math.Clamp(pos / dur, 0f, 1f) : 0f;
        int   fillWidth = (int)(progress * PopupWidth);
        _progressFill.Style.Size = new(fillWidth, 4);

        // Time display
        _timeNode.NodeValue = $"{FormatTime(pos)} / {FormatTime(dur)}";
    }

    private void OnPlayPauseClicked()
    {
        if (_ipc.GetPlaybackState() == 1) {
            _ipc.Pause();
        } else {
            _ipc.Play();
        }
    }

    private void OnProgressClicked(Node node)
    {
        float dur = _ipc.GetDuration();
        if (dur <= 0f) return;

        float mouseX  = ImGui.GetMousePos().X;
        float trackX1 = _progressTrack.Bounds.ContentRect.X1;
        float trackW  = _progressTrack.Bounds.ContentSize.Width;

        if (trackW <= 0f) return;

        float t = Math.Clamp((mouseX - trackX1) / trackW, 0f, 1f);
        _ipc.SetPosition(t * dur);
    }

    private static Node CreateControlButton(string id, FontAwesomeIcon icon)
    {
        return new() {
            Id        = id,
            NodeValue = icon.ToIconString(),
            Style     = new() {
                Font         = FontAwesomeFont,
                FontSize     = 16,
                Size         = new(32, 32),
                Anchor       = Anchor.MiddleCenter,
                TextAlign    = Anchor.MiddleCenter,
                Color        = new("Widget.PopupMenuText"),
                OutlineColor = new("Widget.PopupMenuTextOutline"),
                OutlineSize  = 1,
            },
        };
    }

    private static Node CreateSeparator()
    {
        return new() {
            ClassList = ["separator"],
            Style     = new() {
                Size        = new(0, 1),
                Margin      = new() { Top = 1, Bottom = 1 },
                BackgroundColor = new("Widget.PopupBorder"),
            },
        };
    }

    private static string FormatTime(float seconds)
    {
        int total   = (int)seconds;
        int minutes = total / 60;
        int secs    = total % 60;
        return $"{minutes}:{secs:D2}";
    }
}
