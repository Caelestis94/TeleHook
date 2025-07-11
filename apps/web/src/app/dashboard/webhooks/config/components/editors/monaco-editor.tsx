"use client";

import { useRef, useImperativeHandle, forwardRef } from "react";
import { useTheme } from "next-themes";
import { editor } from "monaco-editor";
import { Editor, type EditorProps, type Monaco } from "@monaco-editor/react";

type BaseMonacoEditorProps = EditorProps & {
  readOnly?: boolean;
  placeholder?: string;
};

export interface BaseMonacoEditorRef {
  insertAtCursor: (text: string) => void;
  getEditor: () => editor.IStandaloneCodeEditor | null;
  getMonaco: () => Monaco | null;
}

export const BaseMonacoEditor = forwardRef<
  BaseMonacoEditorRef,
  BaseMonacoEditorProps
>(
  (
    {
      value,
      onChange,
      language,
      height = 300,
      readOnly = false,
      placeholder,
      options = {},
      onMount,
      beforeMount,
    },
    ref
  ) => {
    const { theme } = useTheme();
    const editorRef = useRef<editor.IStandaloneCodeEditor | null>(null);
    const monacoRef = useRef<Monaco | null>(null);

    useImperativeHandle(
      ref,
      () => ({
        insertAtCursor: (text: string) => {
          if (editorRef.current) {
            const editor = editorRef.current;
            const selection = editor.getSelection();
            const id = { major: 1, minor: 1 };
            const op = {
              identifier: id,
              range: selection,
              text: text,
              forceMoveMarkers: true,
            };
            editor.executeEdits("insertAtCursor", [op]);
            editor.focus();
          }
        },
        getEditor: () => editorRef.current,
        getMonaco: () => monacoRef.current,
      }),
      []
    );

    const handleEditorWillMount = (monaco: Monaco) => {
      monacoRef.current = monaco;
      beforeMount?.(monaco);
    };

    const handleEditorDidMount = (
      editor: editor.IStandaloneCodeEditor,
      monaco: Monaco
    ) => {
      editorRef.current = editor;

      // Default editor options
      editor.updateOptions({
        minimap: { enabled: false },
        scrollBeyondLastLine: false,
        automaticLayout: true,
        wordWrap: "on",
        ...options,
      });

      onMount?.(editor, monaco);
      editor.focus();
    };

    const handleChange = (
      newValue: string | undefined,
      event: editor.IModelContentChangedEvent
    ) => {
      if (newValue !== undefined) {
        onChange(newValue, event);
      }
    };

    return (
      <div className="border rounded-md overflow-hidden">
        <Editor
          height={height}
          language={language}
          theme={theme === "light" ? "vs" : "vs-dark"}
          value={value}
          onChange={handleChange}
          beforeMount={handleEditorWillMount}
          onMount={handleEditorDidMount}
          options={{
            readOnly,
            placeholder,
            selectOnLineNumbers: true,
            roundedSelection: false,
            cursorStyle: "line",
            tabSize: 2,
            insertSpaces: true,
            detectIndentation: false,
            autoIndent: "advanced",
            formatOnType: true,
            formatOnPaste: true,
            autoClosingBrackets: "always",
            autoClosingQuotes: "always",
            autoSurround: "languageDefined",
            smoothScrolling: true,
            ...options,
          }}
        />
      </div>
    );
  }
);

BaseMonacoEditor.displayName = "BaseMonacoEditor";
