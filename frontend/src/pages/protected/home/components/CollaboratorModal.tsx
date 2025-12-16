import { useState, useEffect } from "react";
import { X, UserPlus, Trash2, Search, Users } from "lucide-react";
import { PlaylistService } from "../../../../services/PlaylistService";
import "./CollaboratorModal.scss";

interface CollaboratorModalProps {
  playlistId: number;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

interface Collaborator {
  id: number;
  username: string;
  profileImage?: string;
}

interface SearchResult {
  id: number;
  username: string;
  profileImage?: string;
}

export default function CollaboratorModal({
  playlistId,
  isOpen,
  onClose,
  onSuccess,
}: CollaboratorModalProps) {
  const [searchQuery, setSearchQuery] = useState("");
  const [searchResults, setSearchResults] = useState<SearchResult[]>([]);
  const [isSearching, setIsSearching] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [collaborators, setCollaborators] = useState<Collaborator[]>([]);
  const [isLoadingCollaborators, setIsLoadingCollaborators] = useState(false);

  useEffect(() => {
    if (isOpen) {
      loadCollaborators();
      setSearchQuery("");
      setSearchResults([]);
    }
  }, [isOpen, playlistId]);

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

  const loadCollaborators = async () => {
    setIsLoadingCollaborators(true);
    try {
      const data = await PlaylistService.getCollaborators(playlistId);
      setCollaborators(data);
    } catch (err) {
      console.error("Failed to load collaborators:", err);
    } finally {
      setIsLoadingCollaborators(false);
    }
  };

  const handleAddCollaborator = async (username: string) => {
    setIsSubmitting(true);
    try {
      await PlaylistService.addCollaborator(playlistId, username);
      setSearchQuery("");
      setSearchResults([]);
      await loadCollaborators();
      onSuccess();
    } catch (err: any) {
      alert(err.message || "Failed to add collaborator");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleRemove = async (userId: number, collaboratorUsername: string) => {
    if (!confirm(`Remove ${collaboratorUsername} as a collaborator?`)) return;

    try {
      await PlaylistService.removeCollaborator(playlistId, userId);
      await loadCollaborators();
      onSuccess();
    } catch (err: any) {
      alert(err.message || "Failed to remove collaborator");
    }
  };

  if (!isOpen) return null;

  return (
    <div className="collaborator-modal__overlay" onClick={onClose}>
      <div className="collaborator-modal" onClick={(e) => e.stopPropagation()}>
        <div className="collaborator-modal__header">
          <h2 className="collaborator-modal__title">Manage Collaborators</h2>
          <button
            type="button"
            className="collaborator-modal__close"
            onClick={onClose}
            aria-label="Close modal"
          >
            <X size={20} />
          </button>
        </div>

        <p className="collaborator-modal__description">
          Add people who can add and remove songs from this playlist
        </p>

        {/* Search Input */}
        <div className="collaborator-modal__search-wrapper">
          <div className="collaborator-modal__input-container">
            <Search size={18} className="collaborator-modal__search-icon" />
            <input
              type="text"
              placeholder="Search users by username..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="collaborator-modal__input"
              disabled={isSubmitting}
            />
          </div>

          {/* Search Results Dropdown */}
          {searchQuery.trim() && (
            <div className="collaborator-modal__results">
              {isSearching ? (
                <div className="collaborator-modal__searching">
                  <div className="collaborator-modal__spinner" />
                  <span>Searching users...</span>
                </div>
              ) : searchResults.length > 0 ? (
                <>
                  {searchResults.map((user) => {
                    const isAlreadyCollaborator = collaborators.some(
                      (c) => c.id === user.id
                    );

                    return (
                      <button
                        key={user.id}
                        type="button"
                        className="collaborator-modal__result-item"
                        onClick={() => handleAddCollaborator(user.username)}
                        disabled={isAlreadyCollaborator || isSubmitting}
                      >
                        {user.profileImage ? (
                          <img
                            src={user.profileImage}
                            alt={user.username}
                            className="collaborator-modal__result-avatar"
                          />
                        ) : (
                          <div className="collaborator-modal__result-avatar">
                            {user.username.charAt(0).toUpperCase()}
                          </div>
                        )}
                        <div className="collaborator-modal__result-info">
                          <div className="collaborator-modal__result-username">
                            {user.username}
                          </div>
                          {isAlreadyCollaborator && (
                            <div className="collaborator-modal__result-hint">
                              Already a collaborator
                            </div>
                          )}
                        </div>
                        {!isAlreadyCollaborator && (
                          <span className="collaborator-modal__add-icon">+</span>
                        )}
                      </button>
                    );
                  })}
                </>
              ) : (
                <div className="collaborator-modal__no-results">
                  <Users size={32} />
                  <p>No users found</p>
                  <p className="collaborator-modal__no-results-hint">
                    Try a different username
                  </p>
                </div>
              )}
            </div>
          )}

          <p className="collaborator-modal__hint">
            Start typing to search for users
          </p>
        </div>

        {/* Current Collaborators List */}
        {isLoadingCollaborators ? (
          <div className="collaborator-modal__loading">Loading collaborators...</div>
        ) : collaborators.length > 0 ? (
          <div className="collaborator-modal__list">
            <h3 className="collaborator-modal__list-title">Current Collaborators</h3>
            {collaborators.map((collab) => (
              <div key={collab.id} className="collaborator-modal__item">
                <div className="collaborator-modal__item-info">
                  {collab.profileImage ? (
                    <img
                      src={collab.profileImage}
                      alt={collab.username}
                      className="collaborator-modal__avatar"
                    />
                  ) : (
                    <div className="collaborator-modal__avatar collaborator-modal__avatar--placeholder">
                      {collab.username.charAt(0).toUpperCase()}
                    </div>
                  )}
                  <span className="collaborator-modal__username">
                    {collab.username}
                  </span>
                </div>
                <button
                  type="button"
                  className="collaborator-modal__remove-btn"
                  onClick={() => handleRemove(collab.id, collab.username)}
                  title="Remove collaborator"
                  aria-label={`Remove ${collab.username}`}
                >
                  <Trash2 size={16} />
                </button>
              </div>
            ))}
          </div>
        ) : (
          <p className="collaborator-modal__empty">No collaborators yet</p>
        )}
      </div>
    </div>
  );
}