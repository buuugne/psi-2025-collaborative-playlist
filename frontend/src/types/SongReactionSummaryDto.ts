export interface SongReactionSummaryDto {
    userId: number;
    username: string;
    profileImage?: string;
    isLike: boolean;
    createdAt: string;
  }