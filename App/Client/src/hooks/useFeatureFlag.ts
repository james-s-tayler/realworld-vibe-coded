import { useContext } from 'react';
import { FeatureFlagContext } from '../context/FeatureFlagContextType';

export const useFeatureFlag = (flagName: string): boolean => {
  const context = useContext(FeatureFlagContext);
  if (!context) {
    throw new Error('useFeatureFlag must be used within a FeatureFlagProvider');
  }
  return context.isEnabled(flagName);
};
