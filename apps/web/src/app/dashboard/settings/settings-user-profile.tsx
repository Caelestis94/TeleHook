"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { UserUpdateValidationSchema } from "@/validation/user-schemas";
import type { UserUpdateFormData } from "@/types/user";
import { useUpdateUser } from "@/hooks/mutations";
import { useOidcAvailable } from "@/hooks/queries";
import { mapApiErrorsToFields } from "@/validation/utils";
import { AppError } from "@/lib/error-handling";
import { toast } from "sonner";
import { User, Shield, Mail, UserIcon, Key, Link } from "lucide-react";
import { signIn } from "next-auth/react";

interface UserProfileSettingsProps {
  currentUser?: {
    id: string;
    email: string;
    firstName?: string;
    lastName?: string;
    username: string;
    provider?: string;
  };
}

export function UserProfileSettings({ currentUser }: UserProfileSettingsProps) {
  const updateUserMutation = useUpdateUser();
  const { data: oidcAvailable } = useOidcAvailable();
  const [showPasswordFields, setShowPasswordFields] = useState(false);

  const isOidcLinked =
    currentUser?.provider && currentUser.provider !== "credentials";
  const canLinkOidc = !isOidcLinked && oidcAvailable?.isOidcConfigured;

  const form = useForm<UserUpdateFormData>({
    resolver: zodResolver(UserUpdateValidationSchema),
    defaultValues: {
      email: currentUser?.email || "",
      firstName: currentUser?.firstName || "",
      lastName: currentUser?.lastName || "",
      username: currentUser?.username || "",
      password: "",
      confirmPassword: "",
    },
    mode: "onChange",
  });

  const handleSubmit = async (data: UserUpdateFormData) => {
    if (!currentUser?.id) {
      toast.error("User ID not found");
      return;
    }

    // Send all fields - backend will handle validation appropriately
    const payload = {
      email: data.email,
      firstName: data.firstName,
      lastName: data.lastName,
      username: data.username,
      // Only include password if it's provided and not empty
      ...(data.password &&
        data.password.trim().length > 0 && {
          password: data.password,
        }),
    };

    try {
      await updateUserMutation.mutateAsync({
        id: currentUser.id,
        data: payload,
      });

      // Clear password fields after successful update
      if (!isOidcLinked) {
        form.setValue("password", "");
        form.setValue("confirmPassword", "");
        setShowPasswordFields(false);
      }
    } catch (error) {
      if (error instanceof AppError && error.details) {
        const fieldErrors = mapApiErrorsToFields(error.details, {
          email: "email",
          firstName: ["firstname", "first"],
          lastName: ["lastname", "last"],
          username: "username",
          password: "password",
        });

        Object.entries(fieldErrors).forEach(([field, messages]) => {
          if (messages && messages.length > 0) {
            form.setError(field as keyof UserUpdateFormData, {
              type: "server",
              message: messages[0],
            });
          }
        });

        toast.error("Please correct the highlighted fields");
      }
    }
  };

  const handleLinkOidc = () => {
    // Use the same OIDC sign-in flow - the backend will handle linking
    signIn("oidc", {
      callbackUrl: "/dashboard/settings",
      redirect: true,
    });
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <User className="h-5 w-5 text-yellow-600 dark:text-yellow-400" />
          User Profile
        </CardTitle>
        <CardDescription>
          Update your personal information and account settings
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {isOidcLinked && (
          <div className="space-y-3">
            <div className="flex items-center gap-2 p-3 bg-green-50 dark:bg-green-950/20 border border-green-200 dark:border-green-800 rounded-lg">
              <Shield className="h-4 w-4 text-green-600 dark:text-green-400" />
              <span className="text-sm text-green-700 dark:text-green-300">
                Account linked via OIDC provider {currentUser?.provider}
              </span>
            </div>
            <div className="p-3 bg-blue-50 dark:bg-blue-950/20 border border-blue-200 dark:border-blue-800 rounded-lg">
              <p className="text-sm text-blue-700 dark:text-blue-300">
                <strong>Note:</strong> Email, first name, and last name are
                managed by your identity provider. To update these fields,
                update them through your {currentUser?.provider} account.
              </p>
            </div>
          </div>
        )}

        {canLinkOidc && (
          <div className="p-4 bg-amber-50 dark:bg-amber-950/20 border border-amber-200 dark:border-amber-800 rounded-lg">
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4 items-center">
              <div className="space-y-2">
                <h4 className="text-sm font-medium text-amber-800 dark:text-amber-200 flex items-center gap-2">
                  <Link className="h-4 w-4" />
                  Link OIDC Account
                </h4>
                <p className="text-sm text-amber-700 dark:text-amber-300">
                  Connect your account with an OIDC provider for enhanced
                  security and centralized authentication.
                </p>
              </div>
              <Button
                type="button"
                variant="outline"
                size="sm"
                onClick={handleLinkOidc}
                className="md:w-[150px] md:ml-auto "
              >
                <Link className="h-4 w-4 mr-2" />
                Link Account
              </Button>
            </div>
          </div>
        )}

        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className="space-y-4"
          >
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="firstName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>First Name</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="John"
                        {...field}
                        disabled={isOidcLinked}
                        className={isOidcLinked ? "bg-muted" : ""}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="lastName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Last Name</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="Doe"
                        {...field}
                        disabled={isOidcLinked}
                        className={isOidcLinked ? "bg-muted" : ""}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel className="flex items-center gap-2">
                    <Mail className="h-4 w-4" />
                    Email
                  </FormLabel>
                  <FormControl>
                    <Input
                      placeholder="john.doe@example.com"
                      {...field}
                      disabled={isOidcLinked}
                      className={isOidcLinked ? "bg-muted" : ""}
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="username"
              render={({ field }) => (
                <FormItem>
                  <FormLabel className="flex items-center gap-2">
                    <UserIcon className="h-4 w-4" />
                    Username
                  </FormLabel>
                  <FormControl>
                    <Input placeholder="johndoe" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {!isOidcLinked && (
              <div className="border-t pt-4">
                <div className="grid grid-cols-1 md:flex md:items-center md:justify-between mb-4">
                  <div className="mb-4">
                    <h4 className="text-sm font-medium flex items-center gap-2">
                      <Key className="h-4 w-4" />
                      Change Password
                    </h4>
                    <p className="text-sm text-muted-foreground">
                      Leave blank to keep current password
                    </p>
                  </div>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={() => setShowPasswordFields(!showPasswordFields)}
                  >
                    {showPasswordFields ? "Cancel" : "Change Password"}
                  </Button>
                </div>

                {showPasswordFields && (
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <FormField
                      control={form.control}
                      name="password"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>New Password</FormLabel>
                          <FormControl>
                            <Input
                              type="password"
                              placeholder="••••••••"
                              {...field}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="confirmPassword"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Confirm Password</FormLabel>
                          <FormControl>
                            <Input
                              type="password"
                              placeholder="••••••••"
                              {...field}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                )}
              </div>
            )}

            <div className="md:flex md:justify-end pt-4 grid grid-cols-1">
              <Button
                type="submit"
                disabled={
                  updateUserMutation.isPending || !form.formState.isValid
                }
              >
                {updateUserMutation.isPending
                  ? "Updating..."
                  : isOidcLinked
                  ? "Update Username"
                  : "Update Profile"}
              </Button>
            </div>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
