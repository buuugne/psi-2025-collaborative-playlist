import { useEffect, useState, useRef } from "react";
import { useSpotifyPlayer } from "../context/SpotifyPlayerContext";

const API_BASE = "http://localhost:5000"

interface SpotifyLoginProps {
  hidden?: boolean;
}

export default function SpotifyLogin({ hidden }: SpotifyLoginProps) {
  const { setSpotifyToken, spotifyToken } = useSpotifyPlayer();
  const [isLoading, setIsLoading] = useState(false);
  const hasProcessedCallback = useRef(false);

  

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const code = params.get("code");

    if (code && !hasProcessedCallback.current) {
      hasProcessedCallback.current = true;
      handleCallback(code);
    }
  }, []);

  const handleCallback = async (code: string) => {
    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/api/SpotifyAuth/callback`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ code }),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to get access token");
      }

      const data = await response.json();
      
      localStorage.setItem("spotifyToken", data.accessToken);
      if (data.refreshToken) {
        localStorage.setItem("spotifyRefreshToken", data.refreshToken);
      }
      
      setSpotifyToken(data.accessToken);
      window.location.href = '/home';
    } catch (error) {
      console.error("Authentication failed:", error);
      alert(`Failed to authenticate with Spotify: ${error}`);
      window.history.replaceState({}, document.title, window.location.pathname);
    } finally {
      setIsLoading(false);
    }
  };

  const handleLogin = async () => {
    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/api/SpotifyAuth/login-url`);
      const data = await response.json();
      window.location.href = data.url;
    } catch (error) {
      console.error("Failed to get login URL:", error);
      alert("Failed to start Spotify login");
      setIsLoading(false);
    }
  };

  useEffect(() => {
    const savedToken = localStorage.getItem("spotifyToken");
    if (savedToken && !spotifyToken) {
      setSpotifyToken(savedToken);
    }
  }, [spotifyToken, setSpotifyToken]);

  const handleLogout = () => {
    localStorage.removeItem('spotifyToken');
    localStorage.removeItem('spotifyRefreshToken');
    setSpotifyToken(null);
  };

  if (hidden) return null;

  if (spotifyToken) {
    return (
      <button
        onClick={handleLogout}
        className="spotify-btn spotify-btn--connected"
        title="Disconnect Spotify"
      >
        🎵 Connected
      </button>
    );
  }

  return (
    <button
      onClick={handleLogin}
      disabled={isLoading}
      className="spotify-btn spotify-btn--disconnected"
      title="Connect to Spotify"
    >
      {isLoading ? "..." : "🎵 Connect"}
    </button>
  );
}