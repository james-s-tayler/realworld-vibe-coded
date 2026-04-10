import { getApiClient } from './clientFactory';

export interface FeatureFlagConfig {
  feature_management: {
    feature_flags: Array<{ id: string; enabled: boolean }>;
  };
}

const emptyConfig: FeatureFlagConfig = {
  feature_management: { feature_flags: [] },
};

export const featureFlagsApi = {
  getConfig: async (): Promise<FeatureFlagConfig> => {
    try {
      const result = await getApiClient().api.featureFlags.get();
      if (!result?.featureManagement?.featureFlags) {
        return emptyConfig;
      }
      return {
        feature_management: {
          feature_flags: result.featureManagement.featureFlags.map((f) => ({
            id: f.id ?? '',
            enabled: f.enabled ?? false,
          })),
        },
      };
    } catch {
      return emptyConfig;
    }
  },
};
