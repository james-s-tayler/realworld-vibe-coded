import { describe, it, expect, vi, beforeEach } from 'vitest';
import { profilesApi } from './profiles';
import * as client from './client';

vi.mock('./client', () => ({
  apiRequest: vi.fn(),
}));

describe('profilesApi', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getProfile', () => {
    it('should fetch a user profile', async () => {
      const mockResponse = { profile: { username: 'johndoe', bio: 'Test bio' } };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await profilesApi.getProfile('johndoe');

      expect(client.apiRequest).toHaveBeenCalledWith('/api/profiles/johndoe');
      expect(result).toEqual(mockResponse);
    });
  });

  describe('followUser', () => {
    it('should follow a user', async () => {
      const mockResponse = { profile: { username: 'johndoe', following: true } };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await profilesApi.followUser('johndoe');

      expect(client.apiRequest).toHaveBeenCalledWith('/api/profiles/johndoe/follow', {
        method: 'POST',
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('unfollowUser', () => {
    it('should unfollow a user', async () => {
      const mockResponse = { profile: { username: 'johndoe', following: false } };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await profilesApi.unfollowUser('johndoe');

      expect(client.apiRequest).toHaveBeenCalledWith('/api/profiles/johndoe/follow', {
        method: 'DELETE',
      });
      expect(result).toEqual(mockResponse);
    });
  });
});
