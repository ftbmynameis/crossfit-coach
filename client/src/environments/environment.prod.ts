declare const __API_BASE_URL__: string;

export const environment = {
  production: true,
  apiBaseUrl: typeof __API_BASE_URL__ !== 'undefined' ? __API_BASE_URL__ : '',
};
