#!/usr/bin/env node

// Checks that all locale JSON files have identical key structures.
// Exits with code 1 if any keys are missing from any locale file.

import { readFileSync, readdirSync } from 'fs';
import { join, basename } from 'path';

const localesDir = join(import.meta.dirname, '..', 'src', 'i18n', 'locales');

function flattenKeys(obj, prefix = '') {
  const keys = [];
  for (const [key, value] of Object.entries(obj)) {
    const fullKey = prefix ? `${prefix}.${key}` : key;
    if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
      keys.push(...flattenKeys(value, fullKey));
    } else {
      keys.push(fullKey);
    }
  }
  return keys;
}

const files = readdirSync(localesDir).filter(f => f.endsWith('.json')).sort();

if (files.length < 2) {
  console.log('Only one locale file found — nothing to compare.');
  process.exit(0);
}

const locales = {};
for (const file of files) {
  const lang = basename(file, '.json');
  const content = JSON.parse(readFileSync(join(localesDir, file), 'utf-8'));
  locales[lang] = new Set(flattenKeys(content));
}

const allKeys = new Set();
for (const keys of Object.values(locales)) {
  for (const key of keys) {
    allKeys.add(key);
  }
}

let hasErrors = false;
for (const key of [...allKeys].sort()) {
  for (const [lang, keys] of Object.entries(locales)) {
    if (!keys.has(key)) {
      console.error(`MISSING: "${key}" not found in ${lang}.json`);
      hasErrors = true;
    }
  }
}

if (hasErrors) {
  console.error('\nLocale files are out of sync. All locale files must have identical keys.');
  process.exit(1);
} else {
  console.log(`All ${files.length} locale files have identical keys (${allKeys.size} keys each)`);
}
