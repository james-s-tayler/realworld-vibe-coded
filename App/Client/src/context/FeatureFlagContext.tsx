import React, { useState, useEffect, useCallback, type ReactNode } from 'react';
import { FeatureManager, ConfigurationObjectFeatureFlagProvider } from '@microsoft/feature-management';
import { featureFlagsApi } from '../api/featureFlagsApi';
import { useAuth } from '../hooks/useAuth';
import { FeatureFlagContext } from './FeatureFlagContextType';

export { FeatureFlagContext };

interface FeatureFlagProviderProps {
  children: ReactNode;
}

export const FeatureFlagProvider: React.FC<FeatureFlagProviderProps> = ({ children }) => {
  const [flags, setFlags] = useState<Record<string, boolean>>({});
  const [loading, setLoading] = useState(true);
  const { user, loading: authLoading } = useAuth();

  useEffect(() => {
    if (authLoading) {
      return;
    }

    if (!user) {
      setFlags({});
      setLoading(false);
      return;
    }

    setLoading(true);
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
        setFlags({});
      })
      .finally(() => {
        setLoading(false);
      });
  }, [user, authLoading]);

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
