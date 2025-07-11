import { Metadata } from "next";
import { WebhooksPageClient } from "./webhooks-page-client";

export const metadata: Metadata = {
  title: "Webhooks | TeleHook",
  description: "Manage your webhook endpoints and configurations",
};

export default function WebhooksPage() {
  return <WebhooksPageClient />;
}
