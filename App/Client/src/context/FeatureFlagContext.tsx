import { ConfigurationObjectFeatureFlagProvider,FeatureManager } from '@microsoft/feature-management';
import React, { type ReactNode,useCallback, useEffect, useRef, useState } from 'react';

import { configApi } from '../api/configApi';
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
  const intervalId = useRef<ReturnType<typeof setInterval>>(undefined);

  const fetchFlags = useCallback(async () => {
    try {
      const config = await featureFlagsApi.getConfig();
      const provider = new ConfigurationObjectFeatureFlagProvider(config as unknown as Record<string, unknown>);
      const manager = new FeatureManager(provider);

      const evaluated: Record<string, boolean> = {};
      for (const ff of config.feature_management?.feature_flags ?? []) {
        evaluated[ff.id] = await manager.isEnabled(ff.id);
      }
      setFlags(evaluated);
    } catch {
      setFlags({});
    }
  }, []);

  useEffect(() => {
    if (authLoading) {
      return;
    }

    if (!user) {
      setFlags({});
      setLoading(false);
      return;
    }

    let cancelled = false;

    const init = async () => {
      try {
        const appConfig = await configApi.getConfig();
        const intervalMs = appConfig.featureFlagRefreshIntervalSeconds * 1000;

        await fetchFlags();
        setLoading(false);

        if (!cancelled) {
          intervalId.current = setInterval(fetchFlags, intervalMs);
        }
      } catch {
        setFlags({});
        setLoading(false);
      }
    };

    init();

    return () => {
      cancelled = true;
      if (intervalId.current) {
        clearInterval(intervalId.current);
      }
    };
  }, [user, authLoading, fetchFlags]);

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
