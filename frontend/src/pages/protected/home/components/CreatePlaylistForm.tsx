import { useState, useEffect } from "react";
import { Sparkles, Search, X, Users } from "lucide-react";
import type { Playlist } from "../../../../types/Playlist";
import { PlaylistService } from "../../../../services/PlaylistService";
import { authService } from "../../../../services/authService";
import "./CreatePlaylistForm.scss";

interface CreatePlaylistFormProps {
  onPlaylistCreated: (newPlaylist: Playlist) => void;
  onCancel: () => void;
}

interface SearchResult {
  id: number;
  username: string;
  profileImage?: string;
}

export default function CreatePlaylistForm({ onPlaylistCreated, onCancel }: CreatePlaylistFormProps) {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [invitedUsers, setInvitedUsers] = useState<SearchResult[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Search as user types
  useEffect(() => {
    const searchUsers = async () => {
      if (searchQuery.trim().length === 0) {
        setSearchResults([]);
        return;
      }

      setIsSearching(true);
      try {
        const results = await PlaylistService.searchUsers(searchQuery.trim());
        setSearchResults(results);
      } catch (err) {
        console.error("Search failed:", err);
        setSearchResults([]);
      } finally {
        setIsSearching(false);
      }
    };

    const debounceTimer = setTimeout(searchUsers, 300);
    return () => clearTimeout(debounceTimer);
  }, [searchQuery]);

  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) setImageFile(file);
  };

  const handleAddUser = (user: SearchResult) => {
    if (!invitedUsers.find((u) => u.id === user.id)) {
      setInvitedUsers([...invitedUsers, user]);
      setSearchQuery("");
      setSearchResults([]);
    }
  };

  const handleRemoveUser = (userId: number) => {
    setInvitedUsers(invitedUsers.filter((u) => u.id !== userId));
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

      // Add invited collaborators after playlist is created
      for (const user of invitedUsers) {
        try {
          await PlaylistService.addCollaborator(newPlaylist.id, user.username);
        } catch (err) {
          console.error(`Failed to add collaborator ${user.username}:`, err);
        }
      }

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
              className="create-playlist-form__image-preview"
            />
          </div>
        )}
      </div>

      <div className="create-playlist-form__section">
        <div className="create-playlist-form__section-header">
          <h3 className="create-playlist-form__section-title">Invite collaborators</h3>
          <p className="create-playlist-form__section-subtitle">You can add more later</p>
        </div>

        {/* Invited Users List */}
        {invitedUsers.length > 0 && (
          <div className="create-playlist-form__invited-list">
            {invitedUsers.map((user) => (
              <div key={user.id} className="create-playlist-form__invited-item">
                {user.profileImage ? (
                  <img
                    src={user.profileImage}
                    alt={user.username}
                    className="create-playlist-form__invited-avatar"
                  />
                ) : (
                  <div className="create-playlist-form__invited-avatar create-playlist-form__invited-avatar--placeholder">
                    {user.username.charAt(0).toUpperCase()}
                  </div>
                )}
                <span className="create-playlist-form__invited-username">
                  {user.username}
                </span>
                <button
                  type="button"
                  className="create-playlist-form__remove-btn"
                  onClick={() => handleRemoveUser(user.id)}
                  aria-label={`Remove ${user.username}`}
                >
                  <X size={14} />
                </button>
              </div>
            ))}
          </div>
        )}

        {/* Search Input */}
        <div className="create-playlist-form__search-wrapper">
          <div className="create-playlist-form__input-container">
            <Search size={16} className="create-playlist-form__search-icon" />
            <input
              type="text"
              className="create-playlist-form__input create-playlist-form__input--search"
              placeholder="Search users to invite..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </div>

          {/* Search Results Dropdown */}
          {searchQuery.trim() && (
            <div className="create-playlist-form__results">
              {isSearching ? (
                <div className="create-playlist-form__searching">
                  <div className="create-playlist-form__spinner" />
                  <span>Searching users...</span>
                </div>
              ) : searchResults.length > 0 ? (
                <>
                  {searchResults.map((user) => {
                    const isAlreadyInvited = invitedUsers.some((u) => u.id === user.id);

                    return (
                      <button
                        key={user.id}
                        type="button"
                        className="create-playlist-form__result-item"
                        onClick={() => handleAddUser(user)}
                        disabled={isAlreadyInvited}
                      >
                        {user.profileImage ? (
                          <img
                            src={user.profileImage}
                            alt={user.username}
                            className="create-playlist-form__result-avatar"
                          />
                        ) : (
                          <div className="create-playlist-form__result-avatar">
                            {user.username.charAt(0).toUpperCase()}
                          </div>
                        )}
                        <div className="create-playlist-form__result-info">
                          <div className="create-playlist-form__result-username">
                            {user.username}
                          </div>
                          {isAlreadyInvited && (
                            <div className="create-playlist-form__result-hint">
                              Already invited
                            </div>
                          )}
                        </div>
                        {!isAlreadyInvited && (
                          <span className="create-playlist-form__add-icon">+</span>
                        )}
                      </button>
                    );
                  })}
                </>
              ) : (
                <div className="create-playlist-form__no-results">
                  <Users size={24} />
                  <p>No users found</p>
                  <p className="create-playlist-form__no-results-hint">
                    Try a different username
                  </p>
                </div>
              )}
            </div>
          )}
        </div>

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