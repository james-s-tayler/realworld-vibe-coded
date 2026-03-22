export interface Profile {
  username: string;
  bio: string;
  image: string | null;
}

export interface ProfileResponse {
  profile: Profile;
}
