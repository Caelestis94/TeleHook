import React from "react";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import {
  User,
  Settings,
  Webhook,
  Bot,
  LogOut,
  ChevronDown,
} from "lucide-react";
import { Session } from "next-auth";

interface DashboardAvatarDropdownMenuProps {
  session: Session | null;
  signOut: (session: { callbackUrl: string }) => void;
}

export const DashboardAvatarDropdownMenu = ({
  session,
  signOut,
}: DashboardAvatarDropdownMenuProps) => {
  const handleSignOut = () => {
    signOut({
      callbackUrl: "/auth/signin",
    });
  };

  const menuItems = [
    {
      label: "Dashboard",
      href: "/dashboard",
      icon: <User className="w-4 h-4" />,
    },
    {
      label: "Webhooks",
      href: "/dashboard/webhooks",
      icon: <Webhook className="w-4 h-4" />,
    },
    {
      label: "Bots",
      href: "/dashboard/bots",
      icon: <Bot className="w-4 h-4" />,
    },
    {
      label: "Settings",
      href: "/dashboard/settings",
      icon: <Settings className="w-4 h-4" />,
    },
  ];

  const userInitials = session?.user?.displayName
    ? session.user.displayName
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
    : "U";

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <div className="flex items-center gap-2 cursor-pointer hover:opacity-80 transition-opacity">
          <Avatar className="w-8 h-8">
            <AvatarImage src={session?.user?.image} />
            <AvatarFallback className="bg-primary text-primary-foreground font-medium">
              {userInitials}
            </AvatarFallback>
          </Avatar>
          <ChevronDown className="w-4 h-4 text-muted-foreground" />
        </div>
      </DropdownMenuTrigger>

      <DropdownMenuContent className="w-56" align="end" forceMount>
        <DropdownMenuLabel className="font-normal">
          <div className="flex flex-col space-y-1">
            <p className="text-sm font-medium leading-none">
              {session?.user?.displayName || "User"}
            </p>
            <p className="text-xs leading-none text-muted-foreground">
              {session?.user?.email || "user@example.com"}
            </p>
          </div>
        </DropdownMenuLabel>

        <DropdownMenuSeparator />

        {menuItems.map((item) => (
          <DropdownMenuItem key={item.href} asChild>
            <a
              href={item.href}
              className="flex items-center gap-2 cursor-pointer"
            >
              {item.icon}
              <span>{item.label}</span>
            </a>
          </DropdownMenuItem>
        ))}

        <DropdownMenuSeparator />

        <DropdownMenuItem
          onClick={handleSignOut}
          className="flex items-center gap-2 cursor-pointer *:hover:text-red-500 "
        >
          <LogOut className="w-4 h-4" />
          <span>Log out</span>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
};
