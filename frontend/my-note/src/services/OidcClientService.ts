import { UserManager } from "oidc-client-ts";

const cognitoAuthConfig = {
    authority: "https://cognito-idp.sa-east-1.amazonaws.com/sa-east-1_r18CRlnE4",
    client_id: "5rfgqff289amvdotaudukqkpop",
    redirect_uri: "http://localhost:9000/callback",
    response_type: "code",
    scope: "email openid phone"
};

// create a UserManager instance
export const userManager = new UserManager({
    ...cognitoAuthConfig,
});

export function signOutRedirect () {
    const clientId = "5rfgqff289amvdotaudukqkpop";
    const logoutUri = "http://localhost:9000";
    const cognitoDomain = "https://sa-east-1r18crlne4.auth.sa-east-1.amazoncognito.com";
    void userManager.removeUser();
    window.location.href = `${cognitoDomain}/logout?client_id=${clientId}&logout_uri=${encodeURIComponent(logoutUri)}`;
};
