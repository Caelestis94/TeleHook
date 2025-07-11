"use client";

import { useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import {
  Drawer,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { HelpCircle, Copy, Check } from "lucide-react";
import { toast } from "sonner";
import { useMediaQuery } from "@/hooks/useMediaQuery";
import { helpData, dateFormatPatterns, tabOptions } from "@/data";
interface WebhookHelpDialogProps {
  children?: React.ReactNode;
}

export function WebhookHelpDialog({ children }: WebhookHelpDialogProps) {
  const [copiedCode, setCopiedCode] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState("basics");
  const isMobile = useMediaQuery("(max-width: 768px)");

  const handleCopyCode = async (code: string) => {
    try {
      await navigator.clipboard.writeText(code);
      setCopiedCode(code);
      setTimeout(() => setCopiedCode(null), 2000);
      toast.success("Code copied to clipboard");
    } catch {
      toast.error("Failed to copy to clipboard");
    }
  };

  const CodeBlock = ({
    code,
    description,
  }: {
    code: string;
    description?: string;
  }) => (
    <div className="bg-muted rounded-md p-2 sm:p-3 font-mono text-xs sm:text-sm relative group">
      <code className="select-all break-all">{code}</code>
      {description && (
        <div className="text-xs text-muted-foreground mt-1">{description}</div>
      )}
      <Button
        variant="ghost"
        size="sm"
        className="absolute top-1 right-1 h-6 w-6 p-0 opacity-70 sm:opacity-0 sm:group-hover:opacity-100 transition-opacity"
        onClick={() => handleCopyCode(code)}
      >
        {copiedCode === code ? (
          <Check className="h-3 w-3" />
        ) : (
          <Copy className="h-3 w-3" />
        )}
      </Button>
    </div>
  );

  const SectionBlock = ({
    title,
    description,
    examples,
  }: {
    title: string;
    description: string;
    examples: Array<{ code: string; description?: string }>;
  }) => (
    <div className="space-y-2">
      <h4 className="font-medium text-sm sm:text-base">{title}</h4>
      {description && (
        <p className="text-xs sm:text-sm text-muted-foreground">
          {description}
        </p>
      )}
      <div className="space-y-2">
        {examples.map((example, index) => (
          <CodeBlock
            key={index}
            code={example.code}
            description={example.description}
          />
        ))}
      </div>
    </div>
  );

  const ContentComponent = () => (
    <div className="space-y-4">
      {/* Mobile: Dropdown Tab Selector */}
      {isMobile ? (
        <div className="space-y-4">
          <div className="space-y-2">
            <label className="text-sm font-medium text-muted-foreground">
              Select a topic to explore:
            </label>
            <Select value={activeTab} onValueChange={setActiveTab}>
              <SelectTrigger className="w-full">
                <SelectValue>
                  {tabOptions.find((tab) => tab.value === activeTab)?.label}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {tabOptions.map((tab) => (
                  <SelectItem key={tab.value} value={tab.value}>
                    {tab.label}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {/* Mobile Content */}
          <div className="space-y-4 max-h-[60vh] overflow-y-auto">
            {activeTab === "basics" && (
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base">Basic Syntax</CardTitle>
                  <CardDescription className="text-sm">
                    Fundamental Scriban template syntax
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {helpData.basics?.map((section, index) => (
                    <SectionBlock
                      key={index}
                      title={section.title}
                      description={section.description}
                      examples={section.examples}
                    />
                  ))}
                </CardContent>
              </Card>
            )}

            {activeTab === "strings" && (
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base">String Functions</CardTitle>
                  <CardDescription className="text-sm">
                    String manipulation and formatting
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {helpData.strings?.map((section, index) => (
                    <SectionBlock
                      key={index}
                      title={section.title}
                      description={section.description}
                      examples={section.examples}
                    />
                  ))}
                </CardContent>
              </Card>
            )}

            {activeTab === "dates" && (
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base">Date Functions</CardTitle>
                  <CardDescription className="text-sm">
                    Date formatting and manipulation
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {helpData.dates?.map((section, index) => (
                    <SectionBlock
                      key={index}
                      title={section.title}
                      description={section.description}
                      examples={section.examples}
                    />
                  ))}

                  <div>
                    <h4 className="font-medium mb-2 text-sm">
                      Common Format Patterns
                    </h4>
                    <div className="text-xs space-y-1">
                      {dateFormatPatterns.map((pattern, index) => (
                        <div
                          key={index}
                          className="flex flex-wrap items-center gap-2"
                        >
                          <Badge variant="outline" className="text-xs">
                            {pattern.pattern}
                          </Badge>
                          <span className="text-xs">-</span>
                          <span className="text-xs">{pattern.description}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                </CardContent>
              </Card>
            )}

            {activeTab === "arrays" && (
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base">Array Functions</CardTitle>
                  <CardDescription className="text-sm">
                    Array manipulation and processing
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {helpData.arrays?.map((section, index) => (
                    <SectionBlock
                      key={index}
                      title={section.title}
                      description={section.description}
                      examples={section.examples}
                    />
                  ))}
                </CardContent>
              </Card>
            )}

            {activeTab === "examples" && (
              <Card>
                <CardHeader className="pb-3">
                  <CardTitle className="text-base">
                    Real-World Examples
                  </CardTitle>
                  <CardDescription className="text-sm">
                    Common webhook template patterns
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-4">
                  {helpData.examples?.map((section, index) => (
                    <SectionBlock
                      key={index}
                      title={section.title}
                      description={section.description}
                      examples={section.examples}
                    />
                  ))}
                </CardContent>
              </Card>
            )}
          </div>
        </div>
      ) : (
        /* Desktop: Regular Tabs */
        <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
          <TabsList className="grid w-full grid-cols-5">
            <TabsTrigger value="basics">Basics</TabsTrigger>
            <TabsTrigger value="strings">Strings</TabsTrigger>
            <TabsTrigger value="dates">Dates</TabsTrigger>
            <TabsTrigger value="arrays">Arrays</TabsTrigger>
            <TabsTrigger value="examples">Examples</TabsTrigger>
          </TabsList>

          <TabsContent value="basics" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Basic Syntax</CardTitle>
                <CardDescription>
                  Fundamental Scriban template syntax
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {helpData.basics?.map((section, index) => (
                  <SectionBlock
                    key={index}
                    title={section.title}
                    description={section.description}
                    examples={section.examples}
                  />
                ))}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="strings" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>String Functions</CardTitle>
                <CardDescription>
                  String manipulation and formatting
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {helpData.strings?.map((section, index) => (
                  <SectionBlock
                    key={index}
                    title={section.title}
                    description={section.description}
                    examples={section.examples}
                  />
                ))}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="dates" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Date Functions</CardTitle>
                <CardDescription>
                  Date formatting and manipulation
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {helpData.dates?.map((section, index) => (
                  <SectionBlock
                    key={index}
                    title={section.title}
                    description={section.description}
                    examples={section.examples}
                  />
                ))}

                <div>
                  <h4 className="font-medium mb-2">Common Format Patterns</h4>
                  <div className="text-sm space-y-1">
                    {dateFormatPatterns.map((pattern, index) => (
                      <div key={index}>
                        <Badge variant="outline">{pattern.pattern}</Badge> -{" "}
                        {pattern.description}
                      </div>
                    ))}
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="arrays" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Array Functions</CardTitle>
                <CardDescription>
                  Array manipulation and processing
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {helpData.arrays?.map((section, index) => (
                  <SectionBlock
                    key={index}
                    title={section.title}
                    description={section.description}
                    examples={section.examples}
                  />
                ))}
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="examples" className="space-y-4">
            <Card>
              <CardHeader>
                <CardTitle>Real-World Examples</CardTitle>
                <CardDescription>
                  Common webhook template patterns
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {helpData.examples?.map((section, index) => (
                  <SectionBlock
                    key={index}
                    title={section.title}
                    description={section.description}
                    examples={section.examples}
                  />
                ))}
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>
      )}
    </div>
  );

  if (isMobile) {
    return (
      <Drawer>
        <DrawerTrigger asChild>
          {children || (
            <Button variant="outline" size="sm">
              <HelpCircle className="h-4 w-4 mr-2" />
              Syntax Help
            </Button>
          )}
        </DrawerTrigger>
        <DrawerContent className="max-h-[90vh]">
          <DrawerHeader className="text-left">
            <DrawerTitle>Scriban Syntax Guide</DrawerTitle>
            <DrawerDescription>
              Complete reference for Scriban templating syntax and functions
            </DrawerDescription>
          </DrawerHeader>
          <div className="px-4 pb-4 overflow-hidden">
            <ContentComponent />
          </div>
        </DrawerContent>
      </Drawer>
    );
  }

  return (
    <Dialog>
      <DialogTrigger asChild>
        {children || (
          <Button variant="outline" size="sm">
            <HelpCircle className="h-4 w-4 mr-2" />
            Syntax Help
          </Button>
        )}
      </DialogTrigger>
      <DialogContent className="max-w-4xl h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Scriban Syntax Guide</DialogTitle>
          <DialogDescription>
            Complete reference for Scriban templating syntax and functions
          </DialogDescription>
        </DialogHeader>
        <ContentComponent />
      </DialogContent>
    </Dialog>
  );
}
