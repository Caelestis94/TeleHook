import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import { Toaster } from "@/components/ui/sonner";
const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  // Add other metadata like title and description
  title: "TeleHook",
  description: "Webhook to Telegram proxy service",

  // Reference your manifest
  manifest: "/site.webmanifest",

  // Define your icons
  icons: {
    icon: [
      { url: "/favicon-32x32.png", sizes: "32x32", type: "image/png" },
      { url: "/favicon-16x16.png", sizes: "16x16", type: "image/png" },
    ],
    apple: "/apple-icon.png", // Next.js will find the one in /app
    other: [
      {
        rel: "android-chrome-192x192",
        url: "/android-chrome-192x192.png",
      },
      {
        rel: "android-chrome-512x512",
        url: "/android-chrome-512x512.png",
      },
    ],
  },
};

import { Providers } from "@/components/providers";
import { cn } from "@/lib/utils";

type RootLayoutProps = {
  children: React.ReactNode;
};

export default function RootLayout({ children }: RootLayoutProps) {
  return (
    <html lang="en" className="h-full min-h-dvh" suppressHydrationWarning>
      <head />
      <body
        className={cn(
          inter.className,
          "h-full min-h-dvh bg-gradient-to-br from-background via-background to-muted/20"
        )}
      >
        <Providers>{children}</Providers>
        <Toaster />
      </body>
    </html>
  );
}
