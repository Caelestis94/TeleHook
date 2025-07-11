import { withAuth } from "next-auth/middleware"

export default withAuth(
  function middleware() {
  },
  {
    callbacks: {
      authorized: ({ token }) => !!token
    },
  }
)

export const config = {
  matcher: [
    "/dashboard/:path*",
    "/api/bots/:path*",
    "/api/webhooks/:path*",
    "/api/logs/:path*",
    "/api/stats/:path*"
  ]
}