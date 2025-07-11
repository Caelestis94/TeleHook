export type HelpExample = {
  code: string;
  description?: string;
};

export type HelpSection = {
  title: string;
  description: string;
  examples: HelpExample[];
};

export type HelpData = {
  basics: HelpSection[];
  strings: HelpSection[];
  dates: HelpSection[];
  arrays: HelpSection[];
  examples: HelpSection[];
};


export type TabValue = (typeof tabOptions)[number]["value"];


export type DateFormatPattern = {
  pattern: string;
  description: string;
}