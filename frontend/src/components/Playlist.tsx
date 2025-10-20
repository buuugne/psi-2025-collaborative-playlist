import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faEdit, faTrash } from "@fortawesome/free-solid-svg-icons";
import { useState } from "react";
import type { Playlist } from "../types/Playlist";
import "../styles/Playlists.scss";

interface PlaylistListProps {
  playlists: Playlist[];
  deletePlaylist: (id: number) => void;
  updatePlaylist: (updated: Playlist) => void;
}

export default function PlaylistList({
  playlists,
  deletePlaylist,
  updatePlaylist,
}: PlaylistListProps) {
  // Local state for editing
  const [editingId, setEditingId] = useState<number | null>(null);
  const [editName, setEditName] = useState("");
  const [editDescription, setEditDescription] = useState("");

  // Save edited playlist to backend
  const saveEdit = (playlistId: number) => {
    fetch(`http://localhost:5000/api/playlists/by-id/${playlistId}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        id: playlistId,
        name: editName,
        description: editDescription,
        songs: playlists.find((p) => p.id === playlistId)?.songs || [],
      }),
    })
      .then(async (res) => {
        if (!res.ok) {
          const text = await res.text();
          throw new Error(`Failed to save edit (${res.status}): ${text}`);
        }
        return res.json();
      })
      .then((updatedPlaylist: Playlist) => {
        updatePlaylist(updatedPlaylist);
        setEditingId(null);
      })
      .catch((err) => {
        console.error("Error updating playlist:", err);
        alert("Failed to save playlist changes.");
      });
  };

  return (
    <div className="playlist-container">
      <h2 className="playlist-title">Playlists:</h2>

      {playlists.length === 0 && (
        <p className="playlist-empty">No playlists yet — create one!</p>
      )}

      {playlists.map((playlist) => (
        <div key={playlist.id} className="playlist-card">
          {editingId === playlist.id ? (
            <>
              <input
                value={editName}
                onChange={(e) => setEditName(e.target.value)}
                placeholder="Playlist name"
              />
              <input
                value={editDescription}
                onChange={(e) => setEditDescription(e.target.value)}
                placeholder="Description"
              />
              <div className="playlist-buttons">
                <button onClick={() => saveEdit(playlist.id)}>Save</button>
                <button onClick={() => setEditingId(null)}>Cancel</button>
              </div>
            </>
          ) : (
            <>
              <div className="playlist-info">
                <h2 className="playlist-name">{playlist.name}</h2>
                <p className="playlist-desc">{playlist.description}</p>
              </div>

              <div className="playlist-icons">
                <FontAwesomeIcon
                  icon={faEdit}
                  className="icon"
                  title="Edit playlist"
                  onClick={() => {
                    setEditingId(playlist.id);
                    setEditName(playlist.name);
                    setEditDescription(playlist.description);
                  }}
                />

                <FontAwesomeIcon
                  icon={faTrash}
                  className="icon"
                  title="Delete playlist"
                  onClick={() => deletePlaylist(playlist.id)}
                />
              </div>
            </>
          )}
        </div>
      ))}
    </div>
  );
}