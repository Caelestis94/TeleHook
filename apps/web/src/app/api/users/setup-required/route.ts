import { API_KEY, BACKEND_URL } from "@/lib/config";
import { NextResponse } from "next/server";

// GET /api/users/setup-required
export async function GET() {
  const res = await fetch(`${BACKEND_URL}/api/users/setup-required`, {
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
