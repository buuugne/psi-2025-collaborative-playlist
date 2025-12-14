import { Music, Users, Edit3, Trash2, Check, X } from "lucide-react";
import "./PlaylistCard.scss";
import type { Playlist } from "../../../../types/Playlist";
import { useState } from "react";
import { PlaylistService } from "../../../../services/PlaylistService";

interface PlaylistCardProps {
  playlist: Playlist;
  onClick?: () => void;
  onUpdated?: (updated: Playlist) => void;
  onDeleted?: (id: number) => void;
}

export default function PlaylistCard({ playlist, onClick, onUpdated, onDeleted }: PlaylistCardProps) {
  const [isEditing, setIsEditing] = useState(false);
  const [nameInput, setNameInput] = useState(playlist.name);
  const [descriptionInput, setDescriptionInput] = useState(playlist.description || "");
  const [isSaving, setIsSaving] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);


  const getImageUrl = () => {
    if (!playlist.imageUrl) {
      return `https://picsum.photos/seed/${playlist.id}/300`;
    }
  
  
    if (playlist.imageUrl.startsWith('http')) {
      return playlist.imageUrl;
    }
 
    const path = playlist.imageUrl.startsWith('/') ? playlist.imageUrl : `/${playlist.imageUrl}`;
    return path;
  };



  const handleSave = async (e?: React.MouseEvent) => {
    e?.stopPropagation();
    if (!nameInput.trim()) {
      alert("Playlist name can't be empty");
      return;
    }

    setIsSaving(true);
    try {
      const payload = {
        name: nameInput.trim(),
        description: descriptionInput.trim(),
      };
      const updated = await PlaylistService.update(playlist.id, payload);
      // Update local inputs to reflect returned data (if any)
      setNameInput(updated.name);
      setDescriptionInput(updated.description || "");
      setIsEditing(false);
      onUpdated?.(updated);
    } catch (err) {
      console.error("Failed to update playlist:", err);
      alert("Failed to update playlist");
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancelEdit = (e?: React.MouseEvent) => {
    e?.stopPropagation();
    setNameInput(playlist.name);
    setDescriptionInput(playlist.description || "");
    setIsEditing(false);
  };

  const handleDelete = async (e?: React.MouseEvent) => {
    e?.stopPropagation();
    if (!confirm(`Delete playlist "${playlist.name}"? This cannot be undone.`)) return;
    setIsDeleting(true);
    try {
      await PlaylistService.delete(playlist.id);
      onDeleted?.(playlist.id);
    } catch (err) {
      console.error("Failed to delete playlist:", err);
      alert("Failed to delete playlist");
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="playlist-card" onClick={onClick} role="button" tabIndex={0}>
      <div className="playlist-card__cover">
        <img
          src={getImageUrl()}
          alt={playlist.name}
          className="playlist-card__cover-image"
          onError={(e) => {
            console.error('Failed to load image:', getImageUrl());
            // Fallback if image fails to load
            e.currentTarget.src = `https://picsum.photos/seed/${playlist.id}/300`;
          }}
        />
        <div className="playlist-card__overlay">
          <button
            type="button"
            className="playlist-card__play-button"
            onClick={(e) => { e.stopPropagation(); /* implement play action if needed */ }}
            aria-label="Play playlist"
          >
            <Music size={24} /> +
          </button>
        </div>
      </div>

      <div className="playlist-card__info">
        {!isEditing ? (
          <>
            <h3 className="playlist-card__title">{playlist.name}</h3>
            {playlist.description && (
              <p className="playlist-card__description">{playlist.description}</p>
            )}
            <div className="playlist-card__meta">
              <Users size={14} />
              <span>Collaborative</span>
            </div>

            <div
              className="playlist-card__actions"
              onClick={(e) => e.stopPropagation()}
            >
              <button
                type="button"
                className="playlist-card__action-btn"
                title="Edit"
                onClick={(e) => { e.stopPropagation(); setIsEditing(true); }}
                aria-label="Edit playlist"
              >
                <Edit3 size={16} />
              </button>
              <button
                type="button"
                className="playlist-card__action-btn playlist-card__action-btn--danger"
                title="Delete"
                onClick={handleDelete}
                aria-label="Delete playlist"
                disabled={isDeleting}
              >
                <Trash2 size={16} />
              </button>
            </div>
          </>
        ) : (
          <>
            <input
              className="playlist-card__edit-input"
              value={nameInput}
              onChange={(e) => setNameInput(e.target.value)}
              maxLength={100}
              onClick={(e) => e.stopPropagation()}
            />
            <input
              className="playlist-card__edit-input playlist-card__edit-input--small"
              value={descriptionInput}
              onChange={(e) => setDescriptionInput(e.target.value)}
              maxLength={200}
              onClick={(e) => e.stopPropagation()}
            />

            <div className="playlist-card__edit-actions" onClick={(e) => e.stopPropagation()}>
              <button
                type="button"
                className="playlist-card__save-btn"
                onClick={handleSave}
                disabled={isSaving}
                aria-label="Save playlist"
              >
                <Check size={14} /> Save
              </button>
              <button
                type="button"
                className="playlist-card__cancel-btn"
                onClick={handleCancelEdit}
                aria-label="Cancel editing"
              >
                <X size={14} /> Cancel
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  );
}