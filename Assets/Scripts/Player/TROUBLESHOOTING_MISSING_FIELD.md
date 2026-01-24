# Troubleshooting: "Input Actions Asset" Field Not Visible

## Quick Fixes to Try:

### 1. **Scroll Down in Inspector**
- The "Input System" section might be below what you can see
- Look for a **scrollbar** on the right side of the Inspector panel
- **Scroll down** to see if the "Input System" section appears below "Ground Check"

### 2. **Expand All Sections**
- Make sure all sections are expanded (not collapsed)
- Look for small **arrows/triangles** next to section headers
- Click any collapsed sections to expand them
- The "Input System" section should be the last one

### 3. **Force Unity to Recompile**
- In Unity, go to **Assets → Refresh** (or press `Ctrl+R` / `Cmd+R`)
- Wait for Unity to finish compiling (check bottom-right corner)
- The field should appear after compilation

### 4. **Check Console for Errors**
- Look at the **Console** panel (bottom of Unity)
- If there are any **red error messages**, fix those first
- Errors can prevent fields from showing in Inspector

### 5. **Verify Script is Attached**
- Make sure the **"Third Person Player Controller (Script)"** component is actually on MainCharacter
- If it's not there, add it: Click "Add Component" → Search "Third Person Player Controller"

### 6. **Try Removing and Re-adding Component**
- Click the **three dots (⋮)** menu on the "Third Person Player Controller" component
- Select **"Remove Component"**
- Then add it again: "Add Component" → "Third Person Player Controller"
- This forces Unity to refresh the component

---

## What the Field Should Look Like:

When visible, you should see:
```
┌─────────────────────────────────────┐
│ Input System                        │
│   Input Actions Asset               │
│   [None (Input Action Asset)]      │
└─────────────────────────────────────┘
```

This appears **below** the "Ground Check" section.

---

## If Still Not Visible:

The field might be hidden due to a Unity Inspector issue. Try this alternative approach:

1. **Select MainCharacter** in Hierarchy
2. In Inspector, find the **"Script"** field (at the top of Third Person Player Controller)
3. Click the **circle icon** next to it
4. This opens the script file - we can verify the field exists in code

If none of these work, the script might need to be reimported or there's a Unity version compatibility issue.
