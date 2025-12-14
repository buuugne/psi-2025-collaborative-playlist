import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Music2, Plus, Search } from "lucide-react";
import PlaylistCard from "../home/components/PlaylistCard";
import Modal from "../../../components/Modal";
import CreatePlaylistForm from "../home/components/CreatePlaylistForm";
import type { Playlist } from "../../../types/Playlist";
import { PlaylistService } from "../../../services/PlaylistService";
import "./playlistsPage.scss";

export default function PlaylistsPage() {
  const navigate = useNavigate();
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const [filteredPlaylists, setFilteredPlaylists] = useState<Playlist[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [sortBy, setSortBy] = useState<"recent" | "name" | "songs">("recent");

  useEffect(() => {
    loadPlaylists();
  }, []);

  useEffect(() => {
    filterAndSortPlaylists();
  }, [playlists, searchQuery, sortBy]);

  const loadPlaylists = async () => {
    setIsLoading(true);
    try {
      const all = await PlaylistService.getAll();
      setPlaylists(all);
    } catch (err) {
      console.error("Failed to load playlists:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const filterAndSortPlaylists = () => {
    let filtered = [...playlists];

    // Filter by search query
    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (p) =>
          p.name.toLowerCase().includes(query) ||
          p.description?.toLowerCase().includes(query) 
      );
    }

    // Sort
    switch (sortBy) {
      case "name":
        filtered.sort((a, b) => a.name.localeCompare(b.name));
        break;
      case "songs":
        filtered.sort((a, b) => (b.songs?.length || 0) - (a.songs?.length || 0));
        break;
      case "recent":
      default:
        filtered.sort((a, b) => b.id - a.id);
        break;
    }

    setFilteredPlaylists(filtered);
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

  if (isLoading) {
    return (
      <div className="playlists-page playlists-page--loading">
        <div className="playlists-page__spinner"></div>
        <p>Loading playlists...</p>
      </div>
    );
  }

  return (
    <div className="playlists-page">
      <div className="playlists-page__header">
        <div className="playlists-page__header-content">
          <Music2 size={32} className="playlists-page__icon" />
          <div>
            <h1 className="playlists-page__title">Your Playlists</h1>
            <p className="playlists-page__subtitle">
              {playlists.length} {playlists.length === 1 ? "playlist" : "playlists"}
            </p>
          </div>
        </div>
        <button
          type="button"
          className="playlists-page__create-btn"
          onClick={() => setIsModalOpen(true)}
        >
          <Plus size={20} />
          New Playlist
        </button>
      </div>

      <div className="playlists-page__controls">
        <div className="playlists-page__search">
          <Search size={18} className="playlists-page__search-icon" />
          <input
            type="text"
            placeholder="Search playlists..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            className="playlists-page__search-input"
          />
        </div>

        <div className="playlists-page__sort">
          <span className="playlists-page__sort-label">Sort by:</span>
          <button
            type="button"
            className={`playlists-page__sort-btn ${
              sortBy === "recent" ? "playlists-page__sort-btn--active" : ""
            }`}
            onClick={() => setSortBy("recent")}
          >
            Recent
          </button>
          <button
            type="button"
            className={`playlists-page__sort-btn ${
              sortBy === "name" ? "playlists-page__sort-btn--active" : ""
            }`}
            onClick={() => setSortBy("name")}
          >
            Name
          </button>
          <button
            type="button"
            className={`playlists-page__sort-btn ${
              sortBy === "songs" ? "playlists-page__sort-btn--active" : ""
            }`}
            onClick={() => setSortBy("songs")}
          >
            Songs
          </button>
        </div>
      </div>

      {filteredPlaylists.length === 0 ? (
        <div className="playlists-page__empty">
          {searchQuery ? (
            <>
              <Search size={48} />
              <p className="playlists-page__empty-title">No playlists found</p>
              <p className="playlists-page__empty-subtitle">
                Try a different search term
              </p>
            </>
          ) : (
            <>
              <Music2 size={48} />
              <p className="playlists-page__empty-title">No playlists yet</p>
              <p className="playlists-page__empty-subtitle">
                Create your first collaborative playlist to get started
              </p>
              <button
                type="button"
                className="playlists-page__empty-btn"
                onClick={() => setIsModalOpen(true)}
              >
                <Plus size={20} />
                Create Playlist
              </button>
            </>
          )}
        </div>
      ) : (
        <div className="playlists-page__grid">
          {filteredPlaylists.map((playlist) => (
            <PlaylistCard
              key={playlist.id}
              playlist={playlist}
              onClick={() => handlePlaylistClick(playlist)}
              onUpdated={handlePlaylistUpdated}
              onDeleted={handlePlaylistDeleted}
            />
          ))}
        </div>
      )}

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