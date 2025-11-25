import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { TagList } from './TagList';

describe('TagList', () => {
  const mockTags = ['javascript', 'react', 'typescript'];

  it('renders all tags', () => {
    render(<TagList tags={mockTags} />);
    expect(screen.getByText('javascript')).toBeInTheDocument();
    expect(screen.getByText('react')).toBeInTheDocument();
    expect(screen.getByText('typescript')).toBeInTheDocument();
  });

  it('renders empty state when no tags provided', () => {
    render(<TagList tags={[]} />);
    expect(screen.getByText('No tags available')).toBeInTheDocument();
  });

  it('calls onTagClick when a tag is clicked', () => {
    const handleTagClick = vi.fn();
    render(<TagList tags={mockTags} onTagClick={handleTagClick} />);
    
    fireEvent.click(screen.getByText('react'));
    expect(handleTagClick).toHaveBeenCalledWith('react');
  });

  it('does not throw when clicking tag without onTagClick handler', () => {
    render(<TagList tags={mockTags} />);
    expect(() => fireEvent.click(screen.getByText('react'))).not.toThrow();
  });
});

