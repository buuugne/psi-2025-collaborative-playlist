import { useState } from "react";
import type { Playlist } from "../types/Playlist.ts";

export default function CreatePlaylistForm({
  onPlaylistCreated,
}: {
  onPlaylistCreated: (p: Playlist) => void;
}) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");

  // runs when the Create button is clicked
  const createPlaylist = () => {
    fetch("http://localhost:5000/api/playlists", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, description, songs: [] }),
    })
      .then((res) => res.json())
      .then((newPlaylist: Playlist) => {
        // ✅ instantly notify parent about the new playlist
        onPlaylistCreated(newPlaylist);

        // clear inputs
        setName("");
        setDescription("");
      });
  };

  return (
    <div>
      <h2>Create new playlist</h2>
      <input
        placeholder="Playlist name"
        value={name}
        onChange={(e) => setName(e.target.value)}
      />
      <input
        placeholder="Description"
        value={description}
        onChange={(e) => setDescription(e.target.value)}
      />
      <button onClick={createPlaylist}>Create</button>
    </div>
  );
}
