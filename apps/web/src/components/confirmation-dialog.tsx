import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";

interface ConfirmationDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
  title: string;
  confirmText?: string;
  cancelText?: string;
  variant?: "default" | "destructive";
  children?: React.ReactNode;
  icon?: React.ReactNode;
  isLoading?: boolean;
  isCancelAvailable?: boolean;
  className?: string;
}

export function ConfirmationDialog({
  open,
  onOpenChange,
  onConfirm,
  title,
  isLoading = false,
  confirmText = "Confirm",
  cancelText = "Cancel",
  variant = "default",
  isCancelAvailable = true,
  children,
  className = "",
  icon = null,
}: ConfirmationDialogProps) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className={className}>
        <DialogHeader>
          <DialogTitle>
            <div className="flex items-center gap-2">
              {icon && <span className="text-lg">{icon}</span>}
              {title}
            </div>
          </DialogTitle>
          <DialogDescription asChild>
            <div>{children}</div>
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          {isCancelAvailable && (
            <Button variant="outline" onClick={() => onOpenChange(false)}>
              {cancelText}
            </Button>
          )}
          <Button
            variant={variant === "destructive" ? "destructive" : "default"}
            onClick={onConfirm}
            disabled={isLoading}
          >
            {isLoading ? (
              <span className="flex items-center gap-2">
                <span className="loader" />
                loading...
              </span>
            ) : (
              confirmText
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
