import { Metadata } from "next";
import { BotsPageClient } from "./bots-page-client";

export const metadata: Metadata = {
  title: "Bots | TeleHook",
  description: "Manage your Telegram bots",
};

export default function BotsPage() {
  return <BotsPageClient />;
}
