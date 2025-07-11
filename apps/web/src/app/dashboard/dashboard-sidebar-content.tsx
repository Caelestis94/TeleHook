import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";
import { X } from "lucide-react";
import Link from "next/link";

type Navigation = {
  name: string;
  href: string;
  icon: React.ComponentType<React.SVGProps<SVGSVGElement>>;
};
const getActiveSideBarItemClass = (pathname: string, href: string) => {
  // Check if we're on the exact page or a subpage
  const isActive =
    href === "/dashboard"
      ? pathname === "/dashboard"
      : pathname.startsWith(href);

  switch (href) {
    case "/dashboard/bots":
      return isActive
        ? "text-blue-600 dark:text-blue-400"
        : "text-muted-foreground hover:text-blue-600 dark:hover:text-blue-400";
    case "/dashboard/webhooks":
      return isActive
        ? "text-green-600 dark:text-green-400"
        : "text-muted-foreground hover:text-green-600 dark:hover:text-green-400";
    case "/dashboard/logs":
      return isActive
        ? "text-red-600 dark:text-red-400"
        : "text-muted-foreground hover:text-red-600 dark:hover:text-red-400";
    case "/dashboard/settings":
      return isActive
        ? "text-yellow-600 dark:text-yellow-400"
        : "text-muted-foreground hover:text-yellow-600 dark:hover:text-yellow-400";
    default:
      return isActive
        ? "text-orange-600 dark:text-orange-400"
        : "text-muted-foreground hover:text-orange-600 dark:hover:text-orange-400";
  }
};

export function DashboardSidebarContent({
  navigation,
  pathname,
  onClose,
  showCloseButton = false,
}: {
  navigation: Navigation[];
  pathname: string;
  onClose?: () => void;
  showCloseButton?: boolean;
}) {
  return (
    <div className="flex flex-col h-full">
      {/* Logo/Header */}
      <div className="flex items-center justify-between p-5 border-b text-foreground">
        <div className="flex items-center gap-3">
          <img
            src="/logo_light.png"
            alt="TeleHook"
            width={120}
            height={32}
            className="h-8 w-auto dark:hidden"
          />
          <img
            src="/logo_dark.png"
            alt="TeleHook"
            width={120}
            height={32}
            className="h-8 w-auto hidden dark:block"
          />
          <h1 className="text-xl font-light">
            Tele<span className="font-semibold">Hook</span>
          </h1>
        </div>
        {showCloseButton && (
          <Button variant="ghost" size="sm" onClick={onClose}>
            <X className="h-5 w-5" />
          </Button>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 p-4">
        <ul className="space-y-1">
          {navigation.map((item) => {
            const isActive =
              item.href === "/dashboard"
                ? pathname === "/dashboard"
                : pathname.startsWith(item.href);
            const Icon = item.icon;

            return (
              <li key={item.name}>
                <Link
                  href={item.href}
                  onClick={onClose}
                  className={cn(
                    "flex items-center gap-3 px-3 py-2 rounded-md text-sm font-medium transition-colors",
                    isActive
                      ? "bg-accent text-accent-foreground inset-shadow-2xs dark:inset-shadow-stone-950"
                      : " text-muted-foreground hover:bg-accent hover:text-foreground dark:hover:text-white",
                    getActiveSideBarItemClass(pathname, item.href)
                  )}
                >
                  <Icon className="h-5 w-5 bg-" />
                  {item.name}
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>
    </div>
  );
}
