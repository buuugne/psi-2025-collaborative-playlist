import { useState, useRef } from "react";
import PlaylistList, {
  type PlaylistListHandle,
} from "../../../components/PlaylistList";
import PlaylistDisplay from "../../../components/PlaylistDisplay";
import SongSearch from "../../../components/SongSearch";
import PlaylistModal from "../../../components/PlaylistModal";
import type { PlaylistResponseDto } from "../../../types/PlaylistResponseDto";
import type { Playlist } from "../../../types/Playlist";
import {PlaylistService } from "../../../services/PlaylistService";
import SpotifyLogin from "../../../components/SpotifyLogin";

export default function MainPage() {
  const [selectedPlaylist, setSelectedPlaylist] =
    useState<PlaylistResponseDto | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [playlists, setPlaylists] = useState<Playlist[]>([]);
  const playlistListRef = useRef<PlaylistListHandle>(null);

  const handleSongListChanged = async () => {
    if (!selectedPlaylist) return;
  
    try {
      const updated = await PlaylistService.getById(selectedPlaylist.id);
      setSelectedPlaylist(updated);
    } catch (err) {
      console.error("Failed to refresh playlist:", err);
    }
  };

  const handlePlaylistsLoaded = (data: Playlist[]) => setPlaylists(data);

  return (
    <>
    <SpotifyLogin />
      <aside className="left-side w-48 p-4 border-r border-neutral-800">
        <button
          data-testid="new-playlist-btn"
          className="mb-4 w-full bg-green-500 hover:bg-green-600 text-white py-2 rounded-lg font-semibold"
          onClick={() => setIsModalOpen(true)}
        >
          + New Playlist
        </button>

        <PlaylistList
          ref={playlistListRef}
          onSelect={(p) => setSelectedPlaylist(p as unknown as PlaylistResponseDto)}
          onPlaylistsLoaded={handlePlaylistsLoaded} />
      </aside>

      <div className="vDivider w-px bg-neutral-800"></div>

      <div className="flex flex-col flex-1 overflow-hidden">
        <SongSearch onSongAdded={handleSongListChanged} playlists={playlists} />
        <div className="flex-1 px-6 pb-6 overflow-hidden mt-4">
          <PlaylistDisplay
            playlist={selectedPlaylist}
            onSongRemoved={handleSongListChanged} />
        </div>
      </div>

      {isModalOpen && (
        <PlaylistModal
          close={() => setIsModalOpen(false)}
          onPlaylistCreated={async () => {
            setIsModalOpen(false);
            await playlistListRef.current?.refresh();
          } } />
      )}
    </>
    
  );
}

