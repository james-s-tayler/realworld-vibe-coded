import { describe, it, expect } from 'vitest';
import { renderHook } from '@testing-library/react';
import { useFeatureFlag } from './useFeatureFlag';
import { FeatureFlagContext } from '../context/FeatureFlagContextType';

const wrapper =
  (flags: Record<string, boolean>) =>
  ({ children }: { children: React.ReactNode }) => (
    <FeatureFlagContext.Provider
      value={{
        isEnabled: (name: string) => flags[name] ?? false,
        loading: false,
      }}
    >
      {children}
    </FeatureFlagContext.Provider>
  );

describe('useFeatureFlag', () => {
  it('returns true when flag is enabled', () => {
    const { result } = renderHook(() => useFeatureFlag('DashboardBanner'), {
      wrapper: wrapper({ DashboardBanner: true }),
    });

    expect(result.current).toBe(true);
  });

  it('returns false when flag is disabled', () => {
    const { result } = renderHook(() => useFeatureFlag('DashboardBanner'), {
      wrapper: wrapper({ DashboardBanner: false }),
    });

    expect(result.current).toBe(false);
  });

  it('returns false for unknown flags', () => {
    const { result } = renderHook(() => useFeatureFlag('NonExistentFlag'), {
      wrapper: wrapper({}),
    });

    expect(result.current).toBe(false);
  });

  it('throws when used outside FeatureFlagProvider', () => {
    expect(() => {
      renderHook(() => useFeatureFlag('DashboardBanner'));
    }).toThrow('useFeatureFlag must be used within a FeatureFlagProvider');
  });
});
