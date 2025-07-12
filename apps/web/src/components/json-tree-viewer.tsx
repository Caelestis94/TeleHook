"use client";

import React from "react";
import JsonView from "@uiw/react-json-view";
import { useTheme } from "next-themes";
import { githubLightTheme } from "@uiw/react-json-view/githubLight";
import { nordTheme } from "@uiw/react-json-view/nord";

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
  githubLightTheme.backgroundColor = "transparent";
  nordTheme.backgroundColor = "transparent";

  const jsonViewTheme = theme === "dark" ? nordTheme : githubLightTheme;

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
