import { Metadata } from "next";
import { LogsPageClient } from "./logs-page-client";

export const metadata: Metadata = {
  title: "Webhook Logs | TeleHook",
  description: "Monitor webhook requests and debug delivery issues",
};

export default function LogsPage() {
  return <LogsPageClient />;
}
