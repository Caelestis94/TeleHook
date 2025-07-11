import { API_KEY, BACKEND_URL } from "@/lib/config";
import { validateSession } from "@/lib/api-auth";
import { BotFormData } from "@/types/bot";
import { NextRequest, NextResponse } from "next/server";

// GET /api/bots
export async function GET(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const res = await fetch(`${BACKEND_URL}/api/bots`, {
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

// POST /api/bots
export async function POST(request: NextRequest) {
  const validationError = await validateSession(request);
  if (validationError) return validationError;

  const data: BotFormData = await request.json();
  const res = await fetch(`${BACKEND_URL}/api/bots`, {
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
