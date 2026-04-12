import { beforeEach,describe, expect, it, vi } from 'vitest';

import { getApiClient } from './clientFactory';
import { tagsApi } from './tags';

vi.mock('./clientFactory');

describe('tagsApi', () => {
  const mockGet = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(getApiClient).mockReturnValue({
      api: { tags: { get: mockGet } },
    } as unknown as ReturnType<typeof getApiClient>);
  });

  describe('getTags', () => {
    it('should fetch all tags', async () => {
      const mockResponse = { tags: ['react', 'javascript', 'typescript'] };
      mockGet.mockResolvedValue(mockResponse);

      const result = await tagsApi.getTags();

      expect(mockGet).toHaveBeenCalled();
      expect(result).toEqual(mockResponse);
    });
  });
});
