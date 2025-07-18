import { BACKEND_URL, API_KEY } from "@/lib/config";
import { validateSession } from "@/lib/api-auth";
import { NextRequest, NextResponse } from "next/server";

// GET /api/webhooks/logs/export
export async function GET(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const res = await fetch(`${BACKEND_URL}/api/webhooks/logs/export`, {
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
