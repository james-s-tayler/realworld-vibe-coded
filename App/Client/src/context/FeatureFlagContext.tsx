import React, { useState, useEffect, useCallback, type ReactNode } from 'react';
import { FeatureManager, ConfigurationObjectFeatureFlagProvider } from '@microsoft/feature-management';
import { featureFlagsApi } from '../api/featureFlagsApi';
import { FeatureFlagContext } from './FeatureFlagContextType';

export { FeatureFlagContext };

interface FeatureFlagProviderProps {
  children: ReactNode;
}

export const FeatureFlagProvider: React.FC<FeatureFlagProviderProps> = ({ children }) => {
  const [flags, setFlags] = useState<Record<string, boolean>>({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    featureFlagsApi
      .getConfig()
      .then(async (config) => {
        const provider = new ConfigurationObjectFeatureFlagProvider(config as unknown as Record<string, unknown>);
        const manager = new FeatureManager(provider);

        const evaluated: Record<string, boolean> = {};
        for (const ff of config.feature_management?.feature_flags ?? []) {
          evaluated[ff.id] = await manager.isEnabled(ff.id);
        }
        setFlags(evaluated);
      })
      .catch(() => {
        // Silently degrade to all-flags-off
      })
      .finally(() => {
        setLoading(false);
      });
  }, []);

  const isEnabled = useCallback(
    (flagName: string): boolean => flags[flagName] ?? false,
    [flags],
  );

  return (
    <FeatureFlagContext.Provider value={{ isEnabled, loading }}>
      {children}
    </FeatureFlagContext.Provider>
  );
};
