# Milestone 2 Status — CortexView Window Capture & Change Detection

### Overview
This document summarizes the completed work for Milestone 2 of the SpectraAssist/CortexView WPF application. The goal was to move from monitor-based capture to **app window–based capture** with interval control, fast change detection, and an updated overlay layout, as specified in the Milestone 2 implementation plan.

---

### Implemented Features

- **Updated Overlay Layout:**

    - Two-row layout: top 1/4 for controls, bottom 3/4 reserved for AI context/help.

    - Bottom area includes a scrollable, read-only text surface for AI responses and a large “Next Suggestion” button stub.

- **Top-Level Window Enumeration:**

    - Uses Win32 APIs (`EnumWindows`, `IsWindowVisible`, `GetWindowText`, `GetWindowTextLength`, `GetShellWindow`) to list visible, user-facing top-level windows.

    - Filters out invisible/empty-title windows and obvious tool windows using extended style flags.

- **App Window Selector:**

    - `WindowSelector` ComboBox bound to a list of `TopLevelWindowInfo` objects (HWND + title).

    - Priority apps (VS Code, browsers, Office) detected via a keyword list and sorted to the top; all other visible windows follow in alphabetical order.

- **Window-Based Screenshot Capture:**

    - Capture operates only **on the selected app window** using `GetWindowRect` to determine bounds.

    - `CaptureNowButton` triggers an immediate capture; images are saved as timestamped PNG files under a dedicated `tests/output` folder next to the app.

- **Capture Interval & Monitoring:**

    - `CaptureIntervalSlider` (1–10 seconds, default 3s) with live label updates.

    - `DispatcherTimer` uses the slider value to control periodic capture when monitoring is enabled.

    - `MonitorToggleButton` starts/stops monitoring and toggles its label between “Start Monitoring” and “Stop Monitoring.”

- **Change Detection (Hash-Based):**

    - Each captured bitmap is hashed using SHA1 (fast, non-security use) via a `ComputeImageHash` helper.

    - `_lastImageHash` and `_lastCaptureTimeUtc` track the previous frame; if the current hash matches, the frame is treated as “unchanged” and no new PNG is written.

    - Status text clearly indicates when no meaningful change was detected versus when a new capture was saved.

- **Exclusion Mode Plumbing (Dynamic Detection):**

    - `ExclusionModeComboBox` shows “Dynamic Detection”, “Static Mask”, and “Configurable Exclusion” but is disabled in v1.0.0 with “Dynamic Detection” pre-selected.

    - An `ExclusionMode` enum and `_currentExclusionMode` field are wired to `SelectionChanged`, preparing for future exclusion behavior while keeping only Dynamic Detection conceptually active for this milestone.

- **Status Messaging & Logging:**

    - `StatusTextBlock` is the central surface for user feedback: startup hints, tracking info, capture results, and gentle error messages.

    - Messages cover scenarios such as no window selected, invalid/expired HWND, minimized/off-screen windows, and errors during timer captures, with details also logged via the existing `Logger`.

    - Startup and selection-change hints remind the user to keep CortexView outside the tracked app window to avoid self-capture; per-window DWM capture is explicitly postponed to a later milestone.

---

### Build & Run Instructions

1. **Clone the repo:**
    ```bash
    git clone https://github.com/smahmud/SpectraAssist.git
    ```
2. **Open the project in VS Code or Visual Studio.**

3.  **Build and run the CortexView app:**
    ```bash
    dotnet build CortexView/CortexView.csproj
    dotnet run --project CortexView/CortexView.csproj
    ```

4. **Use the overlay:**

    - Select an app window from the **App window** ComboBox.

    - Adjust the **Interval (s)** slider as needed.

    - Click **Capture Now** for a one-off capture or use **Start Monitoring** for periodic captures.

    - Inspect PNG files under `CortexView/tests/output` (or the configured output folder) to review captured windows.

---

### Known Issues / Limitations

- The window list still includes some background/utility windows (e.g., Alienware tools, Windows Input Experience); they are ordered after priority apps but not fully blacklisted yet.

- “Dynamic Detection” is wired only as a mode flag; actual input-region masking and exclusion logic are deferred to a later milestone.

- Per-window capture using DWM/`PrintWindow` is not implemented; captures use `CopyFromScreen`, so if CortexView overlaps the tracked window, the overlay appears in screenshots. Users are advised to keep CortexView outside the tracked window area.

- AI integration is stubbed: `NextSuggestionButton` and the AI context text area do not yet call AWS Bedrock or render real model output.

---

### Acceptance Checklist
 - [X] Only visible/app windows are selectable; top-level windows are enumerated via Win32 with basic filtering.

 - [X] Window-based capture implemented; monitor/screen capture UI paths from Milestone 1 are not exposed for v1.0.0.

 - [X] Capture interval slider (1–10s, default 3s) updates the timer; “Capture Now” performs immediate window capture.

 - [X] Change detection uses a fast hash (SHA1) to skip unchanged frames and avoid unnecessary downstream work.

 - [X] Exclusion mode ComboBox present with “Dynamic Detection” active and other modes shown for roadmap only.

 - [X] Gentle, user-facing status messages for capture issues and general operation; errors are logged.

 - [X] Overlay layout matches the updated spec: top control panel, bottom AI context area, modern minimal styling using only built-in WPF controls.

---

###  Next Steps

- Introduce JSON-based configuration for priority/blacklisted windows and, later, per-app exclusion settings.

- Explore per-window capture using DWM/`PrintWindow` to avoid including the overlay when windows overlap.

- Implement real “Dynamic Detection” masking to ignore user input regions during change detection.

- Begin AWS Bedrock integration to feed captured context into an LLM and populate the AI context/help area, wiring NextSuggestionButton into this flow.