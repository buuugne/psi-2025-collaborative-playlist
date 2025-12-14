import { useState } from "react";
import { Sparkles} from "lucide-react";
import type { Playlist } from "../../../../types/Playlist";
import { PlaylistService } from "../../../../services/PlaylistService";
import { authService } from "../../../../services/authService";
import "./CreatePlaylistForm.scss";

interface CreatePlaylistFormProps {
  onPlaylistCreated: (newPlaylist: Playlist) => void;
  onCancel: () => void;
}

export default function CreatePlaylistForm({ onPlaylistCreated, onCancel }: CreatePlaylistFormProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [inviteEmail, setInviteEmail] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) setImageFile(file);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!name.trim()) {
      alert("Please enter a playlist name");
      return;
    }

    const currentUser = authService.getUser();
    if (!currentUser) {
      alert("You must be logged in to create a playlist");
      return;
    }

    setIsSubmitting(true);
    try {

      const newPlaylist = await PlaylistService.create({
        name: name.trim(),
        description: description.trim() || undefined,
        imageFile: imageFile || undefined
      });
      
      console.log("Created playlist response:", newPlaylist);
      console.log("Image URL:", newPlaylist.imageUrl);
      
      onPlaylistCreated(newPlaylist);
    } catch (err) {
      console.error("Failed to create playlist:", err);
      alert("Failed to create playlist. Please try again.");
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <form className="create-playlist-form" onSubmit={handleSubmit}>
      <div className="create-playlist-form__header">
        <p className="create-playlist-form__subtitle">
          Name your playlist, choose visibility and start inviting collaborators.
        </p>
        <div className="create-playlist-form__badge">
          <Sparkles size={16} />
          <span>Real-time collaboration enabled</span>
        </div>
      </div>

      <div className="create-playlist-form__field">
        <label className="create-playlist-form__label">
          Playlist name
          <span className="create-playlist-form__required">Required</span>
        </label>
        <input
          type="text"
          className="create-playlist-form__input"
          placeholder="Friday Night with the crew"
          value={name}
          onChange={(e) => setName(e.target.value)}
          maxLength={100}
          required
        />
      </div>

      <div className="create-playlist-form__field">
        <label className="create-playlist-form__label">
          Description
          <span className="create-playlist-form__optional">Optional</span>
        </label>
        <input
          type="text"
          className="create-playlist-form__input"
          placeholder="Add a short vibe or theme for this playlist..."
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          maxLength={200}
        />
      </div>

      <div className="create-playlist-form__field">
        <label className="create-playlist-form__label" htmlFor="Cover-image">
          Cover image
          <span className="create-playlist-form__optional">Optional</span>
        </label>
        <input
          id="Cover-image"
          type="file"
          accept="image/*"
          className="create-playlist-form__file-input"
          onChange={handleImageChange}
        />
        {imageFile && (
          <div className="create-playlist-form__file-preview">
            <p className="create-playlist-form__file-name">{imageFile.name}</p>
            <img 
              src={URL.createObjectURL(imageFile)} 
              alt="Preview" 
              style={{ maxWidth: '200px', maxHeight: '200px', marginTop: '8px', borderRadius: '4px' }}
            />
          </div>
        )}
      </div>

      <div className="create-playlist-form__section">
        <div className="create-playlist-form__section-header">
          <h3 className="create-playlist-form__section-title">Invite collaborators</h3>
          <p className="create-playlist-form__section-subtitle">You can add more later</p>
        </div>
        <div className="create-playlist-form__invite">
          <input
            type="text"
            className="create-playlist-form__input"
            placeholder="Type a name or email..."
            value={inviteEmail}
            onChange={(e) => setInviteEmail(e.target.value)}
          />
          <button
            type="button"
            className="create-playlist-form__invite-button"
            disabled={!inviteEmail.trim()}
          >
            Add
          </button>
        </div>
        <p className="create-playlist-form__help-text">
          Or share later with a link from the playlist header.
        </p>
      </div>


      <div className="create-playlist-form__actions">
        <button
          type="button"
          className="create-playlist-form__button create-playlist-form__button--secondary"
          onClick={onCancel}
          disabled={isSubmitting}
        >
          Cancel
        </button>
        <button
          type="submit"
          className="create-playlist-form__button create-playlist-form__button--primary"
          disabled={isSubmitting || !name.trim()}
        >
          {isSubmitting ? "Creating..." : "+ Create playlist"}
        </button>
      </div>
    </form>
  );
}