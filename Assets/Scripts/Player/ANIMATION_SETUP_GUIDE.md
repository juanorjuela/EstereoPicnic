# Animation Setup Guide

## Overview
This guide will help you set up the Animator Controller to work with the Third Person Player Controller. The script automatically updates Animator parameters based on movement states.

## Step 1: Add Animator Component

1. **Select MainCharacter** in the Hierarchy
2. In the Inspector, click **"Add Component"**
3. Search for **"Animator"** and add it
4. **Note:** If your character model is a child object (like "Walking (1)"), you may need to add the Animator to that child instead

## Step 2: Assign Animator Controller

1. With MainCharacter selected, find the **Animator** component in the Inspector
2. In the **"Controller"** field, assign your Animator Controller:
   - If you have `mixamorig_Hips.controller`, drag it into this field
   - Or create a new Animator Controller (Right-click in Project → Create → Animator Controller)

## Step 3: Set Up Animator Parameters

Open your Animator Controller (double-click it in the Project panel):

1. In the **Parameters** tab (top-left of Animator window), click the **"+"** button to add parameters:

   **Add these parameters (exact names are important!):**
   
   - **Speed** (Float) - Controls animation speed based on movement
   - **IsGrounded** (Bool) - Whether character is on ground
   - **MovementState** (Int) - Current movement state (0=Idle, 1=Walking, 2=Running, 3=Sprinting, 4=Jumping, 5=Falling)

## Step 4: Create Animation States

In the Animator Controller window, create states for each movement type:

### Basic Setup (Minimum):

1. **Idle State:**
   - Right-click in empty space → Create State → Empty
   - Name it "Idle"
   - Set as default state (right-click → Set as Layer Default State)
   - If you have an idle animation, assign it in the Motion field

2. **Walking State:**
   - Right-click → Create State → Empty
   - Name it "Walking"
   - In the Motion field, assign your **Walking.fbx** animation clip
   - Set Speed to 1.0 (or adjust based on your animation)

3. **Running State** (optional, can use Walking with higher speed):
   - Right-click → Create State → Empty
   - Name it "Running"
   - Assign running animation if available, or reuse Walking

4. **Sprinting State** (optional):
   - Right-click → Create State → Empty
   - Name it "Sprinting"
   - Assign sprint animation if available

5. **Jumping State:**
   - Right-click → Create State → Empty
   - Name it "Jumping"
   - Assign jump animation if available

6. **Falling State:**
   - Right-click → Create State → Empty
   - Name it "Falling"
   - Assign falling animation if available

## Step 5: Create Transitions

Create transitions between states based on parameters:

### From Idle:
- **Idle → Walking:** 
  - Condition: `MovementState` Equals `1`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1-0.2

- **Idle → Jumping:**
  - Condition: `MovementState` Equals `4`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

### From Walking:
- **Walking → Idle:**
  - Condition: `MovementState` Equals `0`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1-0.2

- **Walking → Running:**
  - Condition: `MovementState` Equals `2`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

- **Walking → Sprinting:**
  - Condition: `MovementState` Equals `3`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

- **Walking → Jumping:**
  - Condition: `MovementState` Equals `4` AND `IsGrounded` Equals `false`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

### From Running:
- **Running → Walking:**
  - Condition: `MovementState` Equals `1`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

- **Running → Idle:**
  - Condition: `MovementState` Equals `0`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

### From Sprinting:
- **Sprinting → Running:**
  - Condition: `MovementState` Equals `2`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

- **Sprinting → Walking:**
  - Condition: `MovementState` Equals `1`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

### From Jumping:
- **Jumping → Falling:**
  - Condition: `MovementState` Equals `5`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

### From Falling:
- **Falling → Idle:**
  - Condition: `MovementState` Equals `0` AND `IsGrounded` Equals `true`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

- **Falling → Walking:**
  - Condition: `MovementState` Equals `1` AND `IsGrounded` Equals `true`
  - Has Exit Time: Unchecked
  - Transition Duration: 0.1

## Step 6: Configure Animation Speed (Optional)

You can use the **Speed** parameter to control animation playback speed:

1. Select a state (e.g., Walking)
2. In the Inspector, find **"Speed"**
3. Set it to use the parameter: Click the parameter icon → Select "Speed"
4. This will make the animation play faster when sprinting

## Step 7: Verify Setup

1. **Select MainCharacter** in Hierarchy
2. In the Inspector, verify:
   - **Animator** component is present
   - **Controller** field has your Animator Controller assigned
   - **Third Person Player Controller** script has the Animator field (it should auto-find it)

3. **Press Play** and test:
   - Character should play walking animation when moving
   - Animation should transition smoothly between states
   - Check the Animator window (Window → Animation → Animator) to see states change in real-time

## Quick Setup (Simplified)

If you only have a Walking animation and want to get started quickly:

1. Create two states: **Idle** and **Walking**
2. Add parameters: `Speed`, `IsGrounded`, `MovementState`
3. Create transitions:
   - Idle → Walking: `MovementState` Equals `1`
   - Walking → Idle: `MovementState` Equals `0`
4. Assign Walking animation to Walking state
5. Done! The character will walk when moving and be idle when stopped.

## Troubleshooting

### Animation not playing:
- Check that Animator component is on MainCharacter (or child with the model)
- Verify Animator Controller is assigned
- Make sure animation clips are assigned to states
- Check that parameters are named exactly: `Speed`, `IsGrounded`, `MovementState`

### Animation stuck in one state:
- Check transitions are set up correctly
- Verify parameter conditions match the values (0-5 for MovementState)
- Check "Has Exit Time" is unchecked for responsive transitions

### Character model not animating:
- Make sure Animator is on the GameObject with the SkinnedMeshRenderer
- If character is a child object, add Animator to the child, not parent
- Verify the Animator Controller has the correct avatar assigned

## Movement State Values Reference

- `0` = Idle
- `1` = Walking
- `2` = Running
- `3` = Sprinting
- `4` = Jumping
- `5` = Falling

These values are automatically set by the ThirdPersonPlayerController script based on character movement and physics state.
