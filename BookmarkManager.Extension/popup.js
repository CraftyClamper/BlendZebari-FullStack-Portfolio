document.addEventListener("DOMContentLoaded", () => {
    const lockScreen = document.getElementById("lock-screen");
    const vaultContent = document.getElementById("vault-content");
    const pinInput = document.getElementById("pin-input");
    const btnUnlock = document.getElementById("btn-unlock");

    const titleInput = document.getElementById("bookmark-title");
    const folderDropdown = document.getElementById("folder-dropdown");
    const btnSave = document.getElementById("btn-save");
    const btnCancel = document.getElementById("btn-cancel");
    const statusMsg = document.getElementById("status-msg");

    const blazorApiBase = "https://localhost:7215/api";
    let currentUsername = "";

    btnCancel.addEventListener("click", () => window.close());

    // 1. Check browser cookie for identity session layout
    chrome.cookies.get({ url: "https://localhost:7215", name: "VaultSession" }, (cookie) => {
        if (!cookie || !cookie.value) {
            lockScreen.style.display = "none";
            showStatus("⚠️ Please log into your website profile first.", "red");
            return;
        }
        currentUsername = cookie.value;
        showStatus(`Signed in as: ${currentUsername.toUpperCase()}. Enter PIN.`, "orange");
    });

    // 2. Unlock action driver (Talks to the MapGet endpoint in Program.cs)
    btnUnlock.addEventListener("click", async () => {
        const typedPin = pinInput.value;
        if (typedPin.length !== 4) {
            showStatus("⚠️ PIN must be exactly 4 digits.", "red");
            return;
        }

        showStatus("Verifying unique account vault PIN...", "blue");

        try {
            // Sends both username and PIN down the line to match your specific DB row entry safely!
            const response = await fetch(`${blazorApiBase}/folders/list?username=${encodeURIComponent(currentUsername)}&extensionPin=${encodeURIComponent(typedPin)}`);

            if (response.ok) {
                const folders = await response.json();

                // Switch panel views on screen
                lockScreen.style.display = "none";
                vaultContent.style.display = "block";
                showStatus("🔒 Vault unlocked successfully.", "green");

                // Populate folder select container dropdown rows
                folderDropdown.innerHTML = "";
                if (folders.length === 0) {
                    folderDropdown.innerHTML = '<option value="">No folders found.</option>';
                } else {
                    folders.forEach(f => {
                        const opt = document.createElement("option");
                        opt.value = f.id;
                        opt.textContent = f.name;
                        folderDropdown.appendChild(opt);
                    });
                }

                // Autoload active window tab information fields
                chrome.tabs.query({ active: true, currentWindow: true }, (tabs) => {
                    if (tabs && tabs.length > 0) {
                        titleInput.value = tabs[0].title || "Untitled Bookmark";
                    }
                });

            } else {
                showStatus("❌ Invalid extension PIN code for this user profile.", "red");
            }
        } catch (error) {
            showStatus("Network failure. Is your Blazor app hosting tunnel live?", "red");
        }
    });

    // 3. Bookmark submission save event handler (Talks to MapPost endpoint)
    btnSave.addEventListener("click", () => {
        chrome.tabs.query({ active: true, currentWindow: true }, async (tabs) => {
            if (!tabs || tabs.length === 0 || !tabs[0].url) {
                showStatus("Error: Unable to capture tab metrics context.", "red");
                return;
            }

            const payload = {
                url: tabs[0].url,
                title: titleInput.value || "Untitled Bookmark",
                username: currentUsername,
                extensionPin: pinInput.value, // Uses the actual PIN typed on the unlock screen!
                folderId: parseInt(folderDropdown.value)
            };

            showStatus("Saving link to database cluster...", "blue");

            try {
                const saveResponse = await fetch(`${blazorApiBase}/bookmarks/capture`, {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify(payload)
                });

                if (saveResponse.ok) {
                    showStatus("✓ Saved successfully to your private folder drawer!", "green");
                    setTimeout(() => window.close(), 1000);
                } else {
                    showStatus("Rejected: Secure database authorization write failed.", "red");
                }
            } catch (error) {
                showStatus("Network transfer failure.", "red");
            }
        });
    });

    function showStatus(text, color) {
        statusMsg.textContent = text;
        statusMsg.style.color = color;
    }
});
