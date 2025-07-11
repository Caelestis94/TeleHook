import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { Webhook } from "@/types/webhook";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Filter } from "lucide-react";
import { LogFilters } from "@/types/log";

interface LogsFiltersProps {
  filters: LogFilters;
  onFiltersChange: (filters: LogFilters) => void;
  webhooks: Webhook[];
  onApplyFilters?: () => void; // Made optional for real-time filtering
  onClearFilters: () => void;
}

export function LogsFilters({
  filters,
  onFiltersChange,
  webhooks,
  onApplyFilters,
  onClearFilters,
}: LogsFiltersProps) {
  const updateFilter = (key: keyof LogFilters, value: string) => {
    onFiltersChange({
      ...filters,
      [key]: value,
    });
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <Filter className="w-4 h-4" />
          <Label className="font-medium">Filters</Label>
        </CardTitle>
        <CardDescription>
          Use the filters below to narrow down webhook logs by endpoint, status,
          date range, and search terms.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
          {/* Endpoint Filter */}
          <div>
            <Label htmlFor="endpoint" className="mb-2">
              Endpoint
            </Label>
            <Select
              value={filters.webhookId || "all"}
              onValueChange={(value) =>
                updateFilter("webhookId", value === "all" ? "" : value)
              }
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder="All webhooks" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All webhooks</SelectItem>
                {webhooks.map((endpoint) => (
                  <SelectItem key={endpoint.id} value={endpoint.id.toString()}>
                    {endpoint.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Status Filter */}
          <div>
            <Label htmlFor="status" className="mb-2">
              Status
            </Label>
            <Select
              value={filters.statusCode || "all"}
              onValueChange={(value) =>
                updateFilter("statusCode", value === "all" ? "" : value)
              }
            >
              <SelectTrigger className="w-full">
                <SelectValue placeholder="All statuses" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                <SelectItem value="200">Success (2xx)</SelectItem>
                <SelectItem value="400">Client Error (4xx)</SelectItem>
                <SelectItem value="500">Server Error (5xx)</SelectItem>
              </SelectContent>
            </Select>
          </div>

          {/* Date Range Filter */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <div className="">
              <Label htmlFor="dateFrom" className="mb-2">
                From Date
              </Label>
              <Input
                id="dateFrom"
                type="date"
                value={filters.dateFrom}
                onChange={(e) => updateFilter("dateFrom", e.target.value)}
              />
            </div>

            <div className="">
              <Label htmlFor="dateTo" className="mb-2">
                To Date
              </Label>
              <Input
                id="dateTo"
                type="date"
                value={filters.dateTo}
                onChange={(e) => updateFilter("dateTo", e.target.value)}
              />
            </div>
          </div>

          {/* Search Filter */}
          <div>
            <Label htmlFor="search" className="mb-2">
              Search
            </Label>
            <Input
              id="search"
              placeholder="Request ID, message..."
              value={filters.searchTerm}
              onChange={(e) => updateFilter("searchTerm", e.target.value)}
            />
          </div>
        </div>
      </CardContent>
      <CardFooter>
        <div className="flex gap-2">
          {onApplyFilters && (
            <Button onClick={onApplyFilters} variant="outline" size="sm">
              Apply Filters
            </Button>
          )}
          <Button onClick={onClearFilters} variant="ghost" size="sm">
            Clear All
          </Button>
        </div>
      </CardFooter>
    </Card>
  );
}
