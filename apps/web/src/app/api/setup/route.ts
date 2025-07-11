import { API_KEY, BACKEND_URL } from "@/lib/config";
import { NextRequest, NextResponse } from "next/server";

// GET /api/setup - Check if setup is required
export async function GET() {
  const res = await fetch(`${BACKEND_URL}/api/user/setup-required`, {
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

// POST /api/setup - Create initial admin user
export async function POST(request: NextRequest) {
  const data = await request.json();
  const res = await fetch(`${BACKEND_URL}/api/user/setup`, {
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
