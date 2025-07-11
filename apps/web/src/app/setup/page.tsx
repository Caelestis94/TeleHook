import { Metadata } from "next";
import { SetupPageClient } from "./setup-page-client";

export const metadata: Metadata = {
  title: "Setup | TeleHook",
  description: "Complete your account setup to start using TeleHook",
};

export default function SetupPage() {
  return <SetupPageClient />;
}
