import { NextRequest, NextResponse } from "next/server";
import { getToken } from "next-auth/jwt";

export async function validateSession(
  request: NextRequest
): Promise<NextResponse | null> {
  const token = await getToken({ req: request });

  if (!token) {
    return NextResponse.json(
      { error: "Authentication required" },
      { status: 401 }
    );
  }

  // Optional: Check if user has admin role
  if (token.role !== "admin") {
    return NextResponse.json(
      { error: "Admin access required" },
      { status: 403 }
    );
  }

  return null; // Valid session
}
