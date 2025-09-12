export default {
  COGNITO_APP_DOMAIN: process.env.COGNITO_APP_DOMAIN?.toString() || '',
  COGNITO_REDIRECT_URI: process.env.COGNITO_REDIRECT_URI?.toString() || '',
  COGNITO_LOGOUT_REDIRECT_URI: process.env.COGNITO_LOGOUT_REDIRECT_URI?.toString() || '',
  COGNITO_AUTHORITY: process.env.COGNITO_AUTHORITY?.toString() || '',
  COGNITO_CLIENT_ID: process.env.COGNITO_CLIENT_ID?.toString() || '',
  API_BASE_URL: process.env.API_BASE_URL?.toString() || '',
};
