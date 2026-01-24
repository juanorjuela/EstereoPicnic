# Third Person Player Controller Setup Instructions

## Prerequisites

1. **Generate Input System C# Class** (if not already done):
   - Select `Assets/InputSystem_Actions.inputactions` in the Project window
   - In the Inspector, click "Generate C# Class" button
   - This creates the `InputSystem_Actions` class that the scripts use

## Scene Setup Steps

### 1. Configure MainCharacter GameObject

1. **Select the MainCharacter GameObject** in the Hierarchy (the "Walking" prefab instance)

2. **Add CharacterController Component** (if not present):
   - Click "Add Component" → Search for "Character Controller"
   - Configure settings:
     - **Height**: 2
     - **Radius**: 0.5
     - **Center**: (0, 1, 0) - Adjust based on your character model
     - **Slope Limit**: 45
     - **Step Offset**: 0.3
     - **Skin Width**: 0.08

3. **Add ThirdPersonPlayerController Script**:
   - Click "Add Component" → Search for "Third Person Player Controller"
   - Assign the **Input Actions** field:
     - Drag `Assets/InputSystem_Actions.inputactions` into the field
   - Configure movement settings (defaults should work, but adjust as needed):
     - **Walk Speed**: 3
     - **Run Speed**: 6
     - **Sprint Speed**: 9
     - **Rotation Speed**: 10
   - Configure jump settings:
     - **Jump Height**: 2
     - **Gravity Multiplier**: 2
   - Configure ground check:
     - **Ground Check Distance**: 0.1
     - **Ground Layer Mask**: Select the layer(s) your ground uses (usually "Default")

### 2. Configure Main Camera

1. **Select the Main Camera** GameObject in the Hierarchy

2. **Add ThirdPersonCameraController Script**:
   - Click "Add Component" → Search for "Third Person Camera Controller"
   - Assign the **Target** field:
     - Drag the MainCharacter GameObject from Hierarchy into this field
   - Assign the **Input Actions** field:
     - Drag `Assets/InputSystem_Actions.inputactions` into the field
   - Configure camera position (adjust as needed):
     - **Camera Distance**: 5
     - **Height Offset**: 2
     - **Min Distance**: 1
     - **Max Distance**: 10
   - Configure camera rotation:
     - **Mouse Sensitivity X**: 2
     - **Mouse Sensitivity Y**: 2
     - **Min Vertical Angle**: -30
     - **Max Vertical Angle**: 60
   - Configure smoothing:
     - **Follow Smoothness**: 10
     - **Rotation Smoothness**: 10
   - Configure collision:
     - **Collision Layer Mask**: Select layers that should block the camera
     - **Collision Radius**: 0.3

### 3. Verify Input System Settings

1. **Check Project Settings**:
   - Edit → Project Settings → Input System Package
   - Ensure "Active Input Handling" is set to "Input System Package (New)" or "Both"

2. **Verify Input Actions**:
   - The `InputSystem_Actions.inputactions` file should have:
     - Move action (WASD/Arrow keys) ✓
     - Jump action (Spacebar) ✓
     - Sprint action (Left Shift) ✓
     - Look action (Mouse delta) ✓

## Testing

After setup, press Play and test:

- **WASD or Arrow Keys**: Character should move
- **Spacebar**: Character should jump
- **Left Shift (while moving)**: Character should sprint
- **Mouse Movement**: Camera should orbit around character

## Troubleshooting

### Character doesn't move:
- Check that Input Actions asset is assigned in ThirdPersonPlayerController
- Verify "Active Input Handling" in Project Settings
- Check that CharacterController component is enabled

### Character falls through ground:
- Verify Ground Layer Mask includes your ground objects' layer
- Adjust Ground Check Distance if needed
- Ensure ground objects have colliders

### Camera doesn't follow:
- Verify Target is assigned in ThirdPersonCameraController
- Check that Input Actions asset is assigned
- Ensure Main Camera is active

### Input not working:
- Ensure Input System Package is installed
- Check Project Settings → Input System Package → Active Input Handling
- Verify Input Actions C# class was generated

## Movement States

The system tracks these states (accessible via `CurrentMovementState` property):
- **Idle**: No movement input
- **Walking**: Moving at walk speed
- **Running**: Moving at run speed
- **Sprinting**: Moving at sprint speed (shift held)
- **Jumping**: In air, ascending
- **Falling**: In air, descending

These states can be used later for animator parameter updates.
