import { OctagonAlert } from "lucide-react";
import { WebhookAuthMethodsDisplay } from "@/app/dashboard/webhooks/config/";
import { useMediaQuery } from "@/hooks/useMediaQuery";

interface SecretKeyInstructionsProps {
  type: "new" | "edit" | "generated";
  secretKey?: string;
  showKey?: boolean;
}

export function WebhookSecretKeyInstructions({
  type,
  secretKey = "",
  showKey = false,
}: SecretKeyInstructionsProps) {
  const isMobile = useMediaQuery("(max-width: 768px)");

  if (type === "new") {
    return (
      <div className="text-sm text-muted-foreground space-y-3">
        <p>
          <strong>Important:</strong> Generate a secret key to protect your
          webhook endpoint.
        </p>
        <WebhookAuthMethodsDisplay
          secretKey="YOUR_SECRET_KEY"
          showKey={true}
          isMobile={isMobile}
        />
      </div>
    );
  }

  if (type === "edit") {
    return (
      <div className="text-sm text-muted-foreground space-y-1">
        <p>
          <strong>Security Notice:</strong> Existing secret keys are permanently
          hidden and cannot be viewed again.
        </p>
        <p>
          If you need to see the key, generate a new one. The old key will be
          immediately invalidated.
        </p>
      </div>
    );
  }

  if (type === "generated" && secretKey) {
    return (
      <div className="text-sm space-y-3">
        <div className="flex items-start gap-2 border border-yellow-200 dark:border-yellow-800 bg-yellow-50 dark:bg-yellow-900/20 p-3 rounded-md">
          <OctagonAlert
            className="dark:text-yellow-500 text-yellow-700 mt-0.5 flex-shrink-0"
            size={16}
          />
          <div>
            <span className="font-bold">Save this key now!</span> You won&apos;t
            be able to see it again after leaving this page.
          </div>
        </div>
        <WebhookAuthMethodsDisplay
          secretKey={secretKey}
          showKey={showKey}
          isMobile={isMobile}
        />
      </div>
    );
  }

  return null;
}
