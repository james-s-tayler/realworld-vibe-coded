import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { apiRequest, ApiError } from './client'

// Mock fetch
global.fetch = vi.fn()

describe('apiRequest', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('makes a successful request', async () => {
    const mockData = { user: { username: 'test' } }
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      headers: new Headers({ 'content-type': 'application/json' }),
      text: async () => JSON.stringify(mockData),
      json: async () => mockData,
    } as Response)

    const result = await apiRequest('/api/test')

    expect(fetch).toHaveBeenCalledWith(
      'http://localhost:5000/api/test',
      expect.objectContaining({
        headers: expect.objectContaining({
          'Content-Type': 'application/json',
        }),
      })
    )
    expect(result).toEqual(mockData)
  })

  it('includes authorization header when token exists', async () => {
    localStorage.setItem('token', 'test-token')
    
    const mockData = { user: { username: 'test' } }
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      headers: new Headers({ 'content-type': 'application/json' }),
      text: async () => JSON.stringify(mockData),
      json: async () => mockData,
    } as Response)

    await apiRequest('/api/user')

    expect(fetch).toHaveBeenCalledWith(
      'http://localhost:5000/api/user',
      expect.objectContaining({
        headers: expect.objectContaining({
          'Content-Type': 'application/json',
          'Authorization': 'Token test-token',
        }),
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
      'http://localhost:5000/api/test',
      expect.objectContaining({
        headers: expect.objectContaining({
          'Content-Type': 'application/json',
          'X-Custom-Header': 'value',
        }),
      })
    )
  })
})
