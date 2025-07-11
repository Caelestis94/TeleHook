import { validateSession } from "@/lib/api-auth";
import { BACKEND_URL, API_KEY } from "@/lib/config";
import { NextRequest, NextResponse } from "next/server";

// GET /api/settings/notification/test
export async function GET(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const res = await fetch(`${BACKEND_URL}/api/settings/notification/test`, {
    headers: {
      "X-API-KEY": API_KEY,
    },
  });
  const body = await res.json();

  if (!res.ok) {
    return NextResponse.json(body, { status: res.status });
  }
  return NextResponse.json(body);
}
