import NextAuth from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";
import type { OAuthConfig } from "next-auth/providers/oauth";
import {
  API_KEY,
  BACKEND_URL,
  OIDC_CLIENT_ID,
  OIDC_CLIENT_SECRET,
  OIDC_WELL_KNOWN_URL,
  OIDC_PROVIDER_NAME,
} from "@/lib/config";
import type { TeleHookUser } from "@/types/user";
import { OIDCProfile } from "@/types/next-auth";

// OIDC Provider configuration - compatible with most OIDC providers
const createOIDCProvider = (): OAuthConfig<TeleHookUser> | null => {
  if (!OIDC_CLIENT_SECRET || !OIDC_CLIENT_ID || !OIDC_WELL_KNOWN_URL) {
    console.warn(
      "OIDC environment variables not set. OIDC provider will be disabled."
    );
    return null;
  }

  return {
    id: "oidc",
    name: OIDC_PROVIDER_NAME,
    type: "oauth",
    wellKnown: OIDC_WELL_KNOWN_URL,
    clientId: OIDC_CLIENT_ID,
    clientSecret: OIDC_CLIENT_SECRET,
    checks: ["state"],
    authorization: {
      params: {
        scope: "openid email profile",
        response_type: "code",
      },
    },
    profile(profile: OIDCProfile) {
      const firstName = profile.given_name || profile.first_name || "";
      const lastName = profile.family_name || profile.last_name || "";
      const username =
        profile.preferred_username ||
        profile.username ||
        profile.name ||
        profile.email?.split("@")[0] ||
        "";

      return {
        id: profile.sub,
        email: profile.email,
        name: profile.name || profile.preferred_username || username,
        image: profile.picture || profile.avatar_url,
        username,
        firstName,
        lastName,
        role: "admin", // Default role for OIDC users
        fullName: profile.name || `${firstName} ${lastName}`.trim() || username,
        displayName:
          profile.name ||
          profile.preferred_username ||
          `${firstName} ${lastName}`.trim() ||
          profile.email,
      };
    },
  } as OAuthConfig<TeleHookUser>;
};

const handler = NextAuth({
  providers: [
    // Add OIDC provider if configured
    ...(createOIDCProvider() ? [createOIDCProvider()!] : []),
    CredentialsProvider({
      name: "credentials",
      credentials: {
        email: { label: "Email", type: "text" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials) {
        if (!credentials?.email || !credentials?.password) {
          return null;
        }

        try {
          const res = await fetch(`${BACKEND_URL}/api/users/signin`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              "X-API-KEY": API_KEY,
            },
            body: JSON.stringify({
              email: credentials.email,
              password: credentials.password,
            }),
          });

          if (res.ok) {
            const user: TeleHookUser = await res.json();

            // Return user object matching NextAuth's expected format
            return {
              id: user.id.toString(),
              email: user.email,
              username: user.username,
              firstName: user.firstName,
              lastName: user.lastName,
              role: user.role,
              fullName: user.fullName,
              displayName: user.displayName,
            };
          }
          return null;
        } catch (error) {
          console.error("Auth error:", error);
          return null;
        }
      },
    }),
  ],

  pages: {
    signIn: "/auth/signin",
    error: "/auth/error",
  },

  callbacks: {
    async jwt({ token, user, account }) {
      // Add user properties to token
      if (user) {
        token.id = user.id;
        token.role = user.role;
        token.username = user.username;
        token.firstName = user.firstName;
        token.lastName = user.lastName;
        token.fullName = user.fullName;
        token.displayName = user.displayName;
        // Use friendly provider name for OIDC, otherwise use provider ID
        token.provider =
          account?.provider === "oidc" ? OIDC_PROVIDER_NAME : account?.provider;
        token.picture = user.image || "";
      }
      return token;
    },

    async session({ session, token }) {
      // Add properties to session
      if (token) {
        session.user.id = token.id as string;
        session.user.role = token.role as string;
        session.user.username = token.username as string;
        session.user.firstName = token.firstName as string;
        session.user.lastName = token.lastName as string;
        session.user.fullName = token.fullName as string;
        session.user.displayName = token.displayName as string;
        session.user.provider = token.provider as string;
        session.user.image = token.picture as string;
      }
      return session;
    },

    async signIn({ user, account }) {
      // Handle OIDC sign-in
      if (account?.provider === "oidc") {
        try {
          const response = await fetch(`${BACKEND_URL}/api/users/oidc-signin`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
              "X-API-KEY": API_KEY,
            },
            body: JSON.stringify({
              oidcId: user.id,
              email: user.email,
              username: user.username,
              firstName: user.firstName,
              lastName: user.lastName,
              fullName: user.fullName,
              displayName: user.displayName,
            }),
          });
          if (!response.ok) {
            console.error(
              "Failed to create/update OIDC user in backend:",
              await response.text()
            );
            return false;
          }
          const backendUser: TeleHookUser = await response.json();
          user.id = backendUser.id.toString();

          return true;
        } catch (error) {
          console.error("Error during OIDC sign-in:", error);
          return false;
        }
      }

      // For credentials provider, the authorization is handled in the provider itself
      return true;
    },

    async redirect({ url, baseUrl }) {
      // Ensure redirects are always to the same origin for security
      if (url.startsWith("/")) return `${baseUrl}${url}`;
      if (new URL(url).origin === baseUrl) return url;
      return baseUrl;
    },
  },

  events: {
    async signOut() {},
    async signIn() {},
  },

  session: {
    strategy: "jwt",
    maxAge: 30 * 24 * 60 * 60, // 30 days
  },

  // Enable debug mode in development
  debug: process.env.NODE_ENV === "development",
});

export { handler as GET, handler as POST };
