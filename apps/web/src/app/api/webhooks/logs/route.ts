import { BACKEND_URL, API_KEY } from "@/lib/config";
import { validateSession } from "@/lib/api-auth";
import { NextRequest, NextResponse } from "next/server";

// GET /api/webhooks/logs
export async function GET(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const url = new URL(request.url);
  const queryParams = url.searchParams.toString();
  const backendUrl = `${BACKEND_URL}/api/webhooks/logs${
    queryParams ? `?${queryParams}` : ""
  }`;

  const res = await fetch(backendUrl, {
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
