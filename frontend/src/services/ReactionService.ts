import api from "./api";
import type { SongReactionSummaryDto } from "../types/SongReactionSummaryDto";

const API_BASE = import.meta.env.VITE_API_URL || '';

export const ReactionService = {
  async toggleReaction(playlistId: number, songId: number, isLike: boolean) {
    await api.post(
      `/api/playlists/${playlistId}/songs/${songId}/reactions`,
      { isLike }
    );
  },

  async getReactions(playlistId: number, songId: number): Promise<SongReactionSummaryDto[]> {
    const res = await api.get(
      `/api/playlists/${playlistId}/songs/${songId}/reactions`
    );
    return res.data;
  },
};