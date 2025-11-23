import type { Profile } from './article';

export interface Comment {
  id: number;
  createdAt: string;
  updatedAt: string;
  body: string;
  author: Profile;
}

export interface CommentResponse {
  comment: Comment;
}

export interface CommentsResponse {
  comments: Comment[];
}

export interface CreateCommentRequest {
  comment: {
    body: string;
  };
}
