using System;
using Dalamud.Plugin.Ipc;
using Umbra.Common;

namespace Umbra.AscianMusicPlayer.Widgets;

/// <summary>
/// Cached IPC client for the Ascian Music Player plugin.
/// </summary>
internal sealed class AmpIpcClient : IDisposable
{
    private readonly ICallGateSubscriber<string>        _getTitle;
    private readonly ICallGateSubscriber<string>        _getArtist;
    private readonly ICallGateSubscriber<string>        _getAlbum;
    private readonly ICallGateSubscriber<float>         _getDuration;
    private readonly ICallGateSubscriber<float>         _getPosition;
    private readonly ICallGateSubscriber<int>           _getPlaybackState;
    private readonly ICallGateSubscriber<float>         _getVolume;
    private readonly ICallGateSubscriber<bool>          _getShuffle;
    private readonly ICallGateSubscriber<int>           _getRepeatMode;
    private readonly ICallGateSubscriber<object>        _play;
    private readonly ICallGateSubscriber<object>        _pause;
    private readonly ICallGateSubscriber<object>        _next;
    private readonly ICallGateSubscriber<object>        _previous;
    private readonly ICallGateSubscriber<object>        _toggleShuffle;
    private readonly ICallGateSubscriber<object>        _toggleRepeat;
    private readonly ICallGateSubscriber<float, object> _setVolume;
    private readonly ICallGateSubscriber<float, object> _setPosition;

    public AmpIpcClient()
    {
        var pi = Framework.DalamudPlugin;

        _getTitle         = pi.GetIpcSubscriber<string>("AscianMusicPlayer.GetTitle");
        _getArtist        = pi.GetIpcSubscriber<string>("AscianMusicPlayer.GetArtist");
        _getAlbum         = pi.GetIpcSubscriber<string>("AscianMusicPlayer.GetAlbum");
        _getDuration      = pi.GetIpcSubscriber<float>("AscianMusicPlayer.GetDuration");
        _getPosition      = pi.GetIpcSubscriber<float>("AscianMusicPlayer.GetPosition");
        _getPlaybackState = pi.GetIpcSubscriber<int>("AscianMusicPlayer.GetPlaybackState");
        _getVolume        = pi.GetIpcSubscriber<float>("AscianMusicPlayer.GetVolume");
        _getShuffle       = pi.GetIpcSubscriber<bool>("AscianMusicPlayer.GetShuffle");
        _getRepeatMode    = pi.GetIpcSubscriber<int>("AscianMusicPlayer.GetRepeatMode");
        _play             = pi.GetIpcSubscriber<object>("AscianMusicPlayer.Play");
        _pause            = pi.GetIpcSubscriber<object>("AscianMusicPlayer.Pause");
        _next             = pi.GetIpcSubscriber<object>("AscianMusicPlayer.Next");
        _previous         = pi.GetIpcSubscriber<object>("AscianMusicPlayer.Previous");
        _toggleShuffle    = pi.GetIpcSubscriber<object>("AscianMusicPlayer.ToggleShuffle");
        _toggleRepeat     = pi.GetIpcSubscriber<object>("AscianMusicPlayer.ToggleRepeat");
        _setVolume        = pi.GetIpcSubscriber<float, object>("AscianMusicPlayer.SetVolume");
        _setPosition      = pi.GetIpcSubscriber<float, object>("AscianMusicPlayer.SetPosition");
    }

    /// <summary>
    /// Returns true if the Ascian Music Player plugin is currently loaded and available.
    /// </summary>
    public bool IsAvailable {
        get {
            try {
                _getPlaybackState.InvokeFunc();
                return true;
            } catch {
                return false;
            }
        }
    }

    public string GetTitle()         { try { return _getTitle.InvokeFunc(); }         catch { return string.Empty; } }
    public string GetArtist()        { try { return _getArtist.InvokeFunc(); }        catch { return string.Empty; } }
    public string GetAlbum()         { try { return _getAlbum.InvokeFunc(); }         catch { return string.Empty; } }
    public float  GetDuration()      { try { return _getDuration.InvokeFunc(); }      catch { return 0f; } }
    public float  GetPosition()      { try { return _getPosition.InvokeFunc(); }      catch { return 0f; } }
    public int    GetPlaybackState() { try { return _getPlaybackState.InvokeFunc(); } catch { return 0; } }
    public float  GetVolume()        { try { return _getVolume.InvokeFunc(); }        catch { return 0f; } }
    public bool   GetShuffle()       { try { return _getShuffle.InvokeFunc(); }       catch { return false; } }
    public int    GetRepeatMode()    { try { return _getRepeatMode.InvokeFunc(); }    catch { return 0; } }

    public void Play()                     { try { _play.InvokeAction(); }                  catch { } }
    public void Pause()                    { try { _pause.InvokeAction(); }                 catch { } }
    public void Next()                     { try { _next.InvokeAction(); }                  catch { } }
    public void Previous()                 { try { _previous.InvokeAction(); }              catch { } }
    public void ToggleShuffle()            { try { _toggleShuffle.InvokeAction(); }         catch { } }
    public void ToggleRepeat()             { try { _toggleRepeat.InvokeAction(); }          catch { } }
    public void SetVolume(float volume)    { try { _setVolume.InvokeAction(volume); }       catch { } }
    public void SetPosition(float seconds) { try { _setPosition.InvokeAction(seconds); }   catch { } }

    public void Dispose() { }
}
