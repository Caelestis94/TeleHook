import { withAuth } from "next-auth/middleware";
import { NextRequest, NextResponse } from "next/server";

export default withAuth(
  function middleware(request: NextRequest) {
    // Forward real client IP to backend API calls
    if (request.nextUrl.pathname.startsWith("/api/")) {
      const requestHeaders = new Headers(request.headers);

      // Get client IP from various sources
      const clientIP =
        request.headers.get("x-forwarded-for")?.split(",")[0] ||
        request.headers.get("x-real-ip") ||
        request.headers.get("x-client-ip") ||
        "unknown";

      requestHeaders.set("x-forwarded-for", clientIP);
      requestHeaders.set("x-real-ip", clientIP);

      return NextResponse.next({
        request: {
          headers: requestHeaders,
        },
      });
    }
  },
  {
    callbacks: {
      authorized: ({ token }) => !!token,
    },
  }
);

export const config = {
  matcher: [
    "/dashboard/:path*",
    "/api/bots/:path*",
    "/api/webhooks/:path*",
    "/api/logs/:path*",
    "/api/stats/:path*",
  ],
};
