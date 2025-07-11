import { API_KEY, BACKEND_URL } from "@/lib/config";
import { validateSession } from "@/lib/api-auth";
import { NextRequest, NextResponse } from "next/server";

type HealthResponse = {
  status: string;
  timestamp?: string;
  environment?: string;
};

// GET /health
export async function GET(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const res = await fetch(`${BACKEND_URL}/health`, {
    headers: {
      "X-API-KEY": API_KEY,
    },
  });
  const body = (await res.json()) as HealthResponse;

  if (!res.ok) {
    return NextResponse.json(body, { status: res.status });
  }

  return NextResponse.json({ status: body.status }, { status: 200 });
}
