"use client";

import { useEffect } from "react";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { AlertTriangle, RefreshCw, Home, Key } from "lucide-react";
import Link from "next/link";

interface ErrorProps {
  error: Error & { digest?: string };
  reset: () => void;
}

export default function Error({ error, reset }: ErrorProps) {
  useEffect(() => {
    // Log the error to an error reporting service
    console.error("Application error:", error);
  }, [error]);
  // Determine error type for better UX
  const is404 =
    error.message?.includes("NEXT_NOT_FOUND") || error.message?.includes("404");
  const isNetworkError =
    error.message?.includes("fetch") || error.message?.includes("network");
  const isServerError =
    error.message?.includes("500") ||
    error.message?.includes("Internal Server Error");

  const isAuthError =
    error.message?.includes("Authentication") ||
    error.message?.includes("Authorization") ||
    error.message?.includes("Access denied");

  const isApiKeyError =
    error.message?.includes("Invalid API key") ||
    error.message?.includes("API key");

  const getErrorDetails = () => {
    if (is404) {
      return {
        title: "Page Not Found",
        description:
          "The page you're looking for doesn't exist or has been moved.",
        icon: AlertTriangle,
        statusCode: "404",
      };
    }

    if (isApiKeyError) {
      return {
        title: "Invalid API Key",
        description:
          "The API key provided is invalid or has expired. Please check your API key settings.",
        icon: Key,
        statusCode: "401",
      };
    }

    if (isAuthError) {
      return {
        title: "Authentication Error",
        description:
          "You are not authorized to access this page. Please log in.",
        icon: Key,
        statusCode: "401",
      };
    }

    if (isNetworkError) {
      return {
        title: "Connection Error",
        description:
          "Unable to connect to the server. Please check your internet connection.",
        icon: AlertTriangle,
        statusCode: "Network",
      };
    }

    if (isServerError) {
      return {
        title: "Server Error",
        description:
          "Something went wrong on our end. Our team has been notified.",
        icon: AlertTriangle,
        statusCode: "500",
      };
    }

    return {
      title: "Something went wrong",
      description: "An unexpected error occurred. Please try again.",
      icon: AlertTriangle,
      statusCode: "Error",
    };
  };

  const errorDetails = getErrorDetails();
  const IconComponent = errorDetails.icon;

  return (
    <div className="min-h-screen flex items-center justify-center p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="flex justify-center mb-4">
            <div className="rounded-full bg-destructive/10 p-3">
              <IconComponent className="h-8 w-8 text-destructive" />
            </div>
          </div>
          <CardTitle className="text-2xl">{errorDetails.title}</CardTitle>
          <CardDescription className="text-base">
            {errorDetails.description}
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          {/* Error details for debugging (only in development) */}
          {process.env.NODE_ENV === "development" && (
            <div className="p-3 bg-muted rounded-lg">
              <p className="text-sm font-medium text-muted-foreground mb-1">
                Error Details (Development):
              </p>
              <p className="text-xs font-mono text-muted-foreground break-all">
                {error.message}
              </p>
              {error.digest && (
                <p className="text-xs font-mono text-muted-foreground mt-1">
                  Digest: {error.digest}
                </p>
              )}
            </div>
          )}

          <div className="flex flex-col gap-2">
            <Button onClick={reset} className="w-full">
              <RefreshCw className="w-4 h-4 mr-2" />
              Try Again
            </Button>
            <Button variant="outline" asChild className="w-full">
              <Link href="/dashboard">
                <Home className="w-4 h-4 mr-2" />
                Go to Dashboard
              </Link>
            </Button>
          </div>

          {/* Additional help for specific error types */}
          {isNetworkError && (
            <div className="text-center text-sm text-muted-foreground">
              <p>If the problem persists, please check:</p>
              <ul className="list-disc list-inside mt-1 text-xs space-y-1">
                <li>Your internet connection</li>
                <li>Server status</li>
                <li>Firewall settings</li>
              </ul>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
