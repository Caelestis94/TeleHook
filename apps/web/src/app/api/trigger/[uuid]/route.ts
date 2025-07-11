import { BACKEND_URL } from "@/lib/config";
import { NextResponse } from "next/server";

//api/trigger/{uuid}

export async function POST(
  request: Request,
  { params }: { params: Promise<{ uuid: string }> }
) {
  const { uuid } = await params;
  const url = new URL(request.url);
  const data = await request.json();

  const headers = new Headers();
  headers.set("Content-Type", "application/json");

  const secretKey =
    request.headers.get("Authorization") || url.searchParams.get("secret_key");
  if (secretKey) {
    headers.set("Authorization", `Bearer ${secretKey}`);
  }

  const res = await fetch(`${BACKEND_URL}/api/trigger/${uuid}`, {
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
