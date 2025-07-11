import { Metadata } from "next";
import { SettingsPageClient } from "./settings-page-client";

export const metadata: Metadata = {
  title: "Settings | TeleHook",
  description: "Configure your application preferences and account settings",
};

export default function SettingsPage() {
  return <SettingsPageClient />;
}
