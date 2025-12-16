import { useState, useEffect } from "react";
import { Heart } from "lucide-react";
import { ReactionService } from "../../../../services/ReactionService";
import type { SongReactionSummaryDto } from "../../../../types/SongReactionSummaryDto";
import "./SongReactions.scss";

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

  const handleToggle = async () => {
    if (!currentUserId || isLoading) return;

    setIsLoading(true);
    try {
      await ReactionService.toggleReaction(playlistId, songId, true);
      await loadReactions();
    } catch (err) {
      console.error("Failed to toggle reaction:", err);
    } finally {
      setIsLoading(false);
    }
  };

  const currentUserReaction = reactions.find((r) => r.userId === currentUserId);
  const hasLiked = !!currentUserReaction?.isLike;
  const likeCount = reactions.filter((r) => r.isLike).length;

  // Get first 3 users who liked
  const likedUsers = reactions.filter((r) => r.isLike).slice(0, 3);

  return (
    <div className="song-reactions">
      <button
        className={`song-reactions__button ${
          hasLiked ? "song-reactions__button--active" : ""
        }`}
        onClick={handleToggle}
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
                  src={user.profileImage}
                  alt={user.username}
                  className="song-reactions__avatar"
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