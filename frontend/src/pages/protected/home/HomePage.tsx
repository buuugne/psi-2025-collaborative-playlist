import RecentPlaylists from "./components/RecentPlaylists";
import SongSearch from "./components/SongSearch";
import { useState, useEffect, useRef } from "react";
import { useNavigate } from "react-router-dom";
import type { Playlist } from "../../../types/Playlist";
import { PlaylistService } from "../../../services/PlaylistService";
import { useSpotifyPlayer } from "../../../context/SpotifyPlayerContext"; // Add this import
import "./HomePage.scss";
import CreatePlaylistForm from "./components/CreatePlaylistForm";
import Modal from "../../../components/Modal";

const API_BASE = "http://localhost:5000/api"; // Or import from config

export default function HomePage() {
  const navigate = useNavigate();
  const { setSpotifyToken } = useSpotifyPlayer(); // Add this
  const hasProcessedCallback = useRef(false); // Add this
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [isModalOpen, setIsModalOpen] = useState(false);

  // Add Spotify callback handler
  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const code = params.get("code");

    if (code && !hasProcessedCallback.current) {
      hasProcessedCallback.current = true;
      handleSpotifyCallback(code);
    }
  }, []);

  const handleSpotifyCallback = async (code: string) => {
    try {
      const response = await fetch(`${API_BASE}/SpotifyAuth/callback`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ code }),
      });

      if (!response.ok) {
        throw new Error("Failed to authenticate");
      }

      const data = await response.json();
      
      // Store tokens
      localStorage.setItem("spotifyToken", data.accessToken);
      if (data.refreshToken) {
        localStorage.setItem("spotifyRefreshToken", data.refreshToken);
      }
      
      setSpotifyToken(data.accessToken);

      // Clean up URL (remove ?code=... from address bar)
      window.history.replaceState({}, document.title, '/home');
    } catch (error) {
      console.error("Spotify authentication failed:", error);
      // Optionally show error to user
    }
  };

  const loadPlaylists = async () => {
    try {
      const all = await PlaylistService.getAll();
      setPlaylists(all);
    } catch (err) {
      console.error("Failed to load playlists:", err);
    }
  };

  useEffect(() => {
    loadPlaylists();
  }, []);

  const handleSongListChanged = async () => {
    await loadPlaylists();
  };

  const handlePlaylistClick = (playlist: Playlist) => {
    navigate(`/playlist/${playlist.id}`);
  };

  const handlePlaylistCreated = async () => {
    setIsModalOpen(false);
    await loadPlaylists();
  };

  const handlePlaylistUpdated = async () => {
    await loadPlaylists();
  };

  const handlePlaylistDeleted = async () => {
    await loadPlaylists();
  };

  return (
    <div className="home-page">
      <SongSearch onSongAdded={handleSongListChanged} playlists={playlists} />
      <RecentPlaylists
        playlists={playlists}
        onPlaylistClick={handlePlaylistClick}
        onCreateClick={() => setIsModalOpen(true)}
        onPlaylistUpdated={handlePlaylistUpdated}
        onPlaylistDeleted={handlePlaylistDeleted}
      />

      <Modal
        isOpen={isModalOpen}
        onClose={() => setIsModalOpen(false)}
        title="New collaborative playlist"
      >
        <CreatePlaylistForm
          onPlaylistCreated={handlePlaylistCreated}
          onCancel={() => setIsModalOpen(false)}
        />
      </Modal>
    </div>
  );
}