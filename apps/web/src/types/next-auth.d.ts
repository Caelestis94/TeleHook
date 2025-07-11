declare module "next-auth" {
  interface Session {
    user: {
      id: string;
      email?: string | null;
      role: string;
      username: string;
      firstName: string;
      lastName: string;
      fullName: string;
      displayName: string;
      provider?: string;
      image?: string | null;
    };
  }

  interface User {
    id: string;
    email?: string | null;
    role: string;
    username: string;
    firstName: string;
    lastName: string;
    fullName: string;
    displayName: string;
    image?: string | null;
  }
}

declare module "next-auth/jwt" {
  interface JWT {
    role: string;
    username: string;
    firstName: string;
    lastName: string;
    fullName: string;
    displayName: string;
    provider?: string;
    picture?: string | null;
  }
}

export type OIDCProfile = {
  sub?: string;
  email?: string;
  name?: string;
  preferred_username?: string;
  username?: string;
  given_name?: string;
  first_name?: string;
  family_name?: string;
  last_name?: string;
  picture?: string;
  avatar_url?: string;
  [key: string]: unknown;
}
