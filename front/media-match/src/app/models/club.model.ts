export interface Club {
  id: number;
  name: string;
  description?: string;
  imageUrl?: string;
  createdAt: Date;
  ownerId: number;
  ownerName?: string;
  membersCount: number;
  mediaListsCount: number;
  isOwner: boolean;
  isMember: boolean;
}

export interface ClubDetail extends Club {
  members: ClubMember[];
  mediaLists: any[];
}

export interface ClubMember {
  userId: number;
  userName: string;
  joinedAt: Date;
  isModerator: boolean;
}

export interface CreateClubDto {
  name: string;
  description?: string;
}

export interface UpdateClubDto {
  name: string;
  description?: string;
  removeImage?: boolean;
}

// ========== POST INTERFACES ==========

export interface Post {
  id: number;
  content: string;
  imageUrl?: string; // Mantido para compatibilidade (deprecated)
  imageUrls: string[]; // Nova propriedade para múltiplas imagens
  createdAt: Date;
  updatedAt?: Date;
  isEdited: boolean;
  clubId: number;
  clubName: string;
  authorId: number;
  authorName: string;
  authorProfilePictureUrl?: string;
  commentsCount: number;
  canEdit: boolean;
  canDelete: boolean;
  comments?: Comment[];
}

export interface PostDetail extends Post {
  comments: Comment[];
}

export interface CreatePostDto {
  content: string;
}

export interface UpdatePostDto {
  content: string;
  removeImage?: boolean;
}

// ========== COMMENT INTERFACES ==========

export interface Comment {
  id: number;
  content: string;
  createdAt: Date;
  updatedAt?: Date;
  postId: number;
  authorId: number;
  authorName: string;
  authorProfilePictureUrl?: string;
  canEdit: boolean;
  canDelete: boolean;
}

export interface CreateCommentDto {
  content: string;
}

export interface UpdateCommentDto {
  content: string;
}
