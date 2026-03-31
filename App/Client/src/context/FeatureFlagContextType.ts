import { createContext } from 'react';

export interface FeatureFlagContextType {
  isEnabled: (flagName: string) => boolean;
  loading: boolean;
}

export const FeatureFlagContext = createContext<FeatureFlagContextType | undefined>(undefined);
