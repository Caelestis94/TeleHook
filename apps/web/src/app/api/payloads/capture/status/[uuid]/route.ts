import { validateSession } from "@/lib/api-auth";
import { BACKEND_URL, API_KEY } from "@/lib/config";
import { NextRequest, NextResponse } from "next/server";

// GET /api/payload/capture/status/[uuid]
export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ uuid: string }> }
) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const { uuid } = await params;
  const res = await fetch(
    `${BACKEND_URL}/api/payloads/capture/status/${uuid}`,
    {
      headers: {
        "X-API-KEY": API_KEY,
      },
    }
  );
  const body = await res.json();
  if (!res.ok) {
    return NextResponse.json(body, { status: res.status });
  }
  return NextResponse.json(body);
}
