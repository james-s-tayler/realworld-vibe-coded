export interface User {
  email: string;
  username: string;
  bio: string;
  image: string | null;
  roles: string[];
  language: string;
}

export interface UserResponse {
  user: User;
}
