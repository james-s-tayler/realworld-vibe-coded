export const FEATURE_FLAGS = {
  DASHBOARD_BANNER: 'DashboardBanner',
} as const;

export type FeatureFlagName = (typeof FEATURE_FLAGS)[keyof typeof FEATURE_FLAGS];
