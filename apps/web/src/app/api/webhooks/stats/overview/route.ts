import { validateSession } from "@/lib/api-auth";
import { API_KEY, BACKEND_URL } from "@/lib/config";
import { NextRequest, NextResponse } from "next/server";

// GET /api/webhooks/stats/overview
export async function GET(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const res = await fetch(`${BACKEND_URL}/api/webhooks/stats/overview`, {
    method: "GET",
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
