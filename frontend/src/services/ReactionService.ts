import api from "./api";
import type { SongReactionSummaryDto } from "../types/SongReactionSummaryDto";

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