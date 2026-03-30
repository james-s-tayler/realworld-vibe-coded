import { AnonymousAuthenticationProvider } from '@microsoft/kiota-abstractions';
import {
  FetchRequestAdapter,
  HttpClient,
} from '@microsoft/kiota-http-fetchlibrary';
import {
  createConduitApiClient,
  type ConduitApiClient,
} from './generated/conduitApiClient.js';

const API_BASE_URL = import.meta.env.VITE_API_URL || '';

let currentLanguage = 'en';

export function setApiLanguage(lang: string) {
  currentLanguage = lang;
}

function getCookie(name: string): string | null {
  const value = `; ${document.cookie}`;
  const parts = value.split(`; ${name}=`);
  if (parts.length === 2) {
    return parts.pop()?.split(';').shift() ?? null;
  }
  return null;
}

const customFetch = async (
  url: string,
  requestInit: RequestInit,
): Promise<Response> => {
  const method = requestInit.method?.toUpperCase() || 'GET';
  const headers = new Headers(requestInit.headers);

  headers.set('Accept-Language', currentLanguage);

  if (['POST', 'PUT', 'DELETE', 'PATCH'].includes(method)) {
    const csrfToken = getCookie('XSRF-TOKEN');
    if (csrfToken) {
      headers.set('X-XSRF-TOKEN', csrfToken);
    }
  }

  requestInit.headers = headers;
  requestInit.credentials = 'include';

  return fetch(url, requestInit);
};

let clientInstance: ConduitApiClient | null = null;

export function getApiClient(): ConduitApiClient {
  if (!clientInstance) {
    const authProvider = new AnonymousAuthenticationProvider();
    const httpClient = new HttpClient(customFetch);
    const adapter = new FetchRequestAdapter(authProvider, undefined, undefined, httpClient);
    adapter.baseUrl = API_BASE_URL;
    clientInstance = createConduitApiClient(adapter);
  }
  return clientInstance;
}
