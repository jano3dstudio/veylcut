# Components and provenance

- JANO App-Kit 0.2.0, Jona's existing local kit. Pinned copies: `ui/kit/tokens.css`, `jano.css`, `jano.js`, and `native/Tokens.cs`, `Theme.cs`, `JanoWindow.cs`. Source in this workspace: `../jano-app-kit`. Window/UI direction preserved; this prototype has not been visually approved by Jona yet.
- Microsoft WebView2 SDK 1.0.2903.40. DLLs and original Microsoft license included as EXE resources and in the local build dependency folder. WebView2 Runtime is supplied by the installed Microsoft runtime, not bundled here.
- FFmpeg 8.1.1 Gyan full build, existing local installation. Invoked as a separate process; binaries are not distributed with this prototype. License/source details belong to the installed distribution. This application does not silently download another build.
- Windows system fonts are referenced by name, not redistributed. User-imported fonts/music remain local user assets.
- All SVG icon and built-in overlay geometry were authored for this project. Demo video/audio are synthetic FFmpeg test sources.

Implementation references checked 2026-09-18:
- https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/working-with-local-content
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2environment.createwebresourceresponse
- https://github.com/MicrosoftEdge/WebView2Feedback/issues/4201

The initial dynamic virtual-host media route failed. Local content now uses an intercepted same-origin resource route with explicit MIME types, bounded streams and byte-range responses. Registered media IDs constrain which external files can be read. No local HTTP listener or arbitrary path endpoint is opened.
