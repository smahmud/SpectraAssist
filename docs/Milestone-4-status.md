# Milestone 4 Status — Overlay UI Features & User Controls

### Overview
This document summarizes the completed work for **Milestone 4 (v0.4.0)** of the SpectraAssist/CortexView WPF application. The goal was to evolve the functional prototype into a polished, ergonomic user interface. The overlay is now a floating, resizable window with a dark transparent aesthetic, collapsible controls, and keyboard accessibility, ready to host AI content in the next milestone.

---

### Implemented Features

- **Dark Transparent Aesthetic:**

    - **Floating Window:** The overlay is now a standard, resizable WPF window that can be moved anywhere on screen (multi-monitor friendly).

    - **Dark Tint Control:** The **Opacity Slider** now correctly controls the alpha channel of the dark background tint (0.2 to 1.0), allowing the desktop/app behind to show through clearly while maintaining text readability.

    - **Visual Polish:** White text on dark background ensures high contrast at any opacity setting.

- **Collapsible Control Panel:**

    - **Top Bar:** The "App Settings" region (Window Selector, Sliders, Capture Buttons) can now be collapsed to save screen space.

    - **Toggle Button:** A stylized toggle button (▲/▼) smoothly hides/shows the settings panel via XAML triggers.

- **Window Management:**

    - **Pin/Unpin:** A "Pin" toggle (📌) allows the user to switch "Always on Top" behavior on or off.

    - **Custom Chrome:** Replaced standard Windows chrome with a custom "Close" (X) button integrated into the header.

- **Keyboard Shortcuts:**

    - **`Ctrl+Shift+R`**: Triggers "Request New Information" (Manual Analysis).

    - **`Ctrl+Shift+N`**: Triggers "Next Suggestion" (Stub).

    - **`Ctrl+Shift+O`**: Toggles the Overlay UI (Collapse/Expand).

- **Enhanced Status Bar:**

    - **Visual Indicator:** Added a colored status dot to give immediate feedback:
        - 🟢 **Green (Idle):** Ready / Monitoring Started.
        - 🔴 **Red (Error):** Capture failed / Window closed.
        - 🟠 **Orange (Significant):** Change detected / Manual request sent.
        - 🔵 **Blue (Minor):** Change ignored (below threshold).

    - **Centralized Logic:** Refactored all status updates to use a unified helper, ensuring text and color are always synced.

---

### Build & Run Instructions

1. **Clone the repo:**
    ```bash
    git clone [https://github.com/smahmud/SpectraAssist.git](https://github.com/smahmud/SpectraAssist.git)
    ```
2. **Open the project in VS Code or Visual Studio.**

3. **Build and run the CortexView app:**
    ```bash
    dotnet build CortexView/CortexView.csproj
    dotnet run --project CortexView/CortexView.csproj 
    ```

4. **Use the overlay:**

    - Use **`Ctrl+Shift+O`** or click the header to collapse/expand settings.

    - Adjust the **Opacity** slider to see the transparency effect.

    - Click the **Pin (📌)** icon to toggle "Always on Top."

    - Use **`Ctrl+Shift+R`** to trigger a manual analysis and observe the status dot change color.

---

### Known Issues / Limitations

- The "Next Suggestion" button (`Ctrl+Shift+N`) is wired to the shortcut but currently only updates the status message (placeholder for Milestone 5).

- The suggestion display area uses a `RichTextBox` but currently displays stubbed text; real AI content formatting will be implemented when Bedrock is connected.

---

### Acceptance Checklist

- [x] Overlay is movable, resizable, and floating (not docked).

- [x] "App Settings" area is collapsible via button and shortcut (`Ctrl+Shift+O`).

- [x] Opacity slider smoothly adjusts background tint transparency without fading text.

- [x] Pin/Unpin toggle correctly sets "Always on Top" behavior.

- [x] Suggestion area uses `RichTextBox` for future rich-text support.

- [x] Status bar displays accurate state text and color-coded visual indicator.

- [x] Keyboard shortcuts (`R`, `N`, `O`) are functional.

- [x] No regressions in capture or change detection logic.

---

### Next Steps

- **AWS Bedrock Integration (Milestone 5):**
    - Replace the local analysis stub with a real `InvokeAWSBedrockAnalysis` client.
    - Send the captured screenshot (and optional OCR text) to an AWS Bedrock foundation model.
    - Display the actual AI response in the Suggestion Area.

- **Loading States:**
    - Add a visual spinner/progress bar during the API call latency.