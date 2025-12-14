import { useState } from "react";
import type { Track } from "../../../../types/Spotify";
import type { Playlist } from "../../../../types/Playlist";
import { songService } from "../../../../services/SongService";
import "./SongSearch.scss";

interface SongSearchProps {
  onSongAdded?: () => void;
  playlists: Playlist[];
}

export default function SongSearch({ onSongAdded, playlists }: SongSearchProps) {
  const [selectedPlaylistId, setSelectedPlaylistId] = useState<number | null>(null);
  const [query, setQuery] = useState("");
  const [results, setResults] = useState<Track[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const safePlaylistsArray = Array.isArray(playlists) ? playlists : [];

  const handleSearch = async () => {
    if (!query.trim()) {
      setResults([]);
      setIsSearching(false);
      return;
    }

    setIsSearching(true);
    setIsLoading(true);

    try {
      const tracks = await songService.search(query);
      setResults(tracks);
    } catch (err) {
      console.error("Search failed:", err);
      setResults([]);
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddSong = async (track: Track) => {
    if (!selectedPlaylistId) {
      alert("Please select a playlist first!");
      return;
    }

    try {
      await songService.addToPlaylist(track, selectedPlaylistId);
      alert("Song added successfully!");
      setQuery("");
      setResults([]);
      setIsSearching(false);
      onSongAdded?.();
    } catch (err) {
      console.error("Failed to add song:", err);
      alert(err instanceof Error ? err.message : "Failed to add song.");
    }
  };

  return (
    <div className="song-search">
      <div className="song-search__controls">
        <select
          aria-label="Choose a playlist"
          data-testid="song-search-playlist-select"
          value={selectedPlaylistId ?? ""}
          onChange={(e) => setSelectedPlaylistId(Number(e.target.value))}
          className="song-search__select"
        >
          <option value="">-- Select playlist --</option>
          {safePlaylistsArray.map((p) => (
            <option key={p.id} value={p.id}>
              {p.name}
            </option>
          ))}
        </select>

        <input
          data-testid="song-search-input"
          type="text"
          placeholder="Search for songs..."
          value={query}
          onChange={(e) => setQuery(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleSearch()}
          className="song-search__input"
        />

        <button
          type="button"
          onClick={handleSearch}
          disabled={isLoading}
          className="song-search__search-button"
        >
          {isLoading ? "..." : "🔍"}
        </button>
      </div>

      {isSearching && results.length > 0 && (
        <div className="song-search__results">
          {results.map((track) => (
            <div key={track.id} className="song-search__result">
              <div className="song-search__result-info">
                <div className="song-search__track-name">{track.name}</div>
                <div className="song-search__track-artists">
                  {track.artists.map((a) => a.name).join(", ")}
                </div>
                <div className="song-search__track-album">{track.album.name}</div>
                <a
                  href={track.external_urls.spotify}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="song-search__spotify-link"
                >
                  Listen on Spotify
                </a>
              </div>

              <button
                className="song-search__add-button"
                data-testid="song-add-btn"
                onClick={() => handleAddSong(track)}
              >
                + Add
              </button>
            </div>
          ))}
        </div>
      )}

      {isSearching && results.length === 0 && !isLoading && (
        <p className="song-search__no-results">No songs found.</p>
      )}
    </div>
  );
}