export interface User {
  email: string;
  username: string;
  bio: string;
  image: string | null;
  token: string;
}

export interface LoginRequest {
  user: {
    email: string;
    password: string;
  };
}

export interface RegisterRequest {
  user: {
    email: string;
    username?: string;
    password: string;
  };
}

export interface UpdateUserRequest {
  user: {
    email?: string;
    username?: string;
    password?: string;
    bio?: string;
    image?: string;
  };
}

export interface UserResponse {
  user: User;
}

export interface ErrorResponse {
  errors: {
    body: string[];
  };
}
