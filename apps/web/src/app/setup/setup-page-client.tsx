"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { signIn, getProviders } from "next-auth/react";
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
import { Separator } from "@/components/ui/separator";
import {
  User,
  Mail,
  Lock,
  Eye,
  EyeOff,
  CheckCircle,
  Settings,
  Shield,
} from "lucide-react";
import { UserSignupFormData } from "@/types/user";
import { useSetupStatus } from "@/hooks/queries";
import { useSetupAdmin } from "@/hooks/mutations";
import { UserSignupValidationSchema } from "@/validation/user-schemas";
import { mapApiErrorsToFields } from "@/validation/utils";
import { toast } from "sonner";
import { ApiError } from "@/types/api-error";
import { handleError } from "@/lib/error-handling";

export function SetupPageClient() {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [ssoLoading, setSsoLoading] = useState(false);
  const [oidcAvailable, setOidcAvailable] = useState(false);
  const router = useRouter();

  // React Hook Form setup
  const form = useForm<UserSignupFormData>({
    resolver: zodResolver(UserSignupValidationSchema),
    defaultValues: {
      email: "",
      username: "",
      password: "",
      confirmPassword: "",
      firstName: "",
      lastName: "",
    },
    mode: "onChange",
  });

  // TanStack Query hooks
  const {
    data: setupRequired,
    isLoading: setupLoading,
    isError,
    error,
  } = useSetupStatus();
  const setupAdminMutation = useSetupAdmin();

  useEffect(() => {
    if (isError && error) {
      handleError(error);
    }
  }, [isError, error]);

  // Check available providers
  useEffect(() => {
    const checkProviders = async () => {
      try {
        const providers = await getProviders();
        setOidcAvailable(!!providers?.oidc);
      } catch {
        setOidcAvailable(false);
      }
    };

    checkProviders();
  }, []);

  // Redirect to dashboard if setup is not required
  useEffect(() => {
    if (setupRequired === false) {
      router.push("/dashboard");
    }
  }, [setupRequired, router]);

  const handleSubmit = async (data: UserSignupFormData) => {
    setupAdminMutation.mutate(
      {
        email: data.email,
        username: data.username,
        password: data.password,
        firstName: data.firstName || "",
        lastName: data.lastName || "",
      },
      {
        onSuccess: () => {
          router.push("/auth/signin?message=Setup complete! Please sign in.");
        },
        onError: (error: ApiError) => {
          if (error && error.details) {
            // Handle server-side validation errors
            const fieldErrors = mapApiErrorsToFields(error.details, {
              email: "email",
              username: "username",
              password: "password",
              firstName: ["firstname", "first name"],
              lastName: ["lastname", "last name"],
            });

            // Set errors on form fields
            Object.entries(fieldErrors).forEach(([field, messages]) => {
              if (messages && messages.length > 0) {
                form.setError(field as keyof UserSignupFormData, {
                  message: messages[0],
                });
              }
            });

            toast.error("Please correct the highlighted fields");
          } else {
            toast.error("Setup failed. Please try again.");
          }
        },
      }
    );
  };

  const handleSsoSetup = async () => {
    setSsoLoading(true);

    try {
      // Sign in with OIDC provider for setup
      const result = await signIn("oidc", {
        redirect: false,
        callbackUrl: "/dashboard",
      });

      if (result?.error) {
        toast.error(`SSO setup failed: ${result.error}`);
      } else if (result?.url) {
        // OIDC provider will redirect to the authorization server
        window.location.href = result.url;
      }
    } catch {
    } finally {
      setSsoLoading(false);
    }
  };

  if (setupLoading || setupRequired === undefined) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-background via-background to-muted/20 p-4">
        <div className="w-full max-w-lg space-y-6">
          <div className="text-center space-y-2">
            <div className="flex items-center justify-center">
              <div className="p-3 rounded-full bg-primary/10 border border-primary/20">
                <Settings className="w-8 h-8 text-primary animate-spin" />
              </div>
            </div>
            <h1 className="text-3xl font-bold tracking-tight">TeleHook</h1>
            <p className="text-muted-foreground">Checking setup status...</p>
          </div>
        </div>
      </div>
    );
  }

  if (setupRequired)
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-background via-background to-muted/20 p-4">
        <div className="w-full max-w-lg space-y-6">
          {/* Brand Header */}
          <div className="text-center space-y-2">
            <div className="flex flex-col items-center gap-3">
              <img
                src="/logo_light.png"
                alt="TeleHook"
                width={120}
                height={32}
                className="h-32 w-auto dark:hidden"
              />
              <img
                src="/logo_dark.png"
                alt="TeleHook"
                width={120}
                height={32}
                className="h-32 w-auto hidden dark:block"
              />
              <h1 className="text-xl font-light">
                Tele<span className="font-semibold">Hook</span>
              </h1>
            </div>
            <p className="text-muted-foreground">
              Setup your administrator account to get started
            </p>
          </div>

          {/* Setup Card */}
          <Card className="border-0 shadow-xl">
            <CardHeader className="space-y-1 text-center">
              <CardTitle className="text-2xl flex items-center justify-center gap-2">
                <User className="w-5 h-5" />
                Create Admin Account
              </CardTitle>
              <CardDescription>
                Set up your administrator account to manage webhooks
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <Form {...form}>
                <form
                  onSubmit={form.handleSubmit(handleSubmit)}
                  className="space-y-4"
                >
                  {/* Account Information */}
                  <div className="space-y-4">
                    <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                      Account Information
                    </div>

                    <div className="grid grid-cols-1 gap-4">
                      <FormField
                        control={form.control}
                        name="email"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Email Address</FormLabel>
                            <FormControl>
                              <div className="relative">
                                <Mail className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                                <Input
                                  type="email"
                                  placeholder="admin@example.com"
                                  className="pl-10"
                                  {...field}
                                />
                              </div>
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
                            <FormLabel>Username</FormLabel>
                            <FormControl>
                              <div className="relative">
                                <User className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                                <Input
                                  type="text"
                                  placeholder="admin"
                                  className="pl-10"
                                  {...field}
                                />
                              </div>
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                  </div>

                  <Separator />

                  {/* Personal Information */}
                  <div className="space-y-4">
                    <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                      Personal Information (Optional)
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <FormField
                        control={form.control}
                        name="firstName"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>First Name</FormLabel>
                            <FormControl>
                              <Input
                                type="text"
                                placeholder="John"
                                {...field}
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
                              <Input type="text" placeholder="Doe" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                  </div>

                  <Separator />

                  {/* Security */}
                  <div className="space-y-4">
                    <div className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                      Security
                    </div>

                    <div className="space-y-4">
                      <FormField
                        control={form.control}
                        name="password"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Password</FormLabel>
                            <FormControl>
                              <div className="relative">
                                <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                                <Input
                                  type={showPassword ? "text" : "password"}
                                  placeholder="Create a strong password"
                                  className="pl-10 pr-10"
                                  {...field}
                                />
                                <Button
                                  type="button"
                                  variant="ghost"
                                  size="sm"
                                  className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                                  onClick={() => setShowPassword(!showPassword)}
                                >
                                  {showPassword ? (
                                    <EyeOff className="h-4 w-4 text-muted-foreground" />
                                  ) : (
                                    <Eye className="h-4 w-4 text-muted-foreground" />
                                  )}
                                </Button>
                              </div>
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
                              <div className="relative">
                                <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                                <Input
                                  type={
                                    showConfirmPassword ? "text" : "password"
                                  }
                                  placeholder="Confirm your password"
                                  className="pl-10 pr-10"
                                  {...field}
                                />
                                <Button
                                  type="button"
                                  variant="ghost"
                                  size="sm"
                                  className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
                                  onClick={() =>
                                    setShowConfirmPassword(!showConfirmPassword)
                                  }
                                >
                                  {showConfirmPassword ? (
                                    <EyeOff className="h-4 w-4 text-muted-foreground" />
                                  ) : (
                                    <Eye className="h-4 w-4 text-muted-foreground" />
                                  )}
                                </Button>
                              </div>
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                  </div>
                  <Button
                    type="submit"
                    className="w-full"
                    disabled={
                      form.formState.isSubmitting ||
                      setupAdminMutation.isPending ||
                      ssoLoading
                    }
                  >
                    {form.formState.isSubmitting ||
                    setupAdminMutation.isPending ? (
                      <>Setting up your account...</>
                    ) : (
                      <>
                        <CheckCircle className="w-4 h-4 mr-2" />
                        Create Admin Account
                      </>
                    )}
                  </Button>

                  {/* SSO Section - Only show if OIDC is available */}
                  {oidcAvailable && (
                    <>
                      <div className="grid grid-cols-3 items-center">
                        <hr />
                        <div className="flex justify-center text-xs uppercase whitespace-nowrap">
                          <span className="px-2 text-muted-foreground">
                            Or continue with
                          </span>
                        </div>
                        <hr />
                      </div>

                      <Button
                        type="button"
                        variant="outline"
                        className="w-full"
                        onClick={handleSsoSetup}
                        disabled={
                          form.formState.isSubmitting ||
                          setupAdminMutation.isPending ||
                          ssoLoading
                        }
                      >
                        {ssoLoading ? (
                          <>Connecting to SSO...</>
                        ) : (
                          <>
                            <Shield className="w-4 h-4 mr-2" />
                            Setup with Single Sign-On (SSO)
                          </>
                        )}
                      </Button>
                    </>
                  )}
                </form>
              </Form>
            </CardContent>
          </Card>

          {/* Footer */}
          <p className="text-center text-sm text-muted-foreground">
            After setup, you&apos;ll be redirected to sign in
          </p>
        </div>
      </div>
    );
}
