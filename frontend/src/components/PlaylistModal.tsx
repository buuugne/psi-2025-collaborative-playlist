import "../styles/PlaylistModal.scss";
import CreatePlaylistForm from "./CreatePlaylist";
import type { Playlist } from "../types/Playlist";

export default function PlaylistModal({
  close,
  onPlaylistCreated,
}: {
  close: () => void;
  onPlaylistCreated: (p: Playlist) => void;
}) {
  return (
    <div className="modal-overlay" onClick={close}>
      <div className="modal-box" onClick={(e) => e.stopPropagation()}>
        <h2>Create Playlist</h2>
        {/* ✅ pass the callback down */}
        <CreatePlaylistForm onPlaylistCreated={onPlaylistCreated} />
        <button onClick={close}>Close</button>
      </div>
    </div>
  );
}
