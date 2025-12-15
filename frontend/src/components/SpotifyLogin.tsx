import { useEffect, useState, useRef } from "react";
import { useSpotifyPlayer } from "../context/SpotifyPlayerContext";

const API_BASE = "http://localhost:5000/api";

export default function SpotifyLogin() {
  const { setSpotifyToken, spotifyToken } = useSpotifyPlayer();
  const [isLoading, setIsLoading] = useState(false);
  const hasProcessedCallback = useRef(false);

  // Check if we're returning from Spotify OAuth
  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const code = params.get("code");

    if (code && !hasProcessedCallback.current) {
      hasProcessedCallback.current = true;
      handleCallback(code);
    }
  }, []);

  // Exchange code for token
  const handleCallback = async (code: string) => {
    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/SpotifyAuth/callback`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ code }),
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to get access token");
      }

      const data = await response.json();
      
      // Store tokens
      localStorage.setItem("spotifyToken", data.accessToken);
      if (data.refreshToken) {
        localStorage.setItem("spotifyRefreshToken", data.refreshToken);
      }
      
      setSpotifyToken(data.accessToken);

      // Redirect to /home after successful connection
      window.location.href = '/home';
    } catch (error) {
      console.error("Authentication failed:", error);
      alert(`Failed to authenticate with Spotify: ${error}`);
      
      // Clear the URL on error so user can try again
      window.history.replaceState({}, document.title, window.location.pathname);
    } finally {
      setIsLoading(false);
    }
  };

  // Initiate Spotify login
  const handleLogin = async () => {
    setIsLoading(true);
    try {
      const response = await fetch(`${API_BASE}/SpotifyAuth/login-url`);
      const data = await response.json();
      
      // Redirect to Spotify authorization
      window.location.href = data.url;
    } catch (error) {
      console.error("Failed to get login URL:", error);
      alert("Failed to start Spotify login");
      setIsLoading(false);
    }
  };

  // Try to restore token from localStorage on mount
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

  if (spotifyToken) {
    return (
      <button
        onClick={handleLogout}
        style={{
          position: 'fixed',
          top: '60rem',
          left: '115rem',
          backgroundColor: '#059669',
          color: 'white',
          fontSize: '0.875rem',
          fontWeight: '500',
          padding: '0.5rem 1rem',
          borderRadius: '0.5rem',
          border: 'none',
          cursor: 'pointer',
          boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1)',
          zIndex: 10000
        }}
        onMouseEnter={(e) => e.currentTarget.style.backgroundColor = '#047857'}
        onMouseLeave={(e) => e.currentTarget.style.backgroundColor = '#059669'}
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
      style={{
        position: 'fixed',
        top: '60rem',
        left: '115rem',
        backgroundColor: isLoading ? '#4b5563' : '#1e293b',
        color: 'white',
        fontSize: '0.875rem',
        fontWeight: '500',
        padding: '0.5rem 1rem',
        borderRadius: '0.5rem',
        border: 'none',
        cursor: isLoading ? 'not-allowed' : 'pointer',
        boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1)',
        opacity: isLoading ? 0.5 : 1,
        zIndex: 10000
      }}
      onMouseEnter={(e) => !isLoading && (e.currentTarget.style.backgroundColor = '#334155')}
      onMouseLeave={(e) => !isLoading && (e.currentTarget.style.backgroundColor = '#1e293b')}
      title="Connect to Spotify"
    >
      {isLoading ? "..." : "🎵 Connect"}
    </button>
  );
}