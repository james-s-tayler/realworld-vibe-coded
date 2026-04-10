import { getApiClient } from './clientFactory';

export interface AppConfig {
  featureFlagRefreshIntervalSeconds: number;
}

export const configApi = {
  getConfig: async (): Promise<AppConfig> => {
    const result = await getApiClient().api.config.get();
    return {
      featureFlagRefreshIntervalSeconds: result?.featureFlagRefreshIntervalSeconds ?? 30,
    };
  },
};
