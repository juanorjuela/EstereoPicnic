# How to Find InputSystem_Actions.inputactions and the "Input Actions Asset" Field

## ✅ Step 1: Find the InputSystem_Actions.inputactions File

**Location:** Project Panel (bottom-left of Unity Editor)

1. Look at the **Project** panel at the bottom-left of your Unity window
2. You should see a folder structure starting with `Assets`
3. Look for a file named **`InputSystem_Actions`** (it might show as just "InputSystem_Actions" without the .inputactions extension)
4. It should be directly in the `Assets` folder (not in a subfolder)
5. The icon should look like a gear/cog or a script icon

**If you can't find it:**
- Make sure the Project panel is showing (if hidden, go to Window → General → Project)
- Try searching in the Project panel's search box: type "InputSystem_Actions"
- It should be at: `Assets/InputSystem_Actions.inputactions`

---

## ✅ Step 2: Find the "Input Actions Asset" Field in Inspector

**Location:** Inspector Panel (top-right of Unity Editor)

1. **First, select MainCharacter** in the Hierarchy (top-left panel)
2. Look at the **Inspector** panel (top-right)
3. Scroll down in the Inspector until you see the **"Third Person Player Controller (Script)"** component
4. Look for a section labeled **"Input System"** (it has a header/folder icon)
5. Under "Input System", you should see a field labeled **"Input Actions Asset"**
6. This field will be empty (showing "None (Input Action Asset)")

**Visual Guide:**
```
Inspector Panel
├── Transform
├── Character Controller
└── Third Person Player Controller (Script)
    ├── Movement Settings
    ├── Jump Settings
    ├── Ground Check
    └── Input System  ← Look for this section
        └── Input Actions Asset  ← This is the field you need!
```

---

## ✅ Step 3: Drag and Drop

1. **Click and hold** on the `InputSystem_Actions` file in the Project panel
2. **Drag** it over to the Inspector panel
3. **Drop** it into the **"Input Actions Asset"** field (the one that says "None (Input Action Asset)")
4. The field should now show "InputSystem_Actions (Input Action Asset)" instead of "None"

**Alternative method:**
1. Click on the small **circle icon** (⭕) next to the "Input Actions Asset" field
2. A window will pop up - search for "InputSystem_Actions"
3. Double-click on it to select it

---

## ✅ Step 4: Verify It Worked

After dragging, the Inspector should show:
- **Input Actions Asset**: `InputSystem_Actions (Input Action Asset)` (instead of "None")

If you see this, you're done with this step! ✅

---

## 🐛 Troubleshooting

### The "Input Actions Asset" field doesn't appear:
- Make sure you selected **MainCharacter** in the Hierarchy
- Make sure the **Third Person Player Controller** component is added to MainCharacter
- Try clicking the small arrow next to "Third Person Player Controller" to expand it
- Check if there's a scrollbar - scroll down in the Inspector

### The file doesn't appear in Project panel:
- Make sure you're looking in the `Assets` folder (not a subfolder)
- Try using the search box in the Project panel: type "InputSystem"
- Check if the file exists at: `Assets/InputSystem_Actions.inputactions`

### After dragging, it still shows "None":
- Make sure you're dragging the correct file (should be `InputSystem_Actions.inputactions`)
- Try the alternative method (click the circle icon)
- Make sure Unity has finished compiling (check bottom-right corner for progress)
