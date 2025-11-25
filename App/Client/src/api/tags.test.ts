import { describe, it, expect, vi, beforeEach } from 'vitest';
import { tagsApi } from './tags';
import * as client from './client';

vi.mock('./client', () => ({
  apiRequest: vi.fn(),
}));

describe('tagsApi', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getTags', () => {
    it('should fetch all tags', async () => {
      const mockResponse = { tags: ['react', 'javascript', 'typescript'] };
      vi.mocked(client.apiRequest).mockResolvedValue(mockResponse);

      const result = await tagsApi.getTags();

      expect(client.apiRequest).toHaveBeenCalledWith('/api/tags');
      expect(result).toEqual(mockResponse);
    });
  });
});
