import { Metadata } from "next";
import { SignInPageClient } from "./signin-page-client";

export const metadata: Metadata = {
  title: "Signin | TeleHook",
  description: "Sign in to your TeleHook account",
};

export default function SignInPage() {
  return <SignInPageClient />;
}
