import { BACKEND_URL } from "@/lib/config";
import { NextResponse } from "next/server";

// /api/payload/capture/{ uuid };
export async function POST(
  request: Request,
  { params }: { params: Promise<{ uuid: string }> }
) {
  const { uuid } = await params;
  const data = await request.json();

  const headers = new Headers();
  headers.set("Content-Type", "application/json");

  const res = await fetch(`${BACKEND_URL}/api/payloads/capture/${uuid}`, {
    method: "POST",
    headers: headers,
    body: JSON.stringify(data),
  });

  const body = await res.json();
  if (!res.ok) {
    return NextResponse.json(body, { status: res.status });
  }
  return NextResponse.json(body);
}
