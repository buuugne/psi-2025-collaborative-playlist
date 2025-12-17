import { useState, useEffect } from "react";
import { Heart, ThumbsDown } from "lucide-react";
import { ReactionService } from "../../../../services/ReactionService";
import type { SongReactionSummaryDto } from "../../../../types/SongReactionSummaryDto";
import "./SongReactions.scss";

const API_BASE = import.meta.env.VITE_API_URL || '';

interface SongReactionsProps {
  playlistId: number;
  songId: number;
  currentUserId?: number;
}

export default function SongReactions({
  playlistId,
  songId,
  currentUserId,
}: SongReactionsProps) {
  const [reactions, setReactions] = useState<SongReactionSummaryDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [hoveredUser, setHoveredUser] = useState<string | null>(null);

  useEffect(() => {
    loadReactions();
  }, [playlistId, songId]);

  const loadReactions = async () => {
    try {
      const data = await ReactionService.getReactions(playlistId, songId);
      setReactions(data);
    } catch (err) {
      console.error("Failed to load reactions:", err);
    }
  };

  const handleToggle = async (isLike: boolean) => {
    if (!currentUserId || isLoading) return;

    setIsLoading(true);
    try {
      await ReactionService.toggleReaction(playlistId, songId, isLike);
      await loadReactions();
    } catch (err) {
      console.error("Failed to toggle reaction:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const getUserImageUrl = (profileImage?: string, username?: string) => {
    if (!profileImage) {
      return `https://api.dicebear.com/7.x/initials/svg?seed=${username || 'User'}`;
    }
    
    if (profileImage.startsWith('http')) {
      return profileImage;
    }
    
    const path = profileImage.startsWith('/') ? profileImage : `/${profileImage}`;
    return `${API_BASE}${path}`;
  };

  const currentUserReaction = reactions.find((r) => r.userId === currentUserId);
  const hasLiked = !!currentUserReaction?.isLike == true;
  const hasDisliked = currentUserReaction?.isLike === false;
  const likeCount = reactions.filter((r) => r.isLike == true).length;
  const dislikeCount = reactions.filter((r) => r.isLike === false).length;
  const likedUsers = reactions.filter((r) => r.isLike == true).slice(0, 3);

  return (
    <div className="song-reactions">
      <button
        className={`song-reactions__button ${
          hasLiked ? "song-reactions__button--active" : ""
        }`}
        onClick={() => handleToggle(true)}
        disabled={isLoading || !currentUserId}
        title={hasLiked ? "Unlike" : "Like"}
      >
        <Heart
          size={16}
          fill={hasLiked ? "currentColor" : "none"}
          className="song-reactions__heart"
        />
        {likeCount > 0 && (
          <span className="song-reactions__count">{likeCount}</span>
        )}
      </button>

      <button
        className={`song-reactions__button ${
          hasDisliked ? "song-reactions__button--dislike-active" : ""
        }`}
        onClick={() => handleToggle(false)}
        disabled={isLoading || !currentUserId}
        title={hasDisliked ? "Remove dislike" : "Dislike"}
        aria-label={hasDisliked ? "Remove dislike" : "Dislike"}
      >
        <ThumbsDown
          size={16}
          fill={hasDisliked ? "currentColor" : "none"}
          className="song-reactions__thumbsdown"
        />
        {dislikeCount > 0 && (
          <span className="song-reactions__count">{dislikeCount}</span>
        )}
      </button>

      {likedUsers.length > 0 && (
        <div className="song-reactions__avatars">
          {likedUsers.map((user) => (
            <div
              key={user.userId}
              className="song-reactions__avatar-wrapper"
              onMouseEnter={() => setHoveredUser(user.username)}
              onMouseLeave={() => setHoveredUser(null)}
            >
              {user.profileImage ? (
                <img
                  src={getUserImageUrl(user.profileImage, user.username)}
                  alt={user.username}
                  className="song-reactions__avatar"
                  onError={(e) => {
                    e.currentTarget.src = `https://api.dicebear.com/7.x/initials/svg?seed=${user.username || 'User'}`;
                  }}
                />
              ) : (
                <div className="song-reactions__avatar song-reactions__avatar--placeholder">
                  {user.username.charAt(0).toUpperCase()}
                </div>
              )}
              {hoveredUser === user.username && (
                <div className="song-reactions__tooltip">{user.username}</div>
              )}
            </div>
          ))}
          {likeCount > 3 && (
            <div className="song-reactions__avatar song-reactions__avatar--more">
              +{likeCount - 3}
            </div>
          )}
        </div>
      )}
    </div>
  );
}