import { describe, it, expect, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import App from './App'

describe('App', () => {
  beforeEach(() => {
    // Clear localStorage before each test
    localStorage.clear()
  })

  it('renders the app without crashing', () => {
    render(<App />)
    expect(screen.getByText('Conduit')).toBeInTheDocument()
  })

  it('renders the homepage when no route is specified', async () => {
    render(<App />)
    await waitFor(() => {
      expect(screen.getByText('Welcome to Conduit')).toBeInTheDocument()
    })
  })

  it('shows sign in and sign up buttons when not authenticated', async () => {
    render(<App />)
    await waitFor(() => {
      expect(screen.getByText('Sign In')).toBeInTheDocument()
      expect(screen.getByText('Sign Up')).toBeInTheDocument()
    })
  })
})