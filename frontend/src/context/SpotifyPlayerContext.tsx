import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
} from "react";
import type { Song } from "../types/Song";
import { API_BASE } from "../config/api";

interface PlayerState {
  isPlaying: boolean;
  currentTrack: Song | null;
  position: number;
  duration: number;
  volume: number;
  isPaused: boolean;
  isReady: boolean;
  currentTrackUri: string | null;
}

interface SpotifyPlayerContextType {
  spotifyToken: string | null;
  setSpotifyToken: (token: string | null) => void;
  playerState: PlayerState;

  deviceId: string | null;

  play: (trackUri: string, type?: "track" | "playlist") => Promise<void>;
  stop: () => Promise<void>;
  pause: () => Promise<void>;
  resume: () => Promise<void>;
  setVolume: (volume: number) => Promise<void>;
}

const SpotifyPlayerContext = createContext<
  SpotifyPlayerContextType | undefined
>(undefined);

export const SpotifyPlayerProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [playerState, setPlayerState] = useState<PlayerState>({
    isPlaying: false,
    currentTrack: null,
    position: 0,
    duration: 0,
    volume: 0.5,
    isPaused: true,
    isReady: false,
    currentTrackUri: null,
  });

  const [spotifyToken, setSpotifyTokenState] = useState<string | null>(null);
  const [deviceId, setDeviceId] = useState<string | null>(null);
  const [player, setPlayer] = useState<any>(null);

  const setSpotifyToken = useCallback((token: string | null) => {
    setSpotifyTokenState(token);
  }, []);

  // Auto-refresh token every 50 minutes
  useEffect(() => {
    if (!spotifyToken) return;

    const refreshToken = async () => {
      const refreshTokenValue = localStorage.getItem("spotifyRefreshToken");
      if (!refreshTokenValue) return;

      try {
        const response = await fetch(`${API_BASE}/api/SpotifyAuth/refresh`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ refreshToken: refreshTokenValue }),
        });

        if (response.ok) {
          const data = await response.json();
          localStorage.setItem("spotifyToken", data.accessToken);
          setSpotifyTokenState(data.accessToken);
          console.log("✅ Token refreshed automatically");
        } else {
          console.error("Failed to refresh token");
          localStorage.removeItem("spotifyToken");
          localStorage.removeItem("spotifyRefreshToken");
          setSpotifyTokenState(null);
        }
      } catch (error) {
        console.error("Error refreshing token:", error);
      }
    };

    // Refresh token every 50 minutes (tokens expire after 1 hour)
    const interval = setInterval(refreshToken, 50 * 60 * 1000);

    return () => clearInterval(interval);
  }, [spotifyToken]);

  // Initialize Spotify Web Playback SDK
  useEffect(() => {
    if (!spotifyToken) return;

    const initializePlayer = () => {
      const spotifyPlayer = new (window as any).Spotify.Player({
        name: "Playlist Web Player",
        getOAuthToken: (cb: (token: string) => void) => cb(spotifyToken),
        volume: 0.5,
      });

      spotifyPlayer.addListener("ready", ({ device_id }: any) => {
        setDeviceId(device_id);
        setPlayerState((prev) => ({ ...prev, isReady: true }));
      });

      spotifyPlayer.addListener("player_state_changed", (state: any) => {
        if (!state) return;

        const track = state.track_window.current_track;

        setPlayerState((prev) => ({
          ...prev,
          isPlaying: !state.paused,
          isPaused: state.paused,
          position: Math.floor(state.position / 1000),
          duration: Math.floor(state.duration / 1000),
          currentTrackUri: track ? track.uri : null,
          currentTrack: track
            ? {
                id: 0,
                spotifyId: track.id,
                spotifyUri: track.uri,
                title: track.name,
                artists: track.artists.map((a: any) => ({ name: a.name })),
                album: track.album?.name || "",
                duration: Math.floor(state.duration / 1000),
                durationFormatted: "",
              }
            : null,
        }));
      });

      spotifyPlayer.connect();
      setPlayer(spotifyPlayer);
    };

    if ((window as any).Spotify) {
      initializePlayer();
      return;
    }

    const script = document.createElement("script");
    script.src = "https://sdk.scdn.co/spotify-player.js";
    script.async = true;
    document.body.appendChild(script);

    (window as any).onSpotifyWebPlaybackSDKReady = initializePlayer;

    return () => {
      if (player) player.disconnect();
    };
  }, [spotifyToken]);

  // PLAY
  const play = useCallback(
    async (uri: string, type: "track" | "playlist" = "track") => {
      if (!spotifyToken || !deviceId) return;

      let body: any = {};

      if (type === "track") {
        body = { uris: [uri] };
      } else if (type === "playlist") {
        body = { context_uri: uri };
      }

      try {
        const response = await fetch(
          `https://api.spotify.com/v1/me/player/play?device_id=${deviceId}`,
          {
            method: "PUT",
            headers: {
              "Content-Type": "application/json",
              Authorization: `Bearer ${spotifyToken}`,
            },
            body: JSON.stringify(body),
          }
        );

        // If token expired (401), try to refresh
        if (response.status === 401) {
          console.log("Token expired, attempting refresh...");
          const refreshTokenValue = localStorage.getItem("spotifyRefreshToken");

          if (refreshTokenValue) {
            const refreshResponse = await fetch(
              `${API_BASE}/api/SpotifyAuth/refresh`,
              {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ refreshToken: refreshTokenValue }),
              }
            );

            if (refreshResponse.ok) {
              const data = await refreshResponse.json();
              localStorage.setItem("spotifyToken", data.accessToken);
              setSpotifyTokenState(data.accessToken);
              return;
            }
          }

          // Refresh failed, force re-login
          localStorage.removeItem("spotifyToken");
          localStorage.removeItem("spotifyRefreshToken");
          setSpotifyTokenState(null);
          alert("Session expired. Please log in again.");
        }

        setPlayerState((prev) => ({
          ...prev,
          currentTrackUri: uri,
        }));
      } catch (error) {
        console.error("Error playing track:", error);
      }
    },
    [spotifyToken, deviceId]
  );

  // STOP — forces pause + resets state
  const stop = useCallback(async () => {
    if (!spotifyToken || !deviceId) return;

    await fetch(
      `https://api.spotify.com/v1/me/player/pause?device_id=${deviceId}`,
      {
        method: "PUT",
        headers: { Authorization: `Bearer ${spotifyToken}` },
      }
    );

    setPlayerState((prev) => ({
      ...prev,
      isPlaying: false,
      isPaused: true,
      position: 0,
    }));
  }, [spotifyToken, deviceId]);

  // PAUSE
  const pause = useCallback(async () => {
    if (!player) return;
    await player.pause();
  }, [player]);

  // RESUME
  const resume = useCallback(async () => {
    if (!player) return;
    await player.resume();
  }, [player]);

  // VOLUME
  const setVolume = useCallback(
    async (volume: number) => {
      if (!player) return;
      await player.setVolume(volume);
      setPlayerState((prev) => ({ ...prev, volume }));
    },
    [player]
  );

  const value: SpotifyPlayerContextType = {
    playerState,
    spotifyToken,
    deviceId,
    play,
    stop,
    pause,
    resume,
    setVolume,
    setSpotifyToken,
  };

  return (
    <SpotifyPlayerContext.Provider value={value}>
      {children}
    </SpotifyPlayerContext.Provider>
  );
};

export const useSpotifyPlayer = () => {
  const context = useContext(SpotifyPlayerContext);
  if (!context)
    throw new Error(
      "useSpotifyPlayer must be used within SpotifyPlayerProvider"
    );
  return context;
};
