export interface Profile {
  username: string;
  bio: string;
  image: string | null;
  following?: boolean;
}

export interface ProfileResponse {
  profile: Profile;
}
