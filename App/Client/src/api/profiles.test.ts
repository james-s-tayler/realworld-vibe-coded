import { describe, it, expect, vi, beforeEach } from 'vitest';
import { profilesApi } from './profiles';
import { getApiClient } from './clientFactory';

vi.mock('./clientFactory');

function createMockClient() {
  const mockProfileGet = vi.fn();
  const mockByUsername = vi.fn();

  mockByUsername.mockReturnValue({
    get: mockProfileGet,
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
});
