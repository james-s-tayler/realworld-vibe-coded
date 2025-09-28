import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/react'
import App from './App'

describe('App', () => {
  it('renders Vite and React logos', () => {
    render(<App />)
    
    expect(screen.getByAltText('Vite logo')).toBeInTheDocument()
    expect(screen.getByAltText('React logo')).toBeInTheDocument()
  })

  it('renders the main heading', () => {
    render(<App />)
    
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Vite + React')
  })

  it('renders the counter button', () => {
    render(<App />)
    
    expect(screen.getByRole('button', { name: /count is 0/i })).toBeInTheDocument()
  })
})