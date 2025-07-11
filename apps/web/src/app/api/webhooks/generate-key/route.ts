import { validateSession } from "@/lib/api-auth";
import { BACKEND_URL, API_KEY } from "@/lib/config";
import { NextRequest, NextResponse } from "next/server";

export async function POST(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const data = await request.json();
  const res = await fetch(`${BACKEND_URL}/api/webhooks/generate-key`, {
    method: "POST",
    headers: {
      "X-API-KEY": API_KEY,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(data),
  });

  const body = await res.json();
  if (!res.ok) {
    return NextResponse.json(body, { status: res.status });
  }
  return NextResponse.json(body);
}
