import z from "zod";
import {
  EmailPasswordSignInValidationSchema,
  OidcSignInValidationSchema,
  UserSignupValidationSchema,
  UserUpdateValidationSchema,
} from "@/validation/user-schemas";

/**
 * A user in the TeleHook system
 * This represents an admin user who can manage bots, webhooks, and schemas
 */
export type TeleHookUser = {
  id: number;
  email: string;
  username: string;
  firstName?: string;
  lastName?: string;
  role: "admin";
  createdAt: string;
  updatedAt: string;
  oidcId?: string;
  authProvider?: string;
  isOidcLinked: boolean;
  fullName: string;
  displayName: string;
};

/**
 * Request data for email/password authentication
 */
export type EmailPasswordSignInRequest = {
  email: string;
  password: string;
};

/**
 * Request data for OIDC authentication
 */
export type OidcSignInRequest = {
  email: string;
  oidcId: string;
  username?: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  displayName?: string;
};

/**
 * Request data for creating a new user
 */
export type CreateUserRequest = {
  email: string;
  username: string;
  password: string;
  firstName?: string;
  lastName?: string;
  role?: string;
};

/**
 * Request data for updating an existing user
 */
export type UpdateUserRequest = {
  email?: string;
  firstName?: string;
  lastName?: string;
  username?: string;
  password?: string;
};

/**
 * Form data for email/password sign-in
 */
export type EmailPasswordSignInFormData = z.infer<
  typeof EmailPasswordSignInValidationSchema
>;

/**
 * Form data for OIDC sign-in
 */
export type OidcSignInFormData = z.infer<typeof OidcSignInValidationSchema>;

/**
 * Form data for user signup
 */
export type UserSignupFormData = z.infer<typeof UserSignupValidationSchema>;

/**
 * Form data for user update
 */
export type UserUpdateFormData = z.infer<typeof UserUpdateValidationSchema>;
