"use client";

import {
  useEffect,
  useRef,
  useState,
  useImperativeHandle,
  forwardRef,
} from "react";
// import type { IDisposable } from "monaco-editor";
import type { EditorProps, Monaco } from "@monaco-editor/react";

import {
  BaseMonacoEditor,
  registerScribanLanguage,
  createScribanCompletionProvider,
  type BaseMonacoEditorRef,
} from "@/app/dashboard/webhooks/config/";
import { editor, type IDisposable } from "monaco-editor";

interface Variable {
  name: string;
  type: string;
  isArray: boolean;
}

type WebhookScribanEditorProps = EditorProps & {
  variables?: Variable[];
  sampleData?: Record<string, unknown>;
  readOnly?: boolean;
};

export interface WebhookScribanEditorRef {
  insertAtCursor: (text: string) => void;
}

export const WebhookScribanEditor = forwardRef<
  WebhookScribanEditorRef,
  WebhookScribanEditorProps
>(
  (
    {
      value,
      onChange,
      variables = [],
      sampleData = {},
      height = 300,
      readOnly = false,
    },
    ref
  ) => {
    const [isReady, setIsReady] = useState(false);
    const completionProviderRef = useRef<IDisposable | null>(null);
    const baseEditorRef = useRef<BaseMonacoEditorRef>(null);

    useImperativeHandle(
      ref,
      () => ({
        insertAtCursor: (text: string) => {
          baseEditorRef.current?.insertAtCursor(text);
        },
      }),
      []
    );

    const handleBeforeMount = (monaco: Monaco) => {
      registerScribanLanguage(monaco);
      setIsReady(true);
    };

    const handleMount = (_: editor.IStandaloneCodeEditor, monaco: Monaco) => {
      // Setup completion provider
      if (completionProviderRef.current) {
        completionProviderRef.current.dispose();
      }
      completionProviderRef.current = createScribanCompletionProvider(
        monaco,
        variables,
        sampleData
      );
    };

    // Update completion provider when variables or sample data change
    useEffect(() => {
      if (isReady && baseEditorRef.current) {
        const monaco = baseEditorRef.current.getMonaco();
        if (monaco && completionProviderRef.current) {
          completionProviderRef.current.dispose();
          completionProviderRef.current = createScribanCompletionProvider(
            monaco,
            variables,
            sampleData
          );
        }
      }
    }, [variables, sampleData, isReady]);

    useEffect(() => {
      return () => {
        if (completionProviderRef.current) {
          completionProviderRef.current.dispose();
        }
      };
    }, []);

    return (
      <BaseMonacoEditor
        ref={baseEditorRef}
        value={value}
        onChange={onChange}
        language="scriban"
        height={height}
        readOnly={readOnly}
        beforeMount={handleBeforeMount}
        onMount={handleMount}
        options={{
          fontSize: 14,
          lineNumbers: "on",
          wrappingIndent: "indent",
          folding: true,
          showFoldingControls: "always",
        }}
      />
    );
  }
);

WebhookScribanEditor.displayName = "ScribanEditor";
