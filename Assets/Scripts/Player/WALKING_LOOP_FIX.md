# Fix: Walking Animation Won't Loop

## Problem
Walking animation plays once and stops, even when holding the walking keys.

## Solution Checklist

### ✅ Step 1: Enable Loop Time on Animation Clip

**This is the most common fix!**

1. In the **Project** panel, find your **Walking.fbx** file
2. **Select** the Walking.fbx file
3. In the **Inspector**, go to the **"Animation"** tab
4. Find the animation clip (it might be named "Walking" or the same as the file)
5. **Check the "Loop Time" checkbox** ✅
6. Click **"Apply"** at the bottom

**If you don't see the animation clip:**
- The FBX might have multiple clips
- Look in the "Clips" section of the Animation tab
- Each clip needs "Loop Time" enabled individually

### ✅ Step 2: Check Walking State in Animator Controller

1. **Open your Animator Controller** (double-click `mixamorig_Hips.controller` in Project)
2. **Click on the "Walking" state** (the gray rectangle)
3. In the **Inspector**, check:
   - **Motion**: Should have your Walking animation assigned
   - **Speed**: Should be `1` (or use the Speed parameter)
   - **Write Defaults**: Can be checked or unchecked (try both if issues persist)

### ✅ Step 3: Check Transitions FROM Walking State

**This is critical!** Make sure transitions OUT of Walking don't trigger too early:

1. In the **Animator Controller**, look at transitions **FROM** the Walking state
2. **Click on each transition arrow** leaving Walking
3. In the **Inspector**, check the **Conditions**:

   **For Walking → Idle transition:**
   - Condition should be: `MovementState` `Equals` `0`
   - **NOT** `Speed` `Greater` `0` or `Speed` `Less` `0.1`
   - **Has Exit Time**: Should be **UNCHECKED** ✅
   - **Transition Duration**: Should be around `0.1-0.2`

   **For Walking → Running transition:**
   - Condition: `MovementState` `Equals` `2`
   - **Has Exit Time**: **UNCHECKED** ✅

   **For Walking → Sprinting transition:**
   - Condition: `MovementState` `Equals` `3`
   - **Has Exit Time**: **UNCHECKED** ✅

### ✅ Step 4: Verify Transition INTO Walking

1. Click on the transition **TO** Walking (from Idle)
2. In Inspector, verify:
   - Condition: `MovementState` `Equals` `1` (or `Speed` `Greater` `0` if using Speed)
   - **Has Exit Time**: **UNCHECKED** ✅
   - **Transition Duration**: `0.1-0.2`

### ✅ Step 5: Check Animator Parameters

In the Animator window, check the **Parameters** tab:

- **MovementState** should be an **Int** (not Float!)
- When walking, **MovementState** should show `1`
- **Speed** should show a value > 0

**If MovementState is showing as Float instead of Int:**
- Delete the parameter
- Re-add it as **Int** type
- Make sure the name is exactly `MovementState` (case-sensitive)

### ✅ Step 6: Test in Play Mode

1. **Press Play**
2. **Hold a movement key** (WASD)
3. **Watch the Animator window** (Window → Animation → Animator)
4. The "Walking" state should:
   - Turn **orange** (active)
   - **Stay orange** while keys are held
   - The animation should **loop continuously**

## Common Issues & Fixes

### Issue: Animation plays once then stops
**Fix:** Enable "Loop Time" on the animation clip (Step 1)

### Issue: Animation keeps switching back to Idle
**Fix:** Check Walking → Idle transition condition. It should be `MovementState == 0`, not `Speed == 0`

### Issue: Animation doesn't start
**Fix:** Check Idle → Walking transition. Make sure condition is correct and "Has Exit Time" is unchecked

### Issue: Animation plays but looks choppy
**Fix:** Increase "Transition Duration" to 0.2-0.3 for smoother blending

## Quick Debug Steps

1. **Select MainCharacter** in Hierarchy
2. **Press Play**
3. **Open Animator window** (Window → Animation → Animator)
4. **Hold a movement key**
5. **Watch the Animator:**
   - Walking state should turn orange
   - MovementState parameter should show `1`
   - Speed parameter should show a value > 0
   - If Walking state keeps switching back to Idle, check transitions

## Still Not Working?

If the animation still won't loop after these steps:

1. **Check the animation clip itself:**
   - Select Walking.fbx in Project
   - In Animation tab, preview the animation
   - Does it loop in the preview? If not, enable Loop Time

2. **Try a simple test:**
   - Create a transition: Walking → Walking (self-loop)
   - Condition: `MovementState` `Equals` `1`
   - This forces the state to stay active

3. **Check for conflicting transitions:**
   - Make sure "Any State" doesn't have a transition to Idle with a condition that triggers during walking

4. **Verify the script is updating parameters:**
   - In Play mode, check the Animator Parameters tab
   - MovementState should be `1` when walking
   - If it's not updating, check the script's Animator reference
