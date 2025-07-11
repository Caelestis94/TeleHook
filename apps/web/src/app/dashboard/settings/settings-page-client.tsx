"use client";

import { Settings } from "lucide-react";
import { useSession } from "next-auth/react";
import { PageHeader } from "@/components/layout/page-header";
import { useOidcAvailable, useSettings, useUser } from "@/hooks/queries";
import { Skeleton } from "@/components/ui/skeleton";
import { Card, CardContent } from "@/components/ui/card";
import { AppSettings, UserProfileSettings } from "@/app/dashboard/settings";
import { handleError } from "@/lib/error-handling";
import { useEffect } from "react";

export function SettingsPageClient() {
  const { data: session } = useSession();
  const {
    data: settings,
    isLoading: settingsLoading,
    isError: isSettingsError,
    error: settingsError,
  } = useSettings();
  const {
    data: user,
    isLoading: userLoading,
    isError: userIsError,
    error: userError,
  } = useUser(session?.user?.id || "");

  const isError = isSettingsError || userIsError;
  const error = settingsError || userError;

  useEffect(() => {
    if (isError && error) {
      handleError(error);
    }
  }, [isError, error]);

  // Preload OIDC availability for the user profile component
  useOidcAvailable();

  return (
    <div className="space-y-6">
      <PageHeader
        title="Settings"
        description="Configure your application preferences and account settings"
        icon={Settings}
        iconBgColor="bg-yellow-100 dark:bg-yellow-900/20"
        iconColor="text-yellow-600 dark:text-yellow-400"
        breadcrumbItems={[
          { label: "Dashboard", href: "/dashboard" },
          { label: "Settings" },
        ]}
      />

      <div className="grid gap-6">
        {/* User Profile Settings */}
        {userLoading ? (
          <Card>
            <CardContent className="p-6">
              <div className="space-y-4">
                <Skeleton className="h-6 w-32" />
                <Skeleton className="h-4 w-64" />
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <Skeleton className="h-10" />
                  <Skeleton className="h-10" />
                </div>
                <Skeleton className="h-10" />
                <Skeleton className="h-10" />
              </div>
            </CardContent>
          </Card>
        ) : (
          <UserProfileSettings
            currentUser={
              user
                ? {
                    id: user.id.toString(),
                    email: user.email,
                    firstName: user.firstName,
                    lastName: user.lastName,
                    username: user.username,
                    provider: session?.user?.provider,
                  }
                : undefined
            }
          />
        )}

        {/* App Settings */}
        {settingsLoading ? (
          <Card>
            <CardContent className="p-6">
              <div className="space-y-4">
                <Skeleton className="h-6 w-40" />
                <Skeleton className="h-4 w-80" />
                <Skeleton className="h-10" />
                <Skeleton className="h-16" />
                <Skeleton className="h-10" />
                <Skeleton className="h-10" />
              </div>
            </CardContent>
          </Card>
        ) : (
          <AppSettings currentSettings={settings} />
        )}
      </div>
    </div>
  );
}
