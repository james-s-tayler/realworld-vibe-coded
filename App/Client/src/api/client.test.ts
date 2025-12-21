import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { apiRequest, ApiError } from './client'

// Mock fetch
global.fetch = vi.fn()

describe('apiRequest', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    // Clear cookies for tests
    document.cookie.split(';').forEach(cookie => {
      const name = cookie.split('=')[0].trim()
      document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;`
    })
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('makes a successful request with credentials', async () => {
    const mockData = { user: { username: 'test' } }
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      headers: new Headers({ 'content-type': 'application/json' }),
      text: async () => JSON.stringify(mockData),
      json: async () => mockData,
    } as Response)

    const result = await apiRequest('/api/test')

    expect(fetch).toHaveBeenCalledWith(
      '/api/test',
      expect.objectContaining({
        headers: {},
        credentials: 'include',
      })
    )
    expect(result).toEqual(mockData)
  })

  it('includes CSRF token for mutating requests', async () => {
    // Set CSRF token cookie
    document.cookie = 'XSRF-TOKEN=test-csrf-token; path=/;'
    
    const mockData = { success: true }
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      headers: new Headers({ 'content-type': 'application/json' }),
      text: async () => JSON.stringify(mockData),
      json: async () => mockData,
    } as Response)

    await apiRequest('/api/articles', { method: 'POST', body: JSON.stringify({ title: 'Test' }) })

    expect(fetch).toHaveBeenCalledWith(
      '/api/articles',
      expect.objectContaining({
        headers: expect.objectContaining({
          'X-XSRF-TOKEN': 'test-csrf-token',
          'Content-Type': 'application/json',
        }),
        credentials: 'include',
      })
    )
  })

  it('throws ApiError on failed request', async () => {
    // ProblemDetails format
    const errorResponse = {
      type: 'https://tools.ietf.org/html/rfc7231#section-6.5.1',
      title: 'Unauthorized',
      status: 401,
      instance: '/api/users/login',
      errors: [
        { name: 'body', reason: 'email or password is invalid' }
      ],
      traceId: '0HNGBGGOMKEVE:00000001'
    }

    vi.mocked(fetch).mockResolvedValue({
      ok: false,
      status: 401,
      headers: new Headers({ 'content-type': 'application/json' }),
      text: async () => JSON.stringify(errorResponse),
      json: async () => errorResponse,
    } as Response)

    await expect(apiRequest('/api/users/login')).rejects.toThrow(ApiError)
    
    try {
      await apiRequest('/api/users/login')
    } catch (error) {
      expect(error).toBeInstanceOf(ApiError)
      if (error instanceof ApiError) {
        expect(error.status).toBe(401)
        expect(error.errors).toEqual(['body: email or password is invalid'])
        expect(error.title).toBe('Unauthorized')
      }
    }
  })

  it('handles missing error body gracefully', async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: false,
      status: 500,
      headers: new Headers({ 'content-type': 'application/json' }),
      text: async () => JSON.stringify({}),
      json: async () => ({}),
    } as Response)

    try {
      await apiRequest('/api/test')
    } catch (error) {
      if (error instanceof ApiError) {
        expect(error.errors).toEqual(['Request failed with status 500'])
      }
    }
  })

  it('merges custom headers with default headers', async () => {
    const mockData = { success: true }
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      headers: new Headers({ 'content-type': 'application/json' }),
      text: async () => JSON.stringify(mockData),
      json: async () => mockData,
    } as Response)

    await apiRequest('/api/test', {
      headers: {
        'X-Custom-Header': 'value',
      },
    })

    expect(fetch).toHaveBeenCalledWith(
      '/api/test',
      expect.objectContaining({
        headers: expect.objectContaining({
          'X-Custom-Header': 'value',
        }),
      })
    )
  })
})
