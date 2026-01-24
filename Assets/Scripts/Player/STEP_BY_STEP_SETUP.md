# Step-by-Step Setup Guide

## ✅ Step 1: Generate C# Class (COMPLETED)
You've already done this! The `InputSystem_Actions.cs` file should now exist.

---

## 📝 Step 2: Configure MainCharacter GameObject

### 2.1 Select MainCharacter
1. In the **Hierarchy** panel (top-left), find and **click** on `MainCharacter`
2. The Inspector panel (top-right) should now show the MainCharacter's components

### 2.2 Add CharacterController Component
1. In the **Inspector** panel, click the **"Add Component"** button (at the bottom of the component list)
2. In the search box that appears, type: `Character Controller`
3. Click on **"Character Controller"** from the list
4. The CharacterController component will be added

### 2.3 Configure CharacterController Settings
In the CharacterController component that just appeared, set these values:
- **Height**: `2`
- **Radius**: `0.5`
- **Center**: 
  - X: `0`
  - Y: `1` (this positions the collider at the character's center)
  - Z: `0`
- **Slope Limit**: `45` (default is fine)
- **Step Offset**: `0.3` (default is fine)
- **Skin Width**: `0.08` (default is fine)

### 2.4 Add ThirdPersonPlayerController Script
1. Still in the **Inspector** panel, click **"Add Component"** again
2. Type: `Third Person Player Controller`
3. Click on **"Third Person Player Controller"** from the list
4. The script component will be added

### 2.5 Configure ThirdPersonPlayerController
In the ThirdPersonPlayerController component:

**Input System Section:**
1. Find the **"Input Actions"** field
2. In the **Project** panel (bottom-left), navigate to `Assets/InputSystem_Actions.inputactions`
3. **Drag** the `InputSystem_Actions.inputactions` file from Project panel into the **"Input Actions"** field in the Inspector

**Movement Settings (defaults are fine, but you can adjust):**
- Walk Speed: `3`
- Run Speed: `6`
- Sprint Speed: `9`
- Rotation Speed: `10`

**Jump Settings:**
- Jump Height: `2`
- Gravity Multiplier: `2`

**Ground Check:**
- Ground Check Distance: `0.1`
- Ground Layer Mask: Click the dropdown and select **"Default"** (or whatever layer your ground uses)

---

## 📷 Step 3: Configure Main Camera

### 3.1 Select Main Camera
1. In the **Hierarchy** panel, find and **click** on `Main Camera`
2. The Inspector panel will now show the Main Camera's components

### 3.2 Add ThirdPersonCameraController Script
1. In the **Inspector** panel, click **"Add Component"**
2. Type: `Third Person Camera Controller`
3. Click on **"Third Person Camera Controller"** from the list

### 3.3 Configure ThirdPersonCameraController

**Target Section:**
1. Find the **"Target"** field
2. In the **Hierarchy** panel, find `MainCharacter`
3. **Drag** `MainCharacter` from Hierarchy into the **"Target"** field in the Inspector

**Input System Section:**
1. Find the **"Input Actions"** field
2. **Drag** `InputSystem_Actions.inputactions` from Project panel into this field (same file as before)

**Camera Position (defaults are fine):**
- Camera Distance: `5`
- Height Offset: `2`
- Min Distance: `1`
- Max Distance: `10`

**Camera Rotation:**
- Mouse Sensitivity X: `2`
- Mouse Sensitivity Y: `2`
- Min Vertical Angle: `-30`
- Max Vertical Angle: `60`

**Smoothing:**
- Follow Smoothness: `10`
- Rotation Smoothness: `10`

**Collision:**
- Collision Layer Mask: Select **"Everything"** or specific layers that should block the camera
- Collision Radius: `0.3`

---

## ✅ Step 4: Test It!

1. Click the **Play** button (top-center of Unity Editor)
2. Test the controls:
   - **WASD** or **Arrow Keys**: Character should move
   - **Spacebar**: Character should jump
   - **Left Shift** (while moving): Character should sprint
   - **Mouse Movement**: Camera should orbit around character

---

## 🐛 Troubleshooting

### Character doesn't move:
- Make sure `InputSystem_Actions.inputactions` is assigned in both scripts
- Check that CharacterController component is enabled (checkbox at top-left of component)
- Verify the character is above the ground (check Transform position Y value)

### Character falls through ground:
- Make sure your ground objects have colliders
- Check that Ground Layer Mask includes your ground's layer
- Try increasing Ground Check Distance to `0.2`

### Camera doesn't follow:
- Verify Target is assigned (should show "MainCharacter" in the field)
- Make sure Main Camera is active (checkbox in Inspector)

### Input not working:
- Go to **Edit → Project Settings → Input System Package**
- Make sure **"Active Input Handling"** is set to **"Input System Package (New)"** or **"Both"**

---

## 📋 Quick Checklist

- [ ] CharacterController added to MainCharacter
- [ ] ThirdPersonPlayerController added to MainCharacter
- [ ] Input Actions assigned to ThirdPersonPlayerController
- [ ] ThirdPersonCameraController added to Main Camera
- [ ] MainCharacter assigned as Target in camera controller
- [ ] Input Actions assigned to ThirdPersonCameraController
- [ ] Press Play and test!
