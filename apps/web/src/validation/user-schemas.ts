import z from "zod";

// Email/Password Sign-in Schema
export const EmailPasswordSignInValidationSchema = z.object({
  email: z
    .string()
    .min(1, "Email is required")
    .email("Please enter a valid email address"),
  password: z.string().min(1, "Password is required"),
});

// OIDC Sign-in Schema
export const OidcSignInValidationSchema = z.object({
  email: z
    .string()
    .min(1, "Email is required")
    .email("Please enter a valid email address"),
  oidcId: z.string().min(1, "OIDC ID is required"),
  username: z.string().optional(),
  firstName: z.string().optional(),
  lastName: z.string().optional(),
  fullName: z.string().optional(),
  displayName: z.string().optional(),
});

// Legacy schema for backward compatibility
export const UserSigninValidationSchema = EmailPasswordSignInValidationSchema;

// Common fields
const emailField = z
  .string()
  .min(1, "Email is required")
  .email("Please enter a valid email address");

const usernameField = z
  .string()
  .min(3, "Username must be at least 3 characters")
  .max(50, "Username must be less than 50 characters")
  .regex(
    /^[a-zA-Z0-9_-]+$/,
    "Username can only contain letters, numbers, underscores, and hyphens"
  );

const firstNameField = z
  .string()
  .max(50, "First name must be less than 50 characters");

const lastNameField = z
  .string()
  .max(50, "Last name must be less than 50 characters");

const passwordField = z
  .string()
  .min(8, "Password must be at least 8 characters")
  .regex(
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/,
    "Password must contain at least one uppercase letter, one lowercase letter, and one number"
  );

// Signup schema
export const UserSignupValidationSchema = z
  .object({
    email: emailField,
    username: usernameField,
    password: passwordField,
    confirmPassword: z.string().min(1, "Please confirm your password"),
    firstName: firstNameField.optional(),
    lastName: lastNameField.optional(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

// Update schema
export const UserUpdateValidationSchema = z
  .object({
    email: emailField,
    username: usernameField,
    firstName: firstNameField.min(1, "First name is required"),
    lastName: lastNameField.min(1, "Last name is required"),
    password: z
      .string()
      .optional()
      .transform((val) => val || "")
      .refine((val) => {
        if (val === "") return true; // Empty is allowed
        return val.length >= 8;
      }, "Password must be at least 8 characters")
      .refine((val) => {
        if (val === "") return true; // Empty is allowed
        return /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/.test(val);
      }, "Password must contain at least one uppercase letter, one lowercase letter, and one number"),
    confirmPassword: z
      .string()
      .optional()
      .transform((val) => val || ""),
  })
  .refine(
    (data) => {
      // Only validate password match if password is provided and not empty
      if (!data.password || data.password === "") return true;
      return data.password === data.confirmPassword;
    },
    {
      message: "Passwords do not match",
      path: ["confirmPassword"],
    }
  );
