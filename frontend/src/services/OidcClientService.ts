import { UserManager } from "oidc-client-ts";
import config from "../config/config";
const cognitoAuthConfig = {
    authority: config.COGNITO_AUTHORITY,
    client_id: config.COGNITO_CLIENT_ID,
    redirect_uri: config.COGNITO_REDIRECT_URI,
    response_type: "code",
    scope: "email openid phone"
};

// create a UserManager instance
export const userManager = new UserManager({
    ...cognitoAuthConfig,
});

export function signOutRedirect () {
    const clientId = config.COGNITO_CLIENT_ID;
    const logoutUri = config.COGNITO_LOGOUT_REDIRECT_URI;
    const cognitoDomain = config.COGNITO_APP_DOMAIN;
    void userManager.removeUser();
    window.location.href = `${cognitoDomain}/logout?client_id=${clientId}&logout_uri=${encodeURIComponent(logoutUri)}`;
};
