import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Play,
  Pause,
  Shuffle,
  Search,
  Clock,
  Music2,
  Edit3,
  Check,
  X,
  UserPlus,
  Users,
  Trash2,
} from "lucide-react";
import { PlaylistService } from "../../../services/PlaylistService";
import { songService } from "../../../services/SongService";
// import { authService } from "../../../services/authService";
import { useSpotifyPlayer } from "../../../context/SpotifyPlayerContext";
import CollaboratorModal from "./components/CollaboratorModal";
import type { PlaylistResponseDto } from "../../../types/PlaylistResponseDto";
import type { Track } from "../../../types/Spotify";
import "./PlaylistDetailPage.scss";

export default function PlaylistDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [playlist, setPlaylist] = useState<PlaylistResponseDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [sortBy, setSortBy] = useState<"recent" | "title" | "artist">("recent");

  const [isEditing, setIsEditing] = useState(false);
  const [titleInput, setTitleInput] = useState("");
  const [descriptionInput, setDescriptionInput] = useState("");
  const [isSaving, setIsSaving] = useState(false);

  const [showSongSearch, setShowSongSearch] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState<Track[]>([]);
  const [isSearching, setIsSearching] = useState(false);

  const [showCollaboratorModal, setShowCollaboratorModal] = useState(false);


  const Equalizer = () => (
    <div className="equalizer">
      <span></span>
      <span></span>
      <span></span>
    </div>
  );

  // Spotify player integration
  const { play, pause, playerState, spotifyToken, deviceId } =
    useSpotifyPlayer();

  // const currentUser = authService.getUser();

  useEffect(() => {
    loadPlaylist();
  }, [id]);

        useEffect(() => {
        console.log("Spotify token:", spotifyToken);
        console.log("Device ID:", deviceId);
        console.log("Player ready:", playerState.isReady);
      }, [spotifyToken, deviceId, playerState.isReady]);


  useEffect(() => {
    if (playlist) {
      setTitleInput(playlist.name);
      setDescriptionInput(playlist.description || "");
    }
  }, [playlist]);

  const loadPlaylist = async () => {
    if (!id) return;

    setIsLoading(true);
    try {
      const data = await PlaylistService.getById(Number(id));
      setPlaylist(data);
    } catch (err) {
      console.error("Failed to load playlist:", err);
      alert("Failed to load playlist");
      navigate("/playlists");
    } finally {
      setIsLoading(false);
    }
  };

  const handleRemoveSong = async (songId: number) => {
    if (!playlist || !confirm("Remove this song from the playlist?")) return;

    try {
      await PlaylistService.removeFromPlaylist(playlist.id, songId);
      await loadPlaylist();
    } catch (err) {
      console.error("Failed to remove song:", err);
      alert("Failed to remove song");
    }
  };

  const handlePlaySong = async (uri: string) => {
    if (!uri) {
      alert("Cannot play - missing Spotify URI");
      return;
    }

    if (!spotifyToken || !deviceId || !playerState.isReady) {

      alert(
        "Spotify player is not ready. Please connect your Spotify account."
      );
      return;
    }

    const isCurrent = playerState.currentTrackUri === uri;

    if (isCurrent && playerState.isPlaying) {
      await pause();
      return;
    }

    await play(uri, "track");
  };

  const handlePlayPlaylist = async () => {
    if (!playlist || playlist.songs.length === 0) {
      alert("No songs to play");
      return;
    }

    const firstSong = playlist.songs[0];
    if (firstSong.spotifyUri) {
      await handlePlaySong(firstSong.spotifyUri);
    }
  };

  const handleSearchSongs = async () => {
    if (!searchQuery.trim()) {
      setSearchResults([]);
      return;
    }

    setIsSearching(true);
    try {
      const tracks = await songService.search(searchQuery);
      setSearchResults(tracks);
    } catch (err) {
      console.error("Search failed:", err);
      setSearchResults([]);
    } finally {
      setIsSearching(false);
    }
  };

  const handleAddSong = async (track: Track) => {
    if (!playlist) return;

    try {
      await songService.addToPlaylist(track, playlist.id);
      setSearchQuery("");
      setSearchResults([]);
      setShowSongSearch(false);
      await loadPlaylist();
    } catch (err) {
      console.error("Failed to add song:", err);
      alert(err instanceof Error ? err.message : "Failed to add song.");
    }
  };

  const getImageUrl = () => {
    console.log('playlist.imageUrl:', playlist?.imageUrl);
    if (!playlist?.imageUrl) return `https://picsum.photos/seed/${playlist?.id}/400`;
    if (playlist.imageUrl.startsWith('http')) return playlist.imageUrl;
    
   
    const path = playlist.imageUrl.startsWith('/') ? playlist.imageUrl : `/${playlist.imageUrl}`;
    console.log('Final image path:', path);
    return path;
  };

  const formatDuration = (durationMs?: number, seconds?: number) => {
    const totalSeconds = durationMs
      ? Math.floor(durationMs / 1000)
      : seconds || 0;
    const minutes = Math.floor(totalSeconds / 60);
    const secs = Math.floor(totalSeconds % 60);
    return `${minutes}:${secs.toString().padStart(2, "0")}`;
  };

  const getTotalDuration = () => {
    if (!playlist?.songs.length) return "0 min";
    const totalSeconds = playlist.songs.reduce((sum, song) => {
      if (song.duration) return sum + song.duration;
      return sum;
    }, 0);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    return hours > 0 ? `${hours} hr ${minutes} min` : `${minutes} min`;
  };

  const handleStartEdit = () => {
    if (!playlist) return;
    setTitleInput(playlist.name);
    setDescriptionInput(playlist.description || "");
    setIsEditing(true);
  };

  const handleCancelEdit = () => {
    if (!playlist) return;
    setTitleInput(playlist.name);
    setDescriptionInput(playlist.description || "");
    setIsEditing(false);
  };

  const handleSaveEdit = async () => {
    if (!playlist) return;
    if (!titleInput.trim()) {
      alert("Playlist name can't be empty");
      return;
    }
    setIsSaving(true);
    try {
      const payload = {
        name: titleInput.trim(),
        description: descriptionInput.trim(),
      };
      const updated = await PlaylistService.update(playlist.id, payload);
      setPlaylist(updated);
      setIsEditing(false);
    } catch (err) {
      console.error("Failed to update playlist:", err);
      alert("Failed to update playlist");
    } finally {
      setIsSaving(false);
    }
  };

  const getSortedSongs = () => {
    if (!playlist?.songs) return [];

    const songs = [...playlist.songs];

    switch (sortBy) {
      case "title":
        return songs.sort((a, b) => a.title.localeCompare(b.title));
      case "artist":
        return songs.sort((a, b) => {
          const artistA = a.artists?.[0]?.name || "";
          const artistB = b.artists?.[0]?.name || "";
          return artistA.localeCompare(artistB);
        });
      case "recent":
      default:
        return songs.sort(
          (a, b) => (b.position || b.id) - (a.position || a.id)
        );
    }
  };

  if (isLoading) {
    return (
      <div className="playlist-detail-page playlist-detail-page--loading">
        Loading...
      </div>
    );
  }

  if (!playlist) {
    return (
      <div className="playlist-detail-page playlist-detail-page--error">
        Playlist not found
      </div>
    );
  }

  return (
    <div className="playlist-detail-page">
      <div className="playlist-detail-page__header">
        <div className="playlist-detail-page__cover">
          <img src={getImageUrl()} alt={playlist.name} />
        </div>

        <div className="playlist-detail-page__info">
          {!isEditing ? (
            <>
              <h1 className="playlist-detail-page__title">{playlist.name}</h1>
              {playlist.description && (
                <p className="playlist-detail-page__description">
                  {playlist.description}
                </p>
              )}
              <div className="playlist-detail-page__meta">
                <span>
                  Created by{" "}
                  <strong>{playlist.host?.username || "Unknown"}</strong>
                </span>
                <span>•</span>
                <span>{getTotalDuration()}</span>
                <span>•</span>
                <span>
                  {playlist.songs.length}{" "}
                  {playlist.songs.length === 1 ? "song" : "songs"}
                </span>
              </div>

              {playlist.collaborators && playlist.collaborators.length > 0 && (
                <div className="playlist-detail-page__collaborators">
                  <Users size={14} />
                  <span>
                    Collaborators:{" "}
                    {playlist.collaborators.map((c) => c.username).join(", ")}
                  </span>
                </div>
              )}

              <div className="playlist-detail-page__header-actions">
                <button
                  type="button"
                  className="playlist-detail-page__edit-btn"
                  onClick={handleStartEdit}
                >
                  <Edit3 size={16} /> Edit
                </button>
                <button
                  type="button"
                  className="playlist-detail-page__edit-btn"
                  onClick={() => setShowCollaboratorModal(true)}
                >
                  <UserPlus size={16} /> Add Collaborator
                </button>
              </div>
            </>
          ) : (
            <>
              <input
                className="playlist-detail-page__title-input"
                value={titleInput}
                onChange={(e) => setTitleInput(e.target.value)}
                placeholder="Playlist name"
              />
              <input
                className="playlist-detail-page__description-input"
                value={descriptionInput}
                onChange={(e) => setDescriptionInput(e.target.value)}
                placeholder="Add a description (optional)"
              />
              <div className="playlist-detail-page__edit-actions">
                <button
                  type="button"
                  className="playlist-detail-page__save-btn"
                  onClick={handleSaveEdit}
                  disabled={isSaving}
                >
                  <Check size={14} /> {isSaving ? "Saving..." : "Save"}
                </button>
                <button
                  type="button"
                  className="playlist-detail-page__cancel-btn"
                  onClick={handleCancelEdit}
                >
                  <X size={14} /> Cancel
                </button>
              </div>
            </>
          )}
        </div>
      </div>

      <div className="playlist-detail-page__controls">
        <button
          type="button"
          className="playlist-detail-page__btn playlist-detail-page__btn--play"
          onClick={handlePlayPlaylist}
          disabled={!spotifyToken || !deviceId || !playerState.isReady}
        >
          <Play size={20} fill="currentColor" />
          Play
        </button>
        <button
          type="button"
          className="playlist-detail-page__btn playlist-detail-page__btn--icon"
        >
          <Shuffle size={20} />
          Shuffle
        </button>
        <button
          type="button"
          className={`playlist-detail-page__btn playlist-detail-page__btn--icon ${
            showSongSearch ? "playlist-detail-page__btn--active" : ""
          }`}
          onClick={() => setShowSongSearch(!showSongSearch)}
        >
          <Search size={20} />
          {showSongSearch ? "Close Search" : "Add Song"}
        </button>
      </div>

      {showSongSearch && (
        <div className="playlist-detail-page__song-search">
          <div className="playlist-detail-page__search-controls">
            <input
              type="text"
              placeholder="Search for songs on Spotify..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleSearchSongs()}
              className="playlist-detail-page__search-input"
              autoFocus
            />
            <button
              type="button"
              onClick={handleSearchSongs}
              disabled={isSearching || !searchQuery.trim()}
              className="playlist-detail-page__search-button"
            >
              {isSearching ? "Searching..." : "Search"}
            </button>
          </div>

          {searchResults.length > 0 && (
            <div className="playlist-detail-page__search-results">
              {searchResults.map((track) => (
                <div
                  key={track.id}
                  className="playlist-detail-page__search-result"
                >
                  {track.album?.images?.[0]?.url && (
                    <img
                      src={track.album.images[0].url}
                      alt={track.album.name}
                      className="playlist-detail-page__search-result-cover"
                    />
                  )}
                  <div className="playlist-detail-page__search-result-info">
                    <div className="playlist-detail-page__search-track-name">
                      {track.name}
                    </div>
                    <div className="playlist-detail-page__search-track-artists">
                      {track.artists.map((a) => a.name).join(", ")}
                    </div>
                    <div className="playlist-detail-page__search-track-album">
                      {track.album.name}
                    </div>
                  </div>
                  <span className="playlist-detail-page__search-track-duration">
                    {formatDuration(track.duration_ms)}
                  </span>
                  <button
                    className="playlist-detail-page__add-song-btn"
                    onClick={() => handleAddSong(track)}
                  >
                    + Add
                  </button>
                </div>
              ))}
            </div>
          )}

          {isSearching && (
            <div className="playlist-detail-page__search-loading">
              Searching Spotify...
            </div>
          )}

          {!isSearching && searchQuery && searchResults.length === 0 && (
            <div className="playlist-detail-page__search-empty">
              No songs found. Try a different search term.
            </div>
          )}
        </div>
      )}

      <CollaboratorModal
        playlistId={playlist.id}
        isOpen={showCollaboratorModal}
        onClose={() => setShowCollaboratorModal(false)}
        onSuccess={loadPlaylist}
      />

      <div className="playlist-detail-page__sort">
        <span className="playlist-detail-page__sort-label">Sort by</span>
        <button
          type="button"
          className={`playlist-detail-page__sort-btn ${
            sortBy === "recent" ? "playlist-detail-page__sort-btn--active" : ""
          }`}
          onClick={() => setSortBy("recent")}
        >
          Recently added
        </button>
        <button
          type="button"
          className={`playlist-detail-page__sort-btn ${
            sortBy === "title" ? "playlist-detail-page__sort-btn--active" : ""
          }`}
          onClick={() => setSortBy("title")}
        >
          By title
        </button>
        <button
          type="button"
          className={`playlist-detail-page__sort-btn ${
            sortBy === "artist" ? "playlist-detail-page__sort-btn--active" : ""
          }`}
          onClick={() => setSortBy("artist")}
        >
          By artist
        </button>
        <span className="playlist-detail-page__track-count">
          <Music2 size={16} />
          {playlist.songs.length} tracks
        </span>
      </div>

      <div className="playlist-detail-page__tracklist">
        <div className="playlist-detail-page__tracklist-header">
          <span className="playlist-detail-page__col-number">#</span>
          <span className="playlist-detail-page__col-track">Track</span>
          <span className="playlist-detail-page__col-artist">Artist</span>
          <span className="playlist-detail-page__col-album">Album</span>
          <span className="playlist-detail-page__col-added-by">Added by</span>
          <span className="playlist-detail-page__col-duration">
            <Clock size={16} />
          </span>
          <span className="playlist-detail-page__col-actions"></span>
        </div>

        {playlist.songs.length === 0 ? (
          <div className="playlist-detail-page__empty">
            <Music2 size={48} />
            <p>No tracks yet</p>
            <p className="playlist-detail-page__empty-subtitle">
              Click "Add Song" above to start building your playlist
            </p>
          </div>
        ) : (
          <div className="playlist-detail-page__tracks">
            {getSortedSongs().map((song, index) => {
              const isPlaying = playerState.currentTrackUri === song.spotifyUri;
              const hasValidUri = song.spotifyUri && song.spotifyUri !== "";

              return (
                <div key={song.id} className="playlist-detail-page__track">
                  <div className="playlist-detail-page__track-number">
                    {isPlaying && playerState.isPlaying ? (
                      <Equalizer />
                    ) : (
                      index + 1
                    )}
                  </div>

                  <div
                    className={`playlist-detail-page__track-info ${
                      isPlaying
                        ? "playlist-detail-page__track-info--playing"
                        : ""
                    }`}
                  >
                    <div className="playlist-detail-page__track-name">
                      {song.title}
                    </div>
                  </div>

                  <div className="playlist-detail-page__track-artist">
                    {song.artists?.map((a) => a.name).join(", ") || "Unknown"}
                  </div>

                  <div className="playlist-detail-page__track-album">
                    {song.album || "-"}
                  </div>

                  <div className="playlist-detail-page__track-added-by">
                    {song.addedBy?.username || "Unknown"}
                  </div>

                  <div className="playlist-detail-page__track-duration">
                    {song.durationFormatted ||
                      formatDuration(undefined, song.duration)}
                  </div>

                  <div className="playlist-detail-page__track-actions">
                    <button
                      type="button"
                      onClick={() => handlePlaySong(song.spotifyUri)}
                      className="playlist-detail-page__track-play"
                      disabled={!hasValidUri}
                      title={
                        hasValidUri
                          ? isPlaying && playerState.isPlaying
                            ? "Pause"
                            : "Play"
                          : "No Spotify URI"
                      }
                    >
                      {isPlaying && playerState.isPlaying ? (
                        <Pause size={18} />
                      ) : (
                        <Play size={18} />
                      )}
                    </button>

                    <button
                      type="button"
                      className="playlist-detail-page__track-delete"
                      onClick={() => handleRemoveSong(song.id)}
                      aria-label="Remove song"
                      title="Remove song"
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
}
