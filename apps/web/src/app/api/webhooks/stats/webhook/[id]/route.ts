import { validateSession } from "@/lib/api-auth";
import { API_KEY, BACKEND_URL } from "@/lib/config";
import { NextResponse, NextRequest } from "next/server";

// GET /api/webhooks/stats/webhook/[id]
export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const { id } = await params;
  const res = await fetch(`${BACKEND_URL}/api/webhooks/stats/${id}`, {
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
