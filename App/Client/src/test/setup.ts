import '@testing-library/jest-dom'

// Polyfill for ResizeObserver (required by Carbon Design System components)
global.ResizeObserver = class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
}