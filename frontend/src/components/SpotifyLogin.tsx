import { useEffect, useState, useRef } from "react";
import { useSpotifyPlayer } from "../context/SpotifyPlayerContext";
import { API_BASE } from "../config/api";

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

  if (!code || hasProcessedCallback.current) return;
  hasProcessedCallback.current = true;

  // ✅ clear query immediately
  window.history.replaceState({}, document.title, window.location.pathname);

  handleCallback(code);
}, []);

  const handleCallback = async (code: string) => {
  setIsLoading(true);
  try {
    const res = await fetch(`${API_BASE}/api/SpotifyAuth/callback`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ code }),
    });

    if (!res.ok) throw new Error("Spotify auth failed");

    const data = await res.json();

    localStorage.setItem("spotifyToken", data.accessToken);
    if (data.refreshToken) {
      localStorage.setItem("spotifyRefreshToken", data.refreshToken);
    }

    setSpotifyToken(data.accessToken);

    // ✅ real navigation (and no history entry)
    window.location.replace("/home");
  } catch (err) {
    console.error(err);
    alert("Spotify authentication failed");
  } finally {
    setIsLoading(false);
  }
};

  const handleLogin = async () => {
    setIsLoading(true);

    try {
      const res = await fetch(`${API_BASE}/api/SpotifyAuth/login-url`);
      const data = await res.json();
      window.location.href = data.url;
    } catch {
      alert("Failed to start Spotify login");
      setIsLoading(false);
    }
  };

  // restore token on reload
  useEffect(() => {
    const saved = localStorage.getItem("spotifyToken");
    if (saved && !spotifyToken) setSpotifyToken(saved);
  }, [spotifyToken, setSpotifyToken]);

  if (hidden) return null;

  return (
    <button onClick={handleLogin} disabled={isLoading}>
      {spotifyToken ? "🎵 Connected" : "🎵 Connect"}
    </button>
  );
}
