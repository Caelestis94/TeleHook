import { validateSession } from "@/lib/api-auth";
import { NextRequest, NextResponse } from "next/server";
import { BACKEND_URL, API_KEY } from "@/lib/config";

// DELETE /api/payload/capture/cancel/[uuid]
export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ uuid: string }> }
) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const { uuid } = await params;
  const res = await fetch(
    `${BACKEND_URL}/api/payloads/capture/cancel/${uuid}`,
    {
      method: "GET",
      headers: {
        "X-API-KEY": API_KEY,
        "Content-Type": "application/json",
      },
    }
  );

  if (res.status === 200) {
    return new NextResponse(null, { status: 200 });
  }
  const body = await res.json();
  if (!res.ok) {
    return NextResponse.json(body, { status: res.status });
  }
}
