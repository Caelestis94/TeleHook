// components/telegram-bots/BotFormDialog.tsx
import { useEffect, forwardRef, useImperativeHandle } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { BotValidationSchema } from "@/validation/bot-schema";
import { BotFormData } from "@/types/bot";
import { mapApiErrorsToFields } from "@/validation/utils";
import { AppError } from "@/lib/error-handling";
import { toast } from "sonner";
import { BotIcon } from "lucide-react";

export interface BotFormDialogRef {
  setError: (field: keyof BotFormData, message: string) => void;
  reset: () => void;
}

interface BotFormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title?: string;
  description?: string;
  defaultValues?: BotFormData;
  onSubmit?: (data: BotFormData) => Promise<void>;
  submitLabel?: string;
  isSubmitting?: boolean;
}

export const BotFormDialog = forwardRef<BotFormDialogRef, BotFormDialogProps>(
  (
    {
      open,
      onOpenChange,
      title = "Create Bot",
      description = "Configure your Telegram bot settings",
      defaultValues = { name: "", botToken: "", chatId: "" },
      onSubmit = async () => {},
      submitLabel,
      isSubmitting = false,
    },
    ref
  ) => {
    const form = useForm<BotFormData>({
      resolver: zodResolver(BotValidationSchema),
      defaultValues,
      mode: "onChange",
    });

    useImperativeHandle(ref, () => ({
      setError: (field: keyof BotFormData, message: string) => {
        form.setError(field, { message });
      },
      reset: () => form.reset(),
    }));

    // Reset form when dialog opens
    useEffect(() => {
      if (open) {
        form.reset(defaultValues);
      } else {
        // Clear form when dialog closes
        form.reset({ name: "", botToken: "", chatId: "" });
      }
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [open]);

    const handleSubmit = async (data: BotFormData) => {
      try {
        await onSubmit(data);
        form.reset();
      } catch (error) {
        // Handle server validation errors
        if (error instanceof AppError && error.details) {
          const fieldErrors = mapApiErrorsToFields(error.details, {
            name: "name",
            botToken: ["bottoken", "token"],
            chatId: ["chatid", "chat"],
          });

          // Set errors on form fields
          Object.entries(fieldErrors).forEach(([field, messages]) => {
            if (messages && messages.length > 0) {
              form.setError(field as keyof BotFormData, {
                message: messages[0],
              });
            }
          });

          toast.error("Please correct the highlighted fields");
        }
      }
    };
    return (
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <BotIcon className="text-blue-600 dark:text-blue-500" />
              {title}
            </DialogTitle>
            <DialogDescription>{description}</DialogDescription>
          </DialogHeader>

          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(handleSubmit)}
              className="space-y-4"
            >
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Instance name</FormLabel>
                    <FormControl>
                      <Input placeholder="Bot instance name" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="botToken"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Bot Token</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="1234567890:ABCdefGHIjklMNOpqrsTUVwxyz"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="chatId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Chat ID</FormLabel>
                    <FormControl>
                      <Input placeholder="-1001234567890" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </form>
          </Form>
          <DialogFooter>
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              Cancel
            </Button>
            <Button
              onClick={form.handleSubmit(handleSubmit)}
              disabled={
                !form.formState.isValid ||
                form.formState.isSubmitting ||
                isSubmitting
              }
            >
              {form.formState.isSubmitting || isSubmitting
                ? "Saving..."
                : submitLabel}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    );
  }
);

BotFormDialog.displayName = "BotFormDialog";
