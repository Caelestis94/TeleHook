import { validateSession } from "@/lib/api-auth";
import { NextRequest, NextResponse } from "next/server";
import { BACKEND_URL, API_KEY } from "@/lib/config";

// POST /api/payload/capture/start
export async function POST(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const data = await request.json();

  const res = await fetch(`${BACKEND_URL}/api/payloads/capture/start`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "X-API-KEY": API_KEY,
    },
    body: JSON.stringify(data),
  });

  const body = await res.json();
  if (!res.ok) {
    return NextResponse.json(body, { status: res.status });
  }
  return NextResponse.json(body);
}
