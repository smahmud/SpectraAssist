# Milestone 3 Status — CortexView Intelligent Change Detection

### Overview

This document summarizes the completed work for Milestone 3 (v0.3.0) of the SpectraAssist/CortexView WPF application. The goal was to evolve the simple hash-only comparison from Milestone 2 into a configurable, layered change‑detection engine that only triggers analysis on meaningful changes or explicit user requests, while still using a local analysis stub (no AWS Bedrock yet).

---

### Implemented Features

- **Change Sensitivity Slider**

    - Added a **“Change sensitivity (%)”** slider (1–50, default 10) in the top overlay control panel.

    - Slider value is mapped to an internal fraction and used to decide when a capture is treated as a **Significant Change** versus a **Minor Change Below Threshold**.

- **Layered Change‑Detection Engine**

    - Extracted existing SHA1 hash logic into a dedicated `ChangeDetection` helper class, keeping hash comparison as a fast equality guard.

    - Implemented a **downsampled grayscale pixel‑difference** algorithm that computes the percentage of changed pixels between the latest capture and a stored reference frame.

    - Introduced a `ChangeDecision` classification (`NoChange`, `MinorChangeBelowThreshold`, `SignificantChange`) driven by the pixel‑diff percentage and the sensitivity slider.

- **Unified Analysis Stub Flow**

    - Added a unified entry point `RunAnalysisIfNeededAsync` that is called whenever a significant change is detected or a manual override is requested.

    Implemented a local stub `RunLocalAnalysisStubAsync` that updates the AI context area with diagnostic information (trigger reason, estimated change percentage, optional OCR text) instead of calling AWS Bedrock.

    Automatic window captures now route through this analysis stub for **Significant Change** events, providing a clear path for future Bedrock integration.

- **Request New Information Button (Manual Override)**

    - Added a new **“Request New Information”** button in the AI context section, separate from the existing “Next Suggestion” stub.

    - On click, the app captures the selected window and **always** triggers the analysis stub with reason `ManualOverride`, bypassing the sensitivity and hash checks so the user can force analysis on demand.

    -Status text reflects manual requests, showing the estimated changed percentage even when it falls below the configured threshold.

- **OCR Hook (Experimental)**

    - Added a placeholder OCR hook `TryExtractOcrText(Bitmap)` in `ChangeDetection`, currently returning `null` but wired into the analysis stub signature.

    - The stub displays OCR text when available, providing a ready integration point for a real OCR engine in a later milestone without changing the trigger flow.

- **Status Messaging Enhancements**

    - Status bar now distinguishes between:

        - No hash change (“No meaningful change detected…”).

        - Minor pixel changes below sensitivity threshold (“Minor change (~N%) below X% threshold – skipping analysis.”).

        - Significant changes where capture and analysis stub run, including approximate change percentage and target file path.

---

### Build & Run Instructions

1. **Clone the repo:**
    ```bash
    git clone https://github.com/smahmud/SpectraAssist.git
    ```
2. **Open the project in VS Code or Visual Studio.**

3. **Build and run the CortexView app:**

    ```bash
    dotnet build CortexView/CortexView.csproj
    dotnet run --project CortexView/CortexView.csproj 
    ```
4. **Use the overlay:**

    - Select an app window from the **App window** ComboBox.

    - Adjust the **Interval (s)** slider and the **Change sensitivity (%)** slider as needed.

    - Use **Start Monitoring** to begin periodic window capture.

    - Watch status messages and the AI context area to see when captures are skipped, when significant changes trigger analysis, and when **Request New Information** forces a manual analysis.

---

### Known Issues / Limitations

- OCR integration is a stub only; `TryExtractOcrText` does not yet call a real OCR engine, so no text is extracted from screenshots in

---

###  Acceptance Checklist

 - [X] Change sensitivity (%) slider (1–50, default 10) is visible in the overlay and updates an internal fraction used by the change‑detection logic.

  - [X] Downsampled grayscale pixel‑diff engine computes a changed‑pixel percentage for each capture.

  - [X] SHA1 hashing from Milestone 2 is still used as a fast equality guard before pixel‑based evaluation.

  - [X] Captures are classified as **NoChange, MinorChangeBelowThreshold**, or **SignificantChange** based on pixel diff and sensitivity.

  - [X] Significant changes trigger screenshot saving and call the unified analysis stub (`RunAnalysisIfNeededAsync`).

  - [X] The **Request New Information** button exists and always triggers analysis with reason `ManualOverride`, regardless of sensitivity/hash.

  - [X] OCR hook exists and is wired into the stub, even though it currently returns no text.
 
  - [X] Status bar and AI context area clearly indicate when changes are skipped, auto‑analyzed, or manually requested.

---

### Next Steps

- Refine pixel‑diff parameters (downsample size, noise threshold) and default sensitivity based on real‑world usage (IDEs, browsers, terminals).

-  Plug a real OCR engine into `TryExtractOcrText` and validate performance and usefulness on text‑heavy windows.

- Implement the richer overlay UI and controls from Milestone 4, reusing the existing Request New Information and Next Suggestion buttons.

- Replace the local analysis stub with an AWS Bedrock integration stub in Milestone 5, keeping the current trigger logic (auto vs manual) unchanged.