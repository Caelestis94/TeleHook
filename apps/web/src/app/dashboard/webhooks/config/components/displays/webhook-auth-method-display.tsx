import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface WebhookAuthMethodsDisplayProps {
  secretKey: string;
  showKey?: boolean;
  isMobile?: boolean;
}

export function WebhookAuthMethodsDisplay({
  secretKey,
  showKey = true,
  isMobile = false,
}: WebhookAuthMethodsDisplayProps) {
  const displayKey = showKey ? secretKey : "***";

  if (isMobile) {
    return (
      <div className="space-y-3">
        <div className="space-y-2">
          <div className="font-medium text-sm">Header Method</div>
          <code className="block bg-muted px-2 py-2 rounded text-xs break-all select-all">
            Authorization: Bearer {displayKey}
          </code>
        </div>
        <div className="space-y-2">
          <div className="font-medium text-sm">Query Parameter Method</div>
          <code className="block bg-muted px-2 py-2 rounded text-xs break-all select-all">
            ?secret_key={displayKey}
          </code>
        </div>
      </div>
    );
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead className="w-[120px]">Method</TableHead>
          <TableHead>Usage</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        <TableRow>
          <TableCell className="font-medium">Header</TableCell>
          <TableCell>
            <code className="bg-muted px-2 py-1 rounded text-sm break-all select-all">
              Authorization: Bearer {displayKey}
            </code>
          </TableCell>
        </TableRow>
        <TableRow>
          <TableCell className="font-medium">Query param</TableCell>
          <TableCell>
            <code className="bg-muted px-2 py-1 rounded text-sm break-all select-all">
              ?secret_key={displayKey}
            </code>
          </TableCell>
        </TableRow>
      </TableBody>
    </Table>
  );
}
