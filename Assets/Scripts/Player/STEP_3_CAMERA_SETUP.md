# Step 3: Configure Main Camera

## Quick Steps:

### 3.1 Select Main Camera
1. In the **Hierarchy** panel (top-left), find and **click** on `Main Camera`
2. The Inspector panel (top-right) will now show the Main Camera's components

### 3.2 Add ThirdPersonCameraController Script
1. In the **Inspector** panel, scroll down and click the **"Add Component"** button
2. In the search box, type: `Third Person Camera Controller`
3. Click on **"Third Person Camera Controller"** from the list
4. The script component will be added

### 3.3 Configure ThirdPersonCameraController

**Target Section:**
1. Find the **"Target"** field (should be at the top of the component)
2. In the **Hierarchy** panel, find `MainCharacter`
3. **Drag** `MainCharacter` from Hierarchy into the **"Target"** field in the Inspector
   - The field should change from "None (Transform)" to "MainCharacter (Transform)"

**Input System Section:**
1. Scroll down to find the **"Input System"** section
2. Find the **"Input Actions Asset"** field
3. **Drag** `InputSystem_Actions.inputactions` from the Project panel into this field
   - (Same file you used for the player controller)
   - The field should show "InputSystem_Actions (Input Action Asset)"

**Other Settings (defaults are fine, but you can adjust later):**
- Camera Distance: `5`
- Height Offset: `2`
- Mouse Sensitivity X: `2`
- Mouse Sensitivity Y: `2`
- Follow Smoothness: `10`
- Rotation Smoothness: `10`

---

## ✅ Verification Checklist:

After setup, you should have:
- [ ] Main Camera selected in Hierarchy
- [ ] ThirdPersonCameraController component added
- [ ] Target field shows "MainCharacter (Transform)"
- [ ] Input Actions Asset field shows "InputSystem_Actions (Input Action Asset)"

---

## 🎮 Ready to Test!

Once both are configured, press **Play** and test:
- **WASD/Arrow Keys**: Character moves
- **Spacebar**: Character jumps
- **Left Shift** (while moving): Character sprints
- **Mouse Movement**: Camera orbits around character
