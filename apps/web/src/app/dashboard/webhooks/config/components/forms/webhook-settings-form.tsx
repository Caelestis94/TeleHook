"use client";

import { useState } from "react";
import { useFormContext } from "react-hook-form";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  FormField,
  FormItem,
  FormLabel,
  FormControl,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Copy, Key, Eye, EyeOff, Info, Target, Shield } from "lucide-react";
import { toast } from "sonner";
import { useBots } from "@/hooks/queries/useBots";
import { useGenerateSecretKey } from "@/hooks/mutations/useWebhookMutations";
import type { WebhookFormData } from "@/types/webhook";
import { Separator } from "@/components/ui/separator";

import { cn } from "@/lib/utils";
import { WebhookSecretKeyInstructions } from "@/app/dashboard/webhooks/config/";

interface WebhookSettingsFormProps {
  isEdit?: boolean;
  isLoading?: boolean;
}

export function WebhookSettingsForm({
  isEdit = false,
}: WebhookSettingsFormProps) {
  const form = useFormContext<WebhookFormData>();
  const [newlyGeneratedKey, setNewlyGeneratedKey] = useState<string | null>(
    null
  );
  const [showKey, setShowKey] = useState(false);

  // TanStack Query hooks
  const { data: bots = [], isLoading: botsLoading } = useBots();
  const generateKeyMutation = useGenerateSecretKey();

  const isProtected = form.watch("isProtected");
  const secretKey = form.watch("secretKey");

  const handleGenerateKey = () => {
    generateKeyMutation.mutate(undefined, {
      onSuccess: (data) => {
        setNewlyGeneratedKey(data.secretKey);
        form.setValue("secretKey", data.secretKey);
        setShowKey(true);
        toast.success("New secret key generated");
      },
    });
  };

  const copySecretKey = () => {
    const keyToCopy = newlyGeneratedKey || secretKey;
    if (keyToCopy && typeof navigator !== "undefined" && navigator.clipboard) {
      navigator.clipboard.writeText(keyToCopy);
      toast.success("Secret key copied to clipboard");
    }
  };

  const getDisplayKey = () => {
    if (newlyGeneratedKey) {
      return showKey ? newlyGeneratedKey : "*".repeat(newlyGeneratedKey.length);
    }
    if (isEdit && secretKey) {
      // In edit mode, existing keys are always hidden and cannot be revealed
      return "*".repeat(32); // Show a fixed number of asterisks
    }
    return "";
  };
  return (
    <div className="space-y-6">
      {/* Basic Information */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Info className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
            Basic Configuration
          </CardTitle>
          <CardDescription>
            Configure the fundamental settings for your webhook endpoint
          </CardDescription>
          <Separator />
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Webhook Name */}
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Webhook Name</FormLabel>
                <FormControl>
                  <Input
                    {...field}
                    placeholder="My Service Webhook"
                    autoComplete="off"
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </CardContent>
      </Card>

      {/* Target Configuration */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Target className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
            Target Configuration
          </CardTitle>
          <CardDescription>
            Select the bot for this webhook and the topic ID (optional)
          </CardDescription>
          <Separator />
        </CardHeader>
        <CardContent className=" grid grid-cols-1 lg:grid-cols-2 item-center justify-between gap-2 space-y-6">
          {/* Bot Selection */}
          <FormField
            control={form.control}
            name="botId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Telegram Bot</FormLabel>
                <Select
                  key={`bot-${bots.length}-${field.value}`}
                  disabled={botsLoading}
                  onValueChange={(value) => field.onChange(parseInt(value))}
                  value={field.value ? field.value.toString() : undefined}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select a bot" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {bots.map((bot) => (
                      <SelectItem key={bot.id} value={bot.id.toString()}>
                        {bot.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
          {/* Topic ID */}
          <FormField
            control={form.control}
            name="topicId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Topic ID (Optional)</FormLabel>
                <FormControl className="w-max-[10px]">
                  <Input {...field} placeholder="123456" autoComplete="off" />
                </FormControl>
                <FormMessage />
                <p className="text-sm text-muted-foreground">
                  Leave empty to send to the main chat. For specific topics,
                  copy the message link and use the second number in the URL.
                </p>
              </FormItem>
            )}
          />
        </CardContent>
      </Card>

      {/* Security Settings */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Shield className="h-4 w-4 sm:h-5 sm:w-5 text-green-600 dark:text-green-400" />
            Security Settings
          </CardTitle>
          <CardDescription>
            Configure access protection and webhook status
          </CardDescription>
          <Separator />
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Protected Toggle */}
          <FormField
            control={form.control}
            name="isProtected"
            render={({ field }) => (
              <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                <div className="space-y-0.5">
                  <FormLabel className="text-base">Protected Webhook</FormLabel>
                  <FormMessage />
                  <p className="text-sm text-muted-foreground">
                    Require Bearer token authentication for incoming requests
                  </p>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
              </FormItem>
            )}
          />

          {/* Secret Key Management */}
          {isProtected && (
            <div className="space-y-4">
              <FormField
                control={form.control}
                name="secretKey"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Secret Key</FormLabel>
                    <div className="flex gap-2">
                      <FormControl>
                        <div className="relative flex-1">
                          <Input
                            {...field}
                            value={getDisplayKey()}
                            readOnly
                            placeholder="Generate a secret key"
                            className={newlyGeneratedKey ? "pr-20" : ""}
                          />
                          {newlyGeneratedKey && (
                            <div className="absolute right-2 top-1/2 -translate-y-1/2 flex gap-1">
                              <Button
                                type="button"
                                variant="ghost"
                                size="sm"
                                className="h-6 w-6 p-0"
                                onClick={() => setShowKey(!showKey)}
                              >
                                {showKey ? (
                                  <EyeOff className="h-3 w-3" />
                                ) : (
                                  <Eye className="h-3 w-3" />
                                )}
                              </Button>
                              <Button
                                type="button"
                                variant="ghost"
                                size="sm"
                                className="h-6 w-6 p-0"
                                onClick={copySecretKey}
                              >
                                <Copy className="h-3 w-3" />
                              </Button>
                            </div>
                          )}
                        </div>
                      </FormControl>
                      <Button
                        type="button"
                        variant="outline"
                        onClick={handleGenerateKey}
                        disabled={generateKeyMutation.isPending}
                        className={cn(
                          "shrink-0",
                          `${secretKey == "" ? "animate-pulse border-2" : ""}`
                        )}
                      >
                        <Key className="w-4 h-4 mr-2" />
                        {generateKeyMutation.isPending
                          ? "Generating..."
                          : isEdit
                          ? "Generate New"
                          : "Generate"}
                      </Button>
                    </div>
                    <FormMessage />

                    {/* Instructions */}
                    {!isEdit && !newlyGeneratedKey && (
                      <WebhookSecretKeyInstructions type="new" />
                    )}

                    {isEdit && secretKey && !newlyGeneratedKey && (
                      <WebhookSecretKeyInstructions type="edit" />
                    )}

                    {newlyGeneratedKey && (
                      <WebhookSecretKeyInstructions
                        type="generated"
                        secretKey={newlyGeneratedKey}
                        showKey={showKey}
                      />
                    )}
                  </FormItem>
                )}
              />
            </div>
          )}

          {/* Disabled Toggle */}
          <FormField
            control={form.control}
            name="isDisabled"
            render={({ field }) => (
              <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                <div className="space-y-0.5">
                  <FormLabel className="text-base">Disable Webhook</FormLabel>
                  <FormMessage />
                  <p className="text-sm text-muted-foreground">
                    Temporarily disable this webhook without deleting it
                  </p>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
              </FormItem>
            )}
          />
        </CardContent>
      </Card>
    </div>
  );
}
