"use client";

import { useSearchParams } from "next/navigation";
import { Suspense } from "react";
import Error from "@/app/error";
import { AppError } from "@/lib/error-handling";

function AuthErrorContent() {
  const searchParams = useSearchParams();
  const error = searchParams.get("error");

  // Create an error object that matches the Error component's expected format
  const authError = new AppError(
    getAuthErrorMessage(error),
    getStatusCode(error)
  );

  console.error("Authentication Error:", authError);
  return (
    <Error
      error={authError}
      reset={() => (window.location.href = "/auth/signin")}
    />
  );
}

function getStatusCode(error: string | null): number {
  switch (error) {
    case "Configuration":
      return 500; // Internal Server Error
    case "AccessDenied":
      return 403; // Forbidden
    case "Verification":
      return 401; // Unauthorized
    case "Default":
    default:
      return 400; // Bad Request
  }
}

function getAuthErrorMessage(error: string | null): string {
  switch (error) {
    case "Configuration":
      return "There is a problem with the server configuration.";
    case "AccessDenied":
      return "Access denied. You do not have permission to sign in.";
    case "Verification":
      return "The verification token has expired or has already been used.";
    case "Default":
    default:
      return "An error occurred during authentication. Please try again.";
  }
}

export default function AuthErrorPage() {
  return (
    <Suspense fallback={<div>Loading...</div>}>
      <AuthErrorContent />
    </Suspense>
  );
}
