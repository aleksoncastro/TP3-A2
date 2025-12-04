export interface MediaList {
  id: number;
  name: string;
  description: string;
  isPublic: boolean;
  createdAt: string;
  clubId: number;
  clubName?: string;
  itemsCount: number;
  commentsCount: number;
  canEdit: boolean;
  canDelete: boolean;
}

export interface MediaListDetail extends MediaList {
  items: MediaListItem[];
  comments: MediaListComment[];
}

export interface MediaListItem {
  id: number;
  mediaListId: number;
  mediaItemId: number;
  addedAt: string;
  note: string;
  tmdbId: number;
  title: string;
  posterPath: string;
  mediaType: 'movie' | 'tv';
  rating: number;
  releaseDate?: string;
}

export interface MediaListComment {
  id: number;
  content: string;
  createdAt: string;
  updatedAt?: string;
  type: 'comment' | 'suggestion';
  suggestedMediaId?: number;
  suggestedMediaType?: 'movie' | 'tv';
  suggestedMediaTitle?: string;
  suggestedMediaPosterPath?: string;
  authorId: number;
  authorName: string;
  authorProfilePictureUrl?: string;
  canEdit: boolean;
  canDelete: boolean;
}

export interface CreateMediaListDto {
  name: string;
  description: string;
  isPublic: boolean;
}

export interface UpdateMediaListDto {
  name: string;
  description: string;
  isPublic: boolean;
}

export interface AddMediaListItemDto {
  tmdbId: number;
  mediaType: 'movie' | 'tv';
  note?: string;
}

export interface CreateMediaListCommentDto {
  content: string;
  type: 'comment' | 'suggestion';
  suggestedMediaId?: number;
  suggestedMediaType?: 'movie' | 'tv';
  suggestedMediaTitle?: string;
  suggestedMediaPosterPath?: string;
}
