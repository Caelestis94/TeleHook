import { validateSession } from "@/lib/api-auth";
import { API_KEY, BACKEND_URL } from "@/lib/config";
import { NextRequest, NextResponse } from "next/server";

// GET /api/webhooks/[id]
export async function GET(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const { id } = await params;
  const res = await fetch(`${BACKEND_URL}/api/webhooks/${id}`, {
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

// PUT /api/webhooks/[id]
export async function PUT(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const { id } = await params;
  const data = await request.json();
  const res = await fetch(`${BACKEND_URL}/api/webhooks/${id}`, {
    method: "PUT",
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

// DELETE /api/webhooks/[id]
export async function DELETE(
  request: NextRequest,
  { params }: { params: Promise<{ id: string }> }
) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const { id } = await params;
  const res = await fetch(`${BACKEND_URL}/api/webhooks/${id}`, {
    method: "DELETE",
    headers: {
      "X-API-KEY": API_KEY,
      "Content-Type": "application/json",
    },
  });

  if (res.status === 204) {
    return new NextResponse(null, { status: 204 });
  }
  const body = await res.json();
  if (!res.ok) {
    return NextResponse.json(body, { status: res.status });
  }
}
