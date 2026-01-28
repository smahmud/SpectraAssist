# SpectraAssist Repo: CortexView App

CortexView (SpectraAssist) is a native .NET 6+ WPF “overlay assistant” that floats above other Windows applications, periodically captures the visible content of a chosen window/screen, and sends that context to AWS Bedrock models to surface intelligent, non-intrusive suggestions in an always-on-top panel. The strategy emphasizes efficient change detection to avoid redundant AI calls, strong privacy controls around screenshot capture and storage, and a modular architecture separating overlay UI, capture, change detection, storage, and Bedrock integration.​

## Strategy and Vision
The overall vision is an “Advanced Windows Overlay Assistant App” that anchors in the top-right ~25% of the active workspace, continuously monitors what the user is doing, and offers context-aware help without disrupting the underlying application. It focuses on: robust screen/window enumeration, change-aware screenshot capture, Bedrock-only inference for runtime intelligence, and user controls (manual triggers, sensitivity, storage, privacy) that keep the user in full control.​

## Architecture

The architecture is explicitly modular: separate components for screen/window detection, screenshot capture, change detection, overlay UI, configuration, Bedrock integration, and optional local storage. The strategy also defines a milestone roadmap from initial overlay bootstrap through change detection, overlay UX, Bedrock stubs, storage/privacy, refactoring, and final integration, all implemented as a native WPF app (no Electron) with AWS as the sole AI backend.

## 📄 License

This project is licensed under  
**Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International (CC BY-NC-SA 4.0)**

You may:
- Share and adapt the material with attribution  
- Not use it for commercial purposes  
- Not use it for training machine learning models (including LLMs) without explicit permission  

See [LICENSE.md](../LICENSE.md) for full legal terms.  
Full license text: [https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode](https://creativecommons.org/licenses/by-nc-sa/4.0/legalcode)