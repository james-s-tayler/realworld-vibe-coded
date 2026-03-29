import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from 'react-router';
import { AppHeader } from './AppHeader';

describe('AppHeader', () => {
  it('renders conduit brand link', () => {
    render(
      <MemoryRouter>
        <AppHeader />
      </MemoryRouter>
    );
    expect(screen.getByText('conduit')).toBeInTheDocument();
  });

  it('does not render navigation links', () => {
    render(
      <MemoryRouter>
        <AppHeader />
      </MemoryRouter>
    );
    expect(screen.queryByText('Home')).not.toBeInTheDocument();
    expect(screen.queryByText('Sign in')).not.toBeInTheDocument();
    expect(screen.queryByText('Settings')).not.toBeInTheDocument();
  });
});
