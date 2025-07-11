"use client";

import React from "react";
import JsonView from "@uiw/react-json-view";
import { useTheme } from "next-themes";
import { githubDarkTheme } from "@uiw/react-json-view/githubDark";
import { githubLightTheme } from "@uiw/react-json-view/githubLight";

interface JsonTreeViewerProps {
  data: object;
  className?: string;
  collapsed?: boolean;
  clipboard?: boolean;
}

export function JsonTreeViewer({
  data,
  collapsed = false,
  className,
  clipboard = false,
}: JsonTreeViewerProps) {
  const { theme } = useTheme();

  // transparent background on both themes
  githubDarkTheme.backgroundColor = "transparent";
  githubLightTheme.backgroundColor = "transparent";
  const jsonViewTheme = theme === "dark" ? githubDarkTheme : githubLightTheme;

  return (
    <div className={`relative ${className || ""}`}>
      <JsonView
        value={data}
        displayDataTypes={false}
        displayObjectSize={false}
        enableClipboard={clipboard}
        collapsed={collapsed}
        style={jsonViewTheme}
      />
    </div>
  );
}
