"use client";

import { signIn } from "next-auth/react";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Alert, AlertDescription } from "@/components/ui/alert";
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
import { Webhook, Lock, Mail, Eye, EyeOff, Shield } from "lucide-react";
import { useSetupStatus } from "@/hooks/queries";
import { getProviders } from "next-auth/react";
import { UserSigninValidationSchema } from "@/validation/user-schemas";
import { mapApiErrorsToFields } from "@/validation/utils";
import { AppError, handleError } from "@/lib/error-handling";
import { toast } from "sonner";
interface SigninFormData {
  email?: string;
  password?: string;
}

export function SignInPageClient() {
  const [showPassword, setShowPassword] = useState(false);
  const [ssoLoading, setSsoLoading] = useState(false);
  const [error, setError] = useState("");
  const [oidcAvailable, setOidcAvailable] = useState(false);
  const router = useRouter();

  // React Hook Form setup
  const form = useForm<SigninFormData>({
    resolver: zodResolver(UserSigninValidationSchema),
    defaultValues: {
      email: "",
      password: "",
    },
    mode: "onChange",
  });

  // TanStack Query hook for setup status
  const {
    data: setupRequired,
    isLoading: setupLoading,
    isError,
    error: setupError,
  } = useSetupStatus();

  useEffect(() => {
    if (isError && setupError) {
      handleError(setupError);
    }
  }, [isError, setupError]);

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

  // Redirect to setup if required
  useEffect(() => {
    if (setupRequired === true) {
      router.push("/setup");
    }
  }, [setupRequired, router]);

  const handleSubmit = async (data: SigninFormData) => {
    setError("");

    try {
      const result = await signIn("credentials", {
        email: data.email,
        password: data.password,
        redirect: false,
      });

      if (result?.error) {
        // Handle different types of auth errors
        if (result.error === "CredentialsSignin") {
          setError("Invalid email or password");
        } else {
          setError("Authentication failed. Please try again.");
        }
      } else {
        router.push("/dashboard");
      }
    } catch (error) {
      if (error instanceof AppError && error.details) {
        // Handle server-side validation errors
        const fieldErrors = mapApiErrorsToFields(error.details, {
          email: "email",
          password: "password",
        });

        // Set errors on form fields
        Object.entries(fieldErrors).forEach(([field, messages]) => {
          if (messages && messages.length > 0) {
            form.setError(field as keyof SigninFormData, {
              message: messages[0],
            });
          }
        });

        toast.error("Please correct the highlighted fields");
      } else {
        setError("Login failed. Please try again.");
      }
    }
  };

  const handleSsoLogin = async () => {
    setSsoLoading(true);
    setError("");

    try {
      // Sign in with OIDC provider
      const result = await signIn("oidc", {
        redirect: false,
        callbackUrl: "/dashboard",
      });
      if (result?.error) {
        setError("SSO login failed. Please try again.");
      } else if (result?.url) {
        window.location.href = result.url;
      }
    } catch {
      setError("SSO login failed. Please contact your administrator.");
    } finally {
      setSsoLoading(false);
    }
  };

  // Show loading state while checking setup requirements
  if (setupLoading || setupRequired === undefined) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-background via-background to-muted/20 p-4">
        <div className="w-full max-w-md space-y-6">
          <div className="text-center space-y-2">
            <div className="flex items-center justify-center">
              <div className="p-3 rounded-full bg-primary/10 border border-primary/20">
                <Webhook className="w-8 h-8 text-primary animate-spin" />
              </div>
            </div>
            <h1 className="text-3xl font-bold tracking-tight">TeleHook</h1>
            <p className="text-muted-foreground">Checking setup status...</p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-background via-background to-muted/20 p-4">
      <div className="w-full max-w-md space-y-6">
        {/* Brand Header */}
        <div className="text-center space-y-2">
          <div className="flex items-center justify-center">
            <div className="flex flex-col items-center gap-3">
              <img
                src="/logo_light.png"
                alt="TeleHook"
                width={120}
                height={32}
                className="h-28 w-auto dark:hidden"
              />
              <img
                src="/logo_dark.png"
                alt="TeleHook"
                width={120}
                height={32}
                className="h-28 w-auto hidden dark:block"
              />
              <h1 className="text-xl font-light">
                Tele<span className="font-semibold">Hook</span>
              </h1>
            </div>
          </div>
          <p className="text-muted-foreground">
            Webhook-to-Telegram forwarding service
          </p>
        </div>

        {/* Login Card */}
        <Card className="border-0 shadow-xl">
          <CardHeader className="space-y-1 text-center">
            <CardTitle className="text-2xl">Welcome back</CardTitle>
            <CardDescription>
              Sign in to your admin account to continue
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <Form {...form}>
              <div className="space-y-4">
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Email</FormLabel>
                      <FormControl>
                        <div className="relative">
                          <Mail className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                          <Input
                            type="email"
                            placeholder="Enter your email"
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
                  name="password"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Password</FormLabel>
                      <FormControl>
                        <div className="relative">
                          <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                          <Input
                            type={showPassword ? "text" : "password"}
                            placeholder="Enter your password"
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

                {error && (
                  <Alert variant="destructive">
                    <AlertDescription>{error}</AlertDescription>
                  </Alert>
                )}

                <Button
                  onClick={form.handleSubmit(handleSubmit)}
                  className="w-full"
                  disabled={form.formState.isSubmitting || ssoLoading}
                >
                  {form.formState.isSubmitting ? "Signing in..." : "Sign In"}
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
                      onClick={handleSsoLogin}
                      disabled={form.formState.isSubmitting || ssoLoading}
                    >
                      {ssoLoading ? (
                        <>Connecting to SSO...</>
                      ) : (
                        <>
                          <Shield className="w-4 h-4 mr-2" />
                          Single Sign-On (SSO)
                        </>
                      )}
                    </Button>
                  </>
                )}
              </div>
            </Form>
          </CardContent>
        </Card>

        {/* Footer */}
        <p className="text-center text-sm text-muted-foreground">
          Secure webhook forwarding powered by TeleHook
        </p>
      </div>
    </div>
  );
}
