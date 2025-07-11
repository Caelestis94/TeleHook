"use client";

import { useState } from "react";
import { usePathname } from "next/navigation";
import { useSession, signOut } from "next-auth/react";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { ModeToggle } from "@/components/ui/mode-toggle";
import { Activity, Settings, MessageSquare, Bot, Menu } from "lucide-react";
import {
  DashboardAvatarDropdownMenu,
  DashboardSidebarContent,
} from "@/app/dashboard/";

const navigation = [
  { name: "Overview", href: "/dashboard", icon: Activity },
  { name: "Webhooks", href: "/dashboard/webhooks", icon: MessageSquare },
  { name: "Bots", href: "/dashboard/bots", icon: Bot },
  { name: "Logs", href: "/dashboard/logs", icon: Activity },
  { name: "Settings", href: "/dashboard/settings", icon: Settings },
];

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const pathname = usePathname();
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { data: session } = useSession();

  return (
    <div className="min-h-dvh flex bg-gradient-to-br from-background via-background to-muted/20">
      {/* Mobile sidebar */}
      <div
        className={cn(
          "fixed inset-0 z-50 lg:hidden",
          sidebarOpen ? "block" : "hidden"
        )}
      >
        <div
          className="fixed inset-0 bg-black/50"
          onClick={() => setSidebarOpen(false)}
        />
        <div className="fixed left-0 top-0 h-full w-64 bg-sidebar shadow-lg">
          <DashboardSidebarContent
            navigation={navigation}
            pathname={pathname}
            onClose={() => setSidebarOpen(false)}
            showCloseButton
          />
        </div>
      </div>

      {/* Desktop sidebar */}
      <div className="hidden lg:block w-64 border-r bg-sidebar">
        <DashboardSidebarContent navigation={navigation} pathname={pathname} />
      </div>

      {/* Main content */}
      <div className="flex-1 flex flex-col min-h-dvh">
        {/* Header */}
        <header className="border-b px-4 lg:px-6 py-4 pt-5 bg-sidebar">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-4">
              <Button
                variant="ghost"
                size="sm"
                className="lg:hidden"
                onClick={() => setSidebarOpen(true)}
              >
                <Menu className="h-5 w-5" />
              </Button>
              <h1 className="text-2xl font-semibold">
                {navigation.find((item) => item.href === pathname)?.name ||
                  "TeleHook"}
              </h1>
            </div>

            <div className="flex items-center gap-3">
              <ModeToggle />
              <DashboardAvatarDropdownMenu
                session={session}
                signOut={signOut}
              />
            </div>
          </div>
        </header>

        {/* Page content */}
        <main className="flex-1 p-4 lg:p-6 bg-background dark:bg-background/50">
          {children}
        </main>

        {/* Footer */}
        <footer className="border-t px-4 lg:px-6 py-3 bg-sidebar mt-auto">
          <div className="flex items-center justify-between text-sm">
            <span>v1.0 - Commit 393822</span>
          </div>
        </footer>
      </div>
    </div>
  );
}
