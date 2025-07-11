import { API_KEY, BACKEND_URL } from "@/lib/config";
import { validateSession } from "@/lib/api-auth";
import { NextRequest, NextResponse } from "next/server";

// GET /api/users/{id}
export async function GET(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const { searchParams } = new URL(request.url);
  const id = searchParams.get("id");

  if (!id) {
    return NextResponse.json({ error: "User ID is required" }, { status: 400 });
  }

  const res = await fetch(`${BACKEND_URL}/api/users/${id}`, {
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

// PUT /api/users/{id}
export async function PUT(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const { searchParams } = new URL(request.url);
  const id = searchParams.get("id");

  if (!id) {
    return NextResponse.json({ error: "User ID is required" }, { status: 400 });
  }

  const data = await request.json();
  const res = await fetch(`${BACKEND_URL}/api/users/${id}`, {
    method: "PUT",
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
