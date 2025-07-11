import { NextRequest, NextResponse } from "next/server";
import { validateSession } from "@/lib/api-auth";
import {
  OIDC_CLIENT_ID,
  OIDC_CLIENT_SECRET,
  OIDC_WELL_KNOWN_URL,
} from "@/lib/config";

// GET /api/auth/oidc-available
export async function GET(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  // Check if OIDC environment variables are configured
  const isOidcConfigured = !!(
    OIDC_WELL_KNOWN_URL &&
    OIDC_CLIENT_SECRET &&
    OIDC_CLIENT_ID
  );

  return NextResponse.json({ isOidcConfigured });
}
