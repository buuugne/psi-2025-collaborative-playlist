import RecentPlaylists from "./components/RecentPlaylists";
import SongSearch from "./components/SongSearch";
import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import type { Playlist } from "../../../types/Playlist";
import { PlaylistService } from "../../../services/PlaylistService";
import "./HomePage.scss";
import CreatePlaylistForm from "./components/CreatePlaylistForm";
import Modal from "../../../components/Modal";

export default function HomePage() {
  const navigate = useNavigate();
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [isModalOpen, setIsModalOpen] = useState(false);

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