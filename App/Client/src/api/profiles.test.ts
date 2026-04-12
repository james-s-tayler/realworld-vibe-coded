import { beforeEach,describe, expect, it, vi } from 'vitest';

import { getApiClient } from './clientFactory';
import { profilesApi } from './profiles';

vi.mock('./clientFactory');

function createMockClient() {
  const mockProfileGet = vi.fn();
  const mockFollowPost = vi.fn();
  const mockFollowDelete = vi.fn();
  const mockByUsername = vi.fn();

  mockByUsername.mockReturnValue({
    get: mockProfileGet,
    follow: {
      post: mockFollowPost,
      delete: mockFollowDelete,
    },
  });

  const client = {
    api: {
      profiles: {
        byUsername: mockByUsername,
      },
    },
  };

  return {
    client,
    mocks: {
      profileGet: mockProfileGet,
      followPost: mockFollowPost,
      followDelete: mockFollowDelete,
      byUsername: mockByUsername,
    },
  };
}

describe('profilesApi', () => {
  let mocks: ReturnType<typeof createMockClient>['mocks'];

  beforeEach(() => {
    vi.clearAllMocks();
    const mock = createMockClient();
    mocks = mock.mocks;
    vi.mocked(getApiClient).mockReturnValue(mock.client as ReturnType<typeof getApiClient>);
  });

  describe('getProfile', () => {
    it('should fetch a user profile', async () => {
      const mockResponse = { profile: { username: 'johndoe', bio: 'Test bio' } };
      mocks.profileGet.mockResolvedValue(mockResponse);

      const result = await profilesApi.getProfile('johndoe');

      expect(mocks.byUsername).toHaveBeenCalledWith('johndoe');
      expect(result).toEqual(mockResponse);
    });
  });

  describe('followUser', () => {
    it('should follow a user', async () => {
      const mockResponse = { profile: { username: 'johndoe', following: true } };
      mocks.followPost.mockResolvedValue(mockResponse);

      const result = await profilesApi.followUser('johndoe');

      expect(mocks.byUsername).toHaveBeenCalledWith('johndoe');
      expect(mocks.followPost).toHaveBeenCalled();
      expect(result).toEqual(mockResponse);
    });
  });

  describe('unfollowUser', () => {
    it('should unfollow a user', async () => {
      const mockResponse = { profile: { username: 'johndoe', following: false } };
      mocks.followDelete.mockResolvedValue(mockResponse);

      const result = await profilesApi.unfollowUser('johndoe');

      expect(mocks.byUsername).toHaveBeenCalledWith('johndoe');
      expect(mocks.followDelete).toHaveBeenCalled();
      expect(result).toEqual(mockResponse);
    });
  });
});
