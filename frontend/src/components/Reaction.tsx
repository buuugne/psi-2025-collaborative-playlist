import React, { useState } from "react";
import { PlaylistService } from "../services/PlaylistService";

interface ReactionProps {
  playlistId: number;
  songId: number;
  currentUserReaction?: boolean | null; // true=like, false=dislike
  score: number;
  likeCount: number;
  dislikeCount: number;
  currentUserId?: number;
  onReactionUpdated?: (newReaction: boolean | null) => void;
}

const Reaction: React.FC<ReactionProps> = ({
  playlistId,
  songId,
  currentUserReaction,
  score,
  likeCount,
  dislikeCount,
  currentUserId,
  onReactionUpdated
}) => {
  const [reaction, setReaction] = useState(currentUserReaction);
  const [likes, setLikes] = useState(likeCount);
  const [dislikes, setDislikes] = useState(dislikeCount);

  const handleClick = async (isLike: boolean) => {
    if (!currentUserId) {
      alert("You must be logged in to react");
      return;
    }

    try {
      if (reaction === isLike) {
        // Remove reaction
        await PlaylistService.removeReaction({ playlistId, songId, userId: currentUserId });
        setReaction(null);
        if (isLike) setLikes(l => l - 1);
        else setDislikes(d => d - 1);
      } else {
        // Add or update reaction
        await PlaylistService.addOrUpdateReaction({ playlistId, songId, userId: currentUserId, isLike });
        if (reaction === true) setLikes(l => l - 1);
        if (reaction === false) setDislikes(d => d - 1);

        if (isLike) setLikes(l => l + 1);
        else setDislikes(d => d + 1);

        setReaction(isLike);
      }
      onReactionUpdated?.(reaction === isLike ? null : isLike);
    } catch (err) {
      alert("Failed to update reaction: " + err);
    }
  };

  return (
    <div className="flex items-center gap-1">
      <button
        className={`hover:scale-110 transition-transform ${reaction === true ? "text-green-400 scale-125" : "text-gray-500"}`}
        onClick={() => handleClick(true)}
        title="Like"
      >
        👍
      </button>
      <button
        className={`hover:scale-110 transition-transform ${reaction === false ? "text-red-400 scale-125" : "text-gray-500"}`}
        onClick={() => handleClick(false)}
        title="Dislike"
      >
        👎
      </button>
      <span className="text-xs text-gray-400">
        ({likes}👍 {dislikes}👎)
      </span>
    </div>
  );
};

export default Reaction;
