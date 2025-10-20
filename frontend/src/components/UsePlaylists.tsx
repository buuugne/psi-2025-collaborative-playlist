// src/hooks/usePlaylists.ts
import { useState, useEffect } from "react";
import type { Playlist } from "../types/Playlist";

export function usePlaylists() {
  const [playlists, setPlaylists] = useState<Playlist[]>([]);

  // Load playlists initially
  useEffect(() => {
    fetch("http://localhost:5000/api/playlists")
      .then(res => res.json())
      .then(data => setPlaylists(data))
      .catch(err => console.error("Failed to load playlists:", err));
  }, []);

  // Add a playlist
  const addPlaylist = (newPlaylist: Playlist) => {
    setPlaylists(prev => [...prev, newPlaylist]);
  };

  // Delete a playlist - NOW CALLS THE API
  const deletePlaylist = async (id: number) => {
    if (!confirm("Are you sure you want to delete this playlist?")) return;

    try {
      const res = await fetch(`http://localhost:5000/api/playlists/${id}`, {
        method: "DELETE"
      });

      if (!res.ok) {
        const text = await res.text();
        throw new Error(`Failed to delete (${res.status}): ${text}`);
      }

      // Only update local state after successful backend deletion
      setPlaylists(prev => prev.filter(p => p.id !== id));
    } catch (err) {
      console.error("Error deleting playlist:", err);
      alert("Failed to delete playlist. Check backend connection.");
    }
  };

  // Update a playlist
  const updatePlaylist = (updated: Playlist) => {
    setPlaylists(prev =>
      prev.map(p => (p.id === updated.id ? updated : p))
    );
  };

  return {
    playlists,
    addPlaylist,
    deletePlaylist,
    updatePlaylist,
  };
}